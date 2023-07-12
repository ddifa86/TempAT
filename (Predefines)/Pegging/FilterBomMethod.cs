using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.Task.Execution;

namespace Mozart.SeePlan.Aleatorik
{
    [FeatureBind()]
    public class FilterBomMethod
    {
        /// <summary>
        /// Ref Plan 정보가 있는 경우, Ref Plan에 등록된 Bom이 아니면 제외
        /// </summary>
        /// <param name="bom"></param>
        /// <param name="factor"></param>
        /// <param name="pegPart"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(FilterBom))]
        public ATFilterValue FilterRefBom(ATBom bom, ATWeightFactor factor, APEPegPart pegPart)
        {
            var refPlan = pegPart.CurRefPlan;
            if (refPlan != null && refPlan.ItemSiteBuffer == pegPart.CurrentItemSiteBuffer)
            {
                if (refPlan.Bom != null)
                {
                    if (bom.BomID == refPlan.Bom.BomID)
                        return new ATFilterValue(false, null);
                    else
                        return new ATFilterValue(true, null);
                }
            }
            return new ATFilterValue(false, null);
        }
    }
}
