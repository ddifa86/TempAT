using Mozart.SeePlan.Aleatorik.Planner;
using Mozart.SeePlan.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik._Predefines_.Planning
{
    public class CompareAddResourceMethod
    {
        /// <summary>
        /// 잔여 Capacity가 많은 Add Resource 우선
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="addBucket"></param>
        /// <param name="lotGroup"></param>
        /// <param name="factor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareAddResource))]
        public ATFactorValue LargerRemainCapaAddResFirst(PBFResource bucket, PBFResource addBucket, APELotGroup lotGroup, ATWeightFactor factor, PBFAllocateContext context)
        {
            double remain = addBucket.CurrentCapaInfo.Remain;
            return new ATFactorValue(remain * -1, string.Format("Remain Capa : {0}", remain));
        }


        [FEActionFactor( RuleID = "test",  RulePoint = typeof(WireteResourcePlan))]
        public Entity WriteResPlan(RES_PLAN entity, ...)
        {
            return null;
        }

        [FEActionFactor(RuleID = "test", RulePoint = typeof(WireteTargetPlan))]
        public Entity test(TARGET_PLAN entity, ...)
        {
            return null;
        }

        /// <summary>
        /// 재공과 메인 설비의 작업시작을 덜 지연시키는 Add Resource 우선 (Capacity Type = Time)
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="addBucket"></param>
        /// <param name="lotGroup"></param>
        /// <param name="factor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareAddResource))]
        public ATFactorValue MinGapTimeAddResFirst(PBFResource bucket, PBFResource addBucket, APELotGroup lotGroup, ATWeightFactor factor, PBFAllocateContext context)
        {
            if (bucket.IsTimeCapa)
            {
                var addAvailableTime = addBucket.CurrentTime;
                var usableTime = ATUtil.MaxTime(lotGroup.Sample.LastStepTime, bucket.CurrentTime);

                double timeGap = (addAvailableTime - usableTime).TotalSeconds;

                if (timeGap <= ATOption.Instance.MinimumAllocationQuantity)
                    return new ATFactorValue(0, "Same add bucket");
                else
                    return new ATFactorValue(timeGap, "");
            }
            else
            {
                return new ATFactorValue(0, "Is not time resource");
            }
        }

        /// <summary>
        /// Main Resource가 사용하던 Add Resource를 우선 선택 (Main Resource or Add Resource의 할당 이력이 없는 경우 모두 같은 우선순위 적용)
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="addBucket"></param>
        /// <param name="lotGroup"></param>
        /// <param name="factor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareAddResource))]
        public ATFactorValue SameAddResFirst(PBFResource bucket, PBFResource addBucket, APELotGroup lotGroup, ATWeightFactor factor, PBFAllocateContext context)
        {
            var lastPlan = bucket.LastPlan;
            if (lastPlan == null)
            {
                return new ATFactorValue(0, "Last plan is null");
            }
            else
            {
                var addLastPlan = addBucket.LastPlan;
                if (addLastPlan == null)
                    return new ATFactorValue(1, "Other add bucket");

                if (lastPlan.AllocationInfo.AllocSeq == addLastPlan.AllocationInfo.AllocSeq)
                    return new ATFactorValue(0, "Same add bucket");

                return new ATFactorValue(1, "Other add bucket");
            }

            //else if (lastPlan.AddPlanInfos == null)
            //{
            //    return new ATFactorValue(0, "There is no add bucket history in last plan");
            //}
            //else
            //{
            //    if (lastPlan.AddPlanInfos.ContainsKey(addBucket))
            //        return new ATFactorValue(0, "Same add bucket");
            //    else
            //        return new ATFactorValue(1, "Other add bucket");
            //}
        }
    }
}
