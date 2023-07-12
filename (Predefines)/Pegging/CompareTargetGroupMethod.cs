
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
    public class CompareTargetGroupMethod
    {
        /// <summary>
        /// Target 의 Week 가 빠른 PegPart 가 포함된 TargetGroup 우선 
        /// </summary>
        /// <param name="group"></param>
        /// <param name="factor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareTargetGroup))]
        public ATFactorValue EarlierTargetWeekTargetGrpFirst(APETargetGroup group, ATWeightFactor factor, APEPegContext context)
        {
            var pegTarget = group.Sample as APETarget;

            return new ATFactorValue(pegTarget.DueDate.WeekOfYear(), pegTarget.DueDate.ToString(ATUtil.DateTimeFormat));
        }

        /// <summary>
        /// TargetGroup의 FirstPegTarget의 Target Date 가 빠른 TargetGroup 우선
        /// </summary>
        /// <param name="group"></param>
        /// <param name="factor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareTargetGroup))]
        public ATFactorValue EarlierTargetDateTargetGroupFirst(APETargetGroup group, ATWeightFactor factor, APEPegContext context)
        {
            var pegTarget = group.Sample as APETarget;

            return new ATFactorValue(pegTarget.TargetDateTime.ToOADate(), pegTarget.TargetDateTime.ToString(ATUtil.DateTimeFormat));
        }

        /// <summary>
        /// 타겟 그룹 내 대표 타겟의 So Due Date + Max Lateness Days기준 빠른 납기 우선
        /// </summary>
        /// <param name="group"></param>
        /// <param name="factor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareTargetGroup))]
        public ATFactorValue EarlierExDueDateTargetGrpFirst(APETargetGroup group, ATWeightFactor factor, APEPegContext context)
        {
            var pegTarget = group.Sample as APETarget;
            var demandInfo = pegTarget.SoDemand;

            return new ATFactorValue(demandInfo.MaxLateDueDateTime.ToOADate(), demandInfo.MaxLateDueDateTime.ToString(ATUtil.DateTimeFormat));
        }

        /// <summary>
        /// TargetGroup 내 수량이 많은 TargetGroup 우선
        /// </summary>
        /// <param name="group"></param>
        /// <param name="factor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareTargetGroup))]
        public ATFactorValue LargerTotalQtyTargetGrpFirst(APETargetGroup group, ATWeightFactor factor, APEPegContext context)
        {
            double sumQty = group.Targets.Sum(x => x.Qty);

            return new ATFactorValue(sumQty * -1, "");
        }

        /// <summary>
        /// Target 의 So의 Priority 가 높은(숫자가 낮은) PegTarget이 포함된 TargetGroup 우선
        /// </summary>
        /// <param name="group"></param>
        /// <param name="factor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareTargetGroup))]
        public ATFactorValue HigherPriorityTargetGrpFirst(APETargetGroup group, ATWeightFactor factor, APEPegContext context)
        {
            var priority = group.Targets.Min(x => (x as APETarget).SoDemand.Priority);

            return new ATFactorValue(priority, "");
        }
    }
}
