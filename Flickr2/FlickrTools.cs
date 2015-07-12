using Flickr2;
using FlickrNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1
{
    class FlickrTools
    {

        public static async void addTagToPhoto(String photoid, String tag)
        {
            List<string> tagsToAdd = new List<string>();
            tagsToAdd.Add(tag);
            await getFlickrObject().PhotosAddTagsAsync(photoid, tagsToAdd);
        }

        public static async void removeTagToPhoto(String photoid, String tag)
        {
            List<string> tagsToRemove = new List<string>();
            tagsToRemove.Add(tag);
            Flickr flickr = getFlickrObject();
            PhotoInfo info = await flickr.PhotosGetInfoAsync(photoid);
            foreach(PhotoInfoTag tg in info.Tags)
            {
                if (String.Equals(tg.TagText, tag, StringComparison.OrdinalIgnoreCase)){
                    await flickr.PhotosRemoveTagAsync(tg.TagId);
                }
            }
        }

        public static async Task<List<string>> getListOfYearsWithPhotos()
        {
            //First find the oldest year with a photo
            List<string> returnList = new List<string>();
            String earliestYear = await getEarlierstYearWithPhoto();
            int startYr = Convert.ToInt32(earliestYear);
            int currentYear = Convert.ToInt32(DateTime.Now.Year.ToString());
            List<DateTime> takenDates = new List<DateTime>();
            for (int i= currentYear; i>=startYr; i--)
            {
                returnList.Add(""+i);
                string date = "01/01/" + (i+1);
                takenDates.Add(Convert.ToDateTime(date));
                date = "01/01/" + i;
                takenDates.Add(Convert.ToDateTime(date));
            }
            takenDates.Reverse();
            PhotoCountCollection counts = await getFlickrObject().PhotosGetCountsAsync(null, takenDates);
            for(int i = returnList.Count() - 1; i >= 0; i--) {
                string year = returnList.ElementAt(i);
                string endYear = (Convert.ToInt32(year)+1).ToString();
                foreach (PhotoCount count in counts){
                    if (count.FromDate.Year.ToString().Equals(year) && count.ToDate.Year.ToString().Equals(endYear))
                    {
                        if(count.Count <= 0)
                        {
                            returnList.RemoveAt(i);break;
                        }else
                        {
                            returnList.RemoveAt(i);returnList.Insert(i, year + " (" + count.Count + ")");break;
                        }
                    }
                }
            }
            return returnList;
        }

        public static async Task<Dictionary<string,List<string>>> getMonthsWithPhotosForYears(List<string> years)
        {
            Dictionary<string, List<string>> returnMap = new Dictionary<string, List<string>>();
            List<DateTime> dateRanges = getDateRangesForYears(years);
            PhotoCountCollection counts = await getFlickrObject().PhotosGetCountsAsync(null, dateRanges);
            foreach(string year in years)
            {
                List<string> monthList = new List<string>();
                for(int i = 12; i >= 1; i--)
                {
                    int count = getCountForYearAndMonth(year, i, counts);
                    if(count > 0)
                    {
                        monthList.Add(getMonthName(i) + " (" + count + ")");
                    }
                }
                returnMap.Add(year, monthList);
            }
            return returnMap;
        }

        private static int getCountForYearAndMonth(string year, int month, PhotoCountCollection counts)
        {
            int yearToUse = 0;
            try { 
                string yrTmp = year;
                char[] sep = { ' ' };
                string[] output = yrTmp.Split(sep);
                yearToUse = Convert.ToInt32(output[0]);
                DateTime startDate = Convert.ToDateTime(Convert.ToString(month).PadLeft(2, '0') + "/01/" + yearToUse);
                DateTime endDate;
                if (month == 12)
                {
                    endDate = Convert.ToDateTime(Convert.ToString(1).PadLeft(2, '0') + "/01/" + (yearToUse + 1));
                }else
                {
                    endDate = Convert.ToDateTime(Convert.ToString(month + 1).PadLeft(2, '0') + "/01/" + (yearToUse));
                }
                foreach (PhotoCount count in counts)
                {
                    if (count.FromDate.Equals(startDate) && count.ToDate.Equals(endDate))
                    {
                        return count.Count;
                    }
                }
            }catch(Exception e)
            {
                e.StackTrace.ToString();
            }
            return 0;
        }

        private static string getMonthIntString(string month)
        {
            if ("January".Equals(month))
            {
                return "01";
            }else if ("Febuary".Equals(month))
            {
                return "02";
            }
            else if ("March".Equals(month))
            {
                return "03";
            }
            else if ("April".Equals(month))
            {
                return "04";
            }
            else if ("May".Equals(month))
            {
                return "05";
            }
            else if ("June".Equals(month))
            {
                return "06";
            }
            else if ("July".Equals(month))
            {
                return "07";
            }
            else if ("August".Equals(month))
            {
                return "08";
            }
            else if ("September".Equals(month))
            {
                return "09";
            }
            else if ("October".Equals(month))
            {
                return "10";
            }
            else if ("November".Equals(month))
            {
                return "11";
            }
            else if ("December".Equals(month))
            {
                return "12";
            }else
            {
                return "01";
            }
        }

        public static string getMonthName(int month)
        {
            if(month == 1)
            {
                return "January";
            }
            else if (month == 2)
            {
                return "Febuary";
            }
            else if (month == 3)
            {
                return "March";
            }
            else if (month == 4)
            {
                return "April";
            }
            else if (month == 5)
            {
                return "May";
            }
            else if (month == 6)
            {
                return "June";
            }
            else if (month == 7)
            {
                return "July";
            }
            else if (month == 8)
            {
                return "August";
            }
            else if (month == 9)
            {
                return "September";
            }
            else if (month == 10)
            {
                return "October";
            }
            else if (month == 11)
            {
                return "November";
            }
            else if (month == 12)
            {
                return "December";
            }
            return "Unknown";
            
        }

        private static List<DateTime> getDateRangesForYears(List<string> years)
        {
            List<DateTime> returnList = new List<DateTime>();
            try {
                for (int i = years.Count() - 1; i >= 0; i--)
                {
                    string yrTmp = years.ElementAt(i);
                    char[] sep = { ' ' };
                    string[] output = yrTmp.Split(sep);
                    int year = Convert.ToInt32(output[0]);

                    for (int j = 1; j <= 12; j++)
                    {
                        string month = Convert.ToString(j).PadLeft(2, '0');
                        string nextMonth = Convert.ToString(j + 1).PadLeft(2, '0');
                        DateTime d = Convert.ToDateTime(month + "/01/" + year);
                        returnList.Add(d);
                        if (!month.Equals("12"))
                        {
                            d = Convert.ToDateTime(nextMonth + "/01/" + (year));
                            returnList.Add(d);
                        }
                        else
                        {
                            d = Convert.ToDateTime("01/01/" + (year + 1));
                            returnList.Add(d);
                        }
                    }
                }
            }catch(Exception e)
            {
                String s = e.StackTrace;
                s.ToString();
            }
            return returnList;
        }

        

        public static Flickr getUnauthorizedFlickrObject()
        {
            Flickr flickr = new Flickr(FlickrApiKey.API_KEY, FlickrApiKey.API_SECRET);
            return flickr;
        }

        private static Flickr getFlickrObject()
        {
            App app = (App)App.Current;
            Flickr flickr = getUnauthorizedFlickrObject();
            flickr.OAuthAccessToken = app.accountKey;
            flickr.OAuthAccessTokenSecret = app.accountSecret;
            return flickr;
        }
        private static async Task<string> getEarlierstYearWithPhoto()
        {
            App app = (App)App.Current;
            Flickr flickr = getFlickrObject();
            var options = new PhotoSearchOptions { MediaType = MediaType.Photos, Tags = "*", PerPage = 1, Page = 1, UserId = app.accountId, Extras = (PhotoSearchExtras.DateTaken), SortOrder = PhotoSearchSortOrder.DateTakenAscending };
            PhotoCollection photos = await flickr.PhotosSearchAsync(options);
            foreach (Photo photo in photos)
            {
                if ((bool)photo.DateTakenUnknown)
                {
                    return "1999";
                }
                return "" + photo.DateTaken.Year;
            }
            return "1999";
        }

        public static async Task<bool> hasUpdatesSinceDate(DateTime sinceDate)
        {
            if (sinceDate == null) return true;
            Flickr flickr = getFlickrObject();
            App app = (App)App.Current;
            PhotoCollection collection = await flickr.PhotosRecentlyUpdatedAsync(sinceDate.ToUniversalTime(), PhotoSearchExtras.DateTaken, 1, 1);
            if(collection != null)
            {
                if(collection.Count > 0)
                {
                    return true;
                }
            }
            return false;

        }

/*        private static List<DateTime> getPhotoCountDatesFromMonths(List<DateNavigationItem> months)
        {
            List<DateTime> photCountDates = new List<DateTime>();
            try
            {
                for (int i = months.Count() - 1; i >= 0; i--)
                {
                    DateNavigationItem item = months.ElementAt(i);
                    string month = getMonthIntString(item.month);
                    string nextMonth = Convert.ToString(Convert.ToUInt32(month) + 1).PadLeft(2, '0');
                    DateTime d = Convert.ToDateTime(month + "/01/" + item.year);
                    photCountDates.Add(d);
                    if (!month.Equals("12"))
                    {
                        d = Convert.ToDateTime(nextMonth + "/01/" + (item.year));
                        photCountDates.Add(d);
                    }
                    else
                    {
                        d = Convert.ToDateTime("01/01/" + (item.year + 1));
                        photCountDates.Add(d);
                    }
                    
                }
            }
            catch (Exception e)
            {
                String s = e.StackTrace;
                s.ToString();
            }
            photCountDates.Sort((a, b) => a.CompareTo(b));
            return photCountDates;
        }

        private static void addNavigationItemToList(List<DateNavigationItem> items, DateNavigationItem it)
        {
            foreach(DateNavigationItem dn in items)
            {
                if(it.month.Equals(dn.month) && it.year == dn.year)
                {
                    return;
                }
            }
            items.Add(it);
        }*/


        public static async Task<List<FlickrPhoto>> getPhotosForMonth(int perPage, int pageNum, string searchTerm, string year, string month)
        {
            var app = App.Current as App;
            string monthIntString = getMonthIntString(month);
            DateTime startDate = Convert.ToDateTime(monthIntString + "/01/" + year);
            DateTime endDate;
            if (!"12".Equals(monthIntString))
            {
                string endMonthString = Convert.ToString(Convert.ToInt32(monthIntString) + 1).PadLeft(2, '0');
                endDate = Convert.ToDateTime(endMonthString+"/01/"+year);
            }else
            {
                string endMonthString = "01";
                string endYear = (Convert.ToInt32(year) + 1).ToString();
                endDate = Convert.ToDateTime(endMonthString + "/01/" + endYear);
            }
            List<FlickrPhoto> photosToReturn = new List<FlickrPhoto>();
            Flickr flickr = getFlickrObject();
            var options = new PhotoSearchOptions { MediaType = MediaType.Photos, PerPage = perPage, Page = pageNum, UserId = app.accountId, Extras = (PhotoSearchExtras.DateTaken | PhotoSearchExtras.OriginalUrl | PhotoSearchExtras.Tags), SortOrder = PhotoSearchSortOrder.DateTakenDescending, MinTakenDate= startDate,MaxTakenDate=endDate };
            if (!"*".Equals(searchTerm))
            {
                options.Tags = searchTerm;
            }
            PhotoCollection photos = await flickr.PhotosSearchAsync(options);
            foreach (Photo photo in photos)
            {
                FlickrPhoto p = new FlickrPhoto();
                p.Title = photo.Title;
                p.Url = photo.WebUrl;
                p.keywords = photo.Tags.ToList();
                p.Farm = Convert.ToInt32(photo.Farm);
                p.Id = Convert.ToInt64(photo.PhotoId);
                p.Secret = photo.Secret;
                p.Server = Convert.ToInt32(photo.Server);
                if (p.keywords.Contains(app.tagToAddString.ToLower()))
                {
                    p.tagImage = App.FAVORITE_ICON;
                }
                else
                {
                    p.tagImage = App.NOT_FAVORITE_ICON;
                }
                photosToReturn.Add(p);


            }
            return photosToReturn;
        }

        public static async Task<List<FlickrPhoto>> getImages(String searchTerm, int pageNum, int numPerPage)
        {
            var app = App.Current as App;
            List<FlickrPhoto> photosToReturn = new List<FlickrPhoto>();
            Flickr flickr = getFlickrObject();
            var options = new PhotoSearchOptions { MediaType = MediaType.Photos, Tags = searchTerm, PerPage = numPerPage, Page = pageNum, UserId = app.accountId, Extras = (PhotoSearchExtras.DateTaken | PhotoSearchExtras.OriginalUrl | PhotoSearchExtras.Tags), SortOrder = PhotoSearchSortOrder.DatePostedDescending };
            PhotoCollection photos = await flickr.PhotosSearchAsync(options);
            foreach (Photo photo in photos)
            {
                FlickrPhoto p = new FlickrPhoto();
                p.Title = photo.Title;
                p.Url = photo.WebUrl;
                p.keywords = photo.Tags.ToList();
                p.Farm = Convert.ToInt32(photo.Farm);
                p.Id = Convert.ToInt64(photo.PhotoId);
                p.Secret = photo.Secret;
                p.Server = Convert.ToInt32(photo.Server);
                
                if (p.keywords.Contains(app.tagToAddString.ToLower()))
                {
                    p.tagImage = App.FAVORITE_ICON;
                }
                else{
                    p.tagImage = App.NOT_FAVORITE_ICON;
                }
                photosToReturn.Add(p);

            }
            return photosToReturn;
        }

    }

    public class FlickrPhoto : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public List<String> keywords { get; set; }
        public long Id { get; set; }
        public string Secret { get; set; }
        public int Farm { get; set; }
        public int Server { get; set; }

        private string m_tagImage;
        public string tagImage {
            get { return m_tagImage; }
            set { m_tagImage = value; NotifyPropertyChanged("tagImage"); }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public string ImageUrl
        {
            get
            {
                return (string.Format(
                  "http://farm{0}.static.flickr.com/{1}/{2}_{3}_m.jpg",
                  Farm, Server, Id, Secret));
            }
        }

        public string ImageUrlLarge
        {
            get
            {
                return (string.Format(
                  "http://farm{0}.static.flickr.com/{1}/{2}_{3}_b.jpg",
                  Farm, Server, Id, Secret));
            }
        }

        private void NotifyPropertyChanged(String updateScore)

        {

            if (PropertyChanged != null)

            {

                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(updateScore));

            }

        }
    }
}
