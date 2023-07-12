using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.ObyO.Data;
using Mozart.SeePlan.Aleatorik.Logic;
using Mozart.Extensions;

namespace Mozart.SeePlan.Aleatorik.ObyO
{
    public partial class OboFactory
    {
        internal void MoveFirst(APELot lot)
        {
            PBOInterface.PlanControl.OnReleasedLot(lot);

            if (ATOption.Instance.ApplyWipReleaseOnAvailableTime == false && lot.IsWipLot)
                lot.LastStepTime = ATUtil.MaxTime(lot.LastStepTime, lot.CurrentTarget.MinEarlyTargetDateTime.StartTimeOfDayT(), ATOption.Instance.PlanStartTime);

            DateTime nowDT = lot.LastStepTime;
            OutputWriter.Instance.WriteLotHistory(lot, lot.Qty, LifeCycle.Release.ToString(), nowDT, string.Empty);

            LotState position = LotState.Wait;

            if (lot.IsRunWip || lot.IsOutWip)
            {
                lot.MoveFirst(nowDT);
                position = lot.LotState;

                if (lot.CurrentTarget == null) // 최초에 Run Wip인 경우에만 Target을 설정하도록
                {
                    var target = GetOperTarget(lot, BomType.None);
                    lot.CurrentTarget = target;
                }
            }

            if (lot.LastStepTime > lot.CurrentTarget.TargetDateTime)
                lot.AddVirtualLateInfo(LateCategory.Tat, LateReason.LateReleaseLot.ToString(), null, nowDT, nowDT);

            MoveNext(lot, position);
        }

        internal void MoveNext(APELot lot, LotState position = LotState.Out)
        {
            try
            {
                bool isDummyOper = MoveNext_(lot, position);

                while (isDummyOper)
                {
                    isDummyOper = MoveNext_(lot, LotState.Run);
                }

                return;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally
            {

            }
        }

        /// <summary>
        /// Lot의 다음 공정을 산출하여 반환        
        /// </summary>
        /// <param name="lot"></param>
        /// <param name="position"></param>
        /// <returns> 찾은 공정이 Dummy 공정인지 여부를 반환 </returns>  
        private bool MoveNext_(APELot lot, LotState position = LotState.Run)
        {
            try
            {
                #region 예외처리.
                if (lot.Qty < ATOption.Instance.MinimumAllocationQuantity)
                {
                    // Min수량으로 반환.
                    lot.IsShort = true;
                    OnDone(lot, LifeCycle.Remain, "MinQty");
                    return false;
                }
                #endregion

                if (position == LotState.Run)
                {
                    this.Allocator.DoAllocate(lot);
                    lot.LotState = LotState.Out;
                    lot.LastStepTime = lot.LastPlan == null ? lot.LastStepTime : lot.LastPlan.EndTime;

                    if (MBSLogic.Instance.SplitChildLot(lot) == false)
                        return false;
                }

                if (PBOInterface.PlanControl.IsShortLot(lot))
                {
                    lot.IsShort = true;
                    OnDone(lot, LifeCycle.Remain, string.Empty);
                    return false;
                }

                if (position == LotState.Run || position == LotState.Out)
                {
                    OboLogic.Instance.OnStepOut(lot);

                    PartChange(lot, true);
                    ApplyYield(lot);
                }

                lot.ClearVirtualLateInfo();

                NextStep(lot);
                
                if (IsFinished(lot) == true)
                {
                    OnDone(lot, LifeCycle.Disposal, string.Empty);
                    return false;
                }

                // InPartChange.. => Target 설정.
                var bomType = PartChange(lot, false);               
                
                var target = GetOperTarget(lot, bomType);

                if (target == null)
                {
                    lot.IsShort = true;
                    OnDone(lot, LifeCycle.Remain, "fail to find target");

                    lot.AddShortInfo(LateCategory.Etc, LateReason.FailToFindTarget.ToString(), lot.Qty, null, lot.LastStepTime, lot.LastStepTime);

                    return false;
                }

                if (bomType == BomType.Assembly)
                {
                    var assyLots = this.AssyManager.AddRun(lot, target.AssyInfo);
                    if (assyLots != null)
                        assyLots.ForEach(x => ReleasedLots.Push(x));

                    return false;
                }

                lot.CurrentTarget = target;

                double usedQty = Math.Min(target.RemainQty, lot.CurrentQty);
                target.UsedQty += usedQty;

                if (lot.CurrentOper.IsBuffer)
                {
                    SelectBom(lot);
                }

                OboLogic.Instance.OnStepIn(lot);

                return MBSLogic.Instance.AddAlignQueue(lot); 
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

            }
        }

        public BomType PartChange(APELot lot, bool isOut)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("RouteLogic_PartChange");
            try
            {
                if (lot.CurrentOper.IsBuffer)
                    return BomType.None;

                var stage = ATExecutionContext.Instance.CurrentStage;

                var type = ATInputData.Boms.GetChangeBomType(lot.CurrentBom, lot.CurrentOper, isOut);

                switch (type)
                {
                    case BomType.Assembly:
                        
                        // 초기 조립 공정인 경우.
                        if (lot.InitOper == lot.CurrentOper)
                            return BomType.None;

                        // 조립 Part 상태로 조립 시도하도록 
                        return BomType.Assembly;

                    case BomType.SplitBy:
                    case BomType.SplitCo:
                        {
                            /*
                             * F/W 하면서 생성된 BinnedWip들을 B/W에서 사용가능하도록 처리 필요.
                             * 확정되는 시점에 Binned Wip 생성 작업 진행
                             */
                            var bom = lot.CurrentTarget.CurrentBom;
                            var detail = lot.CurrentTarget.CurrentBomDetail;

                            var soItem = lot.CurrentTarget.SODemand.ItemSiteBuffer;
                            var ratio = bom.GetBomRatio(soItem, detail.ToItemSiteBuffer, lot.CurrentTarget.TargetDateTime);

                            double orgQty = lot.Qty;

                            double splitRatio = ratio / lot.CurrentBomDetail.FromQty;

                            lot.CurrentItemSiteBuffer = detail.ToItemSiteBuffer;
                            lot.Qty *= splitRatio;

                            #region OBO 전용 작업

                            ATSplitInfo info = new ATSplitInfo(lot, orgQty);
                            lot.SplitInfos.Add(info);
                            #endregion

                            return type;
                        }

                    case BomType.Normal:
                        {
                            var detail = lot.CurrentTarget.CurrentBomDetail;
                            lot.CurrentItemSiteBuffer = detail.ToItemSiteBuffer;
                            lot.Qty = lot.Qty.ConvertValue(detail.FromQty, detail.ToQty, PlanType.Forward); 

                            return BomType.Normal;
                        }
                    case BomType.None:
                    default:
                        return BomType.None;
                }
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("RouteLogic_PartChange");
            }
        }

