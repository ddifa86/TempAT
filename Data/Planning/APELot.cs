using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Mozart.SeePlan.Aleatorik.Data
{
    public partial class APELot : IAPELot, IDisposable, IFactorObject, IPropertyObject, IConstraintManager, ILateTracker
    {
        public string LotID { get; set; }

        public string FactorObjectKey
        {
            get
            {
                return this.LotID;
            }
        }

        public double Qty
        {
            get; set;
        }

        public DateTime ReleaseTime { get; set; }

        public DateTime LastStepTime { get; set; }

        public double CurrentQty { get; set; }

        public string CurrentOperID
        {
            get
            {
                return this.CurrentOper.OperID;
            }
        }

        public ATOperation InitOper { get; set; }

        public ATOperation CurrentOper { get; set; }

        public ATItemSiteBuffer CurrentItemSiteBuffer;

        public ATRoute CurrentRoute { get; set; }

        public bool IsReserved { get; set; }

        public ATBom CurrentBom
        {
            get
            {
                if (this.CurrentTarget != null)
                    return this.CurrentTarget.CurrentBom;
                else
                    return this.InitOperTarget.CurrentBom;
            }
        }

        public ATBomDetail CurrentBomDetail
        {
            get
            {
                if (this.CurrentTarget != null)
                    return this.CurrentTarget.CurrentBomDetail;
                else
                    return this.InitOperTarget.CurrentBomDetail;
            }
        }

        public IAPELot Sample
        {
            get { return this; }
        }

        public string LotGroupKey { get; set; }

        public APEWip Wip { get; internal set; }

        private ATOperTarget _currentTarget;

        public ATOperTarget CurrentTarget
        {
            get
            {
                if (_currentTarget != null)
                {
                    return _currentTarget;
                }
                else
                {
                    return InitOperTarget;
                }
            }
            set
            {
                _currentTarget = value;
            }
        }

        public LotCreateType LotType { get; internal set; }

        public LotState LotState { get; internal set; }

        public bool IsWipLot
        {
            get
            {
                return this.Wip != null ? true : false;
            }
        }

        public bool IsRunWip
        {
            get
            {
                return this.LotState == LotState.Run;
            }
        }

        /// <summary>
        /// 최초 생성된 Lot이거나 Assembly된 Lot
        /// </summary>
        public bool IsReleasedLot
        {
            get
            {
                return this.CurrentOper == null;
            }
        }

        public bool IsNoTargetSplitLot { get; internal set; }

        public bool IsOutWip
        {
            get
            {
                return this.LotState == LotState.Out;
            }
        }

        public bool IsHold
        {
            get
            {
                return this.LotState == LotState.Hold;
            }
        }

        public bool IsFinish
        {
            get
            {
                return this.CurrentOper == null;
            }
        }

        public ATItem Item
        {
            get
            {
                return this.CurrentItemSiteBuffer.Item;
            }
        }

        public List<APEPlanInfo> Plans { get; set; }
        
        public APEPlanInfo CurrentPlanInfo { get; set; }

        /// <summary>
        /// F/W 진행하면서 변경되는 Qty 수량 산출.
        /// </summary>
        public double CumChangeRatio { get; internal set; }

        // ?
        public ATOperResource CurrentArrange { get; set; }


        public ATOperTarget InputTarget { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public ATOperTarget InitOperTarget { get; private set; }

        public ATOperTarget LastOperTarget
        {
            get
            {
                if (Plans.Count() == 0)
                    return InitOperTarget;
                else
                    return Plans.Last().OperTarget;
            }
        }

        public ATOperTarget ShortTarget
        {
            get
            {
                if (this.CurrentTarget != null)
                    return this.CurrentTarget;
                else
                    return this.LastOperTarget;
            }
        }

        public APEPlanInfo LastPlan
        {
            get
            {
                if (Plans.Count() == 0)
                    return null;

                return Plans.Last();
            }
        }

        public APEPlanInfo FirstPlan
        {
            get
            {
                if (Plans.Count() == 0)
                    return null;

                return Plans.First();
            }
        }


        /// <summary>
        /// 조립 배치의 경우 어떤 PartLot을 이용하여 조립되었는지에 대한 정보
        /// </summary>
        internal ATAssemblyInfo AssemblyInfo { get; set; }

        /// <summary>
        /// 스플릿 된 경우 OrgLot에 대한 정보
        /// </summary>
        public APELot OrgLot { get; set; }

        public Status Status { get; set; } //IsShort를 Status로 대체

        public Dictionary<string, ATFactorValue> FactorInfos { get; set; }

        public Dictionary<string, ATFilterValue> FilterInfos { get; set; }

        public string FactorValues { get; set; }

        public string FilterValues { get; set; }

        public APELot(string lotID, double qty, ATOperation oper, ATOperTarget target, APEWip wip, LotCreateType lotType)
        {
            this.LotID = lotID;
            this.Qty = qty;
            this.CurrentQty = qty;

            this.CurrentOper = null;
            this.InitOper = oper;

            this.CurrentTarget = null;
            this.InitOperTarget = target;
            this.Wip = wip;

            if (wip != null)
            {
                ATItemSiteBuffer itembuffer = ATInputData.ItemSiteBuffers.GetItemSite(wip.WipInfo.SiteID,  wip.ItemID, wip.Buffer.BufferID);
                this.CurrentItemSiteBuffer = itembuffer;

                // 임시
                this.CurrentItemSiteBuffer = target.CurrentItemSiteBuffer; //CurrentBomDetail.FromItemSiteBuffer;
                this.CurrentRoute = target.CurrentBomRoute;
                this.LastStepTime = ATUtil.MaxTime(wip.AvailableTime, ATOption.Instance.PlanStartTime);
            }
            else
            {
                this.CurrentItemSiteBuffer = target.CurrentItemSiteBuffer; // target.CurrentBomDetail.FromItemSiteBuffer;
                this.CurrentRoute = target.CurrentBomRoute;
                this.LastStepTime = ATOption.Instance.PlanStartTime; //target.TargetDateTime;
            }

            this.LotState = wip != null ? wip.State : LotState.Wait;

            this.LotType = lotType;

            this.Plans = new List<APEPlanInfo>();
            this.CapaPlans = new List<APEPlanInfo>();

            this.CumChangeRatio = 1;

            this.Property = new DynamicDictionary();
            this.CalendarInfo = new ATCalendarManager();
            this.Status = Status.Normal;

            this.FactorInfos = new Dictionary<string, ATFactorValue>();
            this.FilterInfos = new Dictionary<string, ATFilterValue>();
            this.VirtualLateInfos = new Dictionary<string, ATLateInfo>();
            //this.Objects = new List<object>();

            //this.Objects.Add(this.Item);
            //this.Objects.Add(this.CurrentItemSiteBuffer.Site);
            //this.Objects.Add(this.CurrentItemSiteBuffer.Buffer);
            //this.Objects.Add( ref this.CurrentItemSiteBuffer);
            //this.Objects.Add(this.CurrentTarget.SODemand.Customer);
            //this.Objects.Add(this.CurrentTarget.SODemand);
            //this.Objects.Add(this.CurrentBom);
            //this.Objects.Add(this.CurrentOper);
            //this.Objects.Add(this.CurrentArrange);

            //if (this.Wip != null)
            //    this.Objects.Add(this.Wip.WipInfo);
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

        void IAPELot.Apply(Action<IAPELot, IAPELot> action)
        {
            if (action != null)
                action(this, null);
        }

        public ATOperation MoveFirst(DateTime now)
        {
            if (this.CurrentOper == null)
                this.CurrentOper = this.InitOper;

            return this.CurrentOper;
        }
        public ATOperation MoveNext(DateTime now)
        {
            ATOperation oper = null;

            if (this.CurrentOper.IsBuffer)
            {
                // Buffer In 시점에 CurrentBom & CurrentRoute 설정.
                if (this.CurrentRoute == null)
                    return null;

                oper = this.CurrentRoute.FirstOper as ATOperation;
            }
            else
            {
                oper = this.CurrentOper.GetDefaultNextStep() as ATOperation;

                if (oper == null && this.CurrentBom != null)
                {
                    oper = this.CurrentBom.ToBuffer;
                }

            }

            return oper;
        }

        public ATBuffer FromBuffer
        {
            get
            {
                ATBuffer buffer;
                if (this.CurrentOper.IsBuffer)
                {
                    buffer = (this.CurrentOper as ATBuffer);
                }
                else
                {
                    buffer = this.CurrentRoute.Bom.FromBuffer;
                }

                return buffer;
            }
        }

        public ATBuffer ToBuffer
        {
            get
            {
                ATBuffer buffer;
                if (this.CurrentOper.IsBuffer)
                {
                    buffer = (this.CurrentOper as ATBuffer);
                }
                else
                {
                    buffer = this.CurrentRoute.Bom.ToBuffer;
                }

                return buffer;
            }
        }

        public ATDemand SoDemand
        {
            get
            {
                return this.CurrentTarget.SODemand;
            }
        }

        public Dictionary<string, ATLateInfo> VirtualLateInfos { get; set; }

        //public List<ref object> Objects { get; set; }

        //public void AddFactorValue(ATWeightFactor factor, ATFactorValue factorValue)
        //{
        //    if (this.FactorInfos.ContainsKey(factor.FactorInfo.FactorID) == false)
        //        this.FactorInfos.Add(factor.FactorInfo.FactorID, factorValue);
        //}

        //public void AddFilterValue(ATWeightFactor factor, ATFilterValue filterValue)
        //{
        //    if (this.FilterInfos.ContainsKey(factor.FactorInfo.FactorID) == false)
        //        this.FilterInfos.Add(factor.FactorInfo.FactorID, filterValue);
        //}

        public void ClearFactorValue()
        {
            this.FactorInfos.Clear();
            this.FilterInfos.Clear();
        }

        public bool AddPlan(APEPlanInfo info)
        {
            info.PrevPlanInfo = this.Plans.LastOrDefault();
            this.Plans.Add(info);

            if (info.AllocationInfo.UsedCapacity != null)
                this.CapaPlans.Add(info);

            return true;
        }

        public virtual APELot DeepCopy()
        {
            APELot clone = (APELot)this.MemberwiseClone();

            clone.Property = new DynamicDictionary();

            var dic = (this.Property as DynamicDictionary);

            dic.ClonePropertys(clone.Property);

            return clone;
        }

        public virtual object Clone()
        {
            return this.DeepCopy();
        }

        public override string ToString()
        {
            return string.Format("{0} / {1} / {2}", this.LotID, this.CurrentItemSiteBuffer, this.CurrentOperID);
        }

        public string GetPropertyValue(string propertyID)
        {
            string value = string.Empty;

            if (propertyID.StartsWith(ATReservedCode.RESERVED_PREFIX_CODE))
            {
                switch (propertyID)
                {
                    case ATReservedCode.ITEM_ID:
                        value = this.Item.ItemID;
                        break;
                    case ATReservedCode.BOM_ID:
                        value = this.CurrentBom.BomID;
                        break;
                    case ATReservedCode.ROUTING_ID:
                        value = this.CurrentRoute.RouteID;
                        break;
                    case ATReservedCode.OPER_ID:
                        value = this.CurrentOperID;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                var property = ATInputData.Properties.GetPropertyByID(propertyID);

                switch (property.Category)
                {
                    case ATReservedCode.ITEM:
                        value = this.Item.GetSetupProperty(propertyID);
                        break;
                    case ATReservedCode.WIP:
                        value = this.Wip.WipInfo.GetSetupProperty(propertyID);
                        break;
                    default:
                        break;
                }
            }

            return value;
        }

        //public List<ATConstraintDetail> GetConstraintDetails(List<ATConstraintDetail> details, string applyDate)
        //{
        //    if (Item.HasConstraint)
        //        Item.GetConstraintDetails(details, applyDate);

        //    if (CurrentItemSiteBuffer.Site.HasConstraint)
        //        CurrentItemSiteBuffer.Site.GetConstraintDetails(details, applyDate);

        //    if (CurrentItemSiteBuffer.Buffer.HasConstraint)
        //        CurrentItemSiteBuffer.Buffer.GetConstraintDetails(details, applyDate);

        //    if (CurrentItemSiteBuffer.HasConstraint)
        //        CurrentItemSiteBuffer.GetConstraintDetails(details, applyDate);

        //    if (CurrentTarget.SODemand.HasConstraint)
        //        CurrentTarget.SODemand.GetConstraintDetails(details, applyDate);

        //    if (CurrentTarget.SODemand.Customer != null && CurrentTarget.SODemand.Customer.HasConstraint)
        //        CurrentTarget.SODemand.Customer.GetConstraintDetails(details, applyDate);

        //    if (CurrentBom.HasConstraint)
        //        CurrentBom.GetConstraintDetails(details, applyDate);

        //    if (CurrentOper.HasConstraint)
        //        CurrentOper.GetConstraintDetails(details, applyDate);

        //    //if (CurrentArrange.HasConstraint)
        //    //    CurrentArrange.GetConstraintDetails(details, applyDate);

        //    if (Wip != null && Wip.WipInfo.HasConstraint)
        //        Wip.WipInfo.GetConstraintDetails(details, applyDate);

        //    // Return Value가 NULL이면 제약이 있었는데 만족을 못한 것이고 Count가 0이면 제약이 없었던것
        //    return details;
        //}

        public string InitFilterLogs()
        {
            return this.FactorObjectKey;
        }

        public string InitFactorLogs()
        {
            return this.FactorObjectKey;
        }

        public void Dispose()
        {
            this.Plans = null;
            this.CapaPlans = null;
            this.VirtualPegWips = null;
            this.SplitInfos = null;
            this.AssemblyHistory = null;
            this.RefPlans = null;

            this.ChildLots = null;
            this.MoLots = null;
            this.OrgLotKeys = null;
            this.FactorInfos = null;
            this.FilterInfos = null;

            if (this.CurrentTarget != null)
            {
                var pegTarget = this.CurrentTarget.PegTarget;
                if (pegTarget != null)
                {
                    pegTarget.Dispose();
                }

                this.CurrentTarget = null;
            }
        }

        public Status CurrentStatus(DateTime currentTime)
        {
            if (this.IsShort)
                return Status.Short;

            if (this.LastOperTarget == null)
                return Status.Normal;

            var lastTarget = this.LastOperTarget;

            if (lastTarget.Next == null)
                return Status.Normal;

            var outTarget = lastTarget.Next;
            if (outTarget == null)
                return Status.Normal;

            if (currentTime > outTarget.TargetDateTime && currentTime <= outTarget.MaxLateTargetDateTime)
                return Status.Late;
            else if (currentTime > this.SoDemand.CalcDueDateTime.AddDays(this.SoDemand.MaxLateDays))
                return Status.Short;

            return Status.Normal;
        }
    }
}
