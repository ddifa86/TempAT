using Mozart.SeePlan.Aleatorik.Data;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.ObyO
{
    public partial class PBOPlanControl : IModelController
    {

        [FEAction]
        public virtual void OnArriveInAssemblyStep(APELot lot, ATAssyInfo partInfo)
        {
        }

        //  [FEAction(DependentTo = "DoAssembly", DependentType = FEDependencyType.Inclusive)]
        [FEAction]
        public virtual double AdjustAssemblyQty(APELot lot, double canAssemblyQty)
        {
            return canAssemblyQty;
        }

        [FEAction]
        public virtual void OnCompleteAssembled(APELot lot, ATAssemblyInfo assyInfo)
        {
        }
        
    }
}
