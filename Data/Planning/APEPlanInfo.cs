using Mozart.SeePlan.Aleatorik.Planner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    /// <summary>
    /// 공정의 스냅샷 정보
    /// </summary>
    public class LotInfo
    {
        #region Lot 속성
        public bool IsFirstOperation { get; private set; }

        public string LotGroupKey { get; set; } // LotInfo를 만든 이후에 Setting이 되므로, Setting시에 따로 Setting 가능하도록 설정

        public string LotType { get; private set; }

        public string WipType { get; private set; }

        public string OrgLotID { get; private set; }

        public ATBom Bom { get; private set; }
        
        public ATItemSiteBuffer ItemSiteBuffer { get; private set; }
    
        public ATOperation Operation { get; private set; }

        public ATRoute Route { get; private set; }

        public ATItem Item { get; private set; }

        public ATOperTarget OperTarget { get; private set; }

        public double Yield { get; private set; }
       
        public bool NoCarryOver { get; private set; }
        #endregion
       
        public DateTime OrgArrivalTime { get; set; }

        public DateTime ArrivalTime { get; set; }

        public LotInfo(APELot lot)
        {
            this.OrgArrivalTime = lot.LastStepTime;
            this.ArrivalTime = lot.LastStepTime;

            this.IsFirstOperation = lot.InitOper == lot.CurrentOper;
            this.LotType = lot.LotType.ToString();
            this.WipType = lot.IsWipLot ? lot.Wip.WipInfo.LotType.ToString() : null;
            this.OrgLotID = lot.LotID;
            this.Bom = lot.CurrentBom;
            this.ItemSiteBuffer = lot.CurrentItemSiteBuffer;
            this.Operation = lot.CurrentOper;
            this.Route = lot.CurrentRoute;
            this.Item = lot.Item;
            this.OperTarget = lot.CurrentTarget;
            this.Yield = lot.CurrentTarget.Yield;
            this.NoCarryOver = false;

            if (lot.IsWipLot && lot.Wip.Oper == lot.CurrentOper)
                this.NoCarryOver = false;
            else if (lot.CurrentOper.OperType == OperType.Dummy || lot.CurrentOper.IsFirstResOper)
                this.NoCarryOver = true;
            else if (lot.CurrentOper.OperType == OperType.Buffer)
                this.NoCarryOver = lot.CurrentItemSiteBuffer.IsNoCarryover;
        }

        public virtual LotInfo DeepCopy()
        {
            LotInfo clone = (LotInfo)this.MemberwiseClone();
            return clone;
        }

        public object Clone()
        {
            return this.DeepCopy();
        }
    }

    /// <summary>
    /// 할당 정보
    /// </summary>
    public class AllocationInfo : ICloneable
    {
        public PlanInfoType Type { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public DateTime BucketEndTime { get; set; }

        public DateTime OrgStartTime { get; set; }

        public DateTime OrgEndTime { get; set; }

        public IBucket Bucket { get; set; }

        public IBucket MainBucket { get; set; }

        public double Tat { get; set; }

        public double TotalTat { get; set; }

        public double RealAllocQty { get; set; }

        public double AllocSeq { get; set; }

        public int Level { get; set; }

        public double AllocLotQty { get; set; }

        public double UsedCapaQty { get; set; }

        public double UsedCapaRatio { get; set; }

        public double UsagePer { get; set; }

        public double UtilizationRate { get; set; }

        public PBFAllocationLog AllocationLog { get; set; }

        public bool IsAllocatedInfo
        {
            get
            {
                return this.Type == PlanInfoType.Allocate || this.Type == PlanInfoType.Constraint;
            }
        }

        public Dictionary<ATConstraintDetail, double> UsedConstraints { get; set; } // PBO (가상 수량의 실적 차감을 위함)

        public APECapacity UsedCapacity { get; set; } // PBO

        public APERefPlan RefPlan { get; set; } // PBO

        public AllocationInfo()
        {
        }

        public AllocationInfo(PlanInfoType type, DateTime startTime, DateTime endTime, DateTime bucketEndTime, IBucket bucket, IBucket mainBucket, double allocKey, int level, double allocLotQty, double usageCapaQty, double usagePer, double utilizationRate, APECapacity capacity, PBFAllocationLog allocationLog)
        {
            this.Type = type;

            this.StartTime = startTime;
            this.EndTime = endTime;
            this.OrgStartTime = startTime;
            this.OrgEndTime = endTime;

            this.Bucket = bucket;
            this.MainBucket = mainBucket;
            if (type == PlanInfoType.Allocate)
            {
                this.Tat = (endTime - startTime).TotalSeconds;
                this.TotalTat = this.Tat;
                this.UtilizationRate = utilizationRate;
                this.UsagePer = usagePer;
            }
            
            this.BucketEndTime = bucketEndTime;
            this.AllocLotQty = allocLotQty;
            this.UsedCapaQty = usageCapaQty;
            this.UsedCapaRatio = allocLotQty / usageCapaQty;
            this.UsedCapacity = capacity;

            // 아래 정보는 Context 하나를 넘기면 모두 받을 수 있음 -> Context를 넘기는것이 바람직하지는 않음
            this.AllocationLog = allocationLog;
            this.AllocSeq = allocKey;
            this.Level = level;
        }

        public bool UpdatePlanInfo(AllocationInfo info, double allocQty)
        {
            // 할당 종료 시간
            this.EndTime = info.EndTime;
            this.OrgEndTime = info.EndTime;
            this.BucketEndTime = info.BucketEndTime;

            // 수량 및 Tat
            this.AllocLotQty += allocQty;
            this.UsedCapaQty += info.UsedCapaQty;
            this.Tat += info.Tat;
            this.TotalTat = this.Tat;

            // 의미가 있을까??
            this.UsagePer = info.UsagePer;
            this.UtilizationRate = info.UtilizationRate;

            return true;
        }

        public virtual AllocationInfo DeepCopy()
        {
            AllocationInfo clone = (AllocationInfo)this.MemberwiseClone();

            return clone;
        }

        public object Clone()
        {
            return this.DeepCopy();
        }
    }

    public class APEPlanInfo : ICloneable, IPropertyObject
    {
        private static int _planSequence = 0;

        public int PlanSeq { get; private set; }

        public LotInfo LotInfo { get; set; }

        public AllocationInfo AllocationInfo { get; set; }

        public dynamic Property { get; set; }

        public bool IsWritePlanInfo { get; set; }

        public APEPlanInfo PrevPlanInfo { get; set; }

        public bool NoCarryOver
        {
            get
            {
                if (this.LotInfo != null)
                    return this.LotInfo.NoCarryOver;

                return false;
            }
        }

        public IBucket Bucket
        {
            get
            {
                return this.AllocationInfo?.Bucket;
            }
        }

        public DateTime StartTime
        {
            get
            {
                return this.AllocationInfo.StartTime;
            }
            set
            {
                this.AllocationInfo.StartTime = value;
            }
        }

        public DateTime EndTime
        {
            get
            {
                return this.AllocationInfo.EndTime;
            }
            set
            {
                this.AllocationInfo.EndTime = value;
            }
        }

        public DateTime ArrivalTime
        {
            get
            {
                if (this.LotInfo != null)
                    return this.LotInfo.ArrivalTime;

                return AleatorikGlobalParameters.Instance.start_time;
            }
            set
            {
                this.LotInfo.ArrivalTime = value;
            }
        }

        public ATOperTarget OperTarget
        {
            get
            {
                if (this.LotInfo != null)
                    return this.LotInfo.OperTarget;

                return null;
            }
        }

        public PlanInfoType Type
        {
            get
            {
                return this.AllocationInfo.Type;
            }
        }

        public int Level { get; set; }
        
        public PBFAllocationLog AllocationLog { get; set; }
        
        public string Description { get; set; }

        public ATCalendarManager CalendarInfo { get; set; }

        public APEPlanInfo(LotInfo lotInfo, AllocationInfo allocationInfo)
        {
            this.PlanSeq = ++_planSequence;

            this.LotInfo = lotInfo;
            this.AllocationInfo = allocationInfo;
            this.Property = new DynamicDictionary();
            this.CalendarInfo = new ATCalendarManager();

            if (allocationInfo != null)
            {
                this.Level = allocationInfo.Level;
                this.AllocationLog = allocationInfo.AllocationLog;
            }
        }

        public virtual APEPlanInfo DeepCopy()
        {
            APEPlanInfo clone = (APEPlanInfo)this.MemberwiseClone();
            clone.AllocationInfo = clone.AllocationInfo.Clone() as AllocationInfo;

            return clone;
        }

        public virtual object Clone()
        {
            return this.DeepCopy();
        }

        internal void UpdatePlanInfo(APEPlanInfo planInfo, double allocQty)
        {
            if (this.AllocationInfo == null)
            {
                this.AllocationInfo = planInfo.AllocationInfo.Clone() as AllocationInfo;
                this.AllocationInfo.AllocLotQty = allocQty;

                this.Level = planInfo.AllocationInfo.Level;
                this.AllocationLog = planInfo.AllocationInfo.AllocationLog;
            }
            else
            {
                this.AllocationInfo.UpdatePlanInfo(planInfo.AllocationInfo, allocQty);
            }
        }

        public void SetProperty(string propertyID, object value)
        {
            this.Property[propertyID] = value;
        }

        public void SetCalendar(string propertyID, ATCalendar calendar)
        {
            this.CalendarInfo.AddCalendar(calendar);
        }
    }
}
