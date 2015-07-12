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

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace Flickr2
{
    public sealed partial class SettingsFlyout1 : SettingsFlyout
    {
        public SettingsFlyout1()
        {
            this.InitializeComponent();
            var app = App.Current as App;
            this.tagTextBox.Text = app.tagToAddString;
            this.searchParamTextBox.Text = app.searchParamString;
            this.tagTextBox.LostFocus += tagTextBox_LostFocus_1;
            this.searchParamTextBox.LostFocus += searchParamTextBox_LostFocus_1;
        }

        private void tagTextBox_LostFocus_1(object sender, RoutedEventArgs e)
        {
            
            Windows.Storage.ApplicationData.Current.RoamingSettings.Values["tagToSet"] = this.tagTextBox.Text;
            var app = App.Current as App;
            app.tagToAddString = this.tagTextBox.Text;
        }

        private void searchParamTextBox_LostFocus_1(object sender, RoutedEventArgs e)
        {
            Windows.Storage.ApplicationData.Current.RoamingSettings.Values["searchParam"] = this.searchParamTextBox.Text;
            var app = App.Current as App;
            app.searchParamString = this.searchParamTextBox.Text;
        }
    }

}
