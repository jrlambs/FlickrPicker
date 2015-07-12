using App1;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Flickr2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PhotoFlip : Page
    {
        public PhotoFlip()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            List<object> parms = (List<object>)e.Parameter;
            if(parms.ElementAt(0) is PhotosToShow)
            {
                PhotosToShow photos = (PhotosToShow)parms.ElementAt(0);
                FlickrPhoto selectedItem = (FlickrPhoto)parms.ElementAt(1);
                this.DataContext = photos;
                flipView.SelectedItem = selectedItem;
            }
            else
            {
                List<FlickrPhoto> photos = new List<FlickrPhoto>(parms.ElementAt(0) as List<FlickrPhoto>);
                FlickrPhoto selectedItem = (FlickrPhoto)parms.ElementAt(1);
                this.DataContext = photos;
                flipView.SelectedItem = selectedItem;
            }
            
            
        }

        private async void OnFlipViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                PhotosToShow list = this.DataContext as PhotosToShow;
                await list.LoadMoreItemsIfNeededAsync(this.flipView.SelectedIndex);


            }
            catch (NullReferenceException exc)
            {

            }
        }



        void Back_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                App app = App.Current as App;
                app.selectedIndex = flipView.SelectedIndex;


            }
            catch (NullReferenceException exc)
            {

            }
            
            Frame.GoBack();
        }

        private void favButton_Click(object sender, RoutedEventArgs e)
        {
            var app = App.Current as App;
            Button button = (Button)e.OriginalSource;
            FlickrPhoto fPhoto = ((FlickrPhoto)(button).DataContext);
            if (App.FAVORITE_ICON.Equals(fPhoto.tagImage))
            {
                fPhoto.tagImage = App.NOT_FAVORITE_ICON;
                FlickrTools.removeTagToPhoto("" + fPhoto.Id, app.tagToAddString);
            }
            else
            {
                fPhoto.tagImage = App.FAVORITE_ICON;
                FlickrTools.addTagToPhoto("" + fPhoto.Id, app.tagToAddString);
            }
        }

        private void Grid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var app = App.Current as App;
            if (e.Key == VirtualKey.Escape)
            {
                try
                {
                    app.selectedIndex = flipView.SelectedIndex;
                }
                catch (NullReferenceException exc){}
                Frame.GoBack();
            }
            else if (e.Key == VirtualKey.F)
            {
                //tag or untag photo
                FlickrPhoto f = (FlickrPhoto)flipView.SelectedItem;
                if (App.FAVORITE_ICON.Equals(f.tagImage))
                {
                    f.tagImage = App.NOT_FAVORITE_ICON;
                    FlickrTools.removeTagToPhoto("" + f.Id, app.tagToAddString);
                }
                else
                {
                    f.tagImage = App.FAVORITE_ICON;
                    FlickrTools.addTagToPhoto("" + f.Id, app.tagToAddString);
                }
            }
        }
    }
}
