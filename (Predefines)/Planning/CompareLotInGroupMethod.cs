using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public class CompareLotInGroupMethod
    {
        /// <summary>
        /// 재공 도착 시간이 엔진 현재 시각보다 과거나 현재면 0, 그렇지 않으면 1 (오름차순)
        /// </summary>
        /// <param name="lot"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareLotInGroup))]
        public ATFactorValue AlreadyArrivedLotFirst(IAPELot lot, ATWeightFactor factor)
        {
            var now = FWExecutor.Current.NowDT;
            // 할당 가능한 랏그룹이 존재하는 경우.
            double remain = lot.LastStepTime <= now ? 0 : 1;

            return new ATFactorValue(remain, string.Format("LastStepTime : {0}, Now : {1}", lot.LastStepTime.ToString("yyyy-MM-dd HH:mm:ss"), now.ToString("yyyy-MM-dd HH:mm:ss")));
        }

        /// <summary>
        /// 타겟의 SO Priority가 높은 (값이 작은) 재공 우선
        /// </summary>
        /// <param name="lot"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareLotInGroup))]
        public ATFactorValue HigherSoPriorityLotFirst(IAPELot lot, ATWeightFactor factor)
        {
            APELot alot = lot as APELot;

            return new ATFactorValue(alot.CurrentTarget.SODemand.Priority, string.Format("OrderPriority : {0}", alot.CurrentTarget.SODemand.Priority));
        }

        /// <summary>
        /// 타겟 일자가 빠른 재공 우선
        /// </summary>
        /// <param name="lot"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareLotInGroup))]
        public ATFactorValue CloserTargetDateLotFirst(IAPELot lot, ATWeightFactor factor)
        {
            APELot alot = lot as APELot;

            var latargetDate = (alot.CurrentTarget.TargetDateTime - FWFactory.Instance.NowDT).TotalDays;

            return new ATFactorValue(latargetDate, string.Format("TargetDate :{0}({0})", alot.CurrentTarget.TargetDateTime.ToString("yyyy-MM-dd"), latargetDate), "d");
        }

        /// <summary>
        /// 타겟의 Extended Target Date가 빠른 재공 우선
        /// </summary>
        /// <param name="lot"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareLotInGroup))]
        public ATFactorValue CloserExTargetDateLotFirst(IAPELot lot, ATWeightFactor factor)
        {
            APELot alot = lot as APELot;

            var lateTargetDate = (alot.CurrentTarget.TargetDateTime.AddDays(alot.CurrentTarget.SODemand.MaxLateDays) - FWFactory.Instance.NowDT).TotalDays;
            return new ATFactorValue(lateTargetDate, string.Format("LateTargetDate : {0}({0})"
                , alot.CurrentTarget.TargetDateTime.AddDays(alot.CurrentTarget.SODemand.MaxLateDays).ToString("yyyy-MM-dd HH:mm:ss"), lateTargetDate), "d");
        }
    }
}
