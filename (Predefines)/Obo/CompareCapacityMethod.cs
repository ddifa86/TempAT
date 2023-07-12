using Mozart.SeePlan.Aleatorik.ObyO.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class CompareCapacityMethod
    {
        /// <summary>
        /// 일자가 빠른 Capacity 우선
        /// </summary>
        /// <param name="peggingGroup"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareCapacity))]
        public ATFactorValue EarlierDateCapaFirst(APECapacity capaInfo, ATWeightFactor factor, PBOAllocateContext context)
        {
            return new ATFactorValue(capaInfo.StartTime.ToOADate(), "");
        }

        /// <summary>
        /// Operation Resource 테이블의 Priority 기준으로 우선순위가 높은 설비를 우선
        /// </summary>
        /// <param name="peggingGroup"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareCapacity))]
        public ATFactorValue HigherPriorityResCapaFirst(APECapacity capaInfo, ATWeightFactor factor, PBOAllocateContext context)
        {
            var lot = context.Lot;
            var bucket = capaInfo.Bucket;
            var arrange = bucket.Target.GetArrange(lot.CurrentOper.RouteID, lot.CurrentOperID);
            
            return new ATFactorValue(arrange.Priority, string.Format("OPPriority : {0}", arrange.Priority));
        }
    }
}
