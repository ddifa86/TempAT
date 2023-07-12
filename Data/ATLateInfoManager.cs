using Mozart.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class ATLateInfoManager
    {
        public Dictionary<string, ATLateInfo> LateInfos { get; set; }

        internal ATDemand Demand { get; set; }

        internal Dictionary<APELot, Dictionary<string, ATLateInfo>> VirtualLateInfos { get; set; }

        public ATLateInfoManager(ATDemand demand)
        {
            this.Demand = demand;
            this.LateInfos = new Dictionary<string, ATLateInfo>();
            this.VirtualLateInfos = new Dictionary<APELot, Dictionary<string, ATLateInfo>>();
        }

        public void CopyLateInfos(APELot orgLot, APELot splitLot)
        {
            var infos = orgLot.VirtualLateInfos;

            if (infos == null)
                return;

            foreach (var info in infos)
            {
                splitLot.AddVirtualLateInfo(info.Value.Category, info.Value.Reason, info.Value.Description, info.Value.FromDate, info.Value.ToDate, info.Value.Resources);
            }
        }

        public void AddShortInfo(ATItemSiteBuffer itemSiteBuffer, ATBom bom, ATRoute route, ATOperation oper, LateCategory category, string reason, double qty, string desc, DateTime fromDate, DateTime toDate, string refPlanID, HashSet<IBucket> resources = null)
        {
            int phase = ATExecutionContext.Instance.CurrentExecutionInfo.CurPhase;
            int retryCount = ATExecutionContext.Instance.CurrentExecutionInfo.CurRetryCount;

            string key = itemSiteBuffer.Key + route?.RouteID + oper?.RouteID + category.ToString() + reason + Status.Short.ToString() + refPlanID + phase + retryCount;

            ATLateInfo info;
            if (LateInfos.TryGetValue(key, out info) == false)
            {
                info = new ATLateInfo(Demand, bom, oper, itemSiteBuffer, Status.Short, category, reason, desc, phase, retryCount, fromDate, toDate, refPlanID);
                LateInfos.Add(key, info);
            }

            if (resources != null)
                info.Resources.AddRange(resources);

            info.ShortQty += qty;
            info.Count += 1;
        }
    }
}