        private bool IsFinished(APELot lot)
        {
            if (lot.IsFinish)
                return true;

            return false;
        }

        public void DoShort(APELot shortlot)
        {
            var shortManager = OboExecutor.Current.ShortManager;

            PBOInterface.PlanControl.OnShortLot(shortlot);

            OboLogic.Instance.DoCancel(shortlot);

            // Write Helper 로 처리.
            WriteShortPlanInfo(shortlot);

            // 첫 공정에서 대기하다가 Short이나는 경우는 별도의 처리가 필요해 보임.
            // 실제 Short이 나는 이유는 다른 Lot으로인해 진행이 무의미해져서 Short을 내는 것으로 해당 부분의 처리가 필요함.
            ATShortInfo info = new ATShortInfo(shortlot, shortlot.ShortTarget, shortlot.Qty);

            shortManager.AddShortInfo(info, false);
        }

        public void OnDone(APELot lot, LifeCycle cycle, string desc)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("RouteLogic_OnDone");
            try
            {
                OutputWriter.Instance.WriteLotHistory(lot, lot.Qty, cycle.ToString(), lot.LastStepTime, string.Empty);

                if (lot.IsShort)
                {
                    // 재투입 여부 체크.
                    if (OboLogic.Instance.IsRetryFoward(lot))
                    {
                        // 할당 이력 rollback.
                        OboLogic.Instance.CommitAllocate(lot, false);
                        //OboLogic.Instance.DoCancel(lot);

                        // 재투입 작업
                        var retryLots = CreateRetryLot(lot, lot.Qty);
                        retryLots.ForEach(x => this.ReleasedLots.Push(x));

                        return;
                    }

                    double maxPreBuildDays = lot.CurrentTarget.MoPlan.Demand.MaxEarlyDays;
                    List<APELot> shortLots = new List<APELot>();
                    shortLots.Add(lot);

                    // Max 선행 진행한 Lot이 Short 난 경우, 모든 대기 Lot들은 Short 처리.
                    //if (lot.PreBuildDays >= maxPreBuildDays && ATOption.Instance.ApplyPlanningPerBuffer)
                    //{
                    //    shortLots.AddRange(this.ReleasedLots);
                    //    this.ReleasedLots.Clear();
                    //}

                    this.ShortLots.AddRange(lot.OrgLotKeys);

                    foreach (var shortLot in shortLots)
                    {
                        shortLot.ShortCategory = lot.ShortCategory;
                        shortLot.ReasonName = lot.ReasonName;

                        DoShort(shortLot);
                    }

                    return;
                }

                OboLogic.Instance.DoCommit(lot);

                WritePlanInfos(lot);

                OutputWriter.Instance.WriteStageOutPlan(lot);

                var executionInfo = ATExecutionContext.Instance.CurrentExecutionInfo as PBOModuleExecutionInfo;
                executionInfo.AddOutStockWip(lot);
#warning InTarget 작성
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("RouteLogic_OnDone");
            }
        }

