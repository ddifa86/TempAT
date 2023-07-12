using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    [FEComponent(FECategory.PlanByForward, FEControl.Factory, Root = FEProvider.Aleatorik)]
    public class FWFactoryControl : FactoryControl, IModelController
    {
        #region IModelController 멤버
        Type IModelController.ControllerType
        {
            get { return typeof(FWFactoryControl); }
        }
        #endregion

        #region Factory Control

        [FEAction]
        public override void OnCreateLot(APELot lot, APELot orgLot, LotCreateType type)
        {
        }

        [FEAction]
        public override bool CanReleaseLot(APELot lot)
        {
            DateTime now = FWFactory.Instance.NextStartTime;

            if (lot.IsWipLot)
            {
                if (ATOption.Instance.ApplyWipReleaseOnAvailableTime)
                    return true;

                lot.LastStepTime = FWFactory.Instance.NowDT;
            }

            if (now <= lot.CurrentTarget.MinEarlyTargetDateTime)
                return false;

            return true;
        }

        [FEAction]
        public override void OnReleasedLot(APELot lot)
        {
        }
 
        [FEAction]
        public override void OnStepIn(APELot lot)
        {
        }

        [FEAction]
        public override bool IsDummyOperation(APELot lot)
        {
            return false;
        }

        [FEAction]
        public override List<IAPEQueueManager> FilterLoadableQueueList(APELot lot, List<IAPEQueueManager> queues)
        {
            return queues;
        }

        [FEAction]
        public virtual double GetTat(APELot lot, bool isRun)
        {
            return lot.CurrentOper.GetTat(lot.LastStepTime, isRun);
        }

        [FEAction]
        public virtual void OnSplitLot(IAPELot lot, APELot splitLot)
        {
            
        }

        [FEAction]
        public override void OnStepOut(APELot lot)
        {
        }

        [FEAction]
        public override bool IsShortLot(APELot lot)
        {
            var handled = false;
            return PBFPredefines.Instance.IS_SHORT_LOG_PBF_DEF(lot, ref handled, false);
        }
        #endregion

        [FEAction]
        public virtual void OnStartCycle(DateTime now, DateTime nextTime)
        {
        }

        [FEAction]
        public virtual void OnEndCycle(DateTime now, DateTime nextTime)
        {
        }
    }
}
