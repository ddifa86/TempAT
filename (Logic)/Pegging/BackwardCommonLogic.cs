using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Inputs;
using Mozart.SeePlan.Pegging;
using Mozart.Simulation.Engine;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Pegger;
using Mozart.SeePlan.Aleatorik.ObyO;
using Mozart.SeePlan.Aleatorik.Logic;

namespace Mozart.SeePlan.Aleatorik
{
    public partial class BackwardCommonLogic
    {
        public IPegControl PegControl { get; set; }

        public static BackwardCommonLogic Instance
        {
            get
            {
                return ServiceLocator.Resolve<BackwardCommonLogic>();
            }
        }

        public BackwardCommonLogic()
        {
        }

        #region Initialize
        public Dictionary<IComparable, ATMoMaster> CreateMoMaster(List<ATDemand> demands)
        {
            Dictionary<IComparable, ATMoMaster> dic = new Dictionary<IComparable, ATMoMaster>();

            foreach (var demand in demands)
            {
                if (string.IsNullOrEmpty(ATOption.Instance.DemandItems) == false
                        && ATOption.Instance.DemandItems.Contains(demand.ItemID) == false)
                    continue;

                if (ATOption.Instance.DemandDueDate < demand.DueDateTime)
                    continue;

                //dic.Add(mm.Key, mm);
                string key = demand.ID;

                ATMoMaster mm;
                if (dic.TryGetValue(key, out mm) == false)
                {
                    mm = ObjectMapper.CreateMoMaster(key, demand.ItemSiteBuffer, demand.SiteID);
                    dic.Add(key, mm);
                }

                var mo = ObjectMapper.CreateMoPlan(mm, demand, demand.DueDateTime);

                mm.MoPlanList.Add(mo);
            }

            return dic;
        }

        public HashSet<APEPegPart> CreatePegPart(IReadOnlyCollection<ATMoMaster> moMasters)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("CreatePegPart");
            try
            {
                HashedSet<APEPegPart> pegparts = new HashedSet<APEPegPart>();

                foreach (var mm in moMasters)
                {
                    var pp = CreatePegPart(mm);
                    pegparts.Add(pp);
                }

                return pegparts;
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("CreatePegPart");
            }
        }

        /// <summary>
        /// 참가 대상 PegPart 정보 (RuleSet 필요)
        /// </summary>
        /// <param name="pegParts"></param>
        /// <returns></returns>
        internal HashSet<APEPegPart> FilterTarget(HashSet<APEPegPart> pegParts, ATWeightPreset filterPreset)
        {
            HashedSet<APEPegPart> selectParts = new HashedSet<APEPegPart>();

            var preset = filterPreset;

            if (preset == null)
            {
                selectParts.AddRange(pegParts);
                return selectParts;
            }

            foreach (var pegPart in pegParts)
            {
                bool bFilter = false;
                foreach (var factor in preset.FactorList)
                {
                    FilterTarget method = factor.Method as FilterTarget;
                    var result = method(pegPart, factor);
                    if (result.Value == true)
                    {
                        bFilter = true;
                        pegPart.AddFilterValue(factor.Name, result);

                        OutputWriter.Instance.WriteInitDemandLog(pegPart, 0);
                        break;
                    }
                }

                if (bFilter == false)
                    selectParts.Add(pegPart);
            }

            return selectParts;
        }

        public APEPegPart CreatePegPart(ATMoMaster mm)
        {
            var pp = ObjectMapper.CreatePegPart(mm, mm.ItemSiteBuffer); //SiteID??

            // OnCreatePegPart

            foreach (ATMoPlan mo in pp.MoMaster.MoPlanList)
            {
                // OnFilterPegTarget

                // TargetID 를 만들어야하나..?
                var targetID = mo.ID;

                //var targetID = PeggingManager.Instance.CreateTargetID(currentStage.StageID);
                var pt = ObjectMapper.CreatePegTarget(pp, mo, targetID);
                // OnCreatePegTarget

                // 에러 발생
                pp.AddPegTarget(pt);
            }

            return pp;
        }

        public void SetAlignQueue(HashSet<IAlignQueue> queues, Dictionary<string, ATBuffer> buffers)
        {
            if (ATOption.Instance.ApplyFirstTarget)
            {
                ATAlignQueue queue = new ATAlignQueue(ATConstants.DefaultKey, null);
                queues.Add(queue);
            }
            else
            {
                foreach (var buffer in buffers)
                {
                    ATAlignQueue queue = new ATAlignQueue(buffer.Value.Sequence * -1, buffer.Value);
                    queues.Add(queue);
                }
            }
        }

        public IComparable GetAlignKey(ITargetGroup part)
        {
            IComparable key = null;

            if (ATOption.Instance.ApplyFirstTarget)
            {
                var pegPart = part as APEPegPart;
                if (pegPart.CurrentOperation == pegPart.SampleTarget.SoDemand.ItemSiteBuffer.Buffer)
                    key = ATConstants.DefaultKey;
            }
            else
            {
                if (part.Step != null)
                {
                    var operation = part.Step as ATOperation;
                    if (operation.IsBuffer)
                        key = operation.Sequence * -1;
                }
            }

            return key;
        }

        internal void CreatePlanWip()
        {
            if (ATOption.Instance.DiscardWip == false)
            {
                var factors = ATRuleAgent.Instance.CurrentRuleSet.GetRule(RulePoint.WipKey, CallType.Init);

                var compareWip = ATRuleAgent.Instance.CurrentRuleSet.GetRule(RulePoint.CompareWip, CallType.Level, 1);

                var wipInfos = ATInputData.Wips.GetWipInfos();

                foreach (var wipInfo in wipInfos)
                {
                    if (ATExecutionContext.Instance.CurrentStage != wipInfo.Stage)
                        continue;

                    var planWip = ObjectMapper.CreatePlanWip(wipInfo, LotCreateType.Wip);

                    AddPlanWipByKey(planWip, factors, compareWip);
                }
            }
        }

