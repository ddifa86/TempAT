using Mozart.SeePlan.Aleatorik.Data;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class ATISBQueueAgent : IAgent
    {
        Dictionary<string, ATISBQueue> _queue = new Dictionary<string, ATISBQueue>();

        public static ATISBQueueAgent Instance
        {
            get
            {
                return ServiceLocator.Resolve<ATISBQueueAgent>();
            }
        }


        internal void AddPlanWipQueue(ATISBQueue queue)
        {
            if (this._queue.ContainsKey(queue.ItemSiteBuffer.Key) == false)
            {
                this._queue.Add(queue.ItemSiteBuffer.Key, queue);
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
