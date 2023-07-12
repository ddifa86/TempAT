
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
    public class ComparePeggingGroupMethod
    {
        /// <summary>
        /// 대표 타겟의 타겟 주차가 빠른 패깅 그룹 우선
        /// </summary>
        /// <param name="peggingGroup"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(ComparePeggingGroup))]
        public ATFactorValue EarlierTargetWeekPegGrpFirst(APEPeggingGroup peggingGroup, ATWeightFactor factor)
        {
            var pegPart = peggingGroup.Items.First() as APEPegPart;
            var targetWeek = pegPart.SampleTarget.TargetWeek;

            int.TryParse(targetWeek, out int week);

            if (week <= 0)
                week = 999901;

            return new ATFactorValue(week, "");
        }

        /// <summary>
        /// 대표 타겟의 타겟 일자가 빠른 패깅 그룹 우선
        /// </summary>
        /// <param name="peggingGroup"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(ComparePeggingGroup))]
        public ATFactorValue EarlierTargetDatePegGrpFirst(APEPeggingGroup peggingGroup, ATWeightFactor factor)
        {
            var pegTarget = peggingGroup.Sample as APETarget;
            var dueDate = pegTarget.TargetDateTime.ToOADate();

            return new ATFactorValue(dueDate, "");
        }

        /// <summary>
        /// 대표 타겟의 등급이 높은 패깅 그룹 우선
        /// </summary>
        /// <param name="peggingGroup"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(ComparePeggingGroup))]
        public ATFactorValue HigherGradePegGrpFirst(APEPeggingGroup peggingGroup, ATWeightFactor factor)
        {
            var pegPart = peggingGroup.Items.First() as APEPegPart; 
            var grade = pegPart.CurrentItem.Grade; 

            return new ATFactorValue(grade, grade.ToString());
        }
    }
}
