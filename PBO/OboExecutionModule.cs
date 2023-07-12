using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.ObyO
{
    public class OboExecutionModule : ExecutionModule
    {
        public override string Name => ModuleType.PBO.ToString(); // "OrderBy";

        protected override Type ExecutorType => typeof(OboExecutor);

        protected override void Configure()
        {
        }
    }
}
