using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.ObyO.Data;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.ObyO
{
    [FEComponent(FECategory.PlanByOrder, FEControl.PBOModule, Root = FEProvider.Aleatorik)]
    public class PBOModuleControl : IModelController
    {
        Type IModelController.ControllerType => typeof(PBOModuleControl);
  

        [FEAction]
        public virtual void OnStartModule(ModuleExecutionInfo executeInfo)
        { 
        }

        [FEAction]
        public virtual void OnCreatePlanWip(APEWip planWip)
        {
        }

        [FEAction]
        public virtual void OnStartPhase(OboPhaseInfo phaseInfo, PBOModuleExecutionInfo executeInfo)
        {
        }

        [FEAction]
        public virtual void OnSelectPegPartInPhase(ITargetGroup pegPart)
        {
        }

        [FEAction]
        public virtual bool IsRetryInPhase(Dictionary<IComparable, APETargetGroup> targetGroups, ITargetGroup pegPart, bool isRetry, APEShortManager shortManager, int retryCount)
        {
            return isRetry;
        }

        [FEAction]
        public virtual string GetRetryTargetGroupKey(Dictionary<IComparable, APETargetGroup> targetGroups, APEPegPart retryPegPart, string targetGroupKey, PBOModuleExecutionInfo executionInfo)
        {
            return targetGroupKey;
        }

        [FEAction]
        public virtual void OnEndPhase(OboPhaseInfo phaseInfo, PBOModuleExecutionInfo executeInfo)
        {
        }

        [FEAction]
        public virtual void OnEndModule(ModuleExecutionInfo executeInfo)
        {
        }
    }
}