        public void ApplyYield(APELot lot)
        {
            double yield = lot.CurrentTarget.Yield;

            var newQty = lot.Qty * yield;

            lot.Qty = newQty;

            lot.CurrentQty = lot.Qty;
        }

        public void NextStep(APELot lot)
        {
            DateTime now = ATOption.Instance.PlanStartTime;

            if (lot.CurrentOper == null)
            {
                lot.CurrentOper = lot.MoveFirst(now);
            }
            else
            {
                lot.CurrentOper = lot.MoveNext(now);
                lot.LotState = LotState.Wait;
            }
        }

        public void SelectBom(APELot lot)
        {
            // Route Custom으로 할 수 있도록 Open
            if (lot.CurrentTarget != null)
                lot.CurrentRoute = lot.CurrentTarget.CurrentBomRoute;
        }

        public ATOperTarget GetOperTarget(APELot lot, BomType bomType)
        {
            if (lot.InitOper == lot.CurrentOper)
                return lot.InitOperTarget;

            var target = lot.CurrentTarget.Next;

            if (target.IsOut == "Y")
                target = target.Next;

            return target; 
        }
 
 
        private void WritePlanInfos(APELot lot)
        {
            if (lot.Plans.Count <= 0)
                return;
            
            var plans = lot.Plans;
            var lastPlan = plans.Last(); // 재공이 MinQty보다 크면 Short

            var lastOperTarget = WritePlanInfos(lot, plans, lastPlan, null);

            while (lot.AssemblyHistory.Count() != 0)
            {
                var assyInfo = lot.AssemblyHistory.First();
                lot.AssemblyHistory.Remove(assyInfo);

                foreach (var partLot in assyInfo.PartLots)
                {
                    plans = partLot.Plans;
                    WritePlanInfos(lot, plans, lot.FirstPlan, lastOperTarget);
                }
            }
        }

