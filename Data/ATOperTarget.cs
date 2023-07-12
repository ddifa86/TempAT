using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{

    /// <summary>
    /// 공정별 Target 정보.
    /// </summary>
    public partial class ATOperTarget : IPropertyObject, IAssemblyKey
    {
        #region values

        public string StageID { get; private set; }

        public string SiteID { get; set; }

        public ATItem Item { get; set; }

        public double TargetQty { get; set; }

        public double ShipEarlyTime { get; internal set; }

        public double RefTime { get; internal set; }

        public DateTime OrgTargetDateTime 
        { 
            get
            {
                return this.TargetDateTime.AddSeconds(ShipEarlyTime + RefTime);
            }
        }

        public DateTime FWTargetDateTime { get; internal set; }

        public DateTime TargetDateTime { get; set; }

        public double InTargetQty
        { 
            get
            {
                return this.TargetQty - this.InComingQty;
            }
        }

        public double UsedQty { get; set; }

        public double InComingQty { get; set; }

        public ATMoPlan MoPlan { get; private set; }

        public ATOperation Oper { get; internal set; }

        public double Tat { get; internal set; }

        /// <summary>
        /// Out의 경우 : Run TAT 정보.
        /// In의 경우 : Run + Wait TAT 정보
        /// </summary>
        public double TotalTat { get; internal set; }

        // 누적 Tat 정보
        public double BCumTat { get; internal set; }

        // Calendar를 고려한 Yield 정보
        public double Yield { get; internal set; }
        //
        public double RemainQty
        {
            get
            {
                return this.TargetQty - this.UsedQty;
            }
        }

        public ATDemand SODemand
        {
            get
            {
                return MoPlan.SODemand;
            }
        }

        public string TargetDate { get { return ATUtil.ToDate(this.TargetDateTime); } }

        public string TargetWeek { get { return ATUtil.ToWeek(this.TargetDateTime); } }

        public string TargetMonth { get { return ATUtil.ToMonth(this.TargetDateTime); } }


        public ATOperTarget Next { get; internal set; }

        public ATOperTarget Prev { get; internal set; }


        #endregion

        // Prev는 여러개가 될 수 있음.. 필요하면 그때 구현.
        //public ATOperTarget Prev { get; set; }

        #region Target Key
        public string TargetID
        {
            get
            {
                return MoPlan.ID;
            }
        }

        public string RouteID
        {
            get
            {
                return this.Oper.RouteID;

            }
        }

        public string OperID
        {
            get
            {
                return this.Oper.OperID;
            }
        }

        public string FromBufferID
        {
            get
            {
                if (this.Oper.IsBuffer)
                    return this.Oper.OperID;

                return this.CurrentBom.FromBuffer.BufferID;
            }
        }

        public string ToBufferID
        {
            get
            {
                if (this.Oper.IsBuffer)
                    return this.Oper.OperID;

                return this.CurrentBom.ToBuffer.BufferID;
            }
        }

        public string IsOut { get; private set; }

        public string PathID { get; private set; }

        #endregion

        #region Planning 시 필요 정보 Link 정보
        public ATBom CurrentBom
        {
            get; internal set;
        }

        public ATRoute CurrentBomRoute { get; internal set; }

        public ATBomDetail CurrentBomDetail { get; internal set; }

        public ATItemSiteBuffer CurrentItemSiteBuffer;

        public string CurrentItemBufferKey
        {
            get
            {
                // 마지막 
                return this.CurrentItemSiteBuffer.Key;
            }
        }

        public APETarget PegTarget;

        public double BCumChangeRatio = 1;

        public DateTime MaxLateTargetDateTime
        {
            get
            {
                double maxdays = (this.MoPlan as ATMoPlan).Demand.MaxLateDays;

                return TargetDateTime.AddDays(maxdays);
            }
        }

        public DateTime MinEarlyTargetDateTime
        {
            get
            {
                double minDays = (this.MoPlan as ATMoPlan).Demand.MaxEarlyDays;

                return TargetDateTime.AddDays(-minDays);
            }
        }

        public APERefPlan CurRefPlan = null;

        #endregion

        #region 생성자

        public ATOperTarget(APETarget pegTarget, ATOperation oper, bool isOut)
        {
            var mo = pegTarget.MoPlan as ATMoPlan;
            var pegPart = pegTarget.PegPart as  APEPegPart;

            this.StageID = ATExecutionContext.Instance.CurrentStage.StageID;
            this.MoPlan = mo;
            this.SiteID = pegPart.CurrentSiteID;
            this.Oper = oper;
            this.Item = pegTarget.Item;
            this.IsOut = isOut ? "Y" : "N";
            this.TargetDateTime = pegTarget.TargetDateTime;
            this.ShipEarlyTime = pegTarget.ShipEarlyTime;
            this.RefTime = pegTarget.RefTime;
            this.TargetQty = pegTarget.RemainQty;
            this.CurrentBom = pegPart.CurrentBom;

            this.CurrentItemSiteBuffer = pegPart.CurrentItemSiteBuffer;

            //?
            this.PathID = mo.Demand.ID + pegPart.PathID;

            this.PegTarget = pegTarget;

            // 수율정보까지 같이 누적하기.
            this.BCumChangeRatio = pegTarget.BCumChangeRatio;

            this.Yield = 1;
        }

        /// <summary>
        /// Dummy
        /// </summary>
        /// <param name="item"></param>
        /// <param name="buffer"></param>
        /// <param name="oper"></param>
        /// <param name="qty"></param>
        /// <param name="duedate"></param>
        /// <param name="isOut"></param>
        internal ATOperTarget(ATSite site, ATItem item, ATBuffer buffer, ATOperation oper, double qty, DateTime duedate, double maxLatDay, double maxEarlyDay, bool isOut = false)
        {
            var itembuffer = ATInputData.ItemSiteBuffers.GetItemSite(site.SiteID, item.ItemID, buffer.BufferID);
            if (itembuffer == null)
            {
                itembuffer = new ATItemSiteBuffer(site, item, buffer, false, false, 0);
                ATInputData.ItemSiteBuffers.AddItemSiteBuffer(itembuffer);
            }

            ATItemSiteBuffer soItemSite = itembuffer.SoItemSiteBuffers.FirstOrDefault();
            if (soItemSite == null)
                soItemSite = itembuffer;

            ATMoMaster mm = new ATMoMaster(soItemSite.Key, soItemSite, soItemSite.SiteID);
            ATDemand demand = new ATDemand(ATExecutionContext.Instance.CurrentStage.StageID, soItemSite.ItemID, soItemSite.SiteID,
                soItemSite, null, duedate, qty, 0, string.Empty, maxLatDay, maxEarlyDay);

            ATMoPlan mo = new ATMoPlan(mm, demand, DateTime.Now);

            this.StageID = ATExecutionContext.Instance.CurrentStage.StageID;
            this.MoPlan = mo;
            this.SiteID = site.SiteID;
            this.Oper = oper;
            this.Item = item;
            this.IsOut = isOut ? "Y" : "N";
            this.TargetDateTime = demand.DueDateTime;
            this.TargetQty = demand.Qty;

            this.CurrentBom = (oper.Route as ATRoute).Bom != null ? (oper.Route as ATRoute).Bom : ATBom.NULL;

            if (this.CurrentBom != ATBom.NULL)
            {
                this.CurrentBomDetail = this.CurrentBom.BomDetails.First();
                this.CurrentBomRoute = oper.Route as ATRoute;
            }

            this.CurrentItemSiteBuffer = itembuffer;

            this.Yield = 1;
            this.Tat = 0;

            this.BCumChangeRatio = 1;
        }

        internal ATOperTarget(ATOperTarget target, ATItemSiteBuffer itemSite, ATOperation oper, double qty)
        {
            this.StageID = target.StageID;
            this.MoPlan = target.MoPlan;
            this.SiteID = string.Empty;
            this.Oper = oper;
            this.Item = itemSite.Item;
            this.IsOut = "N";
            this.TargetDateTime = target.TargetDateTime;
            this.TargetQty = qty;

            this.CurrentBom = (oper.Route as ATRoute).Bom != null ? (oper.Route as ATRoute).Bom : ATBom.NULL;
            

            if (this.CurrentBom != ATBom.NULL)
            {
                this.CurrentBomRoute = oper.Route as ATRoute;
                this.CurrentBomDetail = this.CurrentBom.BomDetails.First();
            }

            this.CurrentItemSiteBuffer = itemSite;

            this.Yield = 1;
            this.Tat = 0;

            this.BCumChangeRatio = 1;
        }

        #endregion

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


        public virtual ATOperTarget DeepCopy()
        {
            ATOperTarget clone = (ATOperTarget)this.MemberwiseClone();

            return clone;
        }
    }
}
