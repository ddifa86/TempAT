using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class ATBomDetailAlt
    {
        public ATBom Bom { get; private set; }

        public string BomID
        {
            get
            {
                return Bom.BomID;
            }
        }

        public ATSite FromSite;
        public ATItem FromItem;
        public ATBuffer FromBuffer;
        public ATItemSiteBuffer FromItemSiteBuffer;


        public ATSite AltSite;
        public ATItem AltItem;
        public ATBuffer AltBuffer;
        public ATItemSiteBuffer AltItemSiteBuffer;

        public string FromSiteID
        {
            get { return this.FromSite.SiteID; }
        }
        public string FromItemID
        {
            get { return this.FromItem.ItemID; }
        }

        public string FromBufferID
        {
            get { return this.FromBuffer.BufferID; }

        }

        public string AltSiteID
        {
            get { return this.AltSite.SiteID; }
        }

        public string AltItemID
        {
            get { return this.AltItem.ItemID; }
        }

        public string AltBufferID
        {
            get { return this.AltBuffer.BufferID; }
        }
 

        public double Priority { get; set; }

        public ATBomDetailAlt(ATBom bom, ATSite fromSite, ATItem fromitem, ATBuffer frombuffer, 
                                     ATSite altSite, ATItem altitem, ATBuffer altbuffer, double priority)
        {
            this.Bom = bom;

            this.FromSite = fromSite;
            this.FromItem = fromitem;
            this.FromBuffer = frombuffer;

            this.AltSite = altSite;
            this.AltItem = altitem;
            this.AltBuffer = altbuffer;

            this.Priority = priority;

            #region Create ItemBuffer

            var itembuffer = ATInputData.ItemSiteBuffers.GetItemSite(this.FromSite, this.FromItem, this.FromBuffer);
            this.FromItemSiteBuffer = itembuffer;

            itembuffer = ATInputData.ItemSiteBuffers.GetItemSite(this.AltSite, this.AltItem, this.AltBuffer);
            this.AltItemSiteBuffer = itembuffer;

            this.AltItemSiteBuffer.OrgItemSiteBuffers.Add(this.FromItemSiteBuffer);
            #endregion
        }



        public override string ToString()
        {
            return string.Format("{0} : {1}/{2} -> {4}/{5}", this.FromBufferID, this.FromItem, this.FromSiteID, this.AltItemID, this.AltSiteID);
        }
    }
}
