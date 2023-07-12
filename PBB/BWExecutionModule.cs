using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Pegger
{
    public class BWExecutionModule : ExecutionModule
    {
        public override string Name => ModuleType.PBB.ToString(); 

        protected override Type ExecutorType => typeof(BWExecutor);

        protected override void Configure()
        {
        }
    }
}
