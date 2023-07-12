using Mozart.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik
{
    public class PeggingKeyMethod
    {
        /// <summary>
        /// 타겟 그룹 내 대표 타겟의 Item ID, Site ID, Buffer ID 조합이 동일하거나 BOM_DETAIL_ALT에 정의된 대체 ItemSiteBuffer와 동일하면 패깅 가능
        /// </summary>
        /// <param name="pegPart"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(PeggingKey))]
        public List<string> AlterItemSiteBufferID(ITargetGroup pegPart, APEPegContext context)
        {
            HashSet<string> keys = new HashSet<string>();
            APEPegPart pp = pegPart is APETargetGroup ? (pegPart as APETargetGroup).FirstPegPart : pegPart as APEPegPart;

            keys.Add(pp.CurrentItemSiteBufferID);

            HashSet<string> alts;
            if (pp.NextBom != null)
            {
                string bomID = pp.NextBom.BomID;
                if (pp.CurrentItemSiteBuffer.AltItemSiteBufferKeys.TryGetValue(bomID, out alts))
                    keys.AddRange(alts);
            }

            if (pp.CurrentItemSiteBuffer.AltItemSiteBufferKeys.TryGetValue(pp.CurrentItemSiteBuffer.Key, out alts))
                keys.AddRange(alts);

            return keys.ToList();
        }

        /// <summary>
        /// 타겟 그룹 내 대표 타겟의 So ItemSiteBuffer와 동일하면 패깅 가능
        /// </summary>
        /// <param name="pegPart"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(PeggingKey))]
        public List<string> AlterSoItemSiteBufferID(ITargetGroup pegPart, APEPegContext context)
        {
            List<string> keys = new List<string>();
            APEPegPart pp = pegPart is APETargetGroup ? (pegPart as APETargetGroup).FirstPegPart : pegPart as APEPegPart;

            var pt = pp.SampleTarget;

            if (pt.CurOperTarget != null)
                keys.Add(pt.CurOperTarget.SODemand.ItemSiteBufferKey);

            return keys;
        }

        /// <summary>
        /// 타겟 그룹 내 대표 타겟의 Item ID와 동일하면 패깅 가능
        /// </summary>
        /// <param name="pegPart"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(PeggingKey))]
        public List<string> ItemID(ITargetGroup pegPart, APEPegContext context)
        {
            List<string> keys = new List<string>();
            APEPegPart pp = pegPart is APETargetGroup ? (pegPart as APETargetGroup).FirstPegPart : pegPart as APEPegPart;

            keys.Add(pp.CurrentItem.ItemID);

            return keys;
        }

        /// <summary>
        /// 타겟 그룹 내 대표 타겟의 Item ID, Site ID, Buffer ID 조합이 동일하면 패깅 가능
        /// </summary>
        /// <param name="targetGroup"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(PeggingKey))]
        public List<string> ItemSiteBufferID(ITargetGroup pegPart, APEPegContext context)
        {
            List<string> keys = new List<string>();
            APEPegPart pp = pegPart is APETargetGroup ? (pegPart as APETargetGroup).FirstPegPart : pegPart as APEPegPart;

            keys.Add(pp.CurrentItemSiteBufferID);

            return keys;
        }

        /// <summary>
        /// 타겟 그룹 내 대표 타겟의 Sales Order ID와 동일하면 패깅 가능
        /// </summary>
        /// <param name="pegPart"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(PeggingKey))]
        public List<string> SalesOrderID(ITargetGroup pegPart, APEPegContext context)
        {
            List<string> keys = new List<string>();
            APEPegPart pp = pegPart is APETargetGroup ? (pegPart as APETargetGroup).FirstPegPart : pegPart as APEPegPart;
            
            var pt = pp.SampleTarget;
            keys.Add(pt.Demand.ID);

            return keys;
        }

        /// <summary>
        /// 타겟 그룹 내 대표 타겟의 SO Item ID와 동일하면 패깅 가능
        /// </summary>
        /// <param name="pegPart"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(PeggingKey))]
        public List<string> SoItemID(ITargetGroup pegPart, APEPegContext context)
        {
            List<string> keys = new List<string>();
            APEPegPart pp = pegPart is APETargetGroup ? (pegPart as APETargetGroup).FirstPegPart : pegPart as APEPegPart;

            var pt = pp.SampleTarget;
            keys.Add(pt.Demand.ItemID);

            return keys;
        }
    }
}
