using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using FlickrNet;
using Windows.Storage;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace Flickr2
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {

        public static string FAVORITE_ICON = "Assets/hearticon.png";
        public static string NOT_FAVORITE_ICON = "Assets/emptyHeartIcon.png";
        public string searchParamString { get; set; }
        public string tagToAddString { get; set; }
        private string _accountKey;
        public int selectedIndex = -1;
        private DateTime _lastDateNavigationLoadDate;
        //public DateTime lastDateNavigationLoadDate { get; set; }

        public DateTime lastDateNavigationLoadDate
        {
            get { return _lastDateNavigationLoadDate; }
            set
            {
                _lastDateNavigationLoadDate = value;
                Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
                Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                if (value != null)
                {
                    DateTime dt = (DateTime)value;
                    localSettings.Values["lastDateNavigationLoadDate"] = dt.ToUniversalTime().GetDateTimeFormats('u');
                }else
                {
                    localSettings.Values["lastDateNavigationLoadDate"] = null;
                }
            }
        }

        public string accountKey
        {
            get { return _accountKey; }
            set { _accountKey = value;
                Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
                roamingSettings.Values["accountKey"] = value;
            }
        }
        private string _accountSecret;
        public string accountSecret
        {
            get { return _accountSecret; }
            set { _accountSecret = value;
                Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
                roamingSettings.Values["accountSecret"] = value;
            }
        }
        private string _accountId;
        public string accountId
        {
            get { return _accountId; }
            set { _accountId = value;
                Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
                roamingSettings.Values["accountId"] = value;
            }
        }
        public OAuthAccessToken AccessToken { get; set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        public async System.Threading.Tasks.Task<string> getSerializedDateOptionsString()
        {

            try { 
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync("dateOptionsData.json");
                if (file == null) return null;
                string text = await Windows.Storage.FileIO.ReadTextAsync(file);
                return text;
            }catch(FileNotFoundException fnf)
            {
                return null;
            }
        }

        public async void setSerializedDateOptionsString(String text)
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync("dateOptionsData.json", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteTextAsync(file, text);
        }

        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested;
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            string curTagToSet = (string)roamingSettings.Values["tagToSet"];
            if(curTagToSet == null)
            {
                roamingSettings.Values["tagToSet"] = "GOODPHOTO";
            }

            string curSearchParam = (string)roamingSettings.Values["searchParam"];
            if (curSearchParam == null)
            {
                roamingSettings.Values["searchParam"] = "*";
            }
            searchParamString = (string)roamingSettings.Values["searchParam"];
            tagToAddString = (string)roamingSettings.Values["tagToSet"];

            accountKey = (string)roamingSettings.Values["accountKey"];
            accountSecret = (string)roamingSettings.Values["accountSecret"];
            accountId = (string)roamingSettings.Values["accountId"];
            if(localSettings.Values["lastDateNavigationLoadDate"] != null){
                string[] s = (string[])localSettings.Values["lastDateNavigationLoadDate"];
                this._lastDateNavigationLoadDate = DateTime.Parse(s[0]).ToLocalTime();
            }

        }

        private void OnCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            args.Request.ApplicationCommands.Add(new SettingsCommand("Tag Settings","Tag Settings",(handler)=>ShowTagSettingsFlyout()));
            args.Request.ApplicationCommands.Add(new SettingsCommand("Log Out", "Log Out", (handler) => LogUserOut()));
        }

        public void ShowTagSettingsFlyout()
        {
            SettingsFlyout1 fo = new SettingsFlyout1();
            //fo.tagTextBox = 
            fo.Show();
            
        }

        public void LogUserOut()
        {
            this.accountId = null;
            this.accountKey = null;
            this.accountSecret = null;
            App.Current.Exit();
           
            

        }



        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                // Set the default language
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
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

        protected override void OnSearchActivated(SearchActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();
                // Set the default language
                Window.Current.Content = rootFrame;
            }

            if(!rootFrame.Navigate(typeof(MainPage), args.QueryText))
            {
                throw new Exception("Failed to create search page");
            }

            Window.Current.Activate();
        }
    }
}
