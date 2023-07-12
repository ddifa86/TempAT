
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik
{
    [FeatureBind()]
    public class CompareTargetInGroupMethod
    {
        /// <summary>
        /// Target date 가 포함된 주차가 빠른 순으로 먼저 패깅하기 위한 Factor
        /// </summary>
        /// <param name="target"></param>
        /// <param name="factor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareTargetInGroup))]
        public ATFactorValue EarlierTargetWeekTargetFirst(APETarget target, ATWeightFactor factor, APEPegContext context)
        {
            return new ATFactorValue(target.DueDate.WeekOfYear(), string.Format("TargetDate : {0}", target.DueDate.ToString("yyyy-MM-dd HH:mm:ss")));
        }

        /// <summary>
        /// Target Date(납기)가 빠른 Target 을 먼저 패깅하기 위한 Factor
        /// </summary>
        /// <param name="pegTarget"></param>
        /// <param name="factor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareTargetInGroup))]
        public ATFactorValue EarlierTargetDateTargetFirst(APETarget pegTarget, ATWeightFactor factor, APEPegContext context)
        {
            return new ATFactorValue(pegTarget.DueDate.ToOADate(), string.Format("TargetDate : {0}", pegTarget.DueDate.ToString("yyyy-MM-dd HH:mm:ss")));
        }

        /// <summary>
        /// Target 의 SO Demand Priority 가 높은(숫자가 낮은) Target 을 먼저 패깅하기 위한 Factor 
        /// </summary>
        /// <param name="pegTarget"></param>
        /// <param name="factor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareTargetInGroup))]
        public ATFactorValue HigherPriorityTargetFirst(APETarget pegTarget, ATWeightFactor factor, APEPegContext context)
        {
            double priority = pegTarget.SoDemand.Priority;
            return new ATFactorValue(priority, null);
        }

        /// <summary>
        /// Target 의 잔여목표량이 많은 경우 우선순위를 주기 위한 factor
        /// </summary>
        /// <param name="pegTarget"></param>
        /// <param name="factor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareTargetInGroup))]
        public ATFactorValue LargerQtyTargetFirst(APETarget pegTarget, ATWeightFactor factor, APEPegContext context)
        {
            return new ATFactorValue(pegTarget.RemainQty * -1, string.Format("RemainQty : {0}", pegTarget.RemainQty));
        }
    }
}
