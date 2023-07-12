using Mozart.SeePlan.Aleatorik.Data;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Pegger
{
    [FeatureBind()]
    public class PBBPredefines
    {
        internal static PBBPredefines Instance
        {
            get { return ServiceLocator.Resolve<PBBPredefines>(); }
        }

        [FeatureAttachTo(FECategory.PlanByBackward + "/" + FEControl.PBBInitialize + "/" + "PrepareDemand", Root = FEProvider.Aleatorik, Bind = true)]
        public List<ATDemand> PREPARE_DEMAND_PBB_DEF(ModuleExecutionInfo info, List<ModuleExecutionInfo> refModules, ref bool handled, List<ATDemand> prevReturnValue)
        {
            List<ATDemand> demands = info.Stage.Demands.Values.ToList();
            if (refModules.Count() == 0)
                return demands;

            var stage = info.Stage;
            List<ATOperTarget> inTarget = null;

            foreach (var refModule in refModules)
            {
                if (refModule.IsPBBModule)
                    inTarget = (refModule as PBBModuleExecutionInfo).StageInTargets;

                else if (refModule.IsPBOModule)
                    inTarget = (refModule as PBOModuleExecutionInfo).StageInTargets;

                if (inTarget == null)
                    continue;

                foreach (var target in inTarget)
                {
                    var prevBoms = target.CurrentItemSiteBuffer.PrevBoms;
                    if (prevBoms.Count <= 0)
                        continue;

                    var prevBom = prevBoms.FirstOrDefault();
                    var prevBomDetail = prevBom.Value.First();

                    var itemBuffer = prevBomDetail.FromItemSiteBuffer;
                    if (itemBuffer == null)
                        continue;

                    string id = CommonHelper.GenerateID(target.SODemand.ID);

                    ATDemand demand = new ATDemand(
                        stage.StageID, id, target.SiteID, itemBuffer, target.SODemand.Customer, target.TargetDateTime, target.TargetQty,
                        target.SODemand.Priority, target.SODemand.CustomerID,
                        target.SODemand.MaxLateDays, target.SODemand.MaxEarlyDays);

                    demand.SoDemand = target.SODemand;
                    demand.Property = target.SODemand.Property;

                    demands.Add(demand);
                }
            }

            return demands;
        }
    }
}
