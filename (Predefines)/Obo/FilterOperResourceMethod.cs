using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.ObyO.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class FilterOperResourceMethod
    {
        /// <summary>
        /// Ref 정보가 있는데 Ref Plan의 Resource가 아니면 제외
        /// </summary>
        /// <param name="arrange"></param>
        /// <param name="factor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(FilterOperResource))]
        public ATFilterValue NonRefArrangeFilter(ATOperResource arrange, ATWeightFactor factor, PBOAllocateContext context)
        {
            var refPlan = context.RefPlan;
            var bucket = arrange.Oper.GetLoadableBucket(arrange);

            if (bucket == null)
                return new ATFilterValue(false, null);

            if (refPlan != null)
            {
                if (refPlan.Resource != null && refPlan.Resource.ResourceID != bucket.Target.ResourceID)
                    return new ATFilterValue(true, null);
            }

            return new ATFilterValue(false, null);
        }
    }
}
