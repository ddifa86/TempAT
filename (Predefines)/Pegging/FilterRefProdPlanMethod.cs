using Mozart.SeePlan.Aleatorik.Data;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    [FeatureBind()]
    public class FilterRefProdPlanMethod
    {
        [RuleFactor(RulePoint = typeof(FilterRefProdPlan))]
        public ATFilterValue FilterFlexiablePlan(APERefPlan refPlan, ATWeightFactor factor, APEPegPart pegPart)
        {
            var pegTarget = pegPart.SampleTarget;
            var demand = pegTarget.Demand;

            DateTime targetDate = demand.DueDateTime.AddSeconds(-pegTarget.BCumTat);
            DateTime maxEarlyDate = targetDate.AddDays(-demand.MaxEarlyDays);
            DateTime maxLateDate = targetDate.AddDays(demand.MaxLateDays);

            if (refPlan.Type == "Fixed") //Constants??
            {
                return new ATFilterValue(false, null);
            }
            else
            {
                if (refPlan.DueDate < maxEarlyDate)
                    return new ATFilterValue(true, null);
                if (refPlan.DueDate > maxLateDate)
                    return new ATFilterValue(true, null);
            }

            return new ATFilterValue(false, null);
        }
    }
}
