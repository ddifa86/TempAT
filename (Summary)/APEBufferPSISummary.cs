using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class APEBufferPSISummary
    {
        public ATItemSiteBuffer ItemSiteBuffer { get; private set; }

        public DateTime Date { get; private set; }

        public double BOH { get; set; }

        public double In { get; set; }

        public double Out { get;set; }

        public string ItemID
        {
            get
            {
                return ItemSiteBuffer.ItemID;
            }
        }

        public string SiteID
        {
            get
            {
                return ItemSiteBuffer.SiteID;
            }
        }

        public string BufferID
        {
            get
            {
                return ItemSiteBuffer.BufferID;
            }
        }

        public APEBufferPSISummary(ATItemSiteBuffer itemSiteBuffer, DateTime date)
        {
            this.ItemSiteBuffer = itemSiteBuffer;
            this.Date = date;
        }
    }
}
