using Mozart.Simulation.Engine;
using Mozart.Extensions;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.ObyO;

namespace Mozart.SeePlan.Aleatorik
{
    [FeatureBind()]
    public class CompareBomMethod
    {
        /// <summary>
        /// 현재 ItemSiteBuffer까지의 누적 TAT와 Target Date (with Max Lateness Days)까지의 잔여 기간을 비교해서 아직 여유 시간이 있는 Bom 우선
        /// </summary>
        /// <param name="bom"></param>
        /// <param name="factor"></param>
        /// <param name="pegPart"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareBom))]
        public ATFactorValue ShortCumTatBomFirst(ATBom bom, ATWeightFactor factor, APEPegPart pegPart, APESelectBomContext context)
        {
            var pegTarget = pegPart.SampleTarget;

            double fromBufferCumTat = bom.GetFromItemSiteCumTat(pegTarget.SoDemand.MaxLateDueDateTime);
            double routeTat = bom.MaxTat;
            double pathSumTat = fromBufferCumTat + routeTat;

            var planStartTime = ATOption.Instance.PlanStartTime;
            var value = (pegTarget.MaxLateTargetDateTime - planStartTime).TotalSeconds - pathSumTat;

            if (value < 0)
                return new ATFactorValue(1, string.Format("Result : {0}, SumTat : {1}", value, ATUtil.TimeToUom(new Time(pathSumTat), UomType.Day)));
            else
                return new ATFactorValue(0, string.Format("Result : {0}, SumTat : {1}", value, ATUtil.TimeToUom(new Time(pathSumTat), UomType.Day)));
        }

        /// <summary>
        /// TAT가 짧은 Routing에 연결된 Bom 우선
        /// </summary>
        /// <param name="bom"></param>
        /// <param name="factor"></param>
        /// <param name="pegPart"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareBom))]
        public ATFactorValue ShortTatBomFirst(ATBom bom, ATWeightFactor factor, APEPegPart pegPart, APESelectBomContext context)
        {
            return new ATFactorValue(bom.MaxTat, string.Format("Bom Tat : {0}", bom.MaxTat));
        }

        /// <summary>
        /// 현 Buffer 앞쪽에 누적 재공이 Target 보다 큰경우 0 아니면 1 반환
        /// </summary>
        /// <param name="bom"></param>
        /// <param name="factor"></param>
        /// <param name="pegPart"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareBom))]
        public ATFactorValue EnoughWipQtyBomFirst(ATBom bom, ATWeightFactor factor, APEPegPart pegPart, APESelectBomContext context)
        {
            var target = pegPart.SampleTarget;
            double value = bom.BomDetails.Sum(x => x.FromItemSiteBuffer.WipQueue.GetCumWipQty(x, target.MaxLateTargetDateTime));

            if (value > pegPart.Targets.Sum(x => x.Qty))
                return new ATFactorValue(0, string.Format("CumWipQty : {0}", value));
            else
                return new ATFactorValue(1, string.Format("CumWipQty : {0}", value));
        }

        /// <summary>
        /// 현재 ItemSiteBuffer로부터 재공이 존재하는 가장 가까운 ItemSiteBuffer의 Buffer Sequence가 빠른 Bom 우선
        /// </summary>
        /// <param name="bom"></param>
        /// <param name="factor"></param>
        /// <param name="pegPart"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareBom))]
        public ATFactorValue CloserWipBomFirst(ATBom bom, ATWeightFactor factor, APEPegPart pegPart, APESelectBomContext context)
        {
            ATItemSiteBuffer wipIsb = null;
            foreach (var info in bom.PrevItemSiteBuffers)
            {
                if (wipIsb != null)
                    break;

                foreach (var isb in info.Value)
                {
                    if (isb.WipQueue.WipQty > 0)
                    {
                        wipIsb = isb;
                        break;
                    }
                }
            }

            if (wipIsb == null)
                return new ATFactorValue(0, string.Empty);

            return new ATFactorValue(wipIsb.Buffer.Sequence * -1, string.Format("{0} : {1}", wipIsb.Key, wipIsb.WipQueue.WipQty));
        }

        /// <summary>
        /// 우선순위 (BOM MASTER의 PRIORITY) 높은 Bom 우선
        /// </summary>
        /// <param name="bom"></param>
        /// <param name="factor"></param>
        /// <param name="pegPart"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareBom))]
        public ATFactorValue HigherPriorityBomFirst(ATBom bom, ATWeightFactor factor, APEPegPart pegPart, APESelectBomContext context)
        {
            return new ATFactorValue(bom.Priority, string.Format("Bom Priority : {0}", bom.Priority));
        }

        /// <summary>
        /// ShortBom정보 누적이 적은 Bom 우선
        /// </summary>
        /// <param name="bom"></param>
        /// <param name="factor"></param>
        /// <param name="pegPart"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareBom))]
        public ATFactorValue LessShortCountBomFirst(ATBom bom, ATWeightFactor factor, APEPegPart pegPart, APESelectBomContext context)
        {
            var shortManager = OboExecutor.Current.ShortManager;

            if (shortManager.CumShortBoms.TryGetValue(bom, out int shortCount) == false)
                shortCount = 0;

            return new ATFactorValue(shortCount, null);
        }
    }
}
