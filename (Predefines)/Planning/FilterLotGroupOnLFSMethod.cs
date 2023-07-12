using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public class FilterLotGroupOnLFSMethod
    {
        /// <summary>
        /// Rolling 구간 내에서 할당이 불가능한 재공 그룹인 경우 제외
        /// </summary>
        /// <param name="lotGroup"></param>
        /// <param name="factor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(FilterLotGroupOnLFS))]
        public ATFilterValue IsLoadableLotGroup(APELotGroup lotGroup, ATWeightFactor factor, PBFAllocateContext context)
        {
            if (lotGroup.Qty <= ATOption.Instance.MinimumAllocationQuantity)
            {
                //다음 공정으로 보내기 전체적으로 다
                //lotgroup.Lots.ForEach(x => this.Line.MoveNext(x));
                return new ATFilterValue(true, string.Format("LotGroupQty : {0}", lotGroup.Qty));

            }

            // 이번 Rolling 구간에 할당이 불가능한 경우
            if (lotGroup.Sample.LastStepTime >= context.NextDt)
            {
                //필터작업
                return new ATFilterValue(true, string.Format("NextDT : {0}, SampleLastStepTime : {1}", context.NextDt.ToString("yyyyMMdd"), lotGroup.Sample.LastStepTime.ToString("yyyyMMdd")));
            }

            return new ATFilterValue(false, null);
        }
    }
}
