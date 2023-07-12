using Mozart.SeePlan.Aleatorik.Planner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class ATReserveManager
    {
        internal List<APELotGroup> LotQueue { get; set; }

        internal FWSetupInfo SetupInfo { get; set; }

        internal APENonWorkingPeriod PmInfo { get; set; }

        internal double MainUtilizationRate { get; set; }

        internal int Level { get; set; }

        internal PBFAllocationLog AllocationLog { get; set; }

        internal Dictionary<IBucket, double> AddUtilizationRate { get; set; }

        internal List<ATOperResource> AddArrangeInfos { get; set; }

        public bool IsReserved {
            get
            {
                return this.LotQueue.Count() > 0;
            }
        }


        //public void Enqueue(IAPELot lot, bool addFirst, PBFAllocateContext context)
        //{
        //    var lotGroupKey = lot is APELot ? (lot as APELot).LotID : lot.LotGroupKey;

        //    foreach (var queue in LotQueue)
        //    {
        //        if (queue.LotGroupKey == lotGroupKey)
        //            return;
        //    }

        //    this.AddArrangeInfos = context.AddArrInfos;
        //    this.MainUtilizationRate = context.MainUtilizationRate;
        //    this.AddUtilizationRate = context.AddUtilizationRate;
        //    this.Level = context.Level;
        //    this.AllocationLog = context.AllocationLog;

        //    FWLotGroup lotGroup;
        //    if (lot is APELot)
        //    {
        //        //lot예약이면 lotGroup에서 해당 lot을 빼서 예약
        //        APEWipAgent.Instance.RemoveLot(lot as APELot, true);

        //        string lotId = (lot as APELot).LotID;
        //        lotGroup = new FWLotGroup(lotId);
        //        //lot.LotGroupKey = lotId;
        //        lotGroup.AddLot(lot);
        //    }
        //    else
        //    {
        //        APEWipAgent.Instance.RemoveLotGroup(lot as FWLotGroup);
        //        lotGroup = lot as FWLotGroup;
        //    }

        //    if (addFirst)
        //        LotQueue.Insert(0, lotGroup);
        //    else
        //        LotQueue.Add(lotGroup);

        //    if (AddArrangeInfos != null && AddArrangeInfos.Count > 0)
        //    {
        //        foreach (var addArr in AddArrangeInfos)
        //        {
        //            var bucket = addArr.Bucket as FWBucket;

        //            if (addFirst)
        //                bucket.ReserveManager.LotQueue.Insert(0, lotGroup);
        //            else
        //                bucket.ReserveManager.LotQueue.Add(lotGroup);
        //        }
        //    }
        //}

        //public IAPELot Dequeue()
        //{
        //    IAPELot lot = LotQueue.FirstOrDefault();

        //    if (lot == null)
        //        return null;

        //    LotQueue.RemoveAt(0);

        //    if (AddArrangeInfos != null && AddArrangeInfos.Count > 0)
        //    {
        //        foreach (var addArr in AddArrangeInfos)
        //        {
        //            var bucket = addArr.Bucket as FWBucket;

        //            bucket.ReserveManager.LotQueue.RemoveAt(0);
        //        }
        //    }

        //    return lot;
        //}

        //public void ClearReserveInfo()
        //{
        //    this.SetupInfo = null;
        //    this.LotQueue.Clear();
        //    this.PmInfo = APENonWorkingPeriod.NULL;
        //}

        //public void AddSetupInfo(FWSetupInfo setupInfo)
        //{
        //    this.SetupInfo = setupInfo;
        //}

        //public void AddPmInfo(APENonWorkingPeriod pmInfo) 
        //{
        //    this.PmInfo = pmInfo;
        //}



        //public void Reserve(FWLotGroup lotGroup, FWSetupInfo setupInfo, PBFAllocateContext context)
        //{
        //    ATBucketReserveInfo reserveInfo = new ATBucketReserveInfo(lotGroup, setupInfo, context);

        //    this.ReserveInfo = reserveInfo;
        //}
    }
}
