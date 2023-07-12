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
    public class CompareRefProdPlanMethod
    {
        /// <summary>
        /// Due Date가 빠른 Ref Production Plan 우선
        /// </summary>
        /// <param name="refPlan"></param>
        /// <param name="factor"></param>
        /// <param name="pegPart"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareRefProdPlan))]
        public ATFactorValue RefPlanDueDateFactor(APERefPlan refPlan, ATWeightFactor factor, APEPegPart pegPart)
        {
            return new ATFactorValue(refPlan.DueDate.ToOADate(), "");
        }

        /// <summary>
        /// 수량이 적은 Ref Production Plan 우선
        /// </summary>
        /// <param name="refPlan"></param>
        /// <param name="factor"></param>
        /// <param name="pegPart"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareRefProdPlan))]
        public ATFactorValue RefPlanQtyFactor(APERefPlan refPlan, ATWeightFactor factor, APEPegPart pegPart)
        {
            return new ATFactorValue(refPlan.Qty, "");
        }
    }
}
