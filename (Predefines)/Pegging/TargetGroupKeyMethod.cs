using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik
{
    [FeatureBind()]
    public class TargetGroupKeyMethod
    {
        /// <summary>
        /// Item ID가 동일한 타겟들을 그룹핑
        /// </summary>
        /// <param name="pegPart"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(TargetGroupKey))]
        public string ItemID(APEPegPart pegPart)
        {
            return pegPart.CurrentItem.ItemID;
        }

        /// <summary>
        /// Sales Order ID가 동일한 타겟들을 그룹핑
        /// </summary>
        /// <param name="pegPart"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(TargetGroupKey))]
        public string SalesOrderID(APEPegPart pegPart)
        {
            return pegPart.MoMaster.Key.ToString();
        }

        /// <summary>
        /// Item ID, Site ID, Buffer ID, Target Week가 동일한 타겟들을 그룹핑
        /// </summary>
        /// <param name="pegPart"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(TargetGroupKey))]
        public string ItemSiteBufferWeek(APEPegPart pegPart)
        {
            string targetWeek = pegPart.SampleTarget.TargetWeek;
            return pegPart.CurrentItemSiteBufferID + "@" + targetWeek;
        }
    }
}
