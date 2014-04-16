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
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace PosphorumUniversal
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }


        public MainPage()
        {
            this.InitializeComponent();
            if (ApplicationData.Current.LocalSettings.Values["ForceKorean"] == null)
                ApplicationData.Current.LocalSettings.Values["ForceKorean"] = false;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }

        void registerToNotifier()
        {
            var notifier = (SettingChangeNotifier)Application.Current.Resources["settingChangeNotifier"];
            notifier.SettingChanged += updateAsSettingChanged;
        }

        void unregisterToNotifier()
        {
            var notifier = (SettingChangeNotifier)Application.Current.Resources["settingChangeNotifier"];
            notifier.SettingChanged -= updateAsSettingChanged;
        }

        void updateAsSettingChanged(object sender, SettingChangedEventArgs e)
        {
            if (e.SettingType == SettingTypes.ForceKorean)
                LanguageOptionUpdate();
        }

        void LanguageOptionUpdate()
        {
            List<object> templist = textList.Items.ToList();
            textList.Items.Clear();
            foreach (Object o in templist)
                textList.Items.Add(o);
        }
        private void PageUnloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Current_SizeChanged;
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            changeState(ActualWidth);
            Window.Current.SizeChanged += Current_SizeChanged;
        }

        Boolean IsSnapped()
        {
            return itemListView.Visibility != Visibility.Collapsed;
        }

        Boolean IsSwitching = false;
        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            IsSwitching = true;
            var isSnapped = IsSnapped();
            switch (changeState(e.Size.Width))
            {
                case "Snapped":
                    if (!isSnapped)
                    {
                        itemListView.SelectedItems.Clear();
                        foreach (Object o in textList.Items)
                            itemListView.SelectedItems.Add(o);
                    }
                    break;
                default:
                    if (isSnapped)
                    {
                        itemGridView.SelectedItems.Clear();
                        foreach (Object o in textList.Items)
                            itemGridView.SelectedItems.Add(o);
                    }
                    break;
            }
            IsSwitching = false;
        }

        String changeState(Double Width)
        {
            String stateName;
            if (Width > 500)
            {
                var winOrientation = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().Orientation;
                if (winOrientation == Windows.UI.ViewManagement.ApplicationViewOrientation.Portrait)
                    stateName = "FullScreenPortrait";
                else
                    stateName = "FullScreenLandscape";
            }
            else
                stateName = "Snapped";

            VisualStateManager.GoToState(this, stateName, true);
            return stateName;
        }

        async Task SetData(Boolean ForceDataReload, Double scrollOffset, Boolean snapped)
        {
            semanticView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
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
                            //itemGridViewZoomedOut.ItemsSource = groupedItemsViewSource.View.CollectionGroups;
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
                    //itemGridViewZoomedOut.ItemsSource = groupedItemsViewSource.View.CollectionGroups;

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

            RoutedEventHandler handler = null;
            handler = delegate
            {
                setOffset(scrollOffset, snapped);
                itemGridView.Loaded -= handler;
            };
            itemGridView.Loaded += handler;
            semanticView.Visibility = Windows.UI.Xaml.Visibility.Visible;

            if (message != null)
                await new Windows.UI.Popups.MessageDialog(message).ShowAsync();
        }

        void setOffset(Double scrollOffset, Boolean snapped)
        {
            if (!IsSnapped())
            {
                if (scrollOffset >= 0 && !snapped)
                    GetVisualChild<ScrollViewer>(itemGridView).ChangeView(scrollOffset, null, null);
                else
                {
                    foreach (object o in itemGridView.Items)
                    {
                        if ((o as MealData).ServedDate.Day == DateTime.Now.Day)
                        {
                            itemGridView.ScrollIntoView(o, ScrollIntoViewAlignment.Leading);
                            break;
                        }
                    }
                }
            }
            else
            {
                if (scrollOffset >= 0 && snapped)
                    GetVisualChild<ScrollViewer>(itemListView).ChangeView(null, scrollOffset, null);
                else
                {
                    foreach (object o in itemListView.Items)
                    {
                        if ((o as MealData).ServedDate.Day == DateTime.Now.Day)
                        {
                            itemListView.ScrollIntoView(o, ScrollIntoViewAlignment.Leading);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Assign a collection of bindable groups to this.DefaultViewModel["Groups"]
            Double previousScrollOffset = -1;
            Boolean snapped = false;
            if (e.PageState != null)
            {
                if (e.PageState.ContainsKey("ScrollOffset"))
                    previousScrollOffset = (Double)e.PageState["ScrollOffset"];
                if (e.PageState.ContainsKey("PageViewState"))
                    snapped = (Boolean)e.PageState["PageViewState"];
            }
            await SetData(false, previousScrollOffset, snapped);
            //SettingsPane.GetForCurrentView().CommandsRequested += DietGroupedPage_CommandsRequested;
            registerToNotifier();
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            Boolean snapped = IsSnapped();

            e.PageState["PageViewState"] = snapped;
            if (!snapped)
                e.PageState["ScrollOffset"] = GetVisualChild<ScrollViewer>(itemGridView).HorizontalOffset;
            else
                e.PageState["ScrollOffset"] = GetVisualChild<ScrollViewer>(itemListView).VerticalOffset;

            //SettingsPane.GetForCurrentView().CommandsRequested -= DietGroupedPage_CommandsRequested;
            unregisterToNotifier();
        }

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
            this.Loaded += PageLoaded;
            this.Unloaded += PageUnloaded;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

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

        private void itemGridView_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (!IsSwitching)
            {
                if ((sender as ListViewBase).SelectedItems.Count == 0)
                {
                    TextListContainer.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    bottomAppBar.IsOpen = false;
                    //bottomAppBar.IsSticky = false;
                }
                else
                {
                    TextListContainer.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    if (!bottomAppBar.IsOpen)
                    {
                        bottomAppBar.IsOpen = true;
                        //bottomAppBar.IsSticky = true;
                    }
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
        }

        private void ClearButtonClicked(object sender, RoutedEventArgs e)
        {
            itemGridView.SelectedItems.Clear();
            itemListView.SelectedItems.Clear();
            bottomAppBar.IsOpen = false;
        }

        private void ItemClicked(object sender, ItemClickEventArgs e)
        {
            ListViewBase gridView = sender as ListViewBase;
            if (gridView.SelectedItems.Contains(e.ClickedItem))
            {
                if (gridView.SelectedItems.Count > 1)
                {
                    gridView.SelectedItem = e.ClickedItem;
                }
                else
                {
                    gridView.SelectedItems.Remove(e.ClickedItem);
                }
            }
            else
            {
                gridView.SelectedItem = e.ClickedItem;
            }
        }

        private async void RefreshButtonClicked(object sender, RoutedEventArgs e)
        {
            await Refresh();
        }

        async Task Refresh()
        {
            Application.Current.Resources.Remove("NewDataChecked");
            Boolean snapped = IsSnapped();
            Double offset;
            ScrollViewer viewer = GetVisualChild<ScrollViewer>(itemGridView);
            if (viewer != null)
                if (snapped)
                    offset = viewer.VerticalOffset;
                else//'H'
                    offset = viewer.HorizontalOffset;
            else
                offset = 0;
            await SetData(true, offset, snapped);
        }

        private void NavigateClicked(object sender, RoutedEventArgs e)
        {
            Day context = (sender as Button).DataContext as Day;
            Frame.Navigate(typeof(DayDetailPage), context);
        }

        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            switch (e.AddedItems[0] as String)
            {
                case "Korean":
                    break;
                case "English-US":
                    break;
            }
        }
    }

    public class MealBlockDataTemplateSelector : DataTemplateSelector
    {
        protected override DataTemplate
            SelectTemplateCore(object item, DependencyObject container)
        {
            if (container != null && item != null && item is MealData)
            {
                var currentFrame = Window.Current.Content as Frame;
                var currentPage = currentFrame.Content as Page;

                MealData data = item as MealData;

                if (data != null)
                {
                    if (!data.HighestCalories)
                        return currentPage.Resources["mealBlockShortTemplate"] as DataTemplate;
                    else
                        return currentPage.Resources["mealBlockTallTemplate"] as DataTemplate;
                }
            }

            return null;
        }
    }
}
