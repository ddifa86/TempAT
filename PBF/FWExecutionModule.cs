using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public class FWExecutionModule : ExecutionModule
    {
        //
        // 요약:
        //     Execution Module name.
        public override string Name  => ModuleType.PBF.ToString(); //"Backward";
        //
        // 요약:
        //     Execution priority of Execution Module.
        public int Priority { get; }
        protected override System.Type ExecutorType
        {
            get
            {
                return typeof(Mozart.SeePlan.Aleatorik.Planner.FWExecutor);
            }
        }

        protected override void Configure()
        {
        }
    }
}
