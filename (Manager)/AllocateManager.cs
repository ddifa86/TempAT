using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Planner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Manager
{
    public class AllocateManager
    {
        private IAllocateStrategy _allocateStrategy { get; set; }

        public PBFResource Bucket { get; private set; }

        public AllocateManager(PBFResource bucket)
        {
            this.Bucket = bucket;

            if (bucket.IsTimeCapa)
                SetAllocateStrategy(new TimeStrategy());
            else
                SetAllocateStrategy(new NoTimeStrategy());
        }

        public void SetAllocateStrategy(IAllocateStrategy strategy)
        {
            this._allocateStrategy = strategy;
        }

        //public bool CanAllocateLot(IAPELot curLot, double lotAvailAllocQty, DateTime loadableTime)
        //{
        //    if (curLot.CurrentQty > lotAvailAllocQty)
        //        return this._allocateStrategy.CanAllocateLot(this.Bucket, loadableTime);

        //    return true;
        //}

        public bool ProcessRemainLot(IAPELot lot, APELotGroup lotGroup, PBFAllocateContext context)
        {
            if (FWInterface.AllocatorControl.OnReservationLot(lot, lotGroup, this.Bucket, context, PlanInfoType.Allocate))
                return false;
            
            if (lot.CurrentQty <= ATOption.Instance.MinimumAllocationQuantity)
                return true;

            this._allocateStrategy.ProcessRemainLot(this.Bucket, lot, lotGroup, context);
            return false;
        }

        public void SplitLot(IAPELot lot, PBFAllocateContext context)
        {
            var sampleLot = lot.Sample as APELot;

            string lotID = LotHelper.GeneratLotID(sampleLot.LotID, ATConstants.SPLIT_BATCH_PREFIX);

            APELot splitLot = LotHelper.GenerateSplitLot(sampleLot, lot.CurrentQty, lotID, false);
            if (splitLot != null)
            {
                LotInfo lotInfo = new LotInfo(splitLot);
                APEPlanInfo info = new APEPlanInfo(lotInfo, null);
                splitLot.CurrentPlanInfo = info;

                var so = sampleLot.CurrentTarget.SODemand;
                so.LateInfoManager.CopyLateInfos(sampleLot, splitLot);

                APEWipAgent.Instance.ReleasedLot(splitLot);
                APEWipAgent.Instance.AddLot(splitLot);

                FWInterface.LotControl.OnSplitLot(lot, splitLot);

                if (splitLot.LotGroupKey != lot.LotGroupKey)
                {
                    var lotGroup = this.Bucket.Queue.GetLotGroup(splitLot.LotGroupKey);
                    context.SelectedLotGroupInLevel.Add(lotGroup);
                }
            }
        }
    }
}
