using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public class CompareLotGroupOnLFSMethod
    {
        /// <summary>
        /// 먼저 도착한 재공 우선
        /// </summary>
        /// <param name="lotGroup"></param>
        /// <param name="factor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareLotGroupOnLFS))]
        public ATFactorValue EarlierAvailTimeLotGrpFirst(APELotGroup lotGroup, ATWeightFactor factor, PBFAllocateContext context)
        {
            // 할당 가능한 랏그룹이 존재하는 경우.
            double remain = (lotGroup.LastStepTime - context.NowDt).TotalHours;

            return new ATFactorValue(remain, string.Format("LastStepTime : {0}, Now : {1}", lotGroup.LastStepTime.ToString("yyyy-MM-dd HH:mm:ss"), context.NowDt.ToString("yyyy-MM-dd")), "h");
        }

        /// <summary>
        /// 타겟의 Extended Target Date가 빠른 재공 그룹 우선
        /// </summary>
        /// <param name="lotGroup"></param>
        /// <param name="factor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareLotGroupOnLFS))]
        public ATFactorValue CloserExTargetDateLotGrpFirst(APELotGroup lotGroup, ATWeightFactor factor, PBFAllocateContext context)
        {
            // 할당 가능한 랏그룹이 존재하는 경우.
            var sample = (lotGroup.Sample as APELot);

            var lateTargetDate = (sample.CurrentTarget.TargetDateTime.AddDays(sample.CurrentTarget.SODemand.MaxLateDays) - context.NowDt).TotalDays;

            return new ATFactorValue(lateTargetDate, string.Format("LateTargetDate : {0}({1})", 
                sample.CurrentTarget.TargetDateTime.AddDays(sample.CurrentTarget.SODemand.MaxLateDays).ToString("yyyy-MM-dd"), lateTargetDate), "d");
        }

        /// <summary>
        /// 타겟의 SO Priority가 높은 (값이 작은) 재공 그룹 우선
        /// </summary>
        /// <param name="lotGroup"></param>
        /// <param name="factor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareLotGroupOnLFS))]
        public ATFactorValue HigherSoPriorityLotGrpFirst(APELotGroup lotGroup, ATWeightFactor factor, PBFAllocateContext context)
        {
            // 할당 가능한 랏그룹이 존재하는 경우.
            var sample = (lotGroup.Sample as APELot);

            return new ATFactorValue(sample.CurrentTarget.SODemand.Priority, string.Format("OrderPriority : {0}", sample.CurrentTarget.SODemand.Priority));
        }

        /// <summary>
        /// 타겟 일자가 빠른 재공 그룹 우선
        /// </summary>
        /// <param name="lotGroup"></param>
        /// <param name="factor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareLotGroupOnLFS))]
        public ATFactorValue CloserTargetDateLotGrpFirst(APELotGroup lotGroup, ATWeightFactor factor, PBFAllocateContext context)
        {
            // 할당 가능한 랏그룹이 존재하는 경우.
            var sample = (lotGroup.Sample as APELot);

            var latargetDate = (sample.CurrentTarget.TargetDateTime - FWFactory.Instance.NowDT).TotalDays;

            return new ATFactorValue(latargetDate, string.Format("TargetDate : {0}({1})", sample.CurrentTarget.TargetDateTime.ToString("yyyy-MM-dd"), latargetDate), "d");
        }
    }
}
