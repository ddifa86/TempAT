using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public partial class FWFactoryLogic
    {
        public static FWFactoryLogic Instance
        {
            get
            {
                return ServiceLocator.Resolve<FWFactoryLogic>();
            }
        }

        internal List<APELot> FilterLot(Dictionary<APELot, APELot> lots, ATWeightPreset filterPreset)
        {
            List<APELot> selectLots = new List<APELot>();

            var preset = filterPreset;
                        
            foreach (var lot in lots.Values.ToList())
            {
                bool bFilter = false;
                if (preset != null)
                {
                    foreach (var factor in preset.FactorList)
                    {
                        FilterLot method = factor.Method as FilterLot;
                        var result = method(lot, factor, ATExecutionContext.Instance.CurrentExecutionInfo.CurPhase);
                        if (result.Value == true)
                        {
                            bFilter = true;
                            lot.AddFilterValue(factor.Name, result);

                            break;
                        }
                    }
                }

                if (bFilter == false && lot.CurrentItemSiteBuffer.IsInputItemSiteBuffer(lot, lot.CurrentTarget) == false)
                {
                    bFilter = true;

                    if (lot.CurrentItemSiteBuffer.IsMaterialItemSiteBuffer == false)
                        lot.AddShortInfo(LateCategory.Material, LateReason.UnReleasedFERTMaterialShort.ToString(), lot.Qty, null, lot.LastStepTime, lot.LastStepTime);
                }

                if (bFilter == false)
                {
                    selectLots.Add(lot);
                    lots.Remove(lot);
                }
            }

            return selectLots;
        }


        public void StepOut(APELot lot)
        {
            FWInterface.LotControl.OnStepOut(lot);
        }

        public void StepIn(APELot lot)
        {
            lot.LotState = LotState.Run;

            LotInfo lotInfo = new LotInfo(lot);
            lot.CurrentPlanInfo = new APEPlanInfo(lotInfo, null);
            lot.CurrentPlanInfo.PrevPlanInfo = lot.Plans.LastOrDefault();

            FWInterface.LotControl.OnStepIn(lot);
        }

        public void ApplyYield(APELot lot)
        {
            var now = lot.LastStepTime;

            double yield = lot.CurrentOper.GetYield(now) ;// LotControl.Instance.GetYield(lot, now);

            var oldQty = lot.Qty;

            var newQty = lot.Qty * yield;

            lot.Qty = newQty;

            lot.CurrentQty = lot.Qty;
        }

        public void FindBom(APELot lot)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("RouteLogic_FindBom");
            try
            {
                if (lot.CurrentTarget != null)
                {
                    lot.CurrentRoute = lot.CurrentTarget.CurrentBomRoute;
                }
            }

            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("RouteLogic_FindBom");
            }
        }
         

        public void NextStep(APELot lot)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("RouteLogic_MoveNext");
            try
            {
                var now = FWFactory.Instance.NowDT;

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
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("RouteLogic_MoveNext");
            }
        }



        public bool IsFinished(APELot lot)
        {
            var isfinish = lot.IsFinish;

            return isfinish;
        }

        public void WritePlanInfos(APELot lot, List<APEPlanInfo> planInfos, APEPlanInfo last)
        {
            if (planInfos == null || planInfos.Count == 0)
                return;

            int idx = planInfos.Count - 1;
            APEPlanInfo refPlan = last;

            while (idx >= 0)
            {
                var info = planInfos[idx];
                if (info.IsWritePlanInfo == true)
                    break;

                PlanCommonLogic.Instance.ApplyNoCarryover(info, refPlan);

                OutputWriter.Instance.WriteProdPlan(lot, info);
                info.LotInfo.ItemSiteBuffer.AddPSISummary(info);

                idx--;

                refPlan = info;
            }
        }

        public void WritePlanInfos(APELot lot)
        {
            if (lot.Plans != null && lot.Plans.Count > 0)
            {
                var plans = lot.Plans;
                var lastPlan = plans.Last();

                WritePlanInfos(lot, plans, lastPlan);
            }
            
            while (lot.AssemblyHistory != null && lot.AssemblyHistory.Count != 0)
            {
                var assyInfo = lot.AssemblyHistory.First();
                lot.AssemblyHistory.Remove(assyInfo);

                foreach (var partLot in assyInfo.PartLots)
                {
                    WritePlanInfos(lot, partLot.Plans, lot.FirstPlan);
                    partLot.Dispose();
                }
            }
        }

        public void OnDone(APELot lot, LifeCycle cycle, string desc)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("RouteLogic_OnDone");
            try
            {
                if (cycle == LifeCycle.Remain && string.IsNullOrEmpty(lot.ShortCategory))
                    lot.ShortCategory = desc;

                OutputWriter.Instance.WriteLotHistory(lot, lot.Qty, cycle.ToString(), lot.LastStepTime, string.Empty);

                WritePlanInfos(lot);

                // 정상 종료
                if (cycle == LifeCycle.Disposal)
                {
                    OutputWriter.Instance.WriteStageOutPlan(lot);

                    var executionInfo = ATExecutionContext.Instance.CurrentExecutionInfo as PBFModuleExecutionInfo;
                    executionInfo.AddOutStockWip(lot);
                }
                else if (cycle == LifeCycle.Remain)
                {
                    OutputWriter.Instance.WriteShortReport(lot);
                }
            }
            finally
            {
                lot.Dispose();

                ATElapsedTimeChecker.Instance.AddElapsedTime("RouteLogic_OnDone");
            }
        }

        public BomType PartChange(APELot lot, bool isOut)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("RouteLogic_PartChange");
            try
            {
                if (lot.CurrentOper.IsBuffer)
                    return BomType.None;

                var line = FWFactory.Instance.DefaultLine;

                var type = ATInputData.Boms.GetChangeBomType(lot.CurrentBom, lot.CurrentOper, isOut);

                switch (type)
                {
                    case BomType.Assembly:
                        
                        // 초기 조립 공정인 경우.
                        if (lot.InitOper == lot.CurrentOper)
                            return BomType.None;

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

                            ATSplitInfo splitInfo = new ATSplitInfo(lot, orgQty);
                            CreateSplitLot(splitInfo);

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

        private void CreateSplitLot(ATSplitInfo splitInfo)
        {
            /*
            Source Target에 의해 생성된 BinnedWip 을 그대로 생성하여 흘리면, 실제 존재하지 않는 Wip으로 인해 실적이 차감되어
            이력 추적의 어려움이 생김.
            */
            var lot = splitInfo.OrgLot;
            var line = FWFactory.Instance.DefaultLine;

            var bom = splitInfo.CurrentBom;
            var detail = splitInfo.CurrentBomDetail;

            ATItemSiteBuffer soItem = splitInfo.CurrentTarget.SODemand.ItemSiteBuffer; //pegPart.MoMaster.ItemBuffer;
            APETarget pegTarget = splitInfo.CurrentTarget.PegTarget;

            var co_by_Details = bom.GetCoByBomDetail(soItem, detail.ToItemSiteBuffer);
            List<APELot> binnedLots = new List<APELot>();
            //Dictionary<ATBomDetail, double> lowerDetailInfos = new Dictionary<ATBomDetail, double>();

            //foreach (var lower in co_by_Details)
            //{
            //    if (lowerDetailInfos.ContainsKey(lower) == false)
            //        lowerDetailInfos.Add(lower, lower.GetToQty(lot.CurrentTarget.TargetDateTime));
            //}

            foreach (var lower in co_by_Details)
            {
                double splitRatio = lower.GetToQty(splitInfo.CurrentTarget.TargetDateTime);

                if (splitRatio > 0)
                {
                    double binnedQty = splitInfo.Qty * splitRatio;

                    binnedLots = GenBinnedLots(splitInfo, splitInfo.CurrentTarget, binnedQty, lower, splitRatio);

                    binnedLots.ForEach(x => line.RemainLots.Add(x));
                }
            }

            string newBatches = string.Join(", ", line.RemainLots.Select(x => string.Format("{0}({1})", x.LotID, x.Qty)));

            OutputWriter.Instance.WriteLotHistory(lot, splitInfo.Qty, LotCreateType.SplitByBom.ToString(), lot.LastStepTime, string.Format("Split Lots : {0}", newBatches));
        }

        public List<APELot> GenBinnedLots(ATSplitInfo info, ATOperTarget mapTarget, double binnedQty, ATBomDetail lowerBomdetail, double splitRatio)
        {
            var executionInfo = ATExecutionContext.Instance.CurrentExecutionInfo as PBFModuleExecutionInfo;
            var binPeggedInfo = executionInfo.BinPeggedInfos;

            List<APELot> binnedLots = new List<APELot>();
        
            // Source Target에 의해 생성된 BinnedWip 중 Unpeg된 정보들은 별도로 생성하지 않음.
            // Pegging 시점에 Source Target에 의해 생성된 BinnedWip 중 Pegging된 WipList

            string key = mapTarget.TargetID + lowerBomdetail.ToItemID + lowerBomdetail.ToBufferID;

            if (binPeggedInfo.TryGetValue(key, out List<APEWip> binnedwips) == false)
                binnedwips = new List<APEWip>();

            // sorting?? => maptarget의 duedate가빠른 순 우선 생성.
            // binnedwips.sort();

            // binnedWip 중 Pegging 된 Lot들
            foreach (var binwip in binnedwips.ToList())
            {
                if (binwip.RemainQty <= 0)
                    continue;

                double splitLotQty = Math.Min(binnedQty, binwip.RemainQty);
#warning FEAction 포인트 LotID

                string lotID = LotHelper.GeneratLotID(ATConstants.BIN_WIP_PREFIX, binwip.MapTarget.TargetID);
                var binnedlot = ObjectMapper.CreateLot(lotID, splitLotQty, binwip.Oper, binwip.MapTarget, binwip, binwip.CreationType);
                binnedlot.OrgLot = info.OrgLot;
                binwip.AvailableTime = info.OrgLot.LastStepTime;
                binnedlot.LastStepTime = info.OrgLot.LastStepTime;

                binnedLots.Add(binnedlot);

                binwip.ItemSiteBuffer.AddBufferPSISummaryPlanWip(binwip);

                // SplitLot 정보 등록
                OutputWriter.Instance.WriteSplitLog(info, info.OrgLot, binwip, lowerBomdetail, splitRatio, splitLotQty);

                // 생성한 수량만큼 BinWip 수량 차감. 중복 생성 방지용.
                binwip.Act += splitLotQty;

                // 생성한 수량만큼 Split Lot 생성 수량 차감.
                binnedQty -= splitLotQty;

                if (binwip.RemainQty <= ATOption.Instance.MinimumAllocationQuantity)
                {
                    string binnedWipKey = binwip.SourceTargetID + binwip.ItemID + binwip.OperID;
                    // Foreach List 내에 지워서 ToList(418)에서 지우도록 처리했는데 확인 필요
                    if (binPeggedInfo.TryGetValue(binnedWipKey, out var value))
                        value.Remove(binwip);
                }

                if (binnedQty <= 0)
                    break;
            }

            if (binnedQty > 0)
            {
                // Lot으로 만들어서 흘려서 자연스럽게 정보가 남도록 처리가 필요함.
                // 여기에 남는 녀석에 대한 정보를 남기자.??
                ATDemand demand = info.CurrentTarget.SODemand;
                ATOperTarget target = new ATOperTarget(lowerBomdetail.FromSite, lowerBomdetail.ToItem, lowerBomdetail.ToBuffer,
                    lowerBomdetail.ToBuffer, binnedQty, info.CurrentTarget.TargetDateTime, demand.MaxLateDays, demand.MaxEarlyDays, false);

                target.CurrentBom = lowerBomdetail.Bom;
                target.CurrentBomDetail = lowerBomdetail;
                if(lowerBomdetail.Bom.BomRoutes.Count() > 0)
                    target.CurrentBomRoute = lowerBomdetail.Bom.BomRoutes.FirstOrDefault().Route; //bom.BomRoutes.First().Route;

                string lotID = LotHelper.GeneratLotID(ATConstants.BIN_WIP_PREFIX, target.TargetID);

                Inputs.WIP entity = new Inputs.WIP();
                entity.WIP_ID = lotID;
                entity.WIP_QTY = binnedQty;
                entity.AVAILABLE_DATETIME= info.OrgLot.LastStepTime;
                entity.TRACK_IN_DATETIME = entity.AVAILABLE_DATETIME;

                var wipinfo = ObjectMapper.CreateWipInfo(entity, WipType.Inventory, LotState.Wait, lowerBomdetail.ToSite, lowerBomdetail.ToItem, null, lowerBomdetail.ToBuffer as ATOperation, null, ATExecutionContext.Instance.CurrentStage, lowerBomdetail.ToBuffer);

                APEWip binnedWip = ObjectMapper.CreatePlanWip(wipinfo, LotCreateType.SplitByBom);
                binnedWip.MapTarget = target;
                binnedWip.AvailableTime = info.OrgLot.LastStepTime;

                var remainlot = ObjectMapper.CreateLot(lotID, binnedWip.RemainQty, binnedWip.Oper, binnedWip.MapTarget, binnedWip, binnedWip.CreationType);
                remainlot.OrgLot = info.OrgLot;
                remainlot.IsNoTargetSplitLot = true;

                binnedLots.Add(remainlot);

                OutputWriter.Instance.WriteSplitLog(info, info.OrgLot, binnedWip, lowerBomdetail, splitRatio, binnedQty);
            }

            return binnedLots;
        }

        public ATOperTarget GetOperTarget(APELot lot)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("RouteLogic_GetOperTarget");
            try
            {
                if (lot.InitOper == lot.CurrentOper)
                    return lot.InitOperTarget;

                var target = lot.CurrentTarget.Next;

                if (target == null)
                    return null;

                if (target.IsOut == "Y")
                    target = target.Next;

                // Key를 통해 Target을 가져오는 로직 추가 필요.
                return target;
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("RouteLogic_GetOperTarget");
            }
        }

        public string GetLotGroupKey(APELot lot)
        {
            string key = null;

            // RuleAction 도출 작업
            var factors = FWFactory.Instance.DefaultLotGroupKeyPreset;
            if (factors == null || factors.FactorList.Count <= 0)
            {
                key = lot.CurrentTarget.CurrentItemBufferKey;
            }
            else
            {
                foreach (var factor in factors.FactorList)
                {
                    var method = factor.Method as LotGroupKey;
                    if (key == null)
                        key = method(lot);
                    else
                        key += "@" + method(lot); 
                }
            }

            return key;
        }
    }
}