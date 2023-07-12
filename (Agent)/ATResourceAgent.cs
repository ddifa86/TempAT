using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class ATResourceAgent : IAgent
    {
        public static ATResourceAgent Instance
        {
            get
            {
                return ServiceLocator.Resolve<ATResourceAgent>();
            }
        }

        public void Initialize()
        {
        }

        public void Dispose()
        {
            var resources = ATExecutionContext.Instance.CurrentExecutionInfo.Stage.Resources;
            resources.ForEach(x => x.Bucket = null);
        }
    }
}
