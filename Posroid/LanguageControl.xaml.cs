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
using Windows.Storage;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Posphorum
{
    public delegate void GlobalSettingChangedEventHandler(object sender, GlobalSettingChangedEventArgs e);
    public class GlobalSettingChangedEventArgs : EventArgs
    {
        public SettingType WhatSetting;
        public Object Value;
    }
    public enum SettingType
    {
        ForceKorean
    }

    public partial class LanguageControl : UserControl
    {
        Boolean loadCompleted;
        public LanguageControl()
        {
            this.InitializeComponent();
            ForceKoreanToggle.IsOn = (Boolean)ApplicationData.Current.LocalSettings.Values["ForceKorean"];
            loadCompleted = true;
        }

        private void ToggleSwitch_Toggled_1(object sender, RoutedEventArgs e)
        {
            if (loadCompleted)
                OnSettingChanged(new GlobalSettingChangedEventArgs() { WhatSetting = SettingType.ForceKorean, Value = ForceKoreanToggle.IsOn });
        }

        public event GlobalSettingChangedEventHandler SettingChanged;

        protected virtual void OnSettingChanged(GlobalSettingChangedEventArgs e)
        {
            if (SettingChanged != null)
            {
                SettingChanged(this, e);
            }
        }
    }
}