        private ATOperTarget WritePlanInfos(APELot lot, List<APEPlanInfo> planInfos, APEPlanInfo last, ATOperTarget lastTarget)
        {
            var executeInfo = ATExecutionContext.Instance.CurrentExecutionInfo as PBOModuleExecutionInfo;

            int idx = planInfos.Count() - 1;

            APEPlanInfo refPlan = last;

            // PlanInfo 적기
            while (idx >= 0)
            {
                var info = planInfos[idx];
                double allocLotQty = lot.Qty * info.OperTarget.GetBCumChangeRatio(lot.CurrentTarget);
                info.AllocationInfo.AllocLotQty = allocLotQty;
                info.AllocationInfo.UsedCapaQty = allocLotQty / info.AllocationInfo.UsedCapaRatio;

                //if (info.AddPlanInfos != null)
                //{
                //    foreach (var addPlan in info.AddPlanInfos.Values)
                //    {
                //        addPlan.AllocLotQty = lot.Qty * info.OperTarget.GetBCumChangeRatio(lot.CurrentTarget);
                //        addPlan.Lot = lot;
                //    }
                //}

                PlanCommonLogic.Instance.ApplyNoCarryover(info, refPlan);

                OutputWriter.Instance.WriteProdPlan(lot, info);

                // 확정 시점의 Allocated 정보 출력.
                PBOInterface.PlanControl.OnCompleteAllocated(lot, info);

                if (info.AllocationInfo.UsedCapacity != null)
                {
                    var resourcePlanInfo = info.Clone() as APEPlanInfo;
                    info.LotInfo = info.LotInfo.Clone() as LotInfo;

                    info.Bucket.AddPlan(resourcePlanInfo, null);
                }

                // 정보 집계.
                // 할당 확정
                info.LotInfo.ItemSiteBuffer.AddBufferPSISummary(info);

                var orgCopyTarget = info.OperTarget.DeepCopy();
                orgCopyTarget.TargetQty = lot.Qty * info.OperTarget.GetBCumChangeRatio(lot.CurrentTarget);
                orgCopyTarget.FWTargetDateTime = info.StartTime;
                orgCopyTarget.UsedQty = 0;

                var nextCopyTarget = info.OperTarget.Next.DeepCopy();
                nextCopyTarget.TargetQty = lot.Qty * info.OperTarget.Next.GetBCumChangeRatio(lot.CurrentTarget);
                nextCopyTarget.FWTargetDateTime = info.EndTime;
                nextCopyTarget.UsedQty = 0;

                if (lastTarget != null)
                    nextCopyTarget.Next = lastTarget;

                orgCopyTarget.Next = nextCopyTarget;

                lastTarget = orgCopyTarget;

                // PegCommonLogic.Instance.AddOperTarget(executeInfo, orgCopyTarget);

                if (info.LotInfo.LotType != LotCreateType.Assembly.ToString())
                {
                    if (string.IsNullOrEmpty(info.LotInfo.WipType) && info.LotInfo.IsFirstOperation)
                    {
                        ATExecutionContext.Instance.CurrentExecutionInfo.Kpi?.AddStageInQty(orgCopyTarget.TargetQty, orgCopyTarget.TargetDateTime);

                        OutputWriter.Instance.WriteInTargetPlan(orgCopyTarget);

                        executeInfo.StageInTargets.Add(orgCopyTarget);
                    }
                }

                OutputWriter.Instance.WriteTargetPlan(null, nextCopyTarget, nextCopyTarget.IsOut == "Y", allocLotQty);
                OutputWriter.Instance.WriteTargetPlan(null, orgCopyTarget, orgCopyTarget.IsOut == "Y", allocLotQty);

                BackwardCommonLogic.Instance.AddOperTarget(executeInfo, nextCopyTarget);
                BackwardCommonLogic.Instance.AddOperTarget(executeInfo, orgCopyTarget);

                AddAssemblyTarget(executeInfo, nextCopyTarget);
                AddAssemblyTarget(executeInfo, orgCopyTarget);

                idx--;

                refPlan = info;
            }
            
            return lastTarget;
        }

        public void AddAssemblyTarget(PBOModuleExecutionInfo executeInfo, ATOperTarget operTarget)
        {
            if (operTarget.CurrentBom?.BomType == BomType.Assembly && 
                operTarget.Oper.IsBuffer == false && 
                (operTarget.Oper.Route as ATRoute).FirstOper == operTarget.Oper && 
                operTarget.IsOut == "N")
            {
                var buffer = operTarget.CurrentBom.FromBuffer;

                List<ATOperTarget> targets;
                if (executeInfo.StageInAssemblyTargets.TryGetValue(buffer, out targets) == false)
                {
                    targets = new List<ATOperTarget>();
                    executeInfo.StageInAssemblyTargets.Add(buffer, targets);
                }

                var copyTarget = operTarget.DeepCopy();
                targets.Add(copyTarget);
            }
        }


        private void WriteShortPlanInfo(APELot shortLot)
        {
            if (OutputWriter.Instance.IsWriteOutput("SHORT_PRODUCTION_PLAN") == false)
                return ;

            var target = shortLot.CurrentTarget;

            // PlanInfo 적기
            foreach (var info in shortLot.Plans)
            {
                info.AllocationInfo.AllocLotQty = shortLot.Qty * info.OperTarget.GetBCumChangeRatio(target);

                OutputWriter.Instance.WriteShortProductPlan(shortLot, info);
            }

            if (shortLot.AssemblyHistory.Count() > 0)
            {
                foreach (var assyInfo in shortLot.AssemblyHistory)
                {
                    foreach (var detail in assyInfo.PartInfo.Keys)
                    {
                        var dic = assyInfo.PartInfo[detail];

                        foreach (var partKey in dic)
                        {
                            var partLot = partKey.Key;
                            var usedQty = partKey.Value;

                            foreach (var info in partLot.Plans)
                            {
                                info.AllocationInfo.AllocLotQty = shortLot.Qty * info.OperTarget.GetBCumChangeRatio(target);

                                OutputWriter.Instance.WriteShortProductPlan(shortLot, info);
                            }
                        }
                    }
                }
            }
        }

    }
}
