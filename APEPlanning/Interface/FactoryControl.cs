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
    /// <summary>
    /// 
    /// </summary>
    public class FactoryControl  
    {
        
        public virtual void OnCreateLot(APELot lot, APELot orgLot, LotCreateType type)
        {
        }

        public virtual bool CanReleaseLot(APELot lot)
        {
            return false;
        }

        
        public virtual void OnReleasedLot(APELot lot)
        {
        }

        
        public virtual void OnStepIn(APELot lot)
        {

        }

        public virtual bool IsDummyOperation(APELot lot)
        {
            return false;
        }

        public virtual List<IAPEQueueManager> FilterLoadableQueueList(APELot lot, List<IAPEQueueManager> queues)
        {
            return queues;
        }

        public virtual void OnStepOut(APELot lot)
        {
        }

        public virtual bool IsShortLot(APELot lot)
        {
            return false;
        }

        public virtual void OnShortLot(APELot lot)
        {
        }
    }
}
