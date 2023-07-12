using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.Simulation.Engine;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    [FEComponent(FECategory.PlanByForward, FEControl.Allocator, Root = FEProvider.Aleatorik)]
    public partial class FWAllocatorControl : IModelController
    {

        #region IModelController 멤버
        Type IModelController.ControllerType
        {
            get { return typeof(FWAllocatorControl); }
        }
        #endregion

        [FEAction]
        public virtual void OnInitialize(FWAllocator allocator, FWAllocGroup allocGroup)
        { 

        }

        [FEAction]
        public virtual void OnStartLevel(int curLevel, int maxLevel, PBFAllocateContext context)
        {

        }

        [FEAction]
        public virtual void OnPrepareAllocate(PBFResourceGroup bucketGroup, PBFAllocateContext context)
        {

        }

        [FEAction]
        public virtual List<PBFResource> GetLoadableBuckets(List<PBFResource> loadableBuckets, APELotGroup lotGroup, PBFAllocateContext context)
        {
            return loadableBuckets;
        }

        [FEAction]
        public virtual bool IsStopAllocateLotGroup(List<PBFResource> loadableBuckets, APELotGroup lotGroup, PBFAllocateContext context)
        {
            return false;
        }

        [FEAction]
        public virtual bool CheckForAvailableBeforeAllocation(PBFResource bucket, APELotGroup lotgroup, PBFAllocateContext context)
        {
            return true;
        }

        [FEAction]
        public virtual double GetAllocableQty(APELotGroup lotGroup, PBFResource bucket, DateTime loadableTime, PBFAllocateContext context)
        {
            var handled = false;
            return PBFPredefines.Instance.GET_ALLOCABLE_QTY_DEF(lotGroup, bucket, loadableTime, context, ref handled, 0);
        }

        [FEAction]
        public virtual void OnSelectAllocateLot(APELotGroup lotGroup, PBFResource bucket, IAPELot current, DateTime loadableTime, PBFAllocateContext context)
        {

        }

        [FEAction]
        public virtual bool IsStopAllocateInLotGroup(PBFResource bucket, APELotGroup lotgroup, IAPELot current, IAPELot last, PBFAllocateContext context)
        {
            APELot lot = current as APELot;

            if (lot.LastStepTime >= bucket.EndTime)
                return true;

            return false;
        }

        [FEAction]
        public virtual void OnBucketAllocated(PBFResource bucket, APELotGroup lotgroup, IAPELot curLot,  APEPlanInfo planInfo, PBFAllocateContext context)
        {
           
        }

        [FEAction]
        public virtual void OnSetup(PBFResource bucket, APELotGroup lotGroup, FWSetupInfo setupInfo, PBFAllocateContext context)
        {

        }

        [FEAction]
        public virtual void OnPM(PBFResource bucket, ATPMPeriod pm)
        {

        }
        
        [FEAction]
        public virtual bool OnReservationLot(IAPELot lot, APELotGroup lotGroup, PBFResource bucket, PBFAllocateContext context, PlanInfoType type)
        {
            return false;
        }

        [FEAction]
        public virtual void OnAllocated(PBFResource bucket, IAPELot lot, APEPlanInfo currentPlanInfo, PBFAllocateContext context)
        {

        }

        [FEAction]
        public virtual APELotGroup GetLoadableLots(PBFResource bucket, APELotGroup lotgroup, PBFAllocateContext context)
        {
            return lotgroup;
        }

        [FEAction]
        public virtual List<APELot> OnInterceptOnExit(APELotGroup lotGroup, PBFAllocateContext context)
        {
            return new List<APELot>();
        }

        [FEAction]
        public virtual void OnEndLevel(int curLevel, int maxLevel, PBFAllocateContext context)
        {

        }

        //[FEAction]
        public virtual double AvailableAllocateQty(PBFResource bucket, APELotGroup lotgroup, double allocQty, PBFAllocateContext context)
        {
            double canAllocQty = allocQty;

            // Setup을 고려한 가능 수량 계산?

            return canAllocQty;
        }
    }
}
