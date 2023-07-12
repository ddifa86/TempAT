using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Inputs;
using Mozart.SeePlan.Aleatorik.Logic;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Pegger
{
    public partial class PeggerLogic
    {
        public static PeggerLogic Instance
        {
            get
            {
                return ServiceLocator.Resolve<PeggerLogic>();
            }
        }

        //        public void SortedTargetGroup(List<APETargetGroup> groups, APEPegContext context)
//        {
//             groups.QuickSort<APETargetGroup>(new ATTargetGroupComparer(context));
//        }

//        public void DoPegging(APETargetGroup targetGroup, ATKitInfo wipGroup, APEPegContext context)
//        {
//            ATElapsedTimeChecker.Instance.ResetTimer("DoPegging");

//            try
//            {
//                var executeInfo = ATExecutionContext.Instance.CurrentExecutionInfo as PBBModuleExecutionInfo;

//                double sumQty = targetGroup.Targets.Sum(x => x.Qty);
//                List<ITarget> targets = new List<ITarget>(targetGroup.Targets);

//                for (int targetIdx = 0; targetIdx < targets.Count; targetIdx++)
//                {
//                    var target = targets[targetIdx] as APETarget;

//                    if (target.Qty <= 0)
//                        continue;

//                    for (int wipIdx = 0; wipIdx < wipGroup.Items.Count; wipIdx++)
//                    {
//                        APEWip wip = wipGroup.Items[wipIdx];
//                        if (wip.RemainQty <= 0)
//                        {
//                            continue;
//                        }

//                        if (BackwardCommonLogic.Instance.FilterWip(wip, target, context))
//                            continue;

//                        double availPegQty = BWInterface.PeggerControl.AvailPegQty(target, wip);

//                        double adjustPegQty = Math.Min(target.Qty, availPegQty);

//                        #region Update PegContextInfo

//                        context.PegQty = adjustPegQty;
//                        context.PegSequence++;
//                        context.PeggingKey = wipGroup.Keys;
//                        context.TargetGroupKey = targetGroup.TargetGroupKey.ToString();

//                        #endregion

//                        target.Qty -= adjustPegQty;
//                        wip.Act += adjustPegQty;

//                        if (wip.RemainQty <= ATOption.Instance.MinimumAllocationQuantity)
//                        {
//                            context.Oper.PlanWipQueue.RemoveWipSet(wip);
//                            wipGroup.Items.RemoveAt(wipIdx--);
//                        }
//#warning FeAction 제공 유무..

//                        OutputWriter.Instance.WritePegInfo(target.CurOperTarget, wip, adjustPegQty, sumQty, context);


//                        BWInterface.PeggerControl.OnCompleteWipPegging(targetGroup, target, wip, adjustPegQty, context, context.IsRun);

//                        // Pegged 결과 등록..?
//                        // 이런 정보 처리를 위해선.. OboPlanWip , PegPlanWip을 분리할 필요가 있어보임...
//                        wip.MapTargets.Add(target.CurOperTarget);
//                        wip.ItemSiteBuffer.PeggedPlanWip(wip, adjustPegQty);

//                        // peggedWip 등록 구간 => Result 로 이동 필요.
//                        var peggedWip = wip.ShallowCopy(target.CurOperTarget, adjustPegQty);

//                        executeInfo.AddPeggWips(peggedWip, target, adjustPegQty, context);

//                        if (target.Qty <= ATOption.Instance.MinimumAllocationQuantity)
//                        {
//                            targetGroup.RemovePegTarget(target);
//                            break;
//                        }
//                    }
//                }
//            }
//            finally
//            {
//                ATElapsedTimeChecker.Instance.AddElapsedTime("DoPegging");
//            }
//        }

        //public APEPegPart PegWip(APEPegPart pegPart, bool isRun)
        //{
        //    ATElapsedTimeChecker.Instance.ResetTimer("NEW_PEG_WIP");
        //    try
        //    {
        //        var pp = pegPart;
        //        var oper = pegPart.CurrentStep;
        //        string peggingGroupKey = null;

        //        ATRuleSet curRuleSet = ATRuleAgent.Instance.GetRuleSet(oper);

        //        // Default 로직을 심플하게 변경
        //        // 현재는 KitPegging을 달성하기 위한 형태로 구성됨.
        //        // KitPegging은 옵션으로 변경하고 심플한 형태로 Pegging을 제공할 수 있도록 구성 변경 필요함.
        //        #region  Target Group 생성.
        //        List<APEPegPart> pegParts = new List<APEPegPart>();

        //        if (pegPart is IMergedTargetGroup)
        //        {
        //            pegParts.AddRange((pegPart as IMergedTargetGroup).Items);
        //            peggingGroupKey = (pegPart as IMergedTargetGroup).Key.ToString();
        //        }
        //        else
        //        {
        //            pegParts.Add(pegPart);
        //        }

        //        List<APETargetGroup> targetGroups = PeggerLogic.Instance.CreateTargetGroup(pegParts, oper, curRuleSet);

        //        BWInterface.PeggerControl.OnCreateTargetGroup(targetGroups, oper);
        //        #endregion

        //        int maxLevel = curRuleSet.Level; // Config에서 정보 Load 필요

        //        for (int curLevel = 0; curLevel < maxLevel; curLevel++)
        //        {
        //            #region Prepare Pegging Context

        //            bool isKitPegging = false;
        //            PegProcedureType pegType = PegProcedureType.TargetFirstSelected; // Config에서 정보 Load 필요
        //            APEPegContext context = new APEPegContext(curLevel + 1, oper, pegType, curRuleSet, isRun, peggingGroupKey);
        //            #endregion

        //            BWInterface.PeggerControl.OnStartLevel(curLevel, maxLevel, targetGroups, context, isRun);

        //            if (context.PegType == PegProcedureType.TargetFirstSelected)
        //            {
        //                #region Target Group 내 Target 소팅
        //                // TargetGroup 내 Target 소팅.
        //                foreach (var selgroup in targetGroups)
        //                {
        //                    BackwardCommonLogic.Instance.SortedTargetInTargetGroup(selgroup, context, context.RuleSet);
        //                }
        //                #endregion

        //                #region Target Group Filter
        //                // 대상 TargetGroup  선정
        //                var selectedgroups = PeggerLogic.Instance.FilterTargetGroup(targetGroups, context);

        //                #endregion

        //                #region TargetGroup 별 KitInfo 생성
        //                // TargetGroup별 Kit 생성
        //                for (int idx = 0; idx < selectedgroups.Count(); idx++)
        //                {
        //                    var group = selectedgroups[idx];

        //                    // 선택된 Target의 PegTarget이 없는 경우.
        //                    if (group.Targets.Count() == 0)
        //                    {
        //                        selectedgroups.RemoveAt(idx--);
        //                        continue;
        //                    }

        //                    // CreateKitInfo - TargetGroup 별 Kit 생성
        //                    group.KitInfo = PeggerLogic.Instance.CreateKitInfo(group, oper, context);

        //                    if (group.KitInfo.Count() == 0)
        //                    {
        //                        // Pegging 대상 Mapping Wip 정보가 없는 경우 Filter
        //                        selectedgroups.RemoveAt(idx--);
        //                        continue;
        //                    }
        //                }
        //                #endregion

        //                // 선택된 TargetGroup을 모두 Pegging 한 경우.
        //                while (selectedgroups.FirstOrDefault<APETargetGroup>() != null)
        //                {
        //                    #region Kit별 최대 생산 수량 계산 및 대표 Kit 선택
        //                    // TargetGroup 내 Kit 정보 Update
        //                    if (oper.IsBuffer && isKitPegging)
        //                    {
        //                        // Kit별 최대 생산가능 수량 계산
        //                        PeggerLogic.Instance.CalculateKitInfo();
        //                        // TargetGroup별 대표 Kit 선택
        //                        //PegLogic.Instance.SortedKit();
        //                        PeggerLogic.Instance.SelectKit();
        //                    }
        //                    #endregion

        //                    #region TargetGroup 소팅 및 선택

        //                    // TargetGroup 정렬
        //                    // 
        //                    SortedTargetGroup(selectedgroups, context);

        //                    // TargetGroup 선택
        //                    var selgroup = selectedgroups.FirstOrDefault();
        //                    selectedgroups.Remove(selgroup);

        //                    context.CurPegPart = selgroup;
        //                    #endregion

        //                    #region Target Group 대표 Kit의 Wip 정렬
        //                    // TargetGroup에 Mapping될 Wip들 소팅

        //                    var selectKit = selgroup.KitInfo.Values.First();

        //                    // Sorting 선택 여부??
        //                    if (ATOption.Instance.ApplyReSortWip)
        //                        selectKit.Items.Sort(new ATPlanWipComparer(context));

        //                    #endregion

        //                    // Pegging
        //                    var selKitInfo = selgroup.KitInfo.Values.First();

        //                    #region PeggingContext 값 설정
        //                    #endregion

        //                    BWInterface.PeggerControl.OnPreparePegging(selgroup, selectKit.Items, context, isRun);

        //                    DoPegging(selgroup, selKitInfo, context);
        //                }
        //            }
        //            else
        //            {

        //            }

        //            BWInterface.PeggerControl.OnEndLevel(curLevel, maxLevel, targetGroups, context, isRun);
        //        }
        //        return pegPart;
        //    }
        //    finally
        //    {
        //        ATElapsedTimeChecker.Instance.AddElapsedTime("NEW_PEG_WIP");
        //    }
        //}

        /// <summary>
        /// TargetGroup 에 Pegging 될 수 있는 Wip의 정보들을 구성
        /// TargetGroup 내 Kit이 여러개인 경우에는
        /// </summary>
        /// <param name="wips"></param>
        /// <param name="targetGroup"></param>
        /// <param name="oper"></param>
        /// <returns></returns>
        /// 
        //public Dictionary<string, ATKitInfo> CreateKitInfo(APETargetGroup targetGroup, ATOperation oper, APEPegContext context)
        //{
        //    ATElapsedTimeChecker.Instance.ResetTimer("CreateKitInfo");
        //    try
        //    {
        //        HashSet<APEWip> candidateWips = new HashSet<APEWip>();
        //        Dictionary<string, ATKitInfo> result = new Dictionary<string, ATKitInfo>();

        //        List<string> keys = BackwardCommonLogic.Instance.GetPeggingKey(targetGroup, context);
        //        context.PeggingKeys = keys;

        //        foreach (var key in keys)
        //        {
        //            var wips = oper.PlanWipQueue.GetWipSet(key, context.IsRun);
        //            candidateWips.AddRange(wips);
        //        }

        //        if (candidateWips.Count() == 0)
        //            return result;

        //        candidateWips.ForEach(x => x.MapCount++);

        //        ATKitInfo info = new ATKitInfo(keys);
        //        info.Items.AddRange(candidateWips);

        //        result.Add(info.Keys, info);

        //        return result;
        //    }
        //    finally
        //    {
        //        ATElapsedTimeChecker.Instance.AddElapsedTime("CreateKitInfo");
        //    }
        //}

        //internal IEnumerable<APEPegPart> SplitPeggingGroup(APEPegPart pegPart)
        //{
        //    if (pegPart is APEPeggingGroup)
        //    {
        //        APEPeggingGroup mpp = pegPart as APEPeggingGroup;
        //        return mpp.Items;
        //    }

        //    return new List<APEPegPart> { pegPart };
        //}

        //public void WriteTargetPlan(APEPegPart pegPart, bool isOut)
        //{
        //    ATElapsedTimeChecker.Instance.ResetTimer("WriteTargetPlan");
        //    try
        //    {
        //        var oper = pegPart.CurrentStep as ATOperation;
        //        var executionInfo = ATExecutionContext.Instance.CurrentExecutionInfo as PBBModuleExecutionInfo;

        //        //var last = pegPart.PegTargetList.LastOrDefault();

        //        // KTR
        //        // Oper에 Yield 혹은 Tat가 Calendar로 존재하지 않는 경우,
        //        // 한번만 구해서 설정하도록 로직 변경 필요.
        //        // 현재는 Calendar 정보가 항상 있다가 가정하고 일반적으로 로직을 구현함.

        //        foreach (APETarget pegTarget in pegPart.Targets)
        //        {
        //            if (pegTarget.RemainQty <= 0)
        //                continue;

        //            ATOperTarget target = new ATOperTarget(pegTarget, oper, isOut);

        //            pegTarget.AddOperTarget(target);

        //            #region Selected Bom / Route

        //            target.CurrentBom = oper.IsBuffer ? pegPart.NextBom : pegPart.CurrentBom;
        //            target.CurrentBomRoute = oper.IsBuffer ? pegPart.NextRoute : pegPart.CurrentRoute;
        //            target.CurrentBomDetail = oper.IsBuffer ? pegPart.NextBomDetail : pegPart.CurrentBomDetail; // 제품을 변경하기 위한 목표
        //            #endregion

        //            BackwardCommonLogic.Instance.AddOperTarget(executionInfo, target);

        //            OutputWriter.Instance.WriteTargetPlan(pegPart, target, isOut, target.TargetQty);

        //            // Assembly의 경우 Target 정보 등록..?

        //            //// Assembly Tartet 여부 확인.
        //            if (BackwardCommonLogic.Instance.IsAssemblyTarget(target, oper, isOut))
        //            {
        //                var buffer = target.CurrentBom.FromBuffer;

        //                List<ATOperTarget> lst;
        //                if (executionInfo.StageInAssemblyTargets.TryGetValue(buffer, out lst) == false)
        //                {
        //                    lst = new List<ATOperTarget>();

        //                    executionInfo.StageInAssemblyTargets.Add(buffer, lst);
        //                }

        //                var copyTarget = target.DeepCopy();

        //                lst.Add(copyTarget);
        //                BackwardCommonLogic.Instance.AddOperTarget(executionInfo, copyTarget); // 왜 필요한지 모르겠음...
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        ATElapsedTimeChecker.Instance.AddElapsedTime("WriteTargetPlan");
        //    }
        //}


        //public ATOperation GetPrevPeggingStep(APEPegPart pegPart)
        //{
        //    if (pegPart.CurrentStep == null)
        //    {
        //        #region GetLastPeggingStep

        //        // PegPart에 연결된 Demand의 마지막 Buffer 정보..?

        //        return pegPart.CurrentItemSiteBuffer.Buffer;
                
        //        #endregion
        //    }
        //    else
        //    {
        //        #region GetPrevPeggingStep
        //        var stage = ATExecutionContext.Instance.CurrentExecutionInfo.Stage;

        //        var step = pegPart.CurrentStep;
        //        if (step.IsBuffer)
        //        {
        //            // 정상
        //            if (step == stage.BufferRoute.FirstOper)
        //                return null;

        //            if (pegPart.CurrentRoute == null)
        //                return null;

        //            step = pegPart.CurrentRoute.Opers.Last as ATOperation;
        //            return step;
        //        }
        //        else
        //        {
        //            // Route의 경우, 첫공정의 이전 공정은 BomDetail의 FromBuffer
        //            step = pegPart.CurrentStep.GetDefaultPrevStep() as ATOperation;

        //            if (step == null)
        //                step = pegPart.CurrentBomDetail.FromBuffer;

        //        }
        //        return step;
        //        #endregion
        //    }
        //}

        //public void ApplyYield(APEPegPart pegPart)
        //{
        //    ATElapsedTimeChecker.Instance.ResetTimer("ApplyYield");
        //    try
        //    {

        //        if (pegPart is APEPeggingGroup)
        //        {
        //            var merged = pegPart as APEPeggingGroup;
        //            foreach (var pp in merged.Items)
        //            {
        //                ApplyYield(pp, pp.CurrentStep);
        //            }
        //        }
        //        else
        //        {
        //            ApplyYield(pegPart, pegPart.CurrentStep);
        //        }
        //    }
        //    finally
        //    {
        //        ATElapsedTimeChecker.Instance.AddElapsedTime("ApplyYield");
        //    }
        //}

        //private void ApplyYield(APEPegPart pegPart, ATOperation oper)
        //{
        //    //ATOperation oper = pegPart.CurrentStep as ATOperation;

        //    foreach (APETarget pegTarget in pegPart.Targets)
        //    {
        //        double yield;
        //        if (pegTarget.CurOperTarget != null)
        //            yield = pegTarget.CurOperTarget.Yield;
        //        else
        //            yield = oper.GetYield(pegTarget.TargetDateTime);

        //        pegTarget.BCumChangeRatio *= ATUtil.ConvertValue(1, 1, yield, PlanType.Backward);

        //        var oldQty = pegTarget.RemainQty;
        //        var newQty = oldQty / yield;

        //        pegTarget.RemainQty = newQty;
        //    }
        //}

        //public void WriteTarget(APEPegPart pegPart, bool isOut)
        //{
        //    if (pegPart is APEPeggingGroup)
        //    {
        //        var merged = pegPart as APEPeggingGroup;
        //        foreach (var pp in merged.Items)
        //        {
        //            PeggerLogic.Instance.WriteTargetPlan(pp, isOut);                
        //        }
        //    }
        //    else
        //    {
        //        PeggerLogic.Instance.WriteTargetPlan(pegPart, isOut);             
        //    }
        //}

        //public double GetTAT(APETarget pegTarget, bool isOut)
        //{
        //    ATElapsedTimeChecker.Instance.ResetTimer("GetTAT");
        //    try
        //    {
        //        var tat = PBBInterface.PegControl.GetTat(pegTarget, isOut);

        //        return tat;
        //    }
        //    finally
        //    {
        //        ATElapsedTimeChecker.Instance.AddElapsedTime("GetTAT");
        //    }
        //}

        //public TimeSpan GetTargetTat(APETarget pegTarget, bool isRun)
        //{
        //    double tat = PeggerLogic.Instance.GetTAT(pegTarget, isRun);
        //    return TimeSpan.FromSeconds(tat);
        //}

        //public APEPegPart ShiftTat(APEPegPart pegPart, bool isRun)
        //{
        //    foreach (APETarget pegTarget in pegPart.Targets)
        //    {
        //        var tat = GetTargetTat(pegTarget, isRun);
        //        if (tat <= TimeSpan.Zero)
        //            tat = TimeSpan.Zero;

        //        if (tat > TimeSpan.Zero)
        //            pegTarget.DueDate = pegTarget.DueDate.Add(-tat);
        //    }

        //    return pegPart;
        //}

        //public List<object> GetPartChangeInfo(APEPegPart pegPart, bool isRun)
        //{
        //    return BWInterface.PeggerControl.GetPartChangeInfos(pegPart, isRun);
        //}

        //public APEPegPart ApplyPartChange(APEPegPart pegPart, object partChangeInfo)
        //{
        //    ATElapsedTimeChecker.Instance.ResetTimer("ApplyPartChangeInfo");
        //    try
        //    {
        //        var info = BWInterface.PeggerControl.ApplyPartChangeInfo(pegPart, partChangeInfo);

        //        if (pegPart.SampleTarget.Qty > ATOption.Instance.MinimumAllocationQuantity)
        //            BackwardCommonLogic.Instance.SetBomPathSearchLog(pegPart);

        //        return info;
        //    }
        //    finally
        //    {
        //        ATElapsedTimeChecker.Instance.AddElapsedTime("ApplyPartChangeInfo");
        //    }
        //}

        //public List<APEPegPart> SelectBoms(APEPegPart pegPart)
        //{
        //    ATElapsedTimeChecker.Instance.ResetTimer("GetSiteAllocateInfos");
        //    try
        //    {
        //        var ruleSet = ATRuleAgent.Instance.GetRuleSet(pegPart.CurrentStep);
        //        List<APEPegPart> allocInfos = new List<APEPegPart>(); // 수정 필요

        //        APESelectBomContext context = new APESelectBomContext(pegPart);

        //        var boms = BWInterface.PeggerControl.FindBom(pegPart);

        //        boms.ForEach(x => x.InitFactorValue());

        //        BWInterface.PeggerControl.OnPrepareSelectBom(pegPart, boms, context);

        //        var filterBoms = new List<ATBom>();
        //        var selectedBoms = BackwardCommonLogic.Instance.FilterBom(boms, pegPart, ref filterBoms);

        //        var preset = ruleSet.GetRule(RulePoint.CompareBom, CallType.Operation);

        //        selectedBoms.Sort(new ATBomComparer(preset, pegPart, context));

        //        // SelectBom....?
        //        var selectedBom = selectedBoms.FirstOrDefault();

        //        BWInterface.PeggerControl.OnSelectedBom(selectedBom, pegPart, boms, context);
                
        //        //

        //        OutputWriter.Instance.WriteCompareBomLog(pegPart, boms, selectedBoms , filterBoms, selectedBom, context);

        //        if (selectedBoms.Count() <= 0)
        //        {
        //            // 아래 내용 검증??
        //            allocInfos.Add(pegPart);
        //            return allocInfos;
        //        }
        //        else
        //        {
        //            pegPart.CurrentBom = selectedBom;
        //            pegPart.CurrentRoute = PeggerLogic.Instance.FindRoute(pegPart);
        //            pegPart.AllocPathID += "_" + selectedBom.BomID;

        //            allocInfos.Add(pegPart);
        //        }

        //        //PegPart를 찢는다? PegTarget을 찢는다?
        //        allocInfos.Reverse();

        //        return allocInfos;
        //    }
        //    finally
        //    {
        //        ATElapsedTimeChecker.Instance.AddElapsedTime("GetSiteAllocateInfos");
        //    }
        //}

        

        //public void OnOutPosition(ATPegPart pegPart)
        //{
        //    var buffer = pegPart.CurrentStep as ATBuffer;

        //    if (buffer.IsShippingLocation)
        //    {
        //        foreach (ATPegTarget target in pegPart.PegTargetList)
        //        {
        //            var so = target.Demand.SoDemand;

        //            var shipDate = so.ShipDate;

        //            if (so.HasShipDaysOfWeek)
        //            {
        //                DateTime shipDate2 = target.TargetDateTime.GetDaysOfWeekDateF(so.ShipDaysOfWeek); //startofWeek.AddDays(shipDays - curDays); //오늘 + (원하는 일 - 오늘 )
        //                shipDate = ATUtil.MinTime(shipDate, shipDate2);
        //            }

        //            target.TargetDateTime = ATUtil.MinTime(target.TargetDateTime, shipDate);
        //        }

        //    }
        //}
    }
}
