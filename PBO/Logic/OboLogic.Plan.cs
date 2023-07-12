using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Logic;
using Mozart.SeePlan.Aleatorik.ObyO.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.ObyO
{
    /// <summary>
    /// OBO 내에서 사용되는 Lot의 확장 메소드.
    /// </summary>    
    public partial class OboLogic
    {
        /// <summary>
        /// ??
        /// </summary>
        /// <param name="lot"></param>
        /// <returns></returns>
        public bool IsRetryFoward(APELot lot)
        {
            return false;
        }
 

        public void DoCancel(APELot lot)
        {
            // 장비 가상 할당 이력 제거
            CommitAllocate(lot, false);

            // 가상 PeggedWip 정보, 실 Pegged 정보로 변경
            CommitPeggedWip(lot, false);
        }

        public void DoCommit(APELot lot)
        {
            // 가상 RefPlan 정보, 실 할당으로 변경
            CommitRefPlan(lot);

            // 장비 가상 할당 정보, 실 할당으로 변경
            CommitAllocate(lot, true);

            // 가상 PeggedWip 정보, 실 Pegged 정보로 변경
            CommitPeggedWip(lot, true);

            // CreateBinnedWip
            CreateSplitWip(lot);
        }

        public void CommitAllocate(APELot lot, bool isCommit)
        {
            var lastOperTarget = lot.LastOperTarget;

            foreach (var planInfo in lot.CapaPlans)
            {
                double allocLotQty = lot.Qty * planInfo.OperTarget.GetBCumChangeRatio(lastOperTarget);

                if (planInfo.AllocationInfo.UsedCapacity != null)
                {
                    double allocCapaQty = allocLotQty * planInfo.AllocationInfo.UsagePer;
                    PBOResource oboBucket = planInfo.Bucket as PBOResource;
                    oboBucket.UpdateAct(planInfo, allocCapaQty, isCommit);
                    planInfo.AllocationInfo.RealAllocQty = allocCapaQty;

                    if (planInfo.AllocationInfo.UsedConstraints != null)
                    {
                        foreach (var constraint in planInfo.AllocationInfo.UsedConstraints)
                        {
                            constraint.Key.UpdateVirtualAct(allocLotQty * constraint.Value, isCommit);
                        }
                    }

                    //if (planInfo.AddPlanInfos != null)
                    //{
                    //    foreach (var addPlan in planInfo.AddPlanInfos.Values)
                    //    {
                    //        oboBucket = addPlan.Bucket as OboBucket;
                    //        allocCapaQty = allocLotQty * addPlan.UsagePer;

                    //        oboBucket.UpdateAct(addPlan, allocCapaQty, isCommit);
                    //    }
                    //}
                }
            }
        }

        private void CommitPeggedWip(APELot lot, bool isCommit)
        {
            if (lot.VirtualPegWips.Count() > 0)
            {
                var lastOperTarget = lot.LastOperTarget;

                foreach (var virtualWip in lot.VirtualPegWips)
                {
                    var target = virtualWip.MapTarget;
                    double pegQty = lot.Qty * target.GetBCumChangeRatio(lastOperTarget);

                    var peggedWip = BackwardCommonLogic.Instance.CommitPeggedWip(pegQty, virtualWip, isCommit);
                    if (peggedWip != null)
                    {
                        var executeInfo = ATExecutionContext.Instance.CurrentExecutionInfo as PBOModuleExecutionInfo;
                        executeInfo.PeggedWips.Add(peggedWip);
                    }
                }
            }
        }

        private void CreateSplitWip(APELot lot)
        {
            foreach (var splitLot in lot.SplitInfos)
            {
                // BinnedWip이 SplitBuffer인 경우, Lot을 생성하여 SplitBuffer까지 PlanInfo 를 적도록 처리.
                APETarget pegTarget = splitLot.CurrentTarget.PegTarget;

                ATBomDetail detail = splitLot.CurrentBomDetail;
                double bCumChangeRatio = splitLot.CurrentTarget.BCumChangeRatio;

                ATItemSiteBuffer soItem = splitLot.CurrentTarget.SODemand.ItemSiteBuffer; //pegPart.MoMaster.ItemBuffer;

                List<ATBomDetail> co_by_Details = splitLot.CurrentBom.GetCoByBomDetail(soItem, detail.ToItemSiteBuffer);

                double binrate = splitLot.CurrentBom.GetBomRatio(soItem, detail.ToItemSiteBuffer, splitLot.CurrentTarget.TargetDateTime);

                var newQty = lot.Qty * bCumChangeRatio;

                if (newQty <= 0)
                    return;

                foreach (var lower in co_by_Details)
                {
                    double splitRatio = lower.GetToQty(splitLot.CurrentTarget.TargetDateTime);
                    if (splitRatio > 0)
                    {
                        var lotID = LotHelper.GeneratLotID(ATConstants.BIN_WIP_PREFIX, splitLot.OrgLot.LotID);

                        var qty = newQty * splitRatio;

                        Inputs.WIP entity = new Inputs.WIP();
                        entity.WIP_ID = lotID;
                        entity.WIP_QTY = qty;
                        entity.AVAILABLE_DATETIME = splitLot.CurrentTarget.TargetDateTime;
                        entity.TRACK_IN_DATETIME = entity.AVAILABLE_DATETIME;

                        var wipinfo = ObjectMapper.CreateWipInfo(entity, WipType.Inventory, LotState.Wait, lower.ToSite,
                            lower.ToItem, null, lower.ToBuffer as ATOperation, null, ATExecutionContext.Instance.CurrentStage, lower.ToBuffer);

                        APEWip binnedWip = ObjectMapper.CreatePlanWip(wipinfo, LotCreateType.SplitByBom);
                        binnedWip.SourceTarget = pegTarget;

                        if (binnedWip.Qty < ATOption.Instance.MinimumAllocationQuantity)
                            continue;

                        binnedWip.SourceTarget = pegTarget;

                        OutputWriter.Instance.WriteSplitLog(splitLot, lot, binnedWip, lower, splitRatio, newQty);

                        BackwardCommonLogic.Instance.AddBinnedWip(binnedWip);
                    }
                }
            }

            OutputWriter.Instance.WriteLotHistory(lot, lot.Qty, LotCreateType.SplitByBom.ToString(), lot.LastStepTime, string.Empty);
        }

        public void OnStepIn(APELot lot)
        {
            PBOInterface.PlanControl.OnStepIn(lot);
            lot.LotState = LotState.Run;
        }

        public void OnStepOut(APELot lot)
        {
            PBOInterface.PlanControl.OnStepOut(lot);
        }
    }
}
