using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class CompareLotMethod
    {
        /// <summary>
        /// Wip으로 만들어진 재공 우선
        /// </summary>
        /// <param name="lot"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareLot))]
        public ATFactorValue WipFirst(APELot lot, ATWeightFactor factor)
        {
            double retValue = lot.IsWipLot ? -1 : 1;
            return new ATFactorValue(retValue, null);
        }

        /// <summary>
        /// Buffer Seq가 작은 것 우선 진행 → 뒤쪽 순서 버퍼에 있는 재공 우선
        /// </summary>
        /// <param name="lot"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareLot))]
        public ATFactorValue DownstreamBufferLotFirst(APELot lot, ATWeightFactor factor)
        {
            return new ATFactorValue(lot.CurrentItemSiteBuffer.Buffer.Sequence * -1, null);
        }

        /// <summary>
        /// AvailableTime이 빠른 재공 우선
        /// </summary>
        /// <param name="lot"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareLot))]
        public ATFactorValue EarlierAvailTimeLotFirst(APELot lot, ATWeightFactor factor)
        {
            return new ATFactorValue(lot.LastStepTime.ToOADate(), null);
        }
    }
}
