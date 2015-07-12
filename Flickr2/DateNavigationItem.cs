using App1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flickr2
{
    class DateNavigationItem
    {
        public DateNavigationItem()
        {

        }

        public DateNavigationItem(DateTime dt)
        {
            this.year = ""+dt.Year;
            this.month = FlickrTools.getMonthName(dt.Month);
            this.subItems = new List<DateNavigationItem>();
        }

        public DateNavigationItem(string title)
        {
            this._title = title;
            char[] sep = { ' ' };
            string[] output = title.Split(sep);
            string baseName = output[0];
            try
            {
                Convert.ToInt32(baseName);
                year = baseName;
            }catch(Exception e)
            {
                month = baseName;
                string numPhotosString = output[1];
                this.numItems = Convert.ToInt32(numPhotosString.Substring(1, numPhotosString.Length - 2));
            }
            this.subItems = new List<DateNavigationItem>();
        }
        private string _title;
        public string title
        {
            get { return year+" - "+month+" ("+numItems+")"; }
            set { _title = value; }
        }

        public List<DateNavigationItem> subItems { get; set; }
        public string year;
        public string month;
        public int numItems;

    }
}
