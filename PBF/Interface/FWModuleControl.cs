using Mozart.Simulation.Engine;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    [FEComponent(FECategory.PlanByForward, FEControl.PBFModule, Root = FEProvider.Aleatorik)]
    public class FWModuleControl : IModelController
    {
        Type IModelController.ControllerType => typeof(FWModuleControl);


        [FEAction]
        public virtual void OnStartModule(ModuleExecutionInfo executeInfo)
        {
        }

        [FEAction]
        public virtual void OnCreateAllocationGroup(List<FWAllocGroup> allocationGroup)
        {
        }

        [FEAction]
        public virtual void OnStartPhase(Data.FWPhaseInfo phaseInfo)
        {

        }

        [FEAction]
        public virtual void OnEndPhase(Data.FWPhaseInfo phaseInfo)
        {

        }

        [FEAction]
        public virtual void OnEndModule(ModuleExecutionInfo executeInfo)
        {
        }


        #region Simulation
        public virtual int CompareSameTimeEvent(Event x, Event y)
        {
            return 0;
        }
        #endregion
    }
}