        internal void AddPlanWipByKey(APEWip planWip, ATWeightPreset getWipSetKeyFactors, ATWeightPreset compareWipPreset)
        {
            List<string> keys = new List<string>();

            if (getWipSetKeyFactors == null || getWipSetKeyFactors.FactorList.Count == 0)
            {
                keys.Add(planWip.ItemSiteBuffer.Key);
            }
            else
            {
                foreach (var factor in getWipSetKeyFactors.FactorList)
                {
                    var method = factor.Method as WipKey;
                    keys.AddRange(method(planWip));
                }
            }

            var oper = planWip.Oper;

            foreach (var key in keys)
            {
                oper.PlanWipQueue.AddWipSet(key, planWip, compareWipPreset);
            }

            planWip.Keys = keys;

            planWip.ItemSiteBuffer.AddPlanWip(planWip);

            ATInputData.Wips.AddPlanWip(planWip);


            if (ATExecutionContext.Instance.CurrentExecutionInfo is PBOModuleExecutionInfo)
            {
                planWip.ItemSiteBuffer.AddBufferPSISummaryPlanWip(planWip);
                PBOInterface.ModuleControl.OnCreatePlanWip(planWip);
            }
            else
            {
                PBBInterface.ModuleControl.OnCreatePlanWip(planWip);
            }
        }
        #endregion

