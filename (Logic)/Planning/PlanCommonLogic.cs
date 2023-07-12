using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.ObyO;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public partial class PlanCommonLogic
    {
        public static PlanCommonLogic Instance
        {
            get
            {
                return ServiceLocator.Resolve<PlanCommonLogic>();
            }
        }

        public void ApplyNoCarryover(APEPlanInfo info, APEPlanInfo refInfo)
        {
            if (ATOption.Instance.DiscardCarryOver == false)
            {
                if (info == null || refInfo == null)
                    return;

                if (info.NoCarryOver == false && refInfo.NoCarryOver == false)
                    return;

                if (object.ReferenceEquals(info, refInfo))
                    return;

                if (info.NoCarryOver)
                {
                    if (info.Bucket == null || info.Bucket == ATDummyAgent.Instance.DummyBucket)
                    {
                        info.EndTime = refInfo.ArrivalTime;
                        info.AllocationInfo.BucketEndTime = refInfo.ArrivalTime;
                        info.StartTime = info.EndTime.AddSeconds(-info.AllocationInfo.Tat);
                    }
                    
                    info.ArrivalTime = info.StartTime;
                }
                else if (info.NoCarryOver == false && refInfo.NoCarryOver)
                {
                    // 임시 예외처리 : Buffer인 경우에만 업데이트.
                    // 장비로 인한 Carry 공정은 업데이트 하지 않음.
                    if (info.Bucket == null || info.Bucket == ATDummyAgent.Instance.DummyBucket)
                    {
                        info.EndTime = refInfo.ArrivalTime;
                        info.AllocationInfo.BucketEndTime = refInfo.ArrivalTime;
                    }
                }
            }
        }

        public virtual bool IsShortLot(APELot lot)
        {
            if (lot.IsShort)
                return true;

            if (lot.LastPlan == null)
                return false;

            var lastPlan = lot.LastPlan;
            var lastTarget = lastPlan.OperTarget;

            if (lot.LastPlan.StartTime > lastTarget.TargetDateTime && lot.LastPlan.StartTime <= lastTarget.MaxLateTargetDateTime)
            {
                if (lot.Status != Status.Late)
                {
                    lot.Status = Status.Late;
                    lot.UpdateLateInfos(Status.Late);
                }
            }
            else if (lot.LastPlan.StartTime > lot.SoDemand.CalcDueDateTime.AddDays(lot.SoDemand.MaxLateDays))
            {
                lot.IsShort = true;
                lot.ShortCategory = "FW Short";
                lot.ReasonName = "Delayed Target Date";

                lot.Status = Status.Short;
                lot.UpdateLateInfos(Status.Short);
                return true;
            }
            lot.ClearVirtualLateInfo();

            return false;
        }
    }
}
