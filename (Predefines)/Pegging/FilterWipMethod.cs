using Mozart.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik
{
    public class FilterWipMethod
    {
        /// <summary>
        /// 재공의 Available Time이 PegPart의 Target Date + Max Lateness Days 보다 미래인 경우, Pegging 대상에서 제외
        /// </summary>
        /// <param name="planWip"></param>
        /// <param name="factor"></param>
        /// <param name="pegTarget"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(FilterWip))]
        public ATFilterValue IsAvailableTime(APEWip planWip, ATWeightFactor factor, APETarget pegTarget, APEPegContext context)
        {
            if (planWip.AvailableTime > pegTarget.MaxLateTargetDateTime)
                return new ATFilterValue(true, string.Format("Wip Availble Time: {0} , Target DueDate {1} "
                    , planWip.AvailableTime, pegTarget.TargetDate));

            return new ATFilterValue(false, null);
        }

        /// <summary>
        /// SplitBy BOM에 의해 생성된 By Product 재공이면서 사용 가능시간이 타겟 시점보다 미래이면 제외
        /// </summary>
        /// <param name="planWip"></param>
        /// <param name="factor"></param>
        /// <param name="pegTarget"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(FilterWip))]
        public ATFilterValue BinnedWipAvailableTime(APEWip planWip, ATWeightFactor factor, APETarget pegTarget, APEPegContext context)
        {
            if (planWip.CreationType == LotCreateType.SplitByBom)
            {
                if (planWip.AvailableTime > pegTarget.TargetDateTime)
                    return new ATFilterValue(true, string.Format("Wip Availble Time: {0} , Target DueDate {1} ", planWip.AvailableTime, pegTarget.TargetDate));
            }
            return new ATFilterValue(false, null);
        }
    }
}
