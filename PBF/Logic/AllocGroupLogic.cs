using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public partial class AllocGroupLogic
    {
        public static AllocGroupLogic Instance
        {
            get
            {
                return ServiceLocator.Resolve<AllocGroupLogic>();
            }
        }
    }
}
