using Mozart.SeePlan.Aleatorik.Data;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class APEWipQueueAgent : IAgent
    {
        public static APEWipQueueAgent Instance
        {
            get
            {
                return ServiceLocator.Resolve<APEWipQueueAgent>();
            }
        }

        private Dictionary<ATOperation, APEWipQueue> _queue = new Dictionary<ATOperation, APEWipQueue>();

        internal void AddPlanWipQueue(APEWipQueue queue)
        {
            if (this._queue.ContainsKey(queue.Key) == false)
            {
                this._queue.Add(queue.Key, queue);
            }
        }

        public void Dispose()
        {
            foreach (var queue in _queue.Values)
            {
                queue.Dispose();
            }
        }

        public void Initialize()
        {
            foreach (var queue in _queue.Values)
            {
                queue.Initialize();
            }
        }
    }
}
