/*
 * Copyright (c) 2012, SaschaNaz
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 * * Redistributions of source code must retain the above copyright notice, this list
 * of conditions and the following disclaimer.
 * * Redistributions in binary form must reproduce the above copyright notice, this list
 * of conditions and the following disclaimer in the documentation and/or other materials
 * provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
 * SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT
 * OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR
 * TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using Posphorum.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Xml.Linq;
using Windows.Web.Http;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.ApplicationSettings;
using System.Text.RegularExpressions;

// The Grouped Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234231

namespace Posphorum
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class DietGroupedPage : Page
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


        //Popup _settingsPopup;

        public DietGroupedPage()
        {
            this.InitializeComponent();
            if (ApplicationData.Current.LocalSettings.Values["ForceKorean"] == null)
                ApplicationData.Current.LocalSettings.Values["ForceKorean"] = false;
            //preferedLanguage = "ko";
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

        //void DietGroupedPage_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        //{
        //    var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
        //    var languageTitle = loader.GetString("LanguageTitle");
        //    SettingsCommand cmd = new SettingsCommand("lang", languageTitle, (x) =>
        //        {
        //            Int32 _settingsWidth = 370;
        //            Rect _windowBounds = Window.Current.Bounds;
        //            _settingsPopup = new Popup();
        //            _settingsPopup.Closed += OnPopupClosed;
        //            Window.Current.Activated += OnWindowActivated;
        //            _settingsPopup.IsLightDismissEnabled = true;
        //            _settingsPopup.Width = _settingsWidth;
        //            _settingsPopup.Height = _windowBounds.Height;

        //            LanguageControl control = new LanguageControl();
        //            SettingsFlyout mypane = new SettingsFlyout(languageTitle, control)
        //            {
        //                Width = _settingsWidth,
        //                Height = _windowBounds.Height
        //            };
        //            control.SettingChanged += delegate(object sender2, GlobalSettingChangedEventArgs e)
        //            {
        //                ApplicationData.Current.LocalSettings.Values["ForceKorean"] = e.Value;
        //                LanguageOptionUpdate();
        //                //await new Windows.UI.Popups.MessageDialog(String.Format("Setting Changed: {0} / {1}", e.WhatSetting.ToString(), e.Value.ToString())).ShowAsync();
        //            };                    

        //            _settingsPopup.Child = mypane;
        //            _settingsPopup.SetValue(Canvas.LeftProperty, _windowBounds.Width - _settingsWidth);
        //            _settingsPopup.SetValue(Canvas.TopProperty, 0);
        //            _settingsPopup.IsOpen = true;
        //        });

        //    args.Request.ApplicationCommands.Add(cmd);

        //    var ppolicyTitle = loader.GetString("PrivacyPolicyTitle");
        //    cmd = new SettingsCommand("ppolicy", ppolicyTitle, (x) =>
        //    {
        //        Int32 _settingsWidth = 370;
        //        Rect _windowBounds = Window.Current.Bounds;
        //        _settingsPopup = new Popup();
        //        _settingsPopup.Closed += OnPopupClosed;
        //        Window.Current.Activated += OnWindowActivated;
        //        _settingsPopup.IsLightDismissEnabled = true;
        //        _settingsPopup.Width = _settingsWidth;
        //        _settingsPopup.Height = _windowBounds.Height;

        //        SettingsFlyout mypane = new SettingsFlyout(ppolicyTitle, new TextBlock() { Text = loader.GetString("PrivacyPolicyContent"), TextWrapping = TextWrapping.Wrap, FontSize = 15 })
        //        {
        //            Width = _settingsWidth,
        //            Height = _windowBounds.Height
        //        };

        //        _settingsPopup.Child = mypane;
        //        _settingsPopup.SetValue(Canvas.LeftProperty, _windowBounds.Width - _settingsWidth);
        //        _settingsPopup.SetValue(Canvas.TopProperty, 0);
        //        _settingsPopup.IsOpen = true;
        //    });

        //    args.Request.ApplicationCommands.Add(cmd);
        //}

        //private void OnWindowActivated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        //{
        //    if (e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
        //    {
        //        _settingsPopup.IsOpen = false;
        //    }
        //}

        //void OnPopupClosed(object sender, object e)
        //{
        //    Window.Current.Activated -= OnWindowActivated;
        //}

        void LanguageOptionUpdate()
        {
            //Application.Current.Resources["preferedLanguage"] = "en-US";
            List<object> templist = textList.Items.ToList();
            textList.Items.Clear();
            foreach (Object o in templist)
                textList.Items.Add(o);
            //await Refresh();
        }

        //public String preferedLanguage
        //{
        //    //get
        //    //{
        //    //    if ((Boolean)Application.Current.Resources["ForceKorean"])
        //    //        return "ko";
        //    //    else
        //    //    {
        //    //        return null;
        //    //    }
        //    //}
        //    get;
        //    set;
        //}
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
                            itemGridViewZoomedOut.ItemsSource = groupedItemsViewSource.View.CollectionGroups;
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
                    XDocument parsedMenu = await ParseDietmenu();
                    DietMenu dietMenu = new DietMenu(parsedMenu);
                    this.DefaultViewModel["Groups"] = dietMenu.Days;
                    itemGridViewZoomedOut.ItemsSource = groupedItemsViewSource.View.CollectionGroups;

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

        async Task<XDocument> ParseDietmenu()
        {
            /*
             * 자 전체 구조는 이런거죠
             * Dietmenu 안에
             * Day가 들어 있고
             * Day 안에
             * Time이 들어 있고 - 이때 속성으로 아침점심저녁
             * Time 안에 각 Foods가 들어 있죠 그런거죠! - 이때 속성으로 ABCDEFG (?
             * 
             */

            XDocument xdietmenu = new XDocument();
            XElement dietofweek = new XElement("DietofWeek");
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(new Uri("http://page.postech.ac.kr/res1/")))
                {
                    String str = await response.Content.ReadAsStringAsync();
                    await Task.Run(() =>
                    {
                        str = str.Replace("&nbsp;", "\u0020")
                            .Replace("&shy;", "\u00AD");
                            //str = str.Remove(str.IndexOf("<![if !supportMisalignedRows]>"), str.IndexOf("<![endif]>") - str.IndexOf("<![if !supportMisalignedRows]>") + 10);

                        str = Regex.Replace(str, @"<(\w+)", m => m.Value.ToLower());
                        str = Regex.Replace(str, @"<br>", "<br/>");
                        str = Regex.Replace(str, @"(</tbody>)\s+(</table>)\s+</div>", "$1$2</body>");
                        
                        //<col>s
                        str = Regex.Replace(str, @"(<col\s.+>)", "$1</col>");

                        //Illegal attributes
                        str = Regex.Replace(str, @"\w+=#?\w+", "");

                        //I don't know why but there are some tags like <TD style="..." 8  >
                        str = Regex.Replace(str, @"\s[0-9]\s+\/?>", ">");
                    });
                    XElement xelm = XElement.Parse(str);
                    XElement[] tablerows = null;
                    {
                        XElement body = xelm.Element("body");
                        XElement[] tables = body.Descendants("tbody").ToArray();
                        foreach (XElement table in tables)
                        {
                            XElement[] rows = table.Elements().ToArray();
                            if (rows.Length >= 14) {
                                tablerows = rows;
                                break;
                            }    
                        }
                    }

                    var startingRow = 3;
                    for (Int32 i = startingRow; i <= startingRow + 14; i += 2)//each two rows are for a day
                    {
                        XElement xday = new XElement("Day");
                        XElement[] columns = tablerows[i].Elements().ToArray();
                        XElement[] columnscal = tablerows[i + 1].Elements().ToArray();

                        #region 날짜
                        {
                            String[] daydayofweek = columns[0]
                                .Elements().First()//p
                                .Elements().First()//font
                                .Value.Split('(');//날짜 및 요일. Filtering by Split('\n') seldomly fails because '\n' disappears randomly.
                            String[] monthday = SplitIntoTwoBySlash(daydayofweek[0]);
                            xday.Add(
                                new XAttribute("Month", Convert.ToInt32(monthday[0])),
                                new XAttribute("Day", Convert.ToInt32(monthday[1])));
                            //new XAttribute("DayofWeek", daydayofweek[1][1])); DateTime 클래스는 이미 DayOfWeek 프로퍼티를 갖고 있다. 따라서 또 넣을 필요는 없음.
                        }
                        #endregion

                        //아침
                        xday.Add(
                            new XElement("Time",
                                new XAttribute("When", "Breakfast"),
                                MakeFoodsElement(columns[1], columnscal[1], "A"),
                                MakeFoodsElement(columns[2], columnscal[2], "B")));

                        //점심
                        xday.Add(
                            new XElement("Time",
                                new XAttribute("When", "Lunch"),
                                MakeFoodsElement(columns[3], columnscal[3], "A"),
                                MakeDualFoodsElement(columns[5], columnscal[5], "C", "D")));

                        //저녁
                        xday.Add(
                            new XElement("Time",
                                new XAttribute("When", "Dinner"),
                                MakeFoodsElement(columns[4], columnscal[4], "A"),
                                columns.Length > 6 ? MakeDualFoodsElement(columns[6], columnscal[6], "C", "D") : null));

                        dietofweek.Add(xday);
                    }
                    xdietmenu.Add(dietofweek);
                }
            }
            //지금은 시간이랑 타입을 하드코딩해서 넣지만 맨 윗줄에서 읽어들여서 자동화하는 방법을 생각해봅시다

            return xdietmenu;
        }

        //그냥 두 줄씩 떼어서 첫줄은 한국어 둘째줄은 영어 하면 될 것 같죠? ㅎㅎ... 쓰다가 영어 한 줄 빼먹은 걸 보셔야...
        //그러므로, 첫줄 한국어면 다음 영어 찾고, 없고 다음줄이 또 한국어면 그냥 넘기고 다음 Food 작성
        //한국어가 안 들어오고 영어가 먼저 들어왔다면 그냥 넘기고 다음 Food 작성
        /// <summary>
        /// 파싱하고 있는 테이블 중 한 칸 떼어 넘기면 파싱된 결과물을 줍니다!
        /// </summary>
        /// <param name="column">파싱하고 있는 테이블 중 한 칸</param>
        /// <param name="type">어느 코너에 나오는 음식인가요</param>
        /// <returns></returns>
        XElement MakeFoodsElement(XElement column, XElement column2, String type)
        {
            List<String> strfood = new List<String>();
            {
                XElement[] foodcode = column
                    .Elements().ToArray();//p
                foreach (XElement code in foodcode)
                {
                    String str = "";
                    foreach (XElement fontcode in code.Elements())//font 태그는 한 개만 있을 거 같죠? ㅎㅎㅎ... 내용 빈 font 태그 한번 보셔야...
                    {
                        str += fontcode.Value;
                    }
                    if (str.Length > 0)
                        strfood.Add(str);
                }
            }

            if (strfood.Count > 1)
            {
                String calint = "";
                {
                    XElement[] foodcode = column2
                        .Elements().ToArray();//p
                    foreach (XElement code in foodcode)
                    {
                        foreach (XElement fontcode in code.Elements())//font 태그는 한 개만 있을 거 같죠? ㅎㅎㅎ... 내용 빈 font 태그 한번 보셔야...
                        {
                            calint += fontcode.Value;
                        }
                    }
                }
                return ParseSingleFoodData(strfood, calint, type);
            }
            else
                return null;
        }

        /// <summary>
        /// 기존 포스로이드 앺과의 호환성 때문에 C/D는 합쳐놓은 듯한데... 그런 거 없고 따로 표시합니다. 그러려고 만들었어요. 역시 파싱하고 있는 테이블 중 한 칸 떼어 넘기면 파싱된 결과물을 줍니다.
        /// </summary>
        /// <param name="column">파싱하고 있는 테이블 중 한 칸</param>
        /// <param name="type1">어느 코너에 나오는 음식인가요1</param>
        /// <param name="type2">어느 코너에 나오는 음식인가요2</param>
        /// <returns></returns>
        XElement[] MakeDualFoodsElement(XElement column, XElement column2, String type1, String type2)
        {
            List<String> strfood = new List<String>();
            {
                XElement[] foodcode = column
                    .Elements().ToArray();//p
                foreach (XElement code in foodcode)
                {
                    String str = "";
                    foreach (XElement fontcode in code.Elements())//font 태그는 한 개만 있을 거 같죠? ㅎㅎㅎ... 내용 빈 font 태그 한번 보셔야...
                    {
                        str += fontcode.Value;
                    }
                    if (str.Length > 0)//빈 문자열은 갖다 버린다!.. 왜 엔터는 쳐서 빈 곳을 만들어요 으아아
                    {
                        if (strfood.Count > 0)
                        {
                            String laststr = strfood.Last();
                            Char last = laststr[laststr.Length - 1];
                            Char first = str[0];
                            if ((last == '/' && first != '/') || (last != '/' && first == '/'))
                            {
                                strfood.RemoveAt(strfood.Count - 1);
                                str = laststr + str;
                            }
                        }
                        strfood.Add(str);
                    }
                }
            }

            Boolean IsThereNoType1 = false, IsThereNoType2 = false;//C, D는 항상 함께 있을 거 같죠? ㅎㅎㅎㅎㅎㅎㅎ.... 'D코너' 한줄 추가되어 있고 C코너 사라진 꼴을 보셔야 ㅎㅎㅎ

            if (strfood.Count > 1)//코너 하나만 있는지 모두 있는지 판별. 하나만 있는 경우는 최상단에 해당 코너 이름을 써 주는 관습이 있습니다.
            {
                if (strfood.First().Contains(String.Format("{0}코너", type1)))//왜 StartsWith 안쓰고 Contains 쓰냐면... 어떤주엔 'D코너' 써놓고 어떤주엔 '<D코너>' 써놓거든요. 제발 <D>는 안 썼으면...
                {
                    IsThereNoType2 = true;
                    strfood.Remove(strfood.First());
                }
                else if (strfood.First().Contains(String.Format("{0}코너", type2)))
                {
                    IsThereNoType1 = true;
                    strfood.Remove(strfood.First());
                }//첫번째 줄을 무조건 지우면 안 돼요... 홀수인 이유가 코너이름 적으려고도 있겠지만 엔터치고 괄호안에 원산지 등 추가정보 적으면서 저렇게 되는 경우가 있어서...
            }

            if (strfood.Count > 1)//<행사이름> 써 놓고 그 이상 아무것도 없을 때 오류 방지
            {
                String calint = "";
                {
                    XElement[] foodcode = column2
                        .Elements().ToArray();//p
                    foreach (XElement code in foodcode)
                    {
                        foreach (XElement fontcode in code.Elements())//font 태그는 한 개만 있을 거 같죠? ㅎㅎㅎ... 내용 빈 font 태그 한번 보셔야...
                        {
                            calint += fontcode.Value;
                        }
                    }
                }

                if (!IsThereNoType1 && !IsThereNoType2)
                {
                    for (Int32 i = 1; i < strfood.Count; i++)//기본적으론 C와 D는 슬래시로만 구분되지만, 슬래시 앞에 엔터로 또 구분되어 있는 경우가 있다
                    {
                        if (strfood[i][0] == '/')
                        {
                            strfood[i - 1] += strfood[i];
                            strfood.Remove(strfood[i]);
                            i--;
                        }
                    }

                    XElement xfoods1 = new XElement("Foods", new XAttribute("Type", type1));
                    XElement xfoods2 = new XElement("Foods", new XAttribute("Type", type2));
                    if (calint.Length > 0)
                    {
                        String[] splittedcalories = SplitIntoTwoBySlash(calint);
                        if (splittedcalories.Length > 1)
                        {
                            xfoods1.Add(new XAttribute("Calories", splittedcalories[0]));
                            xfoods2.Add(new XAttribute("Calories", splittedcalories[1]));
                        }
                        else //이건 중간에 슬래시를 빼 먹은 것
                        {
                            Int32 length = splittedcalories[0].Length;
                            if (length > 4)
                            {
                                if (length % 2 == 1 && splittedcalories[0][0] < splittedcalories[0][length / 2])//길이가 같거나, 앞 식단 자리수가 뒷 식단 자리수보다 큼
                                {
                                    xfoods1.Add(new XAttribute("Calories", splittedcalories[0].Substring(0, length / 2 + 1)));
                                    xfoods2.Add(new XAttribute("Calories", splittedcalories[0].Substring(length / 2 + 1)));
                                }
                                else
                                {
                                    xfoods1.Add(new XAttribute("Calories", splittedcalories[0].Substring(0, length / 2)));
                                    xfoods2.Add(new XAttribute("Calories", splittedcalories[0].Substring(length / 2)));
                                }
                            }
                            else
                            {
                                xfoods1.Add(new XAttribute("Calories", splittedcalories[0]));
                                xfoods2.Add(new XAttribute("Calories", -1));
                            }
                        }
                    }
                    else
                    {
                        xfoods1.Add(new XAttribute("Calories", -1));
                        xfoods2.Add(new XAttribute("Calories", -1));
                    }

                    Char FormerLineLanguage = 'E';//K for Korean, E for English
                    XElement xfood1 = null;
                    XElement xfood2 = null;
                    foreach (String str in strfood)
                    {
                        Nullable<BracketData> bdata = ExtractBracket(str);//가끔 괄호 안에 '/'가 있는 바람에 파싱을 방해하므로 빼 놨다가 나중에 다시 붙임
                        String processed = String.Empty;
                        if (bdata.HasValue)
                        {
                            processed = bdata.Value.AfterExtracted;
                            if (processed.Length == 0)
                            {
                                //마지막 코드에 괄호 추가, 더 할 일 없음. 바로 브레이크
                                XElement langToBeFixed = xfood2.Elements().Last();
                                XAttribute fixstr = langToBeFixed.Attribute("Value");
                                fixstr.Remove();
                                langToBeFixed.Add(new XAttribute("Value", (String)fixstr + bdata.Value.ExtractedString));
                                break;
                            }
                        }

                        //문자열 처리부분
                        String[] splitted = SplitIntoTwoBySlashWithException(str);
                        Char PresentLineLanguage;
                        if (IsEnglish(str))
                            PresentLineLanguage = 'E';
                        else
                            PresentLineLanguage = 'K';
                        switch (FormerLineLanguage)
                        {
                            case 'E':
                                switch (PresentLineLanguage)
                                {
                                    case 'E':
                                        {
                                            xfoods1.Add(xfood1);
                                            xfoods2.Add(xfood2);
                                            xfood1 = new XElement("Food");
                                            xfood2 = new XElement("Food");
                                            xfood1.Add(new XElement("Name", new XAttribute("language", "ko"), new XAttribute("Value", "[이름 미등록]")));
                                            xfood2.Add(new XElement("Name", new XAttribute("language", "ko"), new XAttribute("Value", "[이름 미등록]")));
                                            xfood1.Add(new XElement("Name", new XAttribute("language", "en-US"), new XAttribute("Value", splitted[0])));
                                            if (splitted.Length > 1)
                                                xfood2.Add(new XElement("Name", new XAttribute("language", "en-US"), new XAttribute("Value", splitted[1])));
                                            else
                                                xfood2.Add(new XElement("Name", new XAttribute("language", "en-US"), new XAttribute("Value", "[name unregistered]")));
                                            break;
                                        }
                                    case 'K':
                                        {
                                            xfoods1.Add(xfood1);
                                            xfoods2.Add(xfood2);
                                            xfood1 = new XElement("Food");
                                            xfood2 = new XElement("Food");
                                            xfood1.Add(new XElement("Name", new XAttribute("language", "ko"), new XAttribute("Value", splitted[0])));
                                            if (splitted.Length > 1)
                                                xfood2.Add(new XElement("Name", new XAttribute("language", "ko"), new XAttribute("Value", splitted[1])));
                                            else
                                                xfood2.Add(new XElement("Name", new XAttribute("language", "ko"), new XAttribute("Value", "[이름 미등록]")));
                                            break;
                                        }
                                }
                                break;
                            case 'K':
                                switch (PresentLineLanguage)
                                {
                                    case 'E':
                                        {
                                            xfood1.Add(new XElement("Name", new XAttribute("language", "en-US"), new XAttribute("Value", splitted[0])));
                                            if (splitted.Length > 1)
                                                xfood2.Add(new XElement("Name", new XAttribute("language", "en-US"), new XAttribute("Value", splitted[1])));
                                            else
                                                xfood2.Add(new XElement("Name", new XAttribute("language", "en-US"), new XAttribute("Value", "[name unregistered]")));
                                            //xfoods.Add(xfood);
                                            //xfood = new XElement("Food");
                                            break;
                                        }
                                    case 'K':
                                        {
                                            xfood1.Add(new XElement("Name", new XAttribute("language", "en-US"), new XAttribute("Value", "[name unregistered]")));
                                            xfood2.Add(new XElement("Name", new XAttribute("language", "en-US"), new XAttribute("Value", "[name unregistered]")));
                                            xfoods1.Add(xfood1);
                                            xfoods2.Add(xfood2);
                                            xfood1 = new XElement("Food");
                                            xfood2 = new XElement("Food");
                                            xfood1.Add(new XElement("Name", new XAttribute("language", "ko"), new XAttribute("Value", splitted[0])));
                                            if (splitted.Length > 1)
                                                xfood2.Add(new XElement("Name", new XAttribute("language", "ko"), new XAttribute("Value", splitted[1])));
                                            else
                                                xfood2.Add(new XElement("Name", new XAttribute("language", "ko"), new XAttribute("Value", "[이름 미등록]")));
                                            break;
                                        }
                                }
                                break;
                        }
                        FormerLineLanguage = PresentLineLanguage;

                        if (processed.Length > 0)
                        {
                            //앞에서 뺀 괄호 다시 더함
                            Int32 splitpoint = str.IndexOf('/');
                            XElement langToBeFixed;
                            if (splitpoint < bdata.Value.StartPoint)
                                langToBeFixed = xfood1.Elements().Last();
                            else
                                langToBeFixed = xfood2.Elements().Last();

                            XAttribute fixstr = langToBeFixed.Attribute("Value");
                            fixstr.Remove();
                            langToBeFixed.Add(new XAttribute("Value", (String)fixstr + bdata.Value.ExtractedString));
                        }
                    }

                    if (xfood1.HasElements)
                        xfoods1.Add(xfood1);
                    if (xfood2.HasElements)
                        xfoods2.Add(xfood2);

                    var returner = new List<XElement>();
                    if (!IsMealBlank(xfoods2))
                        returner.Add(xfoods2);
                    if (!IsMealBlank(xfoods1))//D와 C의 순서가 이유는 몰라도 최근 바뀌었으므로 C보다 D를 앞으로.
                        returner.Add(xfoods1);
                    return returner.ToArray();
                }
                else if (IsThereNoType1)
                    return new XElement[] { ParseSingleFoodData(strfood, calint, type2) };
                else
                    return new XElement[] { ParseSingleFoodData(strfood, calint, type1) };
            }
            else
                return new XElement[0];
        }

        XElement ParseSingleFoodData(List<String> strfood, String calstr, String type)
        {
            XElement xfoods = new XElement("Foods", new XAttribute("Type", type));
            if (calstr.Length > 0)
                xfoods.Add(new XAttribute("Calories", calstr));
            else
                xfoods.Add(new XAttribute("Calories", -1));

            Char FormerLineLanguage = 'E';//K for Korean, E for English
            XElement xfood = null;//new XElement("Food");
            foreach (String str in strfood)
            {
                Nullable<BracketData> bdata = ExtractBracket(str);
                String processed = String.Empty;
                if (bdata.HasValue)
                {
                    processed = bdata.Value.AfterExtracted;
                    if (processed.Length == 0)
                    {
                        //마지막 코드에 괄호 추가, 더 할 일 없음. 바로 브레이크
                        XElement langToBeFixed = xfood.Elements().Last();
                        XAttribute fixstr = langToBeFixed.Attribute("Value");
                        fixstr.Remove();
                        langToBeFixed.Add(new XAttribute("Value", (String)fixstr + bdata.Value.ExtractedString));
                        break;
                    }
                }


                Char PresentLineLanguage;
                if (IsEnglish(str))
                    PresentLineLanguage = 'E';
                else
                    PresentLineLanguage = 'K';
                switch (FormerLineLanguage)
                {
                    case 'E':
                        switch (PresentLineLanguage)
                        {
                            case 'E':
                                xfoods.Add(xfood);
                                xfood = new XElement("Food");
                                xfood.Add(new XElement("Name", new XAttribute("language", "en-US"), new XAttribute("Value", str)));
                                break;
                            case 'K':
                                xfoods.Add(xfood);
                                xfood = new XElement("Food");
                                xfood.Add(new XElement("Name", new XAttribute("language", "ko"), new XAttribute("Value", str)));
                                break;
                        }
                        break;
                    case 'K':
                        switch (PresentLineLanguage)
                        {
                            case 'E':
                                xfood.Add(new XElement("Name", new XAttribute("language", "en-US"), new XAttribute("Value", str)));
                                break;
                            case 'K':
                                xfoods.Add(xfood);
                                xfood = new XElement("Food");
                                xfood.Add(new XElement("Name", new XAttribute("language", "ko"), new XAttribute("Value", str)));
                                break;
                        }
                        break;
                }
                FormerLineLanguage = PresentLineLanguage;

                if (processed.Length > 0)
                {
                    XElement langToBeFixed = xfood.Elements().Last();
                    XAttribute fixstr = langToBeFixed.Attribute("Value");
                    fixstr.Remove();
                    langToBeFixed.Add(new XAttribute("Value", (String)fixstr + bdata.Value.ExtractedString));
                }
            }

            if (xfood.HasElements)
                xfoods.Add(xfood);

            return xfoods;
        }

        struct BracketData
        {
            public Int32 StartPoint;
            public String AfterExtracted;
            public String ExtractedString;
        }

        /// <summary>
        /// String 안에 괄호가 있는지 검사하여 있으면 그에 대한 정보가 반환되며, 없을 경우 null이 반환됩니다.
        /// </summary>
        /// <param name="input">검사할 String</param>
        /// <returns>괄호의 시작점, 괄호를 포함해 괄호 안의 문자열, 또 그것을 제외한 문자열을 반환합니다.</returns>
        Nullable<BracketData> ExtractBracket(String input)
        {
            Int32 bracketStartPoint = input.IndexOf('(');
            if (bracketStartPoint != -1)
            {
                Int32 bracketEndPoint = input.LastIndexOf(')');
                Int32 Length = bracketEndPoint - bracketStartPoint + 1;
                if (Length > 0)
                    return new BracketData()
                    {
                        StartPoint = bracketStartPoint,
                        AfterExtracted = input.Remove(bracketStartPoint, Length),
                        ExtractedString = input.Substring(bracketStartPoint, Length)
                    };
            }
            return null;
        }

        Boolean IsEnglish(String str)
        {
            foreach (Char c in str)
            {
                //if (!(c == '\u0020' // space
                //    || (c >= 0x0041 && c <= 0x005A) // english capital letter
                //    || (c >= 0x0061 && c <= 0x007A))) // english small letter
                if (c > 0x007F) // is it out of ASCII code - combines all punctuation marks and English letters.
                    return false;
            }
            return true;
        }

        String[] SplitIntoTwoBySlash(String str)
        {
            return str.Split(new Char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        }

        String[] SplitIntoTwoBySlashWithException(String str)// results '쇠고기볶음밥', '삼계탕1/2' when splitting 쇠고기볶음밥/삼계탕1/2
        {
            List<String> splitted = SplitIntoTwoBySlash(str).ToList();
            for (Int32 i = splitted.Count - 1; i > 0; i--)
            {
                String firststr = splitted[i];
                Char first = firststr[0];
                if (first >= '0' && first <= '9')
                {
                    String laststr = splitted[i - 1];
                    Char last = laststr[laststr.Length - 1];
                    if (last >= '0' && first <= '9')
                    {
                        Int32 index = i;
                        splitted.RemoveAt(i);
                        splitted.RemoveAt(i - 1);
                        laststr += '/' + firststr;
                        splitted.Insert(index - 1, laststr);
                    }
                }
            }
            return splitted.ToArray();
        }

        Boolean IsMealBlank(XElement xfoods)//코너가 하나뿐일 땐 <D코너> 등으로 표시하다 요즘은 그마저도 표시 안 하길래 따로 필터링
        {
            if (xfoods.Attribute("Calories").Value != "-1")
                return false;
            if (!xfoods.HasElements)
                return true;

            var foodNames = xfoods.Element("Food").Elements("Name").ToList();
            if (foodNames[0].Attribute("Value").Value == "[이름 미등록]" && foodNames[1].Attribute("Value").Value == "[name unregistered]")
                return true;
            else return false;
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

        Boolean IsSwitching = false;

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
