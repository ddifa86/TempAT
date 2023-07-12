using Mozart.SeePlan.Aleatorik.Data;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.ObyO
{
    [FeatureBind()]
    public class PBOPredefines
    {
        internal static PBOPredefines Instance
        {
            get { return ServiceLocator.Resolve<PBOPredefines>(); }
        }

        [FeatureAttachTo(FECategory.PlanByOrder + "/" + FEControl.PBOInitialize + "/" + "PrepareDemand", Root = FEProvider.Aleatorik, Bind = true)]
        public List<ATDemand> PREPARE_DEMAND_PBO_DEF(ModuleExecutionInfo info, List<ModuleExecutionInfo> refModules, ref bool handled, List<ATDemand> prevReturnValue)
        {
            if (refModules.Count() == 0)
            {
                return info.Stage.Demands.Values.ToList();
            }

            List<ATDemand> demands = new List<ATDemand>();
            var stage = info.Stage;

            foreach (var refModule in refModules)
            {
                if (refModule.IsPBBModule == false)
                    continue;

                PBBModuleExecutionInfo pbbModule = refModule as PBBModuleExecutionInfo;

                foreach (var target in pbbModule.StageInTargets)
                {
                    string id = CommonHelper.GenerateID(target.SODemand.ID);

                    var bufferID = stage.BufferRoute.LastOper.StepID;
                    ATItemSiteBuffer itemBuffer = ATInputData.ItemSiteBuffers.GetItemSite(target.SiteID, target.Item.ItemID, bufferID);

                    if (itemBuffer == null)
                        continue;

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

        [FeatureAttachTo(FECategory.PlanByOrder + "/" + FEControl.PBOPlanner + "/" + "IsShortLot", Root = FEProvider.Aleatorik, Bind = true)]
        public bool IS_SHORT_LOG_PBO_DEF(APELot lot, ref bool handled, bool prevReturnValue)
        {
            var status = lot.CurrentStatus(lot.LastStepTime);

            if (lot.IsShort)
                return true;

            if (status == Status.Late)
            {
                if (lot.Status != Status.Late)
                {
                    lot.Status = Status.Late;
                    lot.UpdateLateInfos(Status.Late);
                }
            }
            else if (status == Status.Short)
            {
                lot.IsShort = true;
                lot.ShortCategory = "FW Short";
                lot.ReasonName = "Delayed Target Date";

                lot.Status = Status.Short;
                lot.UpdateLateInfos(Status.Short);
                return true;
            }

            return false;
        }
    }
}
