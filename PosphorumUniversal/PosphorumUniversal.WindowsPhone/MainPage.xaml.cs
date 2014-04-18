using PosphorumUniversal.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace PosphorumUniversal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public MainPage()
        {
            this.InitializeComponent();
            if (ApplicationData.Current.LocalSettings.Values["ForceKorean"] == null)
                ApplicationData.Current.LocalSettings.Values["ForceKorean"] = true;//temporary setting

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        private void NavigateClicked(object sender, RoutedEventArgs e)
        {
            Day context = (sender as Button).DataContext as Day;
            Frame.Navigate(typeof(DayDetailPage), context);
        }

        private void itemGridView_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListViewBase).SelectedItems.Count == 0)
            {
                ClearButton.Visibility = Visibility.Collapsed;
                DietCommand.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
                TextListContainer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                ClearButton.Visibility = Visibility.Visible;
                DietCommand.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
                TextListContainer.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }

            foreach (object o in e.AddedItems)
            {
                textList.Items.Insert(0, o);
            }
            foreach (object o in e.RemovedItems)
            {
                textList.Items.Remove(o);
            }
        }

        private void ClearButtonClicked(object sender, RoutedEventArgs e)
        {
            itemListView.SelectedItems.Clear();
        }

        private async void RefreshButtonClicked(object sender, RoutedEventArgs e)
        {
            await Refresh();
        }

        async Task Refresh()
        {
            Application.Current.Resources.Remove("NewDataChecked");
            await SetData(true, getOffset());
        }

        async Task SetData(Boolean ForceDataReload, Double scrollOffset)
        {
            Boolean IsNewDataNeeded = false;

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            if (!ForceDataReload)
            {
                try
                {
                    StorageFile file = await localFolder.GetFileAsync("dietMenuData.xml");
                    try
                    {
                        String str = await FileIO.ReadTextAsync(file);
                        DietMenu dietmenu = new DietMenu(XDocument.Parse(str));
                        if (new TimeSpan(DateTime.Now.Ticks - dietmenu.Days[0].ServedDate.Ticks).Days >= 7)
                            IsNewDataNeeded = true;
                        else
                        {
                            this.DefaultViewModel["Groups"] = dietmenu.Days;
                        }
                    }
                    catch
                    {
                        IsNewDataNeeded = true;
                    }
                }
                catch
                {
                    IsNewDataNeeded = true;
                }
            }
            else
                IsNewDataNeeded = true;

            String message = null;
            if (IsNewDataNeeded)
            {
                try
                {
                    XDocument parsedMenu = await (new MealLoader()).GetDietmenu();
                    DietMenu dietMenu = new DietMenu(parsedMenu);
                    this.DefaultViewModel["Groups"] = dietMenu.Days;

                    if (new TimeSpan(DateTime.Now.Ticks - dietMenu.Days[0].ServedDate.Ticks).Days < 7)
                    {
                        StorageFile file = await localFolder.CreateFileAsync("dietMenuData.xml", CreationCollisionOption.ReplaceExisting);
                        await FileIO.WriteTextAsync(file, parsedMenu.ToString());
                    }
                    else
                    {
                        if (!Application.Current.Resources.ContainsKey("NewDataChecked"))
                        {
                            message = "이번 주 새 식단표가 아직 올라오지 않았습니다. 몇 시간 뒤에 리프레시해서 다시 확인해 주세요.";
                            Application.Current.Resources["NewDataChecked"] = true;
                        }
                    }
                }
                catch (FormatException)
                {
                    var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                    message = loader.GetString("FormatException");//"식단표 형식이 바뀌어 현재 PosPhorum에서 식단표를 읽어들일 수 없습니다. 새로운 형식에 맞춘 PosPhorum 업데이트를 기다려 주세요. 업데이트가 계속 나오지 않으면 saschanaz@outlook.com으로 문제를 신고해 주세요.";
                }
                catch
                {
                    var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                    message = loader.GetString("UnknownException");//"서버에 문제가 있거나 인터넷 연결이 원활하지 않습니다. 상태를 확인해 주신 후 앺 바에서 리프레시 버튼을 눌러 시간표를 다시 읽어 주시기 바랍니다.";
                }
                //localSettings.Values["dietMenuData"] = dietMenu.Stringify();
            }

            setOffsetHandler(scrollOffset);

            if (message != null)
                await new Windows.UI.Popups.MessageDialog(message).ShowAsync();
        }

        void setOffsetHandler(Double scrollOffset)
        {
            var scrollViewer = GetVisualChild<ScrollViewer>(itemListView);
            if (scrollViewer != null && scrollViewer.ScrollableHeight >= scrollOffset)
            {
                System.Diagnostics.Debug.WriteLine("trying scroll: count " + itemListView.Items.Count + " height " + scrollViewer.ScrollableHeight);
                System.Diagnostics.Debug.WriteLine("scroll no waiting");
                setOffset(scrollOffset);
            }
            else if (scrollViewer != null)
            {
                System.Diagnostics.Debug.WriteLine("trying scroll: count " + itemListView.Items.Count + " height " + scrollViewer.ScrollableHeight);
                System.Diagnostics.Debug.WriteLine("waiting scrollviewer");
                EventHandler<Object> handler = null;
                handler = delegate
                {
                    System.Diagnostics.Debug.WriteLine("waited scroll");
                    setOffset(scrollOffset);
                    scrollViewer.LayoutUpdated -= handler;
                };
                scrollViewer.LayoutUpdated += handler;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("scroll waiting");
                RoutedEventHandler handler = null;
                handler = delegate
                {
                    System.Diagnostics.Debug.WriteLine("waited scroll");
                    setOffset(scrollOffset);
                    itemListView.Loaded -= handler;
                };
                itemListView.Loaded += handler;
            }
        }
        void setOffset(Double scrollOffset)
        {
            if (scrollOffset >= 0)
                GetVisualChild<ScrollViewer>(itemListView).ChangeView(null, scrollOffset, null, true);
            else
            {
                foreach (object o in itemListView.Items)
                {
                    if ((o as MealData).ServedDate.Day == DateTime.Now.Day)
                    {
                        itemListView.ScrollIntoView(o);
                        break;
                    }
                }
            }
        }
        Double getOffset()
        {
            return GetVisualChild<ScrollViewer>(itemListView).VerticalOffset;
        }

        public T GetVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                DependencyObject v = (DependencyObject)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                    child = GetVisualChild<T>(v);
                if (child != null)
                    break;
            }
            return child;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            Double previousScrollOffset = -1;
            if (e.PageState != null)
            {
                if (e.PageState.ContainsKey("ScrollOffset"))
                    previousScrollOffset = (Double)e.PageState["ScrollOffset"];
            }
            await SetData(false, previousScrollOffset);
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            e.PageState["ScrollOffset"] = GetVisualChild<ScrollViewer>(itemListView).VerticalOffset;
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}
