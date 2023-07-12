using Mozart.SeePlan.Aleatorik.Data;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Pegger
{
    [FEComponent(FECategory.PlanByBackward, FEControl.PBBModule, Root = FEProvider.Aleatorik)]
    public partial class PBBModuleControl : IModelController
    {
        #region IModelController 멤버
        Type IModelController.ControllerType
        {
            get { return typeof(PBBModuleControl); }
        }

        #endregion


        [FEAction]
        public virtual void OnStartModule(ModuleExecutionInfo executeInfo)
        {
            //executeInfo.exe
        }

        [FEAction]
        public virtual void OnCreatePlanWip(APEWip planWip)
        {

        }

        [FEAction]
        public virtual void OnEndModule(ModuleExecutionInfo executeInfo)
        {

        }

        [FEAction]
        public virtual void OnStartPhase(Data.BWPhaseInfo phaseInfo)
        {
            //executeInfo.exe
        }

        [FEAction]
        public virtual bool IsRetryInPhase(List<APEPegPart> pegParts, bool isRetry, APEShortManager shortManager, int retryCount)
        {
            return isRetry;
        }

        [FEAction]
        public virtual void OnEndPhase(Data.BWPhaseInfo phaseInfo)
        {
            //executeInfo.exe
        }
    }
}
