using Mozart.SeePlan.Aleatorik.Data;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.ObyO
{
    [FEComponent(FECategory.PlanByOrder, FEControl.PBOInitialize, Root = FEProvider.Aleatorik)]
    public class PBOInitControl : IModelController
    {
        Type IModelController.ControllerType => typeof(PBOInitControl);

        [FEAction]
        public virtual List<ATDemand> PrepareDemand(PBOModuleExecutionInfo curExecuteInfo, List<ModuleExecutionInfo> refModules)
        {
           return curExecuteInfo.Stage.Demands.Values.ToList();
 
        }

        [FEAction]
        public virtual void OnPrepareCustomInput(PBOModuleExecutionInfo curExecuteInfo, List<ModuleExecutionInfo> refModules)
        {

        }

        [FEAction]
        public virtual List<ATDemand> DemandSmoothing(List<ATDemand> demands)
        {
            return demands;
        }
    }
}
