using Mozart.SeePlan.Aleatorik.Data;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    [FEComponent(FECategory.PlanByForward, FEControl.PBFInitialize, Root = FEProvider.Aleatorik)]
    public class FWInitControl : IModelController
    {
        /// <summary>CbsInitiator Instance object. It is used when FEComponent needs to be called directly. </summary>
        #region IModelController 멤버
        Type IModelController.ControllerType
        {
            get { return typeof(FWInitControl); }
        }
        #endregion

        [FEAction]
        public virtual List<ATOperTarget> PrepareInTarget(PBFModuleExecutionInfo info, List<ModuleExecutionInfo> refModules)
        {
            return null;
        }

        [FEAction]
        public virtual List<APEWip> PrepareWip(PBFModuleExecutionInfo info, List<ModuleExecutionInfo> refModules)
        {
            return null;
        }

        [FEAction]
        public virtual Dictionary<IComparable, List<ATOperTarget>> PrepareOperTarget(PBFModuleExecutionInfo info, List<ModuleExecutionInfo> refModules)
        {
            var handled = false;
            return PBFPredefines.Instance.PREPARE_OPER_TARGET_DEF(info, refModules, ref handled, null);
        }

        [FEAction]
        public virtual List<APEWip> PrepareIncomingWip(PBFModuleExecutionInfo info, List<ModuleExecutionInfo> refModules)
        {
            var handled = false;
            return PBFPredefines.Instance.PREPARE_INCOMING_WIPS_DEF(info, refModules, ref handled, null);
        }

        [FEAction]
        public virtual void OnPrepareCustomInput(PBFModuleExecutionInfo curExecuteInfo, List<ModuleExecutionInfo> refModules)
        {

        }
    }
}
