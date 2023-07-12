using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Logic;
using Mozart.SeePlan.Aleatorik.ObyO.Data;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.ObyO
{
    [FEComponent(FECategory.PlanByOrder, FEControl.PBOPlanner, Root = FEProvider.Aleatorik)]
    public partial class PBOPlanControl : FactoryControl, IModelController
    {
        Type IModelController.ControllerType
        {
            get { return typeof(PBOPlanControl); }
        }

        [FEAction]
        public override void OnReleasedLot(APELot lot)
        {

        }

        [FEAction]
        public override void OnStepIn(APELot lot)
        {
            
        }

        [FEAction]
        public override bool IsDummyOperation(APELot lot)
        {
            return false;
        }

        [FEAction]
        public override void OnStepOut(APELot lot)
        {
            
        }

        [FEAction]
        public override bool IsShortLot(APELot lot)
        {
            var handled = false;
            return PBOPredefines.Instance.IS_SHORT_LOG_PBO_DEF(lot, ref handled, false);
        }

        [FEAction]
        public override void OnShortLot(APELot lot)
        {

        }
    }
}
