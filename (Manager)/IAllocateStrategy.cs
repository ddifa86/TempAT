using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Planner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Manager
{
    public interface IAllocateStrategy
    {
        //bool CanAllocateLot(FWBucket bucket, DateTime loadableTime);

        void ProcessRemainLot(PBFResource bucket, IAPELot lot, APELotGroup lotGroup, PBFAllocateContext context);
    }

    public class TimeStrategy : IAllocateStrategy
    {
        // 없앰
        //public bool CanAllocateLot(FWBucket bucket, DateTime loadableTime)
        //{
        //    // 수량제약 (MBS(GetAllocableQty) or Constraint)에 의해 전량 할당하지 못했으면 Allocate 가능
        //    // 다음날로 연속되는 경우 예약하여 Allocate 가능
        //    // 아닌 경우 break
        //    var nonWorkTime = bucket.GetNextNonWorkingTime(loadableTime, bucket.EndTime, LotSplitOption.Split);

        //    if (nonWorkTime != bucket.EndTime)
        //        return false;

        //    return true;
        //}

        public void ProcessRemainLot(PBFResource bucket, IAPELot lot, APELotGroup lotGroup, PBFAllocateContext context)
        {
            // 아래와 통합 가능?
            if (context.IsQuantityConstraint)
                bucket.AllocateManager.SplitLot(lot, context);
            else
                bucket.ReserveManager.ReserveLot(lot, false, context.AddArrInfos, null);
        }
    }

    public class NoTimeStrategy : IAllocateStrategy
    {
        //public bool CanAllocateLot(FWBucket bucket, DateTime loadableTime)
        //{
        //    return true;
        //}

        public void ProcessRemainLot(PBFResource bucket, IAPELot lot, APELotGroup lotGroup, PBFAllocateContext context)
        {
            bucket.AllocateManager.SplitLot(lot, context);
        }
    }
}
