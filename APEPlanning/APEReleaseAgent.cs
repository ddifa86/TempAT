using Mozart.SeePlan.Aleatorik.Data;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class APEReleaseAgent : IAgent
    {
        public static APEReleaseAgent Instance
        {
            get { return ServiceLocator.Resolve<APEReleaseAgent>(); }
        }
         
        public HashedSet<APELot> ReleaseLots { get; private set; }

        internal FactoryControl Control;

        public APEReleaseAgent()
        {
        }

        #region IAgent Interface 구현
        public void Initialize()
        {
            ReleaseLots = new HashedSet<APELot>();

            var current = ATExecutionContext.Instance.CurrentExecutionInfo.ModuleType;
            if (current == ModuleType.PBO)
            {
                Control = new FactoryControl();
            }
            else if (current == ModuleType.PBF)
            {
                Control = Planner.FWInterface.LotControl;
            }
        }

        public void Dispose()
        {
            // 잔여 Lot의 정보는 출력작업.
            ReleaseLots = null;

            Control = null;
        }
        #endregion

        public void DoReleaseLot(APELine line)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("Lot_Released");
            try
            {
                var releaseLots = this.ReleaseLots.ToList();

                foreach (var lot in releaseLots)
                {
                    if (lot.Qty <= ATOption.Instance.MinimumAllocationQuantity)
                    {
                        this.ReleaseLots.Remove(lot);
                        continue;
                    }
                    
                    //sh wip인경우, canReleaseLot의 의미가 없이 Release가 되고있음,
                    //(PlanStart시간을 고려하지 못하고 Release가 됨, Resource Rolling이 되지 않기때문에 Dummy공정만 진행하지만
                    //Intarget은 Release가 되지않고 Wip은 Release가 되는것은 문제임)
                    if (Control.CanReleaseLot(lot) == false)
                    {
                        continue;
                    }

                    if (lot.IsWipLot == false)
                        ATExecutionContext.Instance.CurrentExecutionInfo.Kpi?.AddStageInQty(lot.CurrentQty, lot.LastStepTime);

                    this.ReleaseLots.Remove(lot);

                    line.MoveFirst(lot);
                }
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("Lot_Released");
            }
        }
         
    }
}
