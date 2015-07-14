using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using App1;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Media.Imaging;
using FlickrNet;
using Windows.Security.Authentication.Web;
using System.Threading.Tasks;
using Newtonsoft.Json;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Flickr2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += OnLoaded;
            
        }

        

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;

            string searchTerm = (string)e.Parameter;
            if (!string.IsNullOrEmpty(searchTerm))
            {
                var app = App.Current as App;

                searchTerm = app.searchParamString;
                SearchFlickr(searchTerm,false);
            }
           
        }
        void SearchFlickr(string searchTerm, Boolean searchIfAlreadyDefined)
        {
            
            
            //List<FlickrPhoto> photos = await FlickrTools.getImages(searchTerm, 1, 40);
            if(searchIfAlreadyDefined || this.DataContext == null)
            {
                PhotosToShow photos = new PhotosToShow(true);
                photos.photosAtATime = 30;
                photos.pageNum = 1;
                this.DataContext = photos;
            }
            
        }
        async void OnLoaded(object sender, RoutedEventArgs e)
        {
            App app = App.Current as App;
            if (app.accountKey != null) { loadDate(); return; }


            var f = FlickrTools.getUnauthorizedFlickrObject();
            var requestToken = await GetRequestToken();
            string output;
            var flickrUri = new Uri(f.OAuthCalculateAuthorizationUrl(requestToken.Token, AuthLevel.Write));
            var webAuthenticationResult = await WebAuthenticationBroker.AuthenticateAsync(
                                                    WebAuthenticationOptions.None,
                                                    flickrUri);

            if (webAuthenticationResult.ResponseStatus == WebAuthenticationStatus.Success)
            {
                output = webAuthenticationResult.ResponseData;
                AccessToken = await f.OAuthAccessTokenAsync(requestToken.Token, requestToken.TokenSecret, output);

                app.accountKey = AccessToken.Token;
                app.accountSecret = AccessToken.TokenSecret;
                app.accountId = AccessToken.UserId;
                loadDate();
            }
            else if (webAuthenticationResult.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
            {
                output = "HTTP Error returned by AuthenticateAsync() : " + webAuthenticationResult.ResponseErrorDetail.ToString();
            }
            else if (webAuthenticationResult.ResponseStatus == WebAuthenticationStatus.UserCancel)
            {
                output = "Authentication process was cancelled by the user";
            }

        }

        private void loadDate()
        {
            var app = App.Current as App;
            string searchTerm = app.searchParamString;
            if (searchTerm == null)
            {
                searchTerm = "*";
            }
            if(this.DataContext != null)
            {
                if (app.selectedIndex >= 0)
                {
                    gridView.SelectedIndex = app.selectedIndex;
                    app.selectedIndex = -1;
                    gridView.UpdateLayout();
                    gridView.ScrollIntoView(gridView.SelectedItem);
                    gridView.SelectedItem = null;
                }
            }
            SearchFlickr(searchTerm, false);
            InitializeSearch();
            InitializeTagging();
            InitializeTreeView();
            
        }

        private DateNavigationItem getItemForYear(string year, List<string> months)
        {
            DateNavigationItem itm = new DateNavigationItem(year);

            foreach (string month in months) {
                DateNavigationItem monthItem = new DateNavigationItem(month);
                monthItem.year = itm.year;
                itm.subItems.Add(monthItem);
            }


            return itm;
        }

        private async void InitializeTreeView()
        {
            
            if (dateList.Items != null && dateList.Items.Count > 0) return;
            App app = App.Current as App;
           
            string serializedList = await app.getSerializedDateOptionsString();
            if(serializedList != null && !"".Equals(serializedList.Trim()))
            {
               
               DateTime lastChecked = app.lastDateNavigationLoadDate;
                bool hasUpdates = await FlickrTools.hasUpdatesSinceDate(lastChecked);
               if (lastChecked != null && !hasUpdates)
               {
                   
                   var dateListFromString = JsonConvert.DeserializeObject<List<DateNavigationItem>>(serializedList);
                   foreach (DateNavigationItem month in dateListFromString)
                   {
                       dateList.Items.Add(month);
                   }
                   //We still need to see if we have any updates since the last time we checked;
                   app.lastDateNavigationLoadDate = DateTime.Now;
                   return;
                   
               }
               
            }

            List<string> years = await FlickrTools.getListOfYearsWithPhotos();
            Dictionary<string, List<string>> monthsForYears = await FlickrTools.getMonthsWithPhotosForYears(years);

            foreach (string year in years)
            {
                DateNavigationItem yr = getItemForYear(year, monthsForYears[year]);
                foreach (DateNavigationItem month in yr.subItems)
                {
                    dateList.Items.Add(month);
                }
                //treeView.Items.Add();
            }
            app.setSerializedDateOptionsString(JsonConvert.SerializeObject(dateList.Items));
            app.lastDateNavigationLoadDate = DateTime.Now;

            




        }

        
        
        private async void navTextTapped(object sender, TappedRoutedEventArgs e)
        {
         /*   var app = App.Current as App;
            int photosAtaTime = 100;
            int pageNum = 1;
            DateNavigationItem item = (DateNavigationItem)dateList.SelectedItem;
            PhotosToShow photosToShow = new PhotosToShow();
            photosToShow.pageNum = 1;
            photosToShow.photosAtATime = photosAtaTime;
            photosToShow.year = item.year;
            photosToShow.month = item.month;
            photosToShow.searchParam = app.searchParamString;

            this.DataContext = photosToShow;
            */
        }

        private void OnGridItemClick(object sender, ItemClickEventArgs e)
        {
            FlickrPhoto item = (FlickrPhoto)e.ClickedItem;
            Image img = new Image();
            BitmapImage bitmapImage = new BitmapImage();
            Uri uri = new Uri(item.ImageUrl);
            bitmapImage.UriSource = uri;
            img.Source = bitmapImage;


        }

        private void PopupKeyDown(object sender, KeyRoutedEventArgs e)
        {
            
            throw new NotImplementedException();
        }

        void InitializeSettings()
        {

            
            
        }

        void InitializeTagging()
        {
            DataTransferManager transferManager = DataTransferManager.GetForCurrentView();
            transferManager.DataRequested += (s, e) =>
            {
                FlickrPhoto selItem = (FlickrPhoto)this.gridView.SelectedItem;
                if(selItem == null)
                {
                    e.Request.FailWithDisplayText("You must select a photo to tag it!");
                }else { 
                    e.Request.Data.Properties.Title = selItem.Title==null?"NoTitle":selItem.Title;
                    e.Request.Data.Properties.Description = "--";
                    e.Request.Data.SetHtmlFormat(HtmlFormatHelper.CreateHtmlFormat(string.Format("<img src='{0}'/>",selItem.ImageUrl)));
                }
            };
            
        }

        void InitializeSearch()
        {
           /* SearchPane searchPane = SearchPane.GetForCurrentView();
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            searchPane.SuggestionsRequested += (s, e) =>
            {
                List<string> suggestions = new List<string>();
                var app = App.Current as App;
                suggestions.Add((string)app.tagToAddString);
                e.Request.SearchSuggestionCollection.AppendQuerySuggestions(suggestions);
            };

            searchPane.QuerySubmitted += (s, e) =>
            {
                this.SearchFlickr(e.QueryText,true);
            };*/

        }

        private void favImgTag_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Image img = (Image)e.OriginalSource;
            FlickrPhoto p = (FlickrPhoto)img.DataContext;

            List<object> parms = new List<object>();
            parms.Add(this.DataContext);
            parms.Add(p);
            this.Frame.Navigate(typeof(PhotoFlip), parms);

        }

        private void favButton_Click(object sender, RoutedEventArgs e)
        {
            var app = App.Current as App;
            Button button = (Button)e.OriginalSource;
            FlickrPhoto fPhoto = ((FlickrPhoto)(button).DataContext);
            if (App.FAVORITE_ICON.Equals(fPhoto.tagImage))
            {
                fPhoto.tagImage = App.NOT_FAVORITE_ICON;
                FlickrTools.removeTagToPhoto(""+fPhoto.Id, app.tagToAddString);
            }else
            {
                fPhoto.tagImage = App.FAVORITE_ICON;
                FlickrTools.addTagToPhoto("" + fPhoto.Id, app.tagToAddString);
            }

            
        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {
            
        }

        private void dateList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var app = App.Current as App;
            int photosAtaTime = 30;
            int pageNum = 1;
            DateNavigationItem item = (DateNavigationItem)dateList.SelectedItem;        
            PhotosToShow photosToShow = new PhotosToShow(false);
            photosToShow.pageNum = 1;
            photosToShow.photosAtATime = photosAtaTime;
            photosToShow.year = item.year;
            photosToShow.month = item.month;
            photosToShow.searchParam = app.searchParamString;

            this.DataContext = photosToShow;
        }



        private OAuthAccessToken AccessToken
        {
            get { return (Application.Current as App).AccessToken; }
            set { (Application.Current as App).AccessToken = value; }
        }

        private async Task<OAuthRequestToken> GetRequestToken()
        {
            var f = FlickrTools.getUnauthorizedFlickrObject();
            
            return await f.OAuthRequestTokenAsync(WebAuthenticationBroker.GetCurrentApplicationCallbackUri().ToString());
        }

        private async void PageRootLoaded(object sender, RoutedEventArgs e)
        {
                    }

        private void textBlock_SelectionChanged_1(object sender, RoutedEventArgs e)
        {

        }
    }








    


}
