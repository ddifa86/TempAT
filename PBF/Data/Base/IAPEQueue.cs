using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public interface IAPEQueueManager
    {
        string QueueName { get; }
        FWWipQueue Queue { get; }
    }
}
