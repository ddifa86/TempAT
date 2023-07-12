using Mozart.SeePlan.Aleatorik.Data;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Pegger
{
    [FEComponent(FECategory.PlanByBackward, FEControl.PBBInitialize, Root = FEProvider.Aleatorik)]
    public partial class PBBInitControl : IModelController
    {
        #region IModelController 멤버
        Type IModelController.ControllerType
        {
            get { return typeof(PBBInitControl); }
        }

        #endregion

        [FEAction]
        public virtual List<ATDemand> PrepareDemand(PBBModuleExecutionInfo curExecuteInfo, List<ModuleExecutionInfo> refModules)
        {
            var handled = false;
            return PBBPredefines.Instance.PREPARE_DEMAND_PBB_DEF(curExecuteInfo, refModules, ref handled, null);
        }

        [FEAction]
        public virtual void OnPrepareCustomInput(PBBModuleExecutionInfo curExecuteInfo, List<ModuleExecutionInfo> refModules)
        {

        }

        [FEAction]
        public virtual List<ATDemand> DemandSmoothing(List<ATDemand> demands)
        {
            return demands;
        }
    }
}
