using Mozart.SeePlan.Aleatorik.Data;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;

namespace Mozart.SeePlan.Aleatorik.ObyO
{
    public partial class OboLogic
    {
        public static OboLogic Instance
        {
            get
            {
                return ServiceLocator.Resolve<OboLogic>();
            }
        }

        /// <summary>
        /// Demand Smoothing 작업 대비
        /// </summary>
        /// <param name="demands"></param>
        /// <returns></returns>
        /// [FEAction]
        public virtual List<ATDemand> DemandSmoothing(List<ATDemand> demands)
        {
            return demands;
        }

        /// <summary>
        /// 현재 확정계획 롤백의 경우 Confirm이 되는 경우에만 확정을 하고, Short인 경우 Rollback은 Phase 종료시점에 일괄적으로 진행 LWS
        /// 추후 수정 필요
        /// </summary>
        /// <param name="lot"></param>
        public void CommitRefPlan(APELot lot)
        {
            var lastOperTarget = lot.LastOperTarget;

            foreach (var refPlan in lot.RefPlans)
            {
                var orgRefPlan = refPlan.OrgRefPlan;
                if (orgRefPlan == null)
                    continue;

                var refOperTarget = refPlan.RefTarget;
                double qty = lot.Qty * refOperTarget.GetBCumChangeRatio(lastOperTarget);
                orgRefPlan.VirtualUsedQty -= qty;

                if (orgRefPlan.VirtualUsedQty <= ATOption.Instance.MinimumAllocationQuantity)
                    orgRefPlan.VirtualUsedQty = 0;

                orgRefPlan.UsedQty += qty;
            }
        }

        internal bool IsRetryInPhase(Dictionary<IComparable, APETargetGroup> targetGroups, ITargetGroup pegPart, APEShortManager shortManager, int retryCount)
        {
            bool isRetry = false;

            if (retryCount < ATOption.Instance.RetryCount)
                isRetry = true;

            isRetry = PBOInterface.ModuleControl.IsRetryInPhase(targetGroups, pegPart, isRetry, shortManager, retryCount);

            return isRetry;
        }

        
    }
}