        #region BOM
        public List<ATBom> FilterBom(List<ATBom> boms, APEPegPart pegPart, List<ATBom> filterBoms, ATWeightPreset preset)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("FilterBom");
            try
            {
                List<ATBom> selectBom = new List<ATBom>();

                if (preset == null)
                {
                    selectBom.AddRange(boms);
                    return selectBom;
                }

                foreach (var bom in boms)
                {
                    bool bFilter = false;
                    foreach (var factor in preset.FactorList)
                    {
                        FilterBom method = factor.Method as FilterBom;
                        var result = method(bom, factor, pegPart);

                        if (result.Value == true)
                        {
                            bom.AddFilterValue(factor.Name, result);

                            filterBoms.Add(bom);
                            // filter사유 남기기.
                            bFilter = true;
                            break;
                        }
                    }

                    if (bFilter == false)
                        selectBom.Add(bom);
                }

                return selectBom;
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("FilterTargetGroup");
            }
        }

        public ATRoute FindRoute(APEPegPart pegPart)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("FindRouteInfo");
            try
            {
                var route = ATInputData.Boms.FindRoute(pegPart, pegPart.CurrentBom);
                route = RefPlanLogic.Instance.FindRefPlanRoute(pegPart, route);

                if (route == null)
                    return null;

                return route;
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("FindRouteInfo");
            }
        }

        public void ClearBomInfo(APEPegPart pegPart)
        {
            pegPart.NextBom = pegPart.CurrentBom;
            pegPart.NextRoute = pegPart.CurrentRoute;
            pegPart.NextBomDetail = pegPart.CurrentBomDetail;

            pegPart.CurrentBom = ATBom.NULL;
            pegPart.CurrentRoute = null;
            pegPart.CurrentBomDetail = null;
        }

        public List<ITargetGroup> SelectBom(ITargetGroup part, APEShortManager shortManager)
        {
            List<ATSelectBomInfo> selectInfos;
            ATSelectBomInfo shortInfo = null;
            List<ITargetGroup> pegParts = new List<ITargetGroup>();

            var pegPart = part as APEPegPart;
            var pegTarget = pegPart.SampleTarget;
            var step = part.Step as ATOperation;

            APESelectBomContext context = new APESelectBomContext(pegPart);

            if (step.IsBuffer == false)
                return pegParts;

            if (IsInputBuffer(pegPart.CurrentItemSiteBuffer, pegPart.CurrentOperation))
            {
                pegParts.Add(pegPart);
                return pegParts;
            }

            selectInfos = GetSelectBom(pegPart, context);

            if (selectInfos == null || selectInfos.Count == 0)
            {
                ATShortInfo info = new ATShortInfo(pegPart, pegTarget.CurOperTarget, pegTarget.RemainQty, ShortCategory.BOM.ToString(), "");

                shortManager.AddShortInfo(info, true);

                pegTarget.AddShortInfo(LateCategory.Tat, LateReason.NoBwBomPathShort.ToString(), pegTarget.RemainQty, null, pegTarget.TargetDateTime, pegTarget.TargetDateTime);
                pegParts.Add(pegPart);

                return pegParts;
            }

            if (pegTarget.RemainQty > ATOption.Instance.MinimumAllocationQuantity)
            {
                var type = SelectType.Short;
                var boms = pegPart.GetPrevBoms();

                // 잔여 타겟에 대한 재할당 여부
                // ReAlloc의 경우, 해당 Step부터 다시 시작.
                // Short의 경우, 다음 Phase에 다시 할당 시도.
                if (PegControl.IsReSelectBom(pegTarget, boms, context))
                    type = SelectType.Retry;

                ATSelectBomInfo info = new ATSelectBomInfo(pegTarget, ATBom.NULL, type, context);
                info.Qty = pegTarget.RemainQty;
                pegTarget.RemainQty = 0;

                shortInfo = info;
            }

            foreach (var selectBom in selectInfos)
            {
                APEPegPart copied = pegPart;

                if (selectInfos.Count() > 1)
                    copied = pegPart.Clone() as APEPegPart;

                APEPegPart branch = ApplyBom(copied, selectBom);
                pegParts.Add(branch);
            }

            if (shortInfo != null)
            {
                APEPegPart newPegPart = ApplyBom(pegPart.Clone() as APEPegPart, shortInfo);

                if (shortInfo.SelectType == SelectType.Short)
                {
                    var newPegTarget = newPegPart.SampleTarget;
                    ATShortInfo info = new ATShortInfo(newPegPart, newPegTarget.CurOperTarget, newPegTarget.RemainQty, ShortCategory.BOM.ToString(), "");

                    shortManager.AddShortInfo(info, true);

                    newPegTarget.AddShortInfo(LateCategory.Tat, LateReason.NoBwBomPathShort.ToString(), newPegTarget.RemainQty, null, newPegTarget.TargetDateTime, newPegTarget.TargetDateTime);
                }
                else if (shortInfo.SelectType == SelectType.Retry)
                {
                    newPegPart.IsRetryPegPart = true;
                    pegParts.Insert(0, newPegPart);
                }
            }

            return pegParts;
        }

        public List<ATSelectBomInfo> GetSelectBom(APEPegPart pegPart, APESelectBomContext context)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("Select Bom");
            try
            {
                var pegTarget = pegPart.Sample as APETarget;

                var boms = pegPart.GetPrevBoms();

                boms.ForEach(x => x.InitFactorValue());

                PegControl.OnPrepareSelectBom(pegPart, boms, context);

                var ruleSet = ATRuleAgent.Instance.CurrentRuleSet;
                var preset = ruleSet.GetRule(RulePoint.FilterBom, CallType.Operation);

                List<ATBom> filterBoms = new List<ATBom>();
                List<ATBom> candidateBoms = FilterBom(boms, pegPart, filterBoms, preset);

                preset = ruleSet.GetRule(RulePoint.CompareBom, CallType.Operation);
                candidateBoms.Sort(new ATBomComparer(preset, pegPart, context));

                var selectBoms = DoSelectBom(pegTarget, candidateBoms, context);
                var selectBom = selectBoms.FirstOrDefault()?.Bom;

                OutputWriter.Instance.WriteCompareBomLog(pegPart, boms, candidateBoms, filterBoms, selectBom, context);

                return selectBoms;
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("Select Bom");
            }
        }

        public List<ATSelectBomInfo> DoSelectBom(APETarget target, List<ATBom> boms, APESelectBomContext context)
        {
            List<ATSelectBomInfo> selectBoms = new List<ATSelectBomInfo>();

            foreach (var bom in boms)
            {
                if (target.RemainQty <= ATOption.Instance.MinimumAllocationQuantity)
                    break;

                ATSelectBomInfo info = new ATSelectBomInfo(target, bom, SelectType.Select, context);

                double availQty = PegControl.AvailableQty(bom, target, boms, context);

                if (availQty <= ATOption.Instance.MinimumAllocationQuantity)
                    continue;

                info.Qty = availQty;

                PegControl.OnSelectedBom(info, bom, target, boms, context);

                selectBoms.Add(info);

                target.RemainQty -= availQty;

                // 다른 봄에다가 분배여부.
                if (PegControl.IsMoreSelecteBom(bom, target, boms, context) == false)
                    break;
            }

            return selectBoms;
        }

        public APEPegPart ApplyBom(APEPegPart pegPart, ATSelectBomInfo bomInfo)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("ApplySiteAllocateInfos");
            try
            {
                var buffer = pegPart.CurrentOperation as ATBuffer;

                pegPart.SampleTarget.Qty = bomInfo.Qty;

                // 재할당하지 않는 pegPart의 경우.
                if (bomInfo.SelectType == SelectType.Select)
                {
                    pegPart.CurrentBom = bomInfo.Bom;

                    if (pegPart.CurrentBom != ATBom.NULL)
                        pegPart.CurrentRoute = FindRoute(pegPart);

                    // 추후 정리 작업 필요.
                    pegPart.AllocPathID += "_" + bomInfo.Bom.BomID;
                }

                return pegPart;
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("ApplySiteAllocateInfos");
            }
        }
        #endregion

        #region Pegging
        public List<string> GetPeggingKey(ITargetGroup pegPart, APEPegContext context)
        {
            List<string> keys = new List<string>();

            var factors = context.PeggingKeyPreset;
            if (factors == null || factors.FactorList.Count <= 0)
            {
                var pegTarget = pegPart.Sample as APETarget;
                keys.Add(pegTarget.ItemSiteBuffer.Key);
            }
            else
            {
                foreach (var factor in factors.FactorList)
                {
                    var method = factor.Method as PeggingKey;
                    keys.AddRange(method(pegPart, context));
                }
            }

            return keys;
        }

        public List<APETargetGroup> CreateTargetGroup(ITargetGroup part, ATRuleSet ruleSet)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("CreateTargetGroup");
            try
            {
                List<ITargetGroup> parts = new List<ITargetGroup>();

                if (part is IMergedTargetGroup)
                    parts.AddRange((part as IMergedTargetGroup).Items);
                else
                    parts.Add(part);

                var ppMap = new Dictionary<IComparable, APETargetGroup>();

                foreach (APEPegPart pp in parts)
                {
                    if (ATOption.Instance.ApplyExcessLogging == false && pp.HasRemainTarget == false)
                        continue;

                    IComparable key = null;
                    var factors = ruleSet.GetRule(RulePoint.TargetGroupKey, CallType.Operation);
                    if (factors == null || factors.FactorList.Count <= 0)
                    {
                        key = pp.MoMaster.Key;
                    }
                    else
                    {
                        foreach (var factor in factors.FactorList)
                        {
                            var method = factor.Method as TargetGroupKey;
                            if (key == null)
                                key = method(pp);
                            else
                                key += "@" + method(pp);
                        }
                    }

                    if (ppMap.TryGetValue(key, out APETargetGroup mpp) == false)
                    {
                        mpp = new APETargetGroup(key);
                        ppMap.Add(key, mpp);
                        mpp.CurrentOperation = pp.CurrentOperation;
                    }

                    mpp.Merge(pp);
                }

                var targetGroups = ppMap.Values.ToList();
                PegControl.OnCreateTargetGroup(targetGroups, part.Step as ATOperation);

                return targetGroups;
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("CreateTargetGroup");
            }
        }

        public List<APETargetGroup> FilterTargetGroup(IEnumerable<APETargetGroup> targetGroups, APEPegContext context)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("FilterTargetGroup");
            try
            {
                List<APETargetGroup> selectTargetGroup = new List<APETargetGroup>();

                var preset = context.FilterTargetGroupPreset;

                foreach (var targetGroup in targetGroups)
                {
                    bool isFilter = false;

                    if (targetGroup.Targets.Count <= 0)
                        isFilter = true;

                    if (isFilter == false && preset != null)
                    {
                        foreach (var factor in context.FilterTargetGroupPreset.FactorList)
                        {
                            FilterTargetGroup method = factor.Method as FilterTargetGroup;
                            if (method(targetGroup, factor, context).Value == true)
                            {
                                isFilter = true;
                                break;
                            }
                        }
                    }

                    if (isFilter == false)
                        selectTargetGroup.Add(targetGroup);
                }

                return selectTargetGroup;
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("FilterTargetGroup");
            }
        }

        /// <summary>
        /// TargetGroup 에 Pegging 될 수 있는 Wip의 정보들을 구성
        /// TargetGroup 내 Kit이 여러개인 경우에는
        /// </summary>
        /// <param name="wips"></param>
        /// <param name="targetGroup"></param>
        /// <param name="oper"></param>
        /// <returns></returns>
        public ATKitInfo CreateKitInfo(APETargetGroup targetGroup, ATOperation oper, APEPegContext context)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("CreateKitInfo");
            try
            {
                HashSet<APEWip> selectedWips = new HashSet<APEWip>();
                ATKitInfo kitInfo = null;

                List<string> keys = GetPeggingKey(targetGroup, context);
                context.PeggingKeys = keys;

                foreach (var key in keys)
                {
                    var wips = oper.PlanWipQueue.GetWipSet(key, context.IsRun);
                    selectedWips.AddRange(wips);
                }

                if (selectedWips.Count == 0)
                    return kitInfo;

                selectedWips.ForEach(x => x.MapCount++);

                kitInfo = new ATKitInfo(keys, selectedWips);

                return kitInfo;
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("CreateKitInfo");
            }
        }

        public void SortedTargetGroup(List<APETargetGroup> groups, APEPegContext context)
        {
            groups.QuickSort<APETargetGroup>(new APETargetGroupComparer(context));
        }

        public List<APEWip> DoPegging(ITargetGroup part, bool isOut)
        {
            var oper = part.Step as ATOperation;
            var ruleSet = ATRuleAgent.Instance.GetRuleSet(oper);
            string peggingGroupKey = null;

            List<APETargetGroup> targetGroups;
            List<APEWip> peggedWips = new List<APEWip>();

            if (part is IMergedTargetGroup)
                peggingGroupKey = (part as IMergedTargetGroup).Key.ToString();

            targetGroups = CreateTargetGroup(part, ruleSet);

            int maxLevel = ruleSet.Level;
            for (int curLevel = 0; curLevel < maxLevel; curLevel++)
            {
                //bool isKitPegging = false;
                PegProcedureType pegType = PegProcedureType.TargetFirstSelected;
                // PegType에 따른 로직은 없어서 내용 삭제(if문 삭제) LWS

                APEPegContext context = new APEPegContext(curLevel + 1, oper, pegType, ruleSet, isOut, peggingGroupKey);

                PegControl.OnStartLevel(curLevel, maxLevel, targetGroups, context, isOut);

                foreach (var selGroup in targetGroups)
                {
                    SortedTargetInTargetGroup(selGroup, context, ruleSet);
                }

                var selectedGroups = FilterTargetGroup(targetGroups, context);
                for (int idx = 0; idx < selectedGroups.Count; idx++)
                {
                    var group = selectedGroups[idx];

                    group.KitInfo = CreateKitInfo(group, oper, context);

                    if (group.KitInfo == null)
                        selectedGroups.RemoveAt(idx--);
                }

                while (selectedGroups.Count > 0)
                {
                    // KitPegging Logic 삭제 LWS 
                    SortedTargetGroup(selectedGroups, context);

                    var selGroup = selectedGroups.FirstOrDefault();
                    selectedGroups.Remove(selGroup);

                    context.CurPegPart = selGroup;

                    var wips = selGroup.KitInfo.Items;
                    context.PlanWips = wips;

                    if (ATOption.Instance.ApplyReSortWip)
                        wips.Sort(new ATPlanWipComparer(context));

                    PegControl.OnPrepareVirtualPegging(selGroup, wips, context, isOut);

                    var pegWips = DoPegging(selGroup, selGroup.KitInfo, context);

                    peggedWips.AddRange(pegWips);
                }

                PegControl.OnEndLevel(curLevel, maxLevel, targetGroups, context, isOut);
            }

            return peggedWips;
        }

        public List<APEWip> DoPegging(APETargetGroup targetGroup, ATKitInfo wipGroup, APEPegContext context)
        {
            List<APEWip> peggedWips = new List<APEWip>();
            List<ITarget> targets = new List<ITarget>(targetGroup.Targets);

            for (int targetIdx = 0; targetIdx < targets.Count; targetIdx++)
            {
                var target = targets[targetIdx] as APETarget;

                if (target.Qty <= 0)
                {
                    targetGroup.RemoveTarget(target);
                    continue;
                }

                context.PeggingKey = wipGroup.Keys;
                context.TargetGroupKey = targetGroup.Key.ToString();

                var wips = wipGroup.Items;

                for (int wipIdx = 0; wipIdx < wips.Count; wipIdx++)
                {
                    APEWip wip = wips[wipIdx];
                    if (wip.RemainQty <= 0)
                        continue;

                    if (FilterWip(wip, target, context))
                        continue;

                    double availPegQty = wip.RemainQty;
                    double adjustPegQty = Math.Min(target.Qty, availPegQty);

                    context.PegQty = adjustPegQty;
                    context.PegSequence++;

                    target.Qty -= adjustPegQty;
                    wip.VirtualAct += adjustPegQty;

                    // KTR : 왜 Pegging을 했는데 누적 Wip 수량이 늘어나지..??
                    wip.ItemSiteBuffer.WipQueue.VirtualCumWipQty += adjustPegQty;

                    foreach (var isb in wip.CumItemSiteBuffes)
                    {
                        if (isb.WipQueue.CumWipQueue.TryGetValue(wip.WipInfo.LotID, out var value))
                            isb.WipQueue.VirtualCumWipQty += adjustPegQty.ConvertValue(value.FromQty, value.ToQty, PlanType.Forward);
                    }
                    // 여기까지는 별도집계 로직으로 먼가 함수를 만들어야 할듯.

                    if (wip.RemainQty <= ATOption.Instance.MinimumAllocationQuantity)
                        wips.RemoveAt(wipIdx--);

                    // KTR : 해당 로직도 한번 검토 해보고 추후 개선을 해야할 듯.
                    // Source만 봐서는 잘 이해가 되지 않음.
                    // Pegging 된 Wip 정보를 APEWip으로 생성해야할까? 다른 Object를 만들어야 할까..
                    // Pegging 된 이력 정보를 가지고 있는 별도의 Class가 더 적합해 보임.
                    // class PeggingInfo {} => 추후 Pegging 정보 출력 시 사용.
                    var peggedWip = wip.ShallowCopy(target.CurOperTarget, adjustPegQty);
                    peggedWip.RealWip = wip;
                    peggedWip.PegContext = context.DeepCopy(); 
                    peggedWips.Add(peggedWip);

                    if (target.Qty <= 0)
                        break;
                }

                if (target.Qty <= ATOption.Instance.MinimumAllocationQuantity)
                {
                    targetGroup.RemoveTarget(target);
                    continue;
                }
            }

            return peggedWips;
        }
        #endregion

        public string SetUnpegReasonDetail(string reasonDetail, APEWip wip)
        {
            var bomRoutes = wip.ItemSiteBuffer.PrevBoms.Keys.Where(x => x.BomRoutes.Where(y => y.Property["product_type"] == "MINIMIZE").Count() > 0);
            List<string> routes = new List<string>();
            foreach (var item in bomRoutes)
            {
                routes.AddRange(item.BomRoutes.Select(x => x.RouteID));
            }
            reasonDetail += routes.Count() > 0 ? string.Format("{0}Minimize to use bom routing = {1}", string.IsNullOrEmpty(reasonDetail) ? string.Empty : ";", string.Join(", ", routes)) : string.Empty;

            return reasonDetail;
        }

        public void SetBomPathSearchLog(APEPegPart pegPart)
        {
            var bomDetail = pegPart.CurrentBomDetail;
            var so = pegPart.SampleTarget.SoDemand;

            string log = string.Format("{0}/{1}/{2}^{3}/{4}", bomDetail.ToBufferID, bomDetail.ToItemSiteBuffer.Key, bomDetail.BomID, bomDetail.FromBufferID, bomDetail.FromItemSiteBuffer.Key);

            so.SoPathLog.Add(log);
        }

        internal void AddBinnedWip(APEWip binnedWip)
        {
            var defaultRuleSet = ATRuleAgent.Instance.CurrentRuleSet;
            var factors = ATRuleAgent.Instance.CurrentRuleSet.GetRule(RulePoint.WipKey, CallType.Init);

            var compareWip = defaultRuleSet.GetRule(RulePoint.CompareWip, CallType.Level, 1);
            AddPlanWipByKey(binnedWip, factors, compareWip);
        }

        internal bool FilterWip(APEWip wip, APETarget target, APEPegContext context)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("FilterWip");
            try
            {
                bool bFilter = false;

                if (context.FilterWipPreset == null)
                    return bFilter;

                foreach (var factor in context.FilterWipPreset.FactorList)
                {
                    FilterWip method = factor.Method as FilterWip;

                    if (method(wip, factor, target, context).Value == true)
                    {
                        // filter사유 남기기.
                        bFilter = true;
                        break;
                    }
                }

                return bFilter;
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("FilterWip");
            }
        }

        internal bool IsInputBuffer(ATItemSiteBuffer itemSiteBuffer, ATOperation oper)
        {
            var stage = ATExecutionContext.Instance.CurrentExecutionInfo.Stage;

            if (oper.IsBuffer == false)
                return false;

            if (oper == stage.BufferRoute.FirstOper)
                return true;

            if (itemSiteBuffer.IsMaterialItemSiteBuffer)
                return true;

            return false;
        }

        public void WriteUnpeg(ModuleExecutionInfo executionInfo)
        {
            bool isPBOModule = executionInfo is PBOModuleExecutionInfo;

            // 잔여 수량이 있는 PlanWip 정보 등록
            var unpegWips = ATInputData.Wips.GetPlanWips().Where(x => x.RemainQty > 0);
            foreach (var wip in unpegWips)
            {
                if (executionInfo.Stage != wip.WipInfo.Stage)
                    continue;

                if (isPBOModule)
                {
                    var info = executionInfo as PBOModuleExecutionInfo;
                    info.UnpeggedWips.Add(wip);
                }
                else
                {
                    var info = executionInfo as PBBModuleExecutionInfo;
                    info.UnpeggedWips.Add(wip);
                }

                ATUnpegReason reason = new ATUnpegReason(UnpegCategory.Remain, string.Empty, string.Empty);
                var hasBomPath = ATInputData.ItemSiteBuffers.GetABomNetwork(false, wip.ItemSiteBuffer.Key);

                if (wip.MapCount <= 0)
                {
                    reason.Reason = "NoTarget";

                    if (hasBomPath.Count() <= 0)
                        reason.ReasonDetail = "InvalidFwdBomPath";
                }
                else
                {
                    reason.Reason = "Excess";
                    HashSet<string> bomPaths = new HashSet<string>();
                    hasBomPath.ForEach(x => bomPaths.Add(x.RootItemSiteBuffer.ToString()));
                    reason.ReasonDetail = bomPaths.Join("/");
                }

                reason.ReasonDetail = SetUnpegReasonDetail(reason.ReasonDetail, wip);
                OutputWriter.Instance.WriteUnpegInfo(wip, reason);

                ATExecutionContext.Instance.CurrentExecutionInfo.Kpi?.AddUnPegResult(wip.RemainQty);
            }

            foreach (var unpegWipInfo in ATInputData.Wips.GetUnpegWips())
            {
                ATExecutionContext.Instance.CurrentExecutionInfo.Kpi?.AddUnPegResult(unpegWipInfo.WIP_QTY);
            }

            foreach (var demand in ATInputData.Demands.GetDemands())
            {
                string log = string.Join(";", demand.SoPathLog);
                OutputWriter.Instance.WriteBomPathSearchInfo(demand.ID, log);

                demand.SoPathLog.Clear();
            }
        }

        public bool IsAssemblyTarget(ATOperTarget target, ATOperation oper, bool isOut)
        {
            return target.CurrentBom != null && target.CurrentBom.BomType == BomType.Assembly & oper.IsBuffer == false && (oper.Route as ATRoute).FirstOper == oper & isOut == false;
        }

        public void AddOperTarget(ModuleExecutionInfo executionInfo, ATOperTarget target)
        {
            string key = "-";
            List<ATOperTarget> targets;
            if (executionInfo.IsPBBModule)
            {
                var info = executionInfo as PBBModuleExecutionInfo;
                if (info.OperTargets.TryGetValue(key, out targets) == false)
                {
                    targets = new List<ATOperTarget>();
                    info.OperTargets.Add(key, targets);
                }
                targets.Add(target);
            }
            else if (executionInfo.IsPBOModule)
            {
                var info = executionInfo as PBOModuleExecutionInfo;
                if (info.OperTargets.TryGetValue(key, out targets) == false)
                {
                    targets = new List<ATOperTarget>();
                    info.OperTargets.Add(key, targets);
                }
                targets.Add(target);
            }
        }

        public void SortedTargetInTargetGroup(APETargetGroup group, APEPegContext context, ATRuleSet ruleSet)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("TargetSortedInGroup");
            try
            {
                var pegTargetList = group.Targets;

                if (context != null)
                {
                    pegTargetList.Sort(new ATTargetComparer(context));
                }
                else
                {
                    var preset = ruleSet.GetRule(RulePoint.CompareTargetInGroup, CallType.Level, 1);
                    if (preset != null)
                        pegTargetList.Sort(new ATTargetComparer(preset));
                }
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("TargetSortedInGroup");
            }
        }

        public void OnStepOut(ITargetGroup part)
        {
            PegControl.OnStepOut(part);

            var step = part.Step as ATOperation;

            if (step.IsBuffer)
            {
                part.Apply((x, _) =>
                {
                    var pegPart = x as APEPegPart;

                    ClearBomInfo(pegPart);
                });
            }
        }

        public ATOperTarget CreateTarget(APEPegPart pegPart, bool isOut)
        {
            var oper = pegPart.CurrentOperation;

            var pegTarget = pegPart.SampleTarget;

            ATOperTarget target = new ATOperTarget(pegTarget, oper, isOut);

            pegTarget.AddOperTarget(target);

            target.CurrentBom = oper.IsBuffer ? pegPart.NextBom : pegPart.CurrentBom;
            target.CurrentBomRoute = oper.IsBuffer ? pegPart.NextRoute : pegPart.CurrentRoute;
            target.CurrentBomDetail = oper.IsBuffer ? pegPart.NextBomDetail : pegPart.CurrentBomDetail;

            return target;
        }
 
        public void ApplyYield(ITargetGroup part)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("ApplyYield");

            part.Apply((x, _) =>
            {
                APEPegPart pegPart = x as APEPegPart;
                APETarget pegTarget = pegPart.SampleTarget;
                ATOperation oper = pegPart.CurrentOperation;

                double yield;
                if (pegTarget.CurOperTarget != null)
                    yield = pegTarget.CurOperTarget.Yield;
                else
                    yield = oper.GetYield(pegTarget.TargetDateTime);

                var oldQty = pegTarget.RemainQty;
                var newQty = oldQty / yield;

                pegTarget.RemainQty = newQty;
                pegTarget.BCumChangeRatio *= ATUtil.ConvertValue(1, 1, yield, PlanType.Backward);
            });

            ATElapsedTimeChecker.Instance.AddElapsedTime("ApplyYield");
        }

        public void ApplyTat(ITargetGroup part, bool isOut)
        {
            part.Apply((x, _) =>
            {
                var pegPart = x as APEPegPart;
                var pegTarget = pegPart.SampleTarget;
                ATOperation oper = pegPart.CurrentOperation;

                double tat;

                if (pegTarget.CurOperTarget != null)
                    tat = pegTarget.CurOperTarget.Tat;
                else
                    tat = oper.GetTat(pegTarget.TargetDateTime, isOut);

                var time = TimeSpan.FromSeconds(tat);

                if (time <= TimeSpan.Zero)
                    time = TimeSpan.Zero;

                pegTarget.TargetDateTime = pegTarget.TargetDateTime.Add(-time);
            });
        }

        public List<object> GetPartChangeInfos(ITargetGroup part, bool isOut, APEShortManager shortManager)
        {
            var step = part.Step as ATOperation;

            List<object> infos = new List<object>();

            // 아래 로직은 간략화
            if (step.IsBuffer)
            {
                if (part.PegPosition == PegPosition.StepIn)
                {
                    foreach (var target in part.Targets)
                    {
                        var pegPart = target.Group as APEPegPart;

                        if (RefPlanLogic.Instance.IsRefPlan(pegPart))
                        {
                            var refPlans = RefPlanLogic.Instance.ApplyRefPlan(pegPart, shortManager);
                            infos.AddRange(refPlans);
                        }
                    }
                }
            }
            else
            {
                var pegPart = part as APEPegPart;

                var type = ATInputData.Boms.GetChangeBomType(pegPart.CurrentBom, step, isOut);

                switch (type)
                {
                    case BomType.Assembly:
                        {
                            var boms = pegPart.CurrentItemSiteBuffer.PrevBoms[pegPart.CurrentBom];
                            infos.AddRange(boms);
                            break;
                        }
                    case BomType.Normal:
                    case BomType.SplitBy:
                    case BomType.SplitCo:
                        {
                            var boms = pegPart.CurrentItemSiteBuffer.PrevBoms[pegPart.CurrentBom];
                            var select = boms.FirstOrDefault();
                            infos.Add(select);
                            break;
                        }
                    case BomType.None:
                    default:
                        break;
                }
            }
         
            return infos;
        }

        public APEPegPart ApplyPartChangeInfo(ITargetGroup part, object partChangeInfo, bool isOut)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("ApplyPartChangeInfo");

            var pegPart = part as APEPegPart;
            var pegTarget = pegPart.SampleTarget;

            if (pegPart.CurrentOperation.IsBuffer)
            {
                pegPart = partChangeInfo as APEPegPart;
            }
            else
            {
                var bomDetail = partChangeInfo as ATBomDetail;
                var adjustToQty = bomDetail.GetToQty(pegTarget.TargetDateTime);

                if (bomDetail.Bom.BomType == BomType.SplitCo || bomDetail.Bom.BomType == BomType.SplitBy)
                {
                    adjustToQty = pegPart.CurrentBom.GetBomRatio(pegPart.MoMaster.ItemSiteBuffer, pegPart.CurrentItemSiteBuffer, pegTarget.TargetDateTime);

                    CreateBinnedWip(pegPart, pegTarget.TargetDateTime);
                }

                if (bomDetail.FromQty / adjustToQty != 1)
                {
                    if (adjustToQty <= ATOption.Instance.MinimumAllocationQuantity)
                    {
                        pegTarget.BCumChangeRatio = 1;
                        pegTarget.RemainQty = 0;
                    }
                    else
                    {
                        pegTarget.BCumChangeRatio *= ATUtil.ConvertValue(1, bomDetail.FromQty, adjustToQty, PlanType.Backward);
                        pegTarget.RemainQty = pegTarget.RemainQty.ConvertValue(bomDetail.FromQty, adjustToQty, PlanType.Backward);
                    }
                }

                pegPart.CurrentBomDetail = bomDetail;
                pegPart.CurrentItemSiteBuffer = (partChangeInfo as ATBomDetail).FromItemSiteBuffer;

                if (bomDetail.Bom.BomType == BomType.Assembly)
                {
                    pegPart.PathID += "_" + pegPart.CurrentItem.ItemID;

                    ATAssyInfo assyInfo = new ATAssyInfo(pegPart.CurrentBom, pegTarget.CurOperTarget, pegPart.CurrentItemSiteBuffer);

                    // PegPart의 RootAssyInfo 등록 필요. => Short 난 경우 Merge를 위한 작업.
                    if (pegPart.RootAssyInfo == null)
                        pegPart.RootAssyInfo = assyInfo;
                }

                if (pegTarget.Qty > ATOption.Instance.MinimumAllocationQuantity)
                    SetBomPathSearchLog(pegPart);

            }

            ATElapsedTimeChecker.Instance.AddElapsedTime("ApplyPartChangeInfo");

            return pegPart;
        }

        /// <summary>
        /// BinnedWip 생성 호출 위치 변경
        /// </summary>
        /// <param name="pegPart"></param>
        public void CreateBinnedWip(APEPegPart pegPart, DateTime time)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("CreateBinnedWip");

            try
            {
                ATItem curItem = pegPart.CurrentItem;
                List<APETarget> targets = pegPart.Targets.OfType<APETarget>().ToList();
                ATItemSiteBuffer soItem = pegPart.MoMaster.ItemSiteBuffer;
                List<ATBomDetail> co_by_Details = pegPart.CurrentBom.GetCoByBomDetail(soItem, pegPart.CurrentItemSiteBuffer);

                double binrate = pegPart.CurrentBom.GetBomRatio(soItem, pegPart.CurrentItemSiteBuffer, time);
                foreach (APETarget pt in targets)
                {
                    APEPegPart pp = pegPart;

                    if (0 < binrate)
                    {
                        var oldQty = pt.Qty;

                        // 수율을 반영한 수량 정보. 해당 정보를 이용하여 BufferWip 생성 필요.
                        var newQty = oldQty / binrate;

                        if (newQty <= 0)
                            continue;

                        foreach (var lower in co_by_Details)
                        {
                            double lowerToQty = lower.GetToQty(time);
                            if (lowerToQty > 0)
                            {
                                var lotID = LotHelper.GeneratLotID(ATConstants.BIN_WIP_PREFIX, pt.TargetID);

                                var qty = newQty * lowerToQty;

                                WIP entity = new WIP();
                                entity.WIP_ID = lotID;
                                entity.WIP_QTY = qty;
                                entity.AVAILABLE_DATETIME = pt.TargetDateTime;
                                entity.TRACK_IN_DATETIME = pt.TargetDateTime;

                                var wipinfo = ObjectMapper.CreateWipInfo(entity, WipType.Inventory, LotState.Wait, lower.ToSite, lower.ToItem, null, pp.CurrentBom.ToBuffer, null, ATExecutionContext.Instance.CurrentStage, pp.CurrentBom.ToBuffer);

                                APEWip binnedWip = ObjectMapper.CreatePlanWip(wipinfo, LotCreateType.SplitByBom);
                                binnedWip.SourceTarget = pt;

                                // 여기 로직을 어떻게 풀지 처리 방안 
                                List<APEWip> wips = new List<APEWip>();
                                wips.Add(binnedWip);

                                // 아래 반복문은 이상함 (wips에 binnedWip 하나만 들어갈 수 있는 구조, 불필요한 반복문 사용)
                                foreach (APEWip wip in wips)
                                {
                                    wip.SourceTarget = pt;
                                    Instance.AddBinnedWip(binnedWip);
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("CreateBinnedWip");
            }
        }

        public ATOperation GetPrevOper(APEPegPart pegPart)
        {
            if (pegPart.CurrentOperation == null)
            {
                return pegPart.CurrentItemSiteBuffer.Buffer;
            }
            else
            {
                var step = pegPart.CurrentOperation;
                if (step.IsBuffer)
                {
                    if (IsInputBuffer(pegPart.CurrentItemSiteBuffer, pegPart.CurrentOperation) == true)
                        return null;

                    if (pegPart.CurrentRoute == null)
                        return null;

                    step = pegPart.CurrentRoute.Opers.Last as ATOperation;
                    return step;
                }
                else
                {
                    // Route의 경우, 첫공정의 이전 공정은 BomDetail의 FromBuffer
                    step = pegPart.CurrentOperation.GetDefaultPrevStep() as ATOperation;

                    if (step == null)
                        step = pegPart.CurrentBomDetail.FromBuffer;
                }

                return step;
            }
        }
    
        public void SetLastOper(APEPegPart pegPart)
        {
            pegPart.CurrentOperation = GetPrevOper(pegPart);
            pegPart.CurrentItemSiteBuffer = pegPart.MoMaster.ItemSiteBuffer;
        }

        public void SetRetryPegPart(ITargetGroup part)
        {
            if (part.PegPosition == PegPosition.MovePrev)
            {
                var pegPart = part as APEPegPart;
                if (pegPart.IsRetryPegPart)
                {
                    pegPart.SetPegPosition(PegPosition.SelectBom);
                    pegPart.IsRetryPegPart = false;
                }
            }
        }

        public APEWip CommitPeggedWip(double pegQty, APEWip wip, bool isCommit)
        {
            APEWip peggedWip = null;
            var realWip = wip.RealWip;
            realWip.VirtualAct -= pegQty;

            if (isCommit)
            {
                OutputWriter.Instance.WritePegInfo(wip.MapTarget, wip, pegQty, 0, wip.PegContext);

                var copyMapTarget = wip.MapTarget.DeepCopy();
                peggedWip = wip.ShallowCopy(copyMapTarget, pegQty);
                peggedWip.VirtualAct = 0;
                copyMapTarget.UsedQty = 0;

                realWip.Act += pegQty;

                if (realWip.IsAllPeggedWip)
                    wip.PegContext.Oper.PlanWipQueue.RemoveWipSet(realWip);

                realWip.ItemSiteBuffer.PeggedPlanWip(realWip, pegQty);
            }
            else
            {
                realWip.ItemSiteBuffer.WipQueue.RollbackPlanWip(realWip, pegQty);
            }

            return peggedWip;
        }

        public void OnCompleteWipPegging(ATOperTarget operTarget, APETarget target, APEWip wip, double pegQty, APEPegContext context)
        {
        }

        public bool IsShortPegPart(APEPegPart pegPart, APEShortManager shortManager)
        {
            var pegTarget = pegPart.SampleTarget;

            var operation = pegPart.CurrentOperation;
            var route = operation.Route as ATRoute;

            if (operation == route.FirstOper)
            {
                if (pegTarget.MaxLateTargetDateTime < ATOption.Instance.PlanStartTime)
                {
                    var so = pegTarget.Demand;
                    double cumTat = so.ItemSiteBuffer.MinCumTat - pegTarget.PegPart.CurrentItemSiteBuffer.MinCumTat;
                    double remain = (so.DueDateTime - ATOption.Instance.PlanStartTime).TotalSeconds;
                    string detail = string.Format("Buffer : {0}, Remain : {1}, CumTat : {2}", pegPart.CurrentOperation.OperID, remain, cumTat);

                    string reason = LateReason.BWBomPathShort.ToString();

                    if (cumTat >= (so.DueDateTime - ATOption.Instance.PlanStartTime).TotalSeconds)
                        reason = LateReason.LackOfWip.ToString();

                    pegTarget.AddShortInfo(LateCategory.Tat, reason, pegTarget.RemainQty, detail, pegTarget.TargetDateTime, pegTarget.TargetDateTime);

                    pegPart.Status = Status.Short;
                }
            }

            if (PegControl.IsShortPegPart(pegPart))
            {
                ATShortInfo info = new ATShortInfo(pegPart, pegTarget.CurOperTarget, pegTarget.Qty, ShortCategory.ETC.ToString(), "IsShortPegPart");

                info.Bom = pegPart.NextBom;
                shortManager.AddShortInfo(info, true);

                pegPart.CurrentOperation = null;

                return true;
            }

            return false;
        }
    }
}
