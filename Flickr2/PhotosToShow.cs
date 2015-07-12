using App1;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Flickr2
{
    public class PhotosToShow : ObservableCollection<FlickrPhoto>, ISupportIncrementalLoading
    {

        public int lastItem = 1;
        public int pageNum = 1;
        public int photosAtATime = 20;
        public string year;
        public string month;
        public string searchParam;
        public bool HasMoreItems { get; set; } = true;
        public bool initialListing { get; set; } = false;

        public PhotosToShow(bool initialListing)
        {
            this.initialListing = initialListing;
        }

        public async Task LoadMoreItemsIfNeededAsync(int currentIndex)
        {
            if(currentIndex >= Count - 2)
            {
                await LoadMoreItemsAsync(20);
            }
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            CoreDispatcher coreDispatcher = Window.Current.Dispatcher;

            return Task.Run<LoadMoreItemsResult>(async () =>
            {
                List<FlickrPhoto> results = null;
                if (!initialListing)
                {
                    results = await FlickrTools.getPhotosForMonth(photosAtATime, pageNum++, searchParam, year, month);
                }
                else
                {
                    App app = App.Current as App;
                    results = await FlickrTools.getImages(app.searchParamString, pageNum++, photosAtATime);
                    this.HasMoreItems = false;
                }
                
                await coreDispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        foreach (FlickrPhoto item in results)
                        {
                            this.Add(item);
                        }
                    });
                if (results.Count != photosAtATime)
                {
                    this.HasMoreItems = false;
                }

                return new LoadMoreItemsResult() { Count = count };
            }).AsAsyncOperation<LoadMoreItemsResult>();
        }
    }
}
