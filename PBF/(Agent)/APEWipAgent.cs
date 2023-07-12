using Mozart.SeePlan.Aleatorik.Data;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public class APEWipAgent : IAgent
    {
        public static APEWipAgent Instance
        {
            get { return ServiceLocator.Resolve<APEWipAgent>(); }
        }

        private HashSet<APELot> _remainLots;
       
        public Dictionary<IAPEQueueManager, FWWipQueue> WipQueues;

        public APEWipAgent()
        {
            this.WipQueues = new Dictionary<IAPEQueueManager, FWWipQueue>();
            this._remainLots = new HashSet<APELot>();
        }

        #region IAgent Interface 구현
        public void Initialize()
        {
            this.WipQueues = new Dictionary<IAPEQueueManager, FWWipQueue>();
            this._remainLots = new HashSet<APELot>();
        }

        public void Dispose()
        {
            this.WipQueues = null;
            this._remainLots = null;
        }
        #endregion

        /// <summary>
        /// WipQueue 등록 작업
        /// </summary>
        /// <param name="queue"></param>
        internal void RegistQueue(IAPEQueueManager queue)
        {
            this.WipQueues.Add(queue, queue.Queue);
        }

        public void ReleasedLot(APELot lot)
        { 
            OutputWriter.Instance.WriteLotHistory(lot as APELot, lot.Qty, LifeCycle.Release.ToString(), lot.LastStepTime, string.Empty);
        }

        public void AddRemainLot(APELot lot)
        {
            this._remainLots.Add(lot);
        }

        public HashSet<APELot> GetRemainLots()
        {
            return this._remainLots;
        }


        public bool AddLot(APELot lot)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("WipAgent_AddLot");
            try
            {
                if (lot.Qty <= ATOption.Instance.MinimumAllocationQuantity)
                {
                    lot.IsShort = true;
                    FWFactoryLogic.Instance.OnDone(lot, LifeCycle.Remain, "fail to add queue, min Qty");
                    return false;
                }

                string lotgroupkey = FWFactoryLogic.Instance.GetLotGroupKey(lot);
                if (string.IsNullOrEmpty(lotgroupkey))
                {
                    lot.Sample.LotGroupKey = string.Empty;
                    lot.IsShort = true;
                    FWFactoryLogic.Instance.OnDone(lot, LifeCycle.Remain, "fail to add queue, null group key");

                    string factorID = string.Join("@", FWFactory.Instance.DefaultLotGroupKeyPreset.FactorList.Select(x => x.FactorInfo.FactorID));
                    lot.AddShortInfo(LateCategory.Constraint, factorID, lot.Qty, null, lot.LastStepTime, lot.LastStepTime);

                    return false;
                }

                // GetLodableQueue
                var queues = lot.CurrentOper.GetWipQueues();
                queues = FWInterface.LotControl.FilterLoadableQueueList(lot, queues);
                if (queues.Count() == 0) 
                {
                    // Queues가 하나도 없으면 Short을 낼 지, Dummy로 흘려보낼지 bool Type의 FEAction도 고려되어야함
                    lot.IsShort = true;
                    FWFactoryLogic.Instance.OnDone(lot, LifeCycle.Remain, "fail to add queue, no queue");

                    lot.AddShortInfo(LateCategory.Capacity, LateReason.NoOpResourceInfo.ToString(), lot.Qty, null, lot.LastStepTime, lot.LastStepTime);
                    return false;
                }

                foreach (var queueManager in queues)
                {
                    queueManager.Queue.AddLot(lotgroupkey, lot);
                }

                lot.LotGroupKey = lotgroupkey;
                lot.CurrentPlanInfo.LotInfo.LotGroupKey = lotgroupkey;

                return true;
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("WipAgent_AddLot");
            }
        }

        //public void RemoveLotGroup(FWLotGroup group)
        //{
        //    foreach (var lot in group.GetLotList())
        //    {
        //        var queues = lot.CurrentOper.GetWipQueues();

        //        foreach (var queueManager in queues)
        //        {
        //            queueManager.Queue.RemoveLotGroup(group.LotGroupKey);
        //        }
        //    }
        //}

        public void RemoveLot(IAPELot lot, bool isRemoveNoContents = true)
        {
            if (lot is APELot)
            {
                if (string.IsNullOrEmpty(lot.LotGroupKey))
                    return;

                var qeueus = lot.CurrentOper.GetWipQueues();

                foreach (var queueManager in qeueus)
                {
                    queueManager.Queue.RemoveLot(lot.LotGroupKey, lot, isRemoveNoContents);
                }

                lot.LotGroupKey = string.Empty;
            }
            else
            {
                var group = lot as APELotGroup;
                var queues = group.CurrentOper.GetWipQueues();

                foreach (var queueManager in queues)
                {
                    queueManager.Queue.RemoveLotGroup(group.LotGroupKey);
                }
            }
        }
    }
}
