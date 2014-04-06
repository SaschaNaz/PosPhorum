using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace Posphorum
{
    public sealed partial class GlobalSettingsFlyout : SettingsFlyout
    {
        public GlobalSettingsFlyout()
        {
            this.InitializeComponent();
            SetSettingValues();
            SetEventListeners();
        }

        void SetSettingValues()
        {
            ForceKoreanToggle.IsOn = (Boolean)ApplicationData.Current.LocalSettings.Values["ForceKorean"];
            
        }

        void SetEventListeners()
        {
            var notifier = (SettingChangeNotifier)Application.Current.Resources["settingChangeNotifier"];
            ForceKoreanToggle.Toggled += 
                (object sender, RoutedEventArgs e) => 
                {
                    ApplicationData.Current.LocalSettings.Values["ForceKorean"] = ForceKoreanToggle.IsOn;
                    notifier.ReportSettingChange(new SettingChangedEventArgs() { SettingType = SettingTypes.ForceKorean, SettingValue = ForceKoreanToggle.IsOn });
                };
        }
    }
}
