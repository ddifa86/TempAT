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
    public class WipKeyMethod
    {
        /// <summary>
        /// ItemID, SiteID, BufferID의 조합 기준으로 재공 그룹핑
        /// </summary>
        /// <param name="wip"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(WipKey))]
        public List<string> ItemSiteBufferID(APEWip wip)
        {
            List<string> lst = new List<string>();

            lst.Add(wip.ItemSiteBuffer.Key);

            return lst;
        }

        /// <summary>
        /// Wip Property “#SalesOrderID” 기준으로 재공 그룹핑
        /// </summary>
        /// <param name="wip"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(WipKey))]
        public List<string> SalesOrderID(APEWip wip)
        {
            List<string> lst = new List<string>();
            lst.Add(wip.WipInfo.Property["#SalesOrderID"]);

            return lst;
        }

        /// <summary>
        /// 재공이 기여할 수 있는 So ItemSiteBuffer ID로 그룹핑
        /// </summary>
        /// <param name="planWip"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(WipKey))]
        public List<string> AlterSoItemSiteBufferID(APEWip planWip)
        {
            List<string> lst = planWip.WipInfo.ItemSiteBuffer.SoItemSiteBuffers.Select(x => x.Key).ToList();

            return lst;
        }

        /// <summary>
        /// Item ID 기준으로 재공 그룹핑
        /// </summary>
        /// <param name="planWip"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(WipKey))]
        public List<string> ItemID(APEWip planWip)
        {
            List<string> lst = new List<string>(); 
            lst.Add(planWip.ItemID); 
            return lst;
        }
    }
}
