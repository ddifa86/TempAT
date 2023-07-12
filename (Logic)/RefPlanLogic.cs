using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.ObyO;
using Mozart.SeePlan.Aleatorik.ObyO.Data;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Logic
{
    public class RefPlanLogic
    {
        public static RefPlanLogic Instance
        {
            get
            {
                return ServiceLocator.Resolve<RefPlanLogic>();
            }
        }

        public bool IsRefPlan(APEPegPart pegPart)
        {
            if (pegPart.CurrentBuffer.IsRefPlanBuffer && string.IsNullOrEmpty(ATOption.Instance.TargetReferencePlanType) == false &&
                pegPart.CurRefPlan == null && pegPart.CurrentOperation.IsBuffer)
                    return true;

            return false;
        }

        /// <summary>
        /// OperTarget에 RefPlan 정보를 복사해주기 위해 RefPlan 여부를 판단하는 메서드
        /// 기존의 OperTarget(CurOperTarget)과 새로 만든 OperTarget의 Buffer가 같은데 RefPlan이 있으면 True
        /// </summary>
        /// <param name="pegTarget"></param>
        /// <param name="operTarget"></param>
        /// <returns></returns>
        public bool IsRefPlan(APETarget pegTarget, ATOperTarget operTarget)
        {
            if (pegTarget.CurOperTarget != null && pegTarget.CurOperTarget.CurRefPlan != null && pegTarget.CurOperTarget.ToBufferID == operTarget.ToBufferID)
                return true;
            else
                return false;
        }

        public ATRoute FindRefPlanRoute(APEPegPart pegPart, ATRoute route)
        {
            if (pegPart.CurRefPlan != null)
            {
                if (pegPart.CurRefPlan.Route != null && pegPart.CurRefPlan.ItemSiteBuffer == pegPart.CurrentItemSiteBuffer)
                    route = pegPart.CurRefPlan.Route;
            }

            return route;
        }

        public void WriteRefProductionPlanLog(List<APERefPlan> refPlans, APEPegPart pegPart, int retryCount)
        {
            var pegTarget = pegPart.SampleTarget;

            foreach (var refPlan in refPlans)
            {
                if (refPlan.RemainQty <= ATOption.Instance.MinimumAllocationQuantity)
                    continue;

                var demand = pegTarget.Demand;

                DateTime targetDate = demand.DueDateTime.AddSeconds(-pegTarget.BCumTat);
                DateTime maxEarlyDate = targetDate.AddDays(-demand.MaxEarlyDays);
                DateTime maxLateDate = targetDate.AddDays(demand.MaxLateDays);

                string reason;
                if (refPlan.Bom != null && pegPart.CurrentItemSiteBuffer.PrevBoms.ContainsKey(refPlan.Bom) == false)
                    reason = "BOM";
                else if (refPlan.DueDate < maxEarlyDate)
                    reason = "PB";
                else if (refPlan.DueDate > maxLateDate)
                    reason = "LATE";
                else
                    reason = "GC";

                //잔여 RefPlan 값 기록
                OutputWriter.Instance.WriteRefProdPlanLog(refPlan, demand, reason, retryCount);
            }
        }

        public List<APEPegPart> ApplyRefPlan(APEPegPart pegPart, APEShortManager shortManager)
        {
            List<APEPegPart> refPegParts = new List<APEPegPart>();

            var executionInfo = ATExecutionContext.Instance.CurrentExecutionInfo;
            var defaultRuleSet = ATRuleAgent.Instance.CurrentRuleSet;

            var pegTarget = pegPart.SampleTarget;
            if (pegPart.CurrentItemSiteBuffer.RefPlans.TryGetValue(ATOption.Instance.TargetReferencePlanType, out var refPlans))
            {
                // CompareRefProdPlans
                var preset = defaultRuleSet.GetRule(RulePoint.CompareRefProdPlan, CallType.Operation);
                refPlans.Sort(new ATRefPlanComparer(preset, pegPart));

                // FilterRefProdPlans
                preset = defaultRuleSet.GetRule(RulePoint.FilterRefProdPlan, CallType.Operation);
                refPlans = FilterRefProdPlan(refPlans, pegPart, preset);

                double remainQty = pegTarget.Qty;
                int idx = 0;
                foreach (var refPlan in refPlans)
                {
                    double refQty = Math.Min(remainQty, refPlan.RemainQty);
                    if (refQty <= ATOption.Instance.MinimumAllocationQuantity)
                        continue;

                    var copyPlan = refPlan.ShallowCopy(refQty);
                    copyPlan.Key = pegPart.FactorObjectKey + "@" + ++idx;
                    copyPlan.OrgRefPlan = refPlan;
                    copyPlan.RefTarget = pegTarget.CurOperTarget;

                    APEPegPart copyPp = pegPart.Clone() as APEPegPart;
                    var copyPt = copyPp.SampleTarget;

                    copyPt.RefTime += (copyPt.DueDate - copyPlan.DueDate).TotalSeconds;
                    copyPt.Qty = copyPlan.RemainQty;
                    copyPt.DueDate = copyPlan.DueDate;

                    copyPp.RefType = copyPlan.Type;
                    copyPp.CurRefPlan = copyPlan;
                    copyPp.FactorObjectKey = copyPlan.Key;

                    var copyTarget = pegTarget.CurOperTarget.DeepCopy();

                    copyPt.CurOperTarget = copyTarget;
                    copyPt.CurOperTarget.CurRefPlan = copyPlan;

                    refPegParts.Add(copyPp);

                    remainQty -= refQty;
                    refPlan.VirtualUsedQty += refQty;

                    BackwardCommonLogic.Instance.PegControl.OnApplyRefPlan(copyPp, copyPlan);

                    if (remainQty <= ATOption.Instance.MinimumAllocationQuantity)
                        break;
                }

                refPegParts.Reverse();

                WriteRefProductionPlanLog(refPlans, pegPart, executionInfo.CurRetryCount);

                if (remainQty > ATOption.Instance.MinimumAllocationQuantity)
                {
                    ATShortInfo shortInfo = new ATShortInfo(pegPart, pegTarget.CurOperTarget, remainQty, ShortCategory.REF_PLAN.ToString(), "", ShortType.NextPhase);
                    shortManager.AddShortInfo(shortInfo, true);

                    pegPart.SampleTarget.AddShortInfo(LateCategory.FixedPlan, LateReason.LackOfReferencePlan.ToString(), remainQty, null, pegPart.SampleTarget.TargetDateTime, pegPart.SampleTarget.TargetDateTime);
                }
            }
            else
            {
                ATShortInfo shortInfo = new ATShortInfo(pegPart, pegTarget.CurOperTarget, pegTarget.RemainQty, ShortCategory.REF_PLAN.ToString(), "", ShortType.NextPhase);
                shortManager.AddShortInfo(shortInfo, true);

                pegPart.SampleTarget.AddShortInfo(LateCategory.FixedPlan, LateReason.LackOfReferencePlan.ToString(), pegTarget.RemainQty, null, pegPart.SampleTarget.TargetDateTime, pegPart.SampleTarget.TargetDateTime);
            }

            return refPegParts;
        }

        public List<APERefPlan> FilterRefProdPlan(List<APERefPlan> refPlans, APEPegPart pegPart, ATWeightPreset preset)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("FilterRefProdPlan");
            try
            {
                List<APERefPlan> selectRefPlan = new List<APERefPlan>();

                if (preset == null)
                {
                    selectRefPlan.AddRange(refPlans);
                    return selectRefPlan;
                }

                foreach (var bom in refPlans)
                {
                    bool bFilter = false;
                    foreach (var factor in preset.FactorList)
                    {
                        FilterRefProdPlan method = factor.Method as FilterRefProdPlan;
                        var result = method(bom, factor, pegPart);
                        if (result.Value == true)
                        {
                            // filter사유 남기기.
                            bFilter = true;
                            break;
                        }
                    }

                    if (bFilter == false)
                        selectRefPlan.Add(bom);
                }

                return selectRefPlan;
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("FilterRefProdPlan");
            }
        }
    }
}
