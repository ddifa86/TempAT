using Mozart.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    [Mozart.Task.Execution.FEBaseClassAttribute(Root = "PKG", Category = "PKG", IsTypeBinding = true, Mandatory = true, Description = null)]
    public class ATBomDetail : IPropertyObject
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

        public double FromQty;

        public ATSite ToSite;
        public ATItem ToItem;
        public ATBuffer ToBuffer;
        public ATItemSiteBuffer ToItemSiteBuffer;
        public double ToQty;

        

        public string FromSiteID
        {
            get { return this.FromSite.SiteID;  }
        }
        public string FromItemID
        {
            get { return this.FromItem.ItemID; }
        }

        public string FromBufferID
        {
            get { return this.FromBuffer.BufferID; }

        }

        public string ToSiteID
        {
            get { return this.ToSite.SiteID; }
        }

        public string ToItemID
        {
            get { return this.ToItem.ItemID; }
        }

        public string ToBufferID
        {
            get { return this.ToBuffer.BufferID; }
        }

        public string Class { get; private set; }
        public BomPathType IsUsableDetail { get; internal set; }

        // 하이닉스용 개발 => 추후 정리작업 필요..
        public ATCalendar Calendar { get; internal set; }

        // 
        public Dictionary<string, double> ToQtyOnCalendar;

        public ATBomDetail(ATBom bom, ATSite fromSite, ATItem fromitem, ATBuffer frombuffer, double fromQty, 
                                      ATSite toSite, ATItem toitem, ATBuffer tobuffer, double toQty)
        {
            this.Bom = bom;

            this.FromSite = fromSite;
            this.FromItem = fromitem;
            this.FromBuffer = frombuffer;
            this.FromQty = fromQty;

            this.ToSite = toSite;
            this.ToItem = toitem;
            this.ToBuffer = tobuffer;
            this.ToQty = toQty;

            this.IsUsableDetail = BomPathType.N;

            #region Create ItemBuffer

            var itembuffer = ATInputData.ItemSiteBuffers.GetItemSite(this.FromSite, this.FromItem, this.FromBuffer);
            this.FromItemSiteBuffer = itembuffer;

            itembuffer = ATInputData.ItemSiteBuffers.GetItemSite(this.ToSite, this.ToItem, this.ToBuffer);
            this.ToItemSiteBuffer = itembuffer;

            #endregion

            this.ToQtyOnCalendar = new Dictionary<string, double>();

            this.Property = new DynamicDictionary();
            this.CalendarInfo = new ATCalendarManager();

        }

        #region DataObject Interface
        public dynamic Property { get; internal set; }

        public ATCalendarManager CalendarInfo { get; internal set; }

        public void SetProperty(string propertyID, object value)
        {
            this.Property[propertyID] = value;
        }
        public void SetCalendar(string propertyID, ATCalendar calendar)
        {
            this.CalendarInfo.AddCalendar(calendar);
        }
        #endregion

        //public override string ToString()
        //{
        //    return string.Format("{0} / {1} -> {2} / {3}", this.BomID, this.FromItemSiteBuffer.Key, this.ToItemSiteBuffer.Key, this.Class);
        //}

        public double GetToQty(DateTime time)
        {
            double toQty = this.ToQty;
            string key = time.ToString(ATUtil.DateFormat);
            if (this.ToQtyOnCalendar.ContainsKey(key))
                toQty = this.ToQtyOnCalendar[key];

            return toQty;
        }

        //public double GetChangeValue()
        //{
        //    if (this.Bom.BomType != BomType.Split)
        //        return _value;

        //    else
        //    {
        //        List<ATBomDetail> upperGrades = this.Bom.GetUpperBomDetail(ToItem, true);
        //        double binrate = upperGrades.Sum(x => x._value);
        //        binrate = Math.Round(binrate, 4);

        //        return binrate;
        //    }
        //}
    }
}
