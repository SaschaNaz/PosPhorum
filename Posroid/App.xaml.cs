using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ApplicationSettings;
using Windows.Globalization.DateTimeFormatting;
using Windows.Storage;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace Posphorum
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            SettingsPane.GetForCurrentView().CommandsRequested += (s, e) =>
            {
                SettingsCommand cmd = new SettingsCommand("ppolicy", 
                    new Windows.ApplicationModel.Resources.ResourceLoader().GetString("PrivacyPolicy"),
                    (handler) =>
                    {
                        var sf = new PrivacyPolicyFlyout();
                        sf.Show();
                    });
                e.Request.ApplicationCommands.Add(cmd);

                cmd = new SettingsCommand("options",
                    new Windows.ApplicationModel.Resources.ResourceLoader().GetString("Options"),
                    (handler) =>
                    {
                        var sf = new GlobalSettingsFlyout();
                        sf.Show();
                    });
                e.Request.ApplicationCommands.Add(cmd);
            };

            base.OnWindowCreated(args);
        }


        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // Do not repeat app initialization when already running, just ensure that
            // the window is active
            if (args.PreviousExecutionState == ApplicationExecutionState.Running)
            {
                Window.Current.Activate();
                return;
            }

            // Create a Frame to act navigation context and navigate to the first page
            var rootFrame = new Frame();
            if (!rootFrame.Navigate(typeof(DietGroupedPage)))
            {
                throw new Exception("Failed to create initial page");
            }

            if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
            {
                //TODO: Load state from previously suspended application
            }

            // Place the frame in the current Window and ensure that it is active
            Window.Current.Content = rootFrame;
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }

    public delegate void SettingChangedEventHandler(object sender, SettingChangedEventArgs e);
    public class SettingChangedEventArgs : EventArgs
    {
        public SettingTypes SettingType;
        public Object SettingValue;
    }
    public enum SettingTypes
    {
        ForceKorean
    }

    public class SettingChangeNotifier
    {

        public void ReportSettingChange(SettingChangedEventArgs e)
        {
            OnSettingChanged(e);
        }

        public event SettingChangedEventHandler SettingChanged;

        protected virtual void OnSettingChanged(SettingChangedEventArgs e)
        {
            if (SettingChanged != null)
            {
                SettingChanged(this, e);
            }
        }
    }

    public class DateConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (value == null)
                throw new ArgumentNullException("value", "Value cannot be null.");

            if (!typeof(DateTime).Equals(value.GetType()))
                throw new ArgumentException("Value must be of type DateTime.", "value");

            DateTime dt = (DateTime)value;

            if (parameter == null)
            {
                // Date "7/27/2011 9:30:59 AM" returns "7/27/2011"
                return DateTimeFormatter.ShortDate.Format(dt);
            }
            else if ((string)parameter == "day")
            {
                // Date "7/27/2011 9:30:59 AM" returns "27"
                DateTimeFormatter dateFormatter = new DateTimeFormatter("{day.integer(2)}");
                return dateFormatter.Format(dt);
            }
            else if ((string)parameter == "month")
            {
                // Date "7/27/2011 9:30:59 AM" returns "JUL"
                DateTimeFormatter dateFormatter = new DateTimeFormatter("{month.integer(2)}");
                return dateFormatter.Format(dt);
            }
            else if ((string)parameter == "MM/DD")
            {
                // Date "7/27/2011 9:30:59 AM" returns "JUL"
                DateTimeFormatter dateFormatter1 = new DateTimeFormatter("{month.integer(2)}");
                DateTimeFormatter dateFormatter2 = new DateTimeFormatter("{day.integer(2)}");
                return String.Format("{0}/{1}", dateFormatter1.Format(dt), dateFormatter2.Format(dt));
            }
            else if ((string)parameter == "MM/DD-dayofweek")
            {

                DateTimeFormatter dateFormatter1 = new DateTimeFormatter("{month.integer(2)}");
                DateTimeFormatter dateFormatter2 = new DateTimeFormatter("{day.integer(2)}");
                DateTimeFormatter dateFormatter3 = new DateTimeFormatter("{dayofweek.full}");
                return String.Format("{0}/{1}\n{2}", dateFormatter1.Format(dt), dateFormatter2.Format(dt), dateFormatter3.Format(dt));
            }
            else if ((string)parameter == "MM/DD/dayofweek")
            {

                DateTimeFormatter dateFormatter1 = new DateTimeFormatter("{month.integer(2)}");
                DateTimeFormatter dateFormatter2 = new DateTimeFormatter("{day.integer(2)}");
                DateTimeFormatter dateFormatter3 = new DateTimeFormatter("{dayofweek.full}");
                return String.Format("{0}/{1}/{2}", dateFormatter1.Format(dt), dateFormatter2.Format(dt), dateFormatter3.Format(dt));
            }
            else if ((string)parameter == "dayofweek")
            {
                DateTimeFormatter dateFormatter = new DateTimeFormatter("{dayofweek.full}");
                return dateFormatter.Format(dt);
            }
            //else if ((string)parameter == "year")
            //{
            //    // Date "7/27/2011 9:30:59 AM" returns "2011"
            //    DateTimeFormatter dateFormatter = new DateTimeFormatter("{year.full}");
            //    return dateFormatter.Format(dt);
            //}
            else
            {
                // Requested format is unknown. Return in the original format.
                return dt.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            string strValue = value as string;
            DateTime resultDateTime;
            if (DateTime.TryParse(strValue, out resultDateTime))
            {
                return resultDateTime;
            }
            return Windows.UI.Xaml.DependencyProperty.UnsetValue;
        }
    }

    public class LanguageConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (value == null)
                throw new ArgumentNullException("value", "Value cannot be null.");

            if (!typeof(LangString).Equals(value.GetType()))
                throw new ArgumentException("Value must be of type LangString.", "value");

            LangString ls = (LangString)value;

            String returner;
            if (!(Boolean)ApplicationData.Current.LocalSettings.Values["ForceKorean"])
            {
                //returns the name for OS default language for default value.
                returner = ls.NameByLanguage(
                    new Windows.Globalization.Language(Windows.Globalization.ApplicationLanguages.Languages[0]));
            }
            else
                returner = ls.NameByLanguage(new Windows.Globalization.Language("ko"));

            if (parameter != null)
                return parameter + returner;
            else
                return returner;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DayBrushConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (value == null)
                throw new ArgumentNullException("value", "Value cannot be null.");

            if (!typeof(DateTime).Equals(value.GetType()))
                throw new ArgumentException("Value must be of type DateTime.", "value");

            DayOfWeek dayofweek = ((DateTime)value).DayOfWeek;

            if (dayofweek == DayOfWeek.Monday)
            {
                return new SolidColorBrush(new Windows.UI.Color() { A = 0xFF, R = 0xFF, G = 0x66, B = 0x66 });
            }
            else if (dayofweek == DayOfWeek.Tuesday)
            {
                return new SolidColorBrush(new Windows.UI.Color() { A = 0xFF, R = 0xFF, G = 0xC1, B = 0x66 });
            }
            else if (dayofweek == DayOfWeek.Wednesday)
            {
                return new SolidColorBrush(new Windows.UI.Color() { A = 0xFF, R = 0xFF, G = 0xFF, B = 0x66 });
            }
            else if (dayofweek == DayOfWeek.Thursday)
            {
                return new SolidColorBrush(new Windows.UI.Color() { A = 0xFF, R = 0x66, G = 0xFF, B = 0x66 });
            }
            else if (dayofweek == DayOfWeek.Friday)
            {
                return new SolidColorBrush(new Windows.UI.Color() { A = 0xFF, R = 0x66, G = 0x66, B = 0xFF });
            }
            else if (dayofweek == DayOfWeek.Saturday)
            {
                return new SolidColorBrush(new Windows.UI.Color() { A = 0xFF, R = 0xC1, G = 0x66, B = 0xFF });
            }
            else if (dayofweek == DayOfWeek.Sunday)
            {
                return new SolidColorBrush(new Windows.UI.Color() { A = 0xFF, R = 0xFF, G = 0x66, B = 0xFF });
            }
            else
            {
                return new SolidColorBrush(Windows.UI.Colors.Black);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    //public class MealInfoConverter : Windows.UI.Xaml.Data.IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, string culture)
    //    {
    //        if (value == null)
    //            throw new ArgumentNullException("value", "Value cannot be null.");

    //        IMealBlock block = value as IMealBlock;

    //        if (block == null)
    //        {
    //            throw new ArgumentException("Value must be of type IMealBlock.", "value");
    //        }

    //        return ((IMealBlock)value).InternalData;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, string culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class MealtimeStringConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (value == null)
                throw new ArgumentNullException("value", "Value cannot be null.");

            if (!typeof(When).Equals(value.GetType()))
                throw new ArgumentException("Value must be of type When.", "value");

            When mealtime = (When)value;
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();

            switch (mealtime)
            {
                case When.Breakfast:
                    return loader.GetString("Breakfast");
                case When.Lunch:
                    return loader.GetString("Lunch");
                case When.Dinner:
                    return loader.GetString("Dinner");
                default:
                    return loader.GetString("(Error)");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MealtimeBrushConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (value == null)
                throw new ArgumentNullException("value", "Value cannot be null.");

            if (!typeof(When).Equals(value.GetType()))
                throw new ArgumentException("Value must be of type When.", "value");

            When mealtime = (When)value;

            switch (mealtime)
            {
                case When.Breakfast:
                    return new SolidColorBrush(new Windows.UI.Color() { A = 0xFF, R = 0x0E, G = 0x9C, B = 0x00 });
                case When.Lunch:
                    return new SolidColorBrush(new Windows.UI.Color() { A = 0xFF, R = 0x00, G = 0x63, B = 0x9C });
                case When.Dinner:
                    return new SolidColorBrush(new Windows.UI.Color() { A = 0xFF, R = 0xE0, G = 0x00, B = 0x85 });
                default:
                    return new SolidColorBrush(Windows.UI.Colors.Transparent);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IntCaloriesConverter : Windows.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (value == null)
                throw new ArgumentNullException("value", "Value cannot be null.");

            if (!typeof(Int32).Equals(value.GetType()))
                throw new ArgumentException("Value must be of type Int32.", "value");

            Int32 cal = (Int32)value;
            if (cal != -1)
                return String.Format("{0}kcal", cal);
            else
                return "(Unknown)";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
