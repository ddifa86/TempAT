using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Planner;

namespace Mozart.SeePlan.Aleatorik
{
    public class CompareResourceOnLFSMethod
    {
        /// <summary>
        /// 잔여 Capacity가 많은 설비 우선 (Time 설비의 경우 작업 시작 시점이 빠른 설비 우선)
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="factor"></param>
        /// <param name="lotGroup"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareResourceOnLFS))]
        public ATFactorValue LargerRemainCapaResFirst(PBFResource bucket, ATWeightFactor factor, APELotGroup lotGroup, PBFAllocateContext context)
        {
            double remain = bucket.CurrentCapaInfo.Remain;

            return new ATFactorValue(remain * -1, string.Format("Remain Capa : {0}", remain));
        }

        /// <summary>
        /// 사용 가능한 설비중 우선순위 (Operation Resource의 Priority)가 높은 설비 우선
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="factor"></param>
        /// <param name="lotGroup"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareResourceOnLFS))]
        public ATFactorValue HigherPriorityResFirst(PBFResource bucket, ATWeightFactor factor, APELotGroup lotGroup, PBFAllocateContext context)
        {
            var sampleLot = lotGroup.Sample as APELot;
            ATOperResource arrange = bucket.Target.GetArrange(sampleLot.CurrentOper.RouteID, sampleLot.CurrentOperID);

            return new ATFactorValue(arrange.Priority, string.Format("OPPriority : {0}", arrange.Priority));
        }
    }
}
