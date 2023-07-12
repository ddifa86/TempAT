using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public enum LateCategory
    {
        StdData,
        Material,
        Capacity,
        Tat,
        FixedPlan,
        Etc,
        Constraint
    }

    public enum LateReason
    {
        None,
        UnReleasedFERTMaterialShort,
        HAWAMaterialShort,
        FERTMaterialShort,
        LackOfResourceCapacity,
        LackOfSetupResourceCapacity,
        NoResourceCapacity,
        ItemConstraint,
        NoOpResourceInfo,
        ResourceConstraint,
        LackOfWip,
        BWBomPathShort,
        PrebuildViolation,
        LateAllowanceViolation,
        OrderQuantityViolation,
        LackOfReferencePlan,
        NoBwBomPathShort,
        MultipleBatchSizeShort,
        FailToFindTarget,
        LateReleaseLot,
        NoOpAddResourceInfo,
        DummyOperationLate,
        RemainPegPart,
        LackOfCapacity
    }

    public class ATLateInfo
    {
        public ATDemand Demand { get; internal set; }

        public ATBom Bom { get; internal set; }

        public ATOperation Oper { get; internal set; }

        public HashSet<IBucket> Resources { get; internal set; }

        public ATItemSiteBuffer ItemSiteBuffer { get; internal set; }

        public int Phase { get; internal set; }

        public int RetryCount { get; internal set; }

        /// <summary>
        /// 동일 사유가 등록된 횟수
        /// </summary>
        public int Count { get; internal set; }

        public double ShortQty { get; internal set; }

        public DateTime FromDate { get; internal set; }

        public DateTime ToDate { get; internal set; }

        public string DetailInfo { get; internal set; }

        public Status ShortType { get; internal set; }

        public LateCategory Category { get; internal set; }

        public string Reason { get; internal set; }

        public string Description { get; internal set; }

        public HashSet<string> ShortLots { get; internal set; }

        public DateTime MinAvailableTime { get; internal set; }

        public string RefPlanID { get; internal set; }

        public string GetLateDetailInfo()
        {
            string detail = this.Description;
            LateReason reason = ATUtil.StringToEnum<LateReason>(Reason, LateReason.None);
            DateTime dt = DateTime.MinValue;
            switch (reason)
            {
                case LateReason.None:
                    break;
                case LateReason.HAWAMaterialShort:
                    if (this.ShortLots != null && this.ShortLots.Count > 0)
                        dt = this.MinAvailableTime;

                    detail = string.Format("Qty : {0}, Min Availalbe Time : {1}", ShortQty, dt);
                    break;
                case LateReason.FERTMaterialShort:
                    if (this.ShortLots != null && this.ShortLots.Count > 0)
                        dt = this.MinAvailableTime;

                    detail = string.Format("Qty : {0}, Min Availalbe Time : {1}", ShortQty, dt);
                    break;
                case LateReason.LackOfResourceCapacity:
                    break;
                case LateReason.NoResourceCapacity:
                    detail = Oper?.OperID;
                    break;
                case LateReason.ItemConstraint:
                    break;
                case LateReason.ResourceConstraint:
                    break;
                case LateReason.PrebuildViolation:
                    break;
                case LateReason.LateAllowanceViolation:
                    break;
                case LateReason.OrderQuantityViolation:
                    break;
                case LateReason.LackOfReferencePlan:
                    break;
                case LateReason.NoBwBomPathShort:
                    break;
                case LateReason.MultipleBatchSizeShort:
                    break;
                default:
                    break;
            }

            return detail;
        }

        public ATLateInfo(ATDemand demand, ATBom bom, ATOperation oper, ATItemSiteBuffer itemSiteBuffer, Status shortType, LateCategory category, string reason, string desc, int phase, int retryCount, DateTime fromDate, DateTime toDate, string refPlanID)
        {
            this.Demand = demand;
            this.Bom = bom;
            this.Oper = oper;
            this.ItemSiteBuffer = itemSiteBuffer;
            this.ShortType = shortType;
            this.Category = category;
            this.Reason = reason;
            this.Description = desc;
            this.Phase = phase;
            this.RetryCount = retryCount;
            this.FromDate = fromDate;
            this.ToDate = toDate;
            this.RefPlanID = refPlanID;

            this.MinAvailableTime = DateTime.MaxValue;
            this.Resources = new HashSet<IBucket>();
            this.ShortLots = new HashSet<string>();
        }

        public void AddShortLot(APELot lot)
        {
            this.ShortLots.Add(lot.LotID);
            this.MinAvailableTime = ATUtil.MinTime(this.MinAvailableTime, lot.LastStepTime);
        }

        public string GetKey()
        {
            string key = this.ItemSiteBuffer.Key + this.Oper?.RouteID + this.Oper?.OperID + this.Category.ToString() + this.Reason + this.ShortType.ToString() + this.RefPlanID + this.Phase + this.RetryCount;

            return key; 
        }
    }
}