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
    public class AssemblyControl : IModelController
    {
        #region IModelController 멤버
        Type IModelController.ControllerType
        {
            get { return typeof(AssemblyControl); }
        }
        #endregion

        [FEAction]
        public virtual bool CanAssembleSmallBatch(APELot assemblyLot, double qty, DateTime now) 
        {
            return true;
        }
    }
}
