using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.ObyO.Data
{
    public class PBOResource : IBucket, IConstraintManager
    {
        public APECapacityManager CapacityManager { get; set; }

        Dictionary<string, List<ATConstraintDetail>> ConstraintDetails { get; set; }

        public List<APEPlanInfo> Plans { get; set; }

        #region 생성자
        public PBOResource(ATResource resource)
        {
            this.Target = resource;
            this.CapacityManager = new APECapacityManager(this, CapacityType.Quantity);
            this.ConstraintDetails = new Dictionary<string, List<ATConstraintDetail>>();
            this.Plans = new List<APEPlanInfo>();
        }

        internal PBOResource()
        {
            ATResource resource = new ATResource();
            this.Target = resource;
            this.CapacityManager = new APECapacityManager(this, CapacityType.Quantity);
            this.ConstraintDetails = new Dictionary<string, List<ATConstraintDetail>>(); // 불필요해보임
            this.Plans = new List<APEPlanInfo>();
        }
        #endregion

        #region IBucket

        public ATResource Target { get; private set; }

        public string BucketID
        {
            get
            {
                return this.Target.ResourceID;
            }
        }

        public CapacityType CapacityType
        {
            get
            {
                return this.Target.CapaType;
            }
        }

        #endregion

        #region Method
        public APEPlanInfo DoAllocated(APELot lot, ATQtyCapacity capa, double allocCapaQty, double allocLotQty, double usagePer, double utilizationRate, APERefPlan refPlan, PBOAllocateContext context)
        {
            capa.VirtualUsedQty += allocCapaQty;

            // 할당한 이력 정보 등록.
            var start = ATUtil.MaxTime(lot.LastStepTime, capa.StartTime);

            //if (lot.CurrentOper.IsMBSOper)
            //    allocationKey = string.Format("MBS_{0}_{1}", lot.LotID, allocationKey);

            PBOModuleExecutionInfo executionInfo = ATExecutionContext.Instance.CurrentExecutionInfo as PBOModuleExecutionInfo;

            LotInfo lotInfo = new LotInfo(lot);
            AllocationInfo allocationInfo = new AllocationInfo(PlanInfoType.Allocate, start, start, start, this, this, executionInfo.AllocationKey++, 0, allocLotQty, allocCapaQty, usagePer, utilizationRate, capa, null);

            APEPlanInfo info = new APEPlanInfo(lotInfo, allocationInfo);
            info.AllocationInfo.RefPlan = refPlan;

            return info;
        }

        internal void UpdateAct(APEPlanInfo planInfo, double qty, bool isCommit)
        {
            ATQtyCapacity capa = planInfo.AllocationInfo.UsedCapacity as ATQtyCapacity;

            capa.VirtualUsedQty -= qty;

            if (capa.VirtualUsedQty <= ATOption.Instance.MinimumAllocationQuantity)
                capa.VirtualUsedQty = 0;

            if (isCommit)
            {
                capa.UsedCapa += qty;

                capa.UsedPlanInfos.Add(planInfo);

                if (capa.Capacity - capa.UsedCapa <= ATOption.Instance.MinimumAllocationQuantity)
                    this.CapacityManager.RemoveCapaInfo(capa);
            }
        }

        public APEPlanInfo DummyAllocated(IAPELot lot, double tat)
        {
            var sampleLot = lot as APELot;
            DateTime start = lot.LastStepTime;
            DateTime lot_end_time = start.AddSeconds(tat);

            LotInfo lotInfo = new LotInfo(sampleLot);

            PBOModuleExecutionInfo executionInfo = ATExecutionContext.Instance.CurrentExecutionInfo as PBOModuleExecutionInfo;

            AllocationInfo allocationInfo = new AllocationInfo(PlanInfoType.Allocate, start, lot_end_time, lot_end_time, this, this, executionInfo.AllocationKey++, 0, sampleLot.Qty, 0, 0, 0, null, null);
            allocationInfo.TotalTat = tat;

            APEPlanInfo info = new APEPlanInfo(lotInfo, allocationInfo);

            sampleLot.AddPlan(info);
            sampleLot.AddVirtualLateInfo(LateCategory.Tat, LateReason.DummyOperationLate.ToString(), null, sampleLot.LastStepTime, sampleLot.LastStepTime, null);

            return info;
        }

        public List<ATConstraintDetail> GetCurrentConstraintDetails(string applyDate)
        {
            List<ATConstraintDetail> details;
            if (this.ConstraintDetails.TryGetValue(applyDate, out details) == false)
            {
                details = this.GetConstraintDetails(applyDate);
                this.ConstraintDetails.Add(applyDate, details);
            }

            return details;
        }

        public void AddPlan(APEPlanInfo info, IAPELot lot)
        {
            info.PrevPlanInfo = this.Plans.LastOrDefault();
            this.Plans.Add(info);
        }
        #endregion
    }
}
