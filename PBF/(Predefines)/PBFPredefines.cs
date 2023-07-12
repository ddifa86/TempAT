using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Logic;
using Mozart.SeePlan.Aleatorik.Planner;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    [FeatureBind()]
    public class PBFPredefines
    {
        public static PBFPredefines Instance
        {
            get { return ServiceLocator.Resolve<PBFPredefines>(); }
        }


        [FeatureAttachTo(FECategory.PlanByForward + "/" + FEControl.PBFInitialize + "/" + "PrepareIncomingWip", Root = FEProvider.Aleatorik, Bind = true)]
        public List<APEWip> PREPARE_INCOMING_WIPS_DEF(PBFModuleExecutionInfo info, List<ModuleExecutionInfo> refModules, ref bool handled, List<APEWip> prevReturnValue)
        {
            Dictionary<ATItemSiteBuffer, List<APEWip>> availableBomWips = new Dictionary<ATItemSiteBuffer, List<APEWip>>();
            var stage = info.Stage;

            foreach (var refModule in refModules)
            {
                if (refModule.IsPBFModule == false)
                    continue;

                var module = refModule as PBFModuleExecutionInfo;

                foreach (var wip in module.StageOutWips)
                {
                    var nextBoms = wip.ItemSiteBuffer.NextBoms;
                    if (nextBoms.Count <= 0)
                        continue;

                    foreach (var bom in nextBoms)
                    {
                        var isb = bom.Value.First().ToItemSiteBuffer;
                        if (isb == null)
                            continue;

                        if (availableBomWips.TryGetValue(isb, out List<APEWip> wips) == false)
                        {
                            wips = new List<APEWip>();
                            availableBomWips.Add(isb, wips);
                        }

                        wips.Add(wip);
                    }
                    // InTarget에 Mapping 하는 작업 필요..?
                    // KitPegging 후 Pegging 된 대상 Wip들만 투입하도록 처리 필요.
                }
            }

            List<APEWip> peggedWips = new List<APEWip>();

            foreach (var target in info.InTargets)
            {
                List<APEWip> mapWips = new List<APEWip>();

                if (availableBomWips.TryGetValue(target.CurrentItemSiteBuffer, out var wips))
                    mapWips.AddRange(wips);
                else
                    continue;

                mapWips.Sort(new ATStageOutPlanWipComparer(target));

                while (mapWips.Count > 0)
                {
                    var wip = mapWips.First();
                    if (target.InTargetQty <= ATOption.Instance.MinimumAllocationQuantity)
                        break;

                    if (wip.RemainQty <= ATOption.Instance.MinimumAllocationQuantity)
                    {
                        mapWips.Remove(wip);
                        continue;
                    }

                    double batchQty = Math.Min(target.InTargetQty, wip.RemainQty);

                    target.InComingQty += batchQty;
                    wip.Act += batchQty;

                    wip.ItemSiteBuffer = target.CurrentItemSiteBuffer;
                    wip.WipInfo.Stage = info.Stage;
                    wip.Oper = target.CurrentItemSiteBuffer.Buffer;

                    var peggedWip = wip.ShallowCopy(target, batchQty);
                    peggedWips.Add(peggedWip);

                    if (wip.RemainQty <= ATOption.Instance.MinimumAllocationQuantity)
                    {
                        mapWips.Remove(wip);
                    }
                }
            }

            return peggedWips;
        }

        [FeatureAttachTo(FECategory.PlanByForward + "/" + FEControl.PBFInitialize + "/" + "PrepareOperTarget", Root = FEProvider.Aleatorik, Bind = true)]
        public Dictionary<IComparable, List<ATOperTarget>> PREPARE_OPER_TARGET_DEF(PBFModuleExecutionInfo info, List<ModuleExecutionInfo> refModules, ref bool handled, Dictionary<IComparable, List<ATOperTarget>> prevReturnValue)
        {
            foreach (var targets in info.OperTargets.Values)
            {
                targets.ForEach(x => x.UsedQty = 0);
            }

            return info.OperTargets;
        }

        [FeatureAttachTo(FECategory.PlanByForward + "/" + FEControl.Factory + "/" + "IsShortLot", Root = FEProvider.Aleatorik, Bind = true)]
        public bool IS_SHORT_LOG_PBF_DEF(APELot lot, ref bool handled, bool prevReturnValue)
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

        [FeatureAttachTo(FECategory.PlanByForward + "/" + FEControl.Allocator + "/" + "GetAllocableQty", Root = FEProvider.Aleatorik, Bind = true)]
        public double GET_ALLOCABLE_QTY_DEF(APELotGroup lotGroup, PBFResource bucket, DateTime loadableTime, PBFAllocateContext context, ref bool handled, double prevReturnValue)
        {
            double allocableQty = lotGroup.Qty;
            if (lotGroup.CurrentOper.IsMBSOper)
                allocableQty = MBSLogic.Instance.GetAllocableQty(lotGroup, bucket, loadableTime, context);

            return allocableQty;
        }
    }
}
