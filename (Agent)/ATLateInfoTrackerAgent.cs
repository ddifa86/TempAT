using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Planner;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class ATLateInfoTrackerAgent : IAgent
    {
        public static ATLateInfoTrackerAgent Instance
        { 
            get
            {
                return ServiceLocator.Resolve<ATLateInfoTrackerAgent>();
            }
        }

        public void Dispose()
        {
        }

        public void Initialize()
        {
            var demands = ATInputData.Demands.GetDemands();
            foreach (var demand in demands)
            {
                demand.LateInfoManager = new ATLateInfoManager(demand);
                //demand.LateInfoTracker.Clear();
            }
        }
    }
}
