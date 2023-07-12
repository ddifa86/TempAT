using Mozart.SeePlan.Aleatorik.Data;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    [FEComponent(FECategory.PlanByForward, FEControl.Assembly, Root = FEProvider.Aleatorik)]
    public class FWAssemblyControl : IModelController
    {
        #region IModelController 멤버
        Type IModelController.ControllerType
        {
            get { return typeof(FWAssemblyControl); }
        }
        #endregion

        [FEAction]
        public virtual void OnPrepareAssembly(List<APELot> possibleAssemblyLots, string soDemandID)
        {
        }

        [FEAction]
        public virtual bool CanAssemblyPartLot(APELot part, APELot assemblyLot, DateTime now, Dictionary<ATBomDetail, List<APELot>> availablePartLot, KeyValuePair<ATBomDetail, List<APELot>> pairKey)
        {
            if (part.CurrentTarget.SODemand.ID != assemblyLot.CurrentTarget.SODemand.ID)
                return false;

            return true;
        }

        [FEAction]
        public virtual bool CanAssembleSmallBatch(APELot assemblyLot, double qty, DateTime now)
        {
            return true;
        }
    }
}
