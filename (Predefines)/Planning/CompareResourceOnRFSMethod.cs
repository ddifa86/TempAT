using Mozart.SeePlan.Aleatorik.Planner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class CompareResourceOnRFSMethod
    {
        /// <summary>
        /// 잔여 Capacity가 많은 설비 우선 (Time 설비의 경우 작업 시작 시점이 빠른 설비 우선)
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="factor"></param>
        /// <param name="lotGroup"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareResourceOnRFS))]
        public ATFactorValue LargerRemainCapaResFirst(PBFResource bucket, ATWeightFactor factor, PBFAllocateContext context)
        {
            double remain = bucket.CurrentCapaInfo.Remain;

            return new ATFactorValue(remain * -1, string.Format("Remain Capa : {0}", remain));
        }
    }
}
