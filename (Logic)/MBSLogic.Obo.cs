using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.ObyO;
using Mozart.SeePlan.Aleatorik.ObyO.Data;
using Mozart.SeePlan.Aleatorik.Outputs;
using Mozart.SeePlan.Aleatorik.Planner;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Logic
{
    public partial class MBSLogic
    {
        public static MBSLogic Instance
        {
            get
            {
                return ServiceLocator.Resolve<MBSLogic>();
            }
        }

        public APELot GetMbsLot(OboAlignQueue queue)
        {
            Dictionary<string, APELotGroup> lotGroups = new Dictionary<string, APELotGroup>();
            List<APELot> remainLots = new List<APELot>(); //MBSLot을 생성하기 위해 대기중인 Lots

            var oper = queue.Operation;
            double mbsValue = queue.Operation.MBSValue;

            var factors = ATRuleAgent.Instance.CurrentRuleSet.GetRule(RulePoint.LotGroupKey, CallType.Operation);

            var lotInGroupPreset = ATRuleAgent.Instance.CurrentRuleSet.GetRule(RulePoint.CompareLotInGroup, CallType.Operation);
            var lotGroupPreset = ATRuleAgent.Instance.CurrentRuleSet.GetRule(RulePoint.CompareLotGroupOnLFS, CallType.Level, 1);

            string factorIDs = null;
            string lotGroupKey = null;
            foreach (var lot in queue.GetLots())
            {
                if (factors != null)
                {
                    foreach (var factor in factors.FactorList)
                    {
                        var method = factor.Method as LotGroupKey;
                        if (lotGroupKey == null)
                        {
                            lotGroupKey = method(lot);
                            factorIDs = factor.FactorInfo.FactorID;
                        }
                        else
                        {
                            lotGroupKey += "@" + method(lot);
                            factorIDs += "@" + factor.FactorInfo.FactorID;
                        }
                    }
                }
                else
                {
                    lotGroupKey = oper.OperID;
                }
                
                if (string.IsNullOrEmpty(lotGroupKey))
                {
                    lot.IsShort = true;
                    lot.AddShortInfo(LateCategory.Constraint, factorIDs, lot.Qty, null, lot.LastStepTime, lot.LastStepTime);

                    continue;
                }

                if (lotGroups.TryGetValue(lotGroupKey, out var group) == false)
                {
                    group = new APELotGroup(lotGroupKey);
                    lotGroups.Add(lotGroupKey, group);
                }

                group.AddLot(lot, lotInGroupPreset); // 여기깔지
            }

            var groups = lotGroups.Values.ToList();
            groups.Sort(new FWLotGroupComparer(null, lotGroupPreset));

            double remainQty = 0;
            foreach (var groupLot in groups)
            {
                var lotGroup = groupLot;
                var lots = lotGroup.GetLotList();

                foreach (APELot lot in lots)
                {
                    queue.RemoveLot(lot);

                    double sumQty = remainQty + lot.Qty;

                    if (lots.Last() == lot)
                    {
                        double ceilingSumQty = sumQty.Ceiling();
                        double gap = ceilingSumQty - sumQty;
                        if (gap < ATOption.Instance.MinimumAllocationQuantity)
                        {
                            lot.Qty = lot.CurrentQty += gap;
                            sumQty = ceilingSumQty;
                        }
                    }

                    double quotient = Math.Truncate(sumQty / mbsValue);

                    if (quotient > ATOption.Instance.MinimumAllocationQuantity)
                    {
                        double remainder = sumQty % mbsValue;

                        string lotID = LotHelper.GeneratLotID(ATConstants.MBS_LOT_PREFIX, lot.LotID);
                        double newQty = sumQty - remainder;
                        var target = remainLots.Count > 0 ? remainLots.First().CurrentTarget : lot.CurrentTarget;
                        APELot mbsLot = new APELot(lotID, newQty, oper, target, null, LotCreateType.Normal);

                        mbsLot.LastStepTime = lot.LastStepTime;
                        mbsLot.CurrentOper = oper;
                        mbsLot.CurrentTarget = target;
                        mbsLot.LotState = LotState.Run;

                        remainLots.ForEach(x => mbsLot.ChildLots.Add(x));
                        remainLots.Clear();

                        double splitQty = lot.Qty - remainder;

                        var cloneLot = LotHelper.GenerateSplitLot(lot, splitQty);
                        if (cloneLot != null)
                            mbsLot.ChildLots.Add(cloneLot);

                        if (lot.Qty >= ATOption.Instance.MinimumAllocationQuantity)
                            queue.AddAlignQueue(lot);

                        return mbsLot;
                    }
                    else
                    {
                        remainLots.Add(lot);
                        remainQty += lot.Qty;
                    }
                }

                remainLots.ForEach(x => queue.AddAlignQueue(x));
                remainLots.Clear();
                remainQty = 0;
            }

            return null;
        }
        internal bool SplitChildLot(APELot lot)
        {
            if (lot.CurrentOper.IsMBSOper)
            {
                lot.ChildLots.Reverse();
                foreach (var childLot in lot.ChildLots)
                {
                    if (childLot.Qty <= ATOption.Instance.MinimumAllocationQuantity)
                        continue;

                    childLot.MoLots.Add(lot);
                    childLot.LastStepTime = lot.LastStepTime;
                    childLot.IsMBSSplitLot = true;
                    childLot.LotState = lot.LotState;
                    childLot.Status = lot.Status;
                    childLot.IsShort = lot.IsShort;
                    childLot.ShortCategory = lot.ShortCategory;
                    childLot.ReasonName = lot.ReasonName;
                    
                    if (lot.IsShort == false)
                    {
                        var copyPlan = lot.LastPlan.Clone() as APEPlanInfo;
                        copyPlan.LotInfo = new LotInfo(childLot);

                        childLot.Plans.Add(copyPlan);
                        childLot.CapaPlans.Add(copyPlan);
                    }

                    ReleaseAgent.Instance.PushDefaultStack(childLot);
                }
                return false;
            }

            return true;
        }

        internal List<APELot> SplitReAllocLot(APELot lot, double remainQty)
        {
            List<APELot> lots = new List<APELot>();
            
            for (int i = lot.ChildLots.Count - 1; i >= 0 ; i--)
            {
                if (remainQty <= ATOption.Instance.MinimumAllocationQuantity)
                    break;

                var childLot = lot.ChildLots[i];
                bool isRemain = childLot.Qty > remainQty;
                double cloneQty = isRemain ? remainQty : childLot.Qty;

                var cloneLot = LotHelper.GenerateSplitLot(childLot, cloneQty);
                lots.Add(cloneLot);
                remainQty -= cloneQty;
            }
           
            return lots;
        }

        internal bool AddAlignQueue(APELot lot)
        {
            if (lot.CurrentOper.IsMBSOper)
            {
                ReleaseAgent.Instance.AddAlignQueue(lot.CurrentOper, lot);

                return false;
            }

            return true;
        }
    }
}
