//using Mozart.SeePlan.Aleatorik.Data;
//using Mozart.SeePlan.Aleatorik.EngineBase;
//using Mozart.SeePlan.Aleatorik.Logic;
//using Mozart.SeePlan.Aleatorik.ObyO.Data;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Mozart.SeePlan.Aleatorik.ObyO
//{
//    /// <summary>
//    /// BW , FW
//    /// </summary>
//    public class OboEngine : IDisposable
//    {
//        /// <summary>
//        /// FW 
//        /// </summary>
//        private OboFactory _factory;

//        internal ATRuleSet DefaultRuleSet
//        {
//            get
//            {
//                return ATRuleAgent.Instance.CurrentRuleSet;
//            }
//        }

//        public APEShortManager ShortManager { get; set; }

//        public OboEngine()
//        {
//            this._factory = new OboFactory(this);
//        }

//        public void Dispose()
//        {
//            this._factory = null;
//        }

//        public void Initialize(APEShortManager shortManager)
//        {
//            this.ShortManager = shortManager;
//            this._factory.ShortManager = shortManager;
//        }

//        //public void RegistRetryTargetGroup(APETargetGroup targetGroup)
//        //{
//        //    targetGroup.Targets.Reverse();
//        //    targetGroup.Targets.ForEach(x => _pegParts.Push((x as APETarget).PegPart));
//        //}

        

//        //private bool StepOut(APEPegPart pegPart)
//        //{
//        //    var pp = pegPart;
//        //    var step = pegPart.CurrentStep as ATOperation;

//        //    if (step.IsBuffer)
//        //    {
//        //        pp.NextBom = pp.CurrentBom;
//        //        pp.NextRoute = pp.CurrentRoute;
//        //        pp.NextBomDetail = pp.CurrentBomDetail;

//        //        pp.CurrentBom = ATBom.NULL;
//        //        pp.CurrentRoute = null;
//        //        pp.CurrentBomDetail = null;

//        //        ShippingLogic.Instance.ApplyShippingDate(pegPart);
//        //    }

//        //    OboInterfaces.PegControl.OnStepOut(pegPart);

//        //    return true;
//        //}

//        //private bool StepIn(APEPegPart pegPart)
//        //{
//        //    bool retValue = true;

//        //    if (RefPlanLogic.Instance.IsRefPlan(pegPart))
//        //        retValue = RefPlanLogic.Instance.ApplyRefPlan(pegPart, this.DefaultRuleSet, _pegParts, this);

//        //    OboInterfaces.PegControl.OnStepIn(pegPart);
//        //    return retValue;
//        //}

//        //private ATOperTarget CreateTarget(APEPegPart pegPart, bool isOut)
//        //{
//        //    var oper = pegPart.CurrentStep;

//        //    var pegTarget = pegPart.SampleTarget;

//        //    ATOperTarget target = new ATOperTarget(pegTarget, oper, isOut);

//        //    pegTarget.AddOperTarget(target);
            
//        //    #region Selected Bom / Route

//        //    target.CurrentBom = oper.IsBuffer ? pegPart.NextBom : pegPart.CurrentBom;
//        //    target.CurrentBomRoute = oper.IsBuffer ? pegPart.NextRoute : pegPart.CurrentRoute;
//        //    target.CurrentBomDetail = oper.IsBuffer ? pegPart.NextBomDetail : pegPart.CurrentBomDetail;

//        //    //if (target.CurrentBom != null && target.CurrentBom.BomType == BomType.Assembly && oper.IsBuffer == false && (oper.Route as ATRoute).FirstOper == oper && isOut == false)
//        //    //{
//        //    //    var buffer = target.CurrentBom.FromBuffer;
//        //    //    List<ATOperTarget> lst;

//        //    //    if (executionInfo.StageInAssemblyTargets.TryGetValue(buffer, out lst) == false)
//        //    //    {
//        //    //        lst = new List<ATOperTarget>();
//        //    //        executionInfo.StageInAssemblyTargets.Add(buffer, lst);
//        //    //    }
//        //    //    var copyTarget = target.DeepCopy();
//        //    //    lst.Add(copyTarget);
//        //    //    PegCommonLogic.Instance.AddOperTarget(executionInfo, copyTarget);
//        //    //}
//        //    #endregion

//        //    //PegCommonLogic.Instance.AddOperTarget(executionInfo, target);

//        //    return target;
//        //}

//        //private List<APEPegPart> SelectBom(APEPegPart pegPart)
//        //{
//        //    List<APEPegPart> branchList = new List<APEPegPart>();
//        //    APESelectBomContext context = new APESelectBomContext(pegPart);
//        //    List<ATSelectBomInfo> selectInfos = new List<ATSelectBomInfo>();
//        //    List<ATSelectBomInfo> shortInfos = new List<ATSelectBomInfo>();

//        //    var selectBoms = OboLogic.Instance.GetSelectBom(pegPart, context);

//        //    if (selectBoms == null || selectBoms.Count == 0)
//        //    {
//        //        var pt = pegPart.Sample;

//        //        if (BackwardCommonLogic.Instance.IsInputBuffer(pegPart.CurrentItemSiteBuffer, pegPart.CurrentStep) == false)
//        //        {
//        //            ATShortInfo shortInfo = new ATShortInfo(pegPart, pt.CurOperTarget, pt.RemainQty, ShortCategory.BOM.ToString(), "");
//        //            this.ShortManager.AddShortInfo(shortInfo, true);
//        //        }

//        //        return branchList;
//        //    }

//        //    // 부분 할당만한 경우.
//        //    if (pegPart.Sample.RemainQty > ATOption.Instance.MinimumAllocationQuantity)
//        //    {
//        //        var type = SelectType.Short;
//        //        var boms = pegPart.GetPrevBoms();

//        //        // 잔여 타겟에 대한 재할당 여부
//        //        // ReAlloc의 경우, 해당 Step부터 다시 시작.
//        //        // Short의 경우, 다음 Phase에 다시 할당 시도.
//        //        if (OboInterfaces.PegControl.IsReSelectBom(pegPart.Sample, boms, context) == true)
//        //            type = SelectType.Retry;

//        //        ATSelectBomInfo info = new ATSelectBomInfo(pegPart.Sample, ATBom.NULL, type, context);
//        //        info.Qty = pegPart.Sample.RemainQty;

//        //        pegPart.Sample.RemainQty = 0;

//        //        selectBoms.Add(info);
//        //    }

//        //    foreach (var selectBom in selectBoms)
//        //    {
//        //        if (selectBom.SelectType == SelectType.Select)
//        //            selectInfos.Add(selectBom);
//        //        else
//        //            shortInfos.Add(selectBom);
//        //    }

//        //    int count = selectInfos.Count();
//        //    foreach (var selectBom in selectInfos)
//        //    {
//        //        APEPegPart copied = pegPart;

//        //        if (count > 1)
//        //            copied = pegPart.Clone();

//        //        APEPegPart branch = BackwardCommonLogic.Instance.ApplyBom(copied, selectBom);
//        //        branchList.Add(branch);
//        //    }

//        //    foreach (var selectBom in shortInfos)
//        //    {
//        //        APEPegPart copied = pegPart.Clone();
//        //        APEPegPart branch = BackwardCommonLogic.Instance.ApplyBom(copied, selectBom);

//        //        if (selectBom.SelectType == SelectType.Short)
//        //        {
//        //            var pt = branch.Sample;
//        //            ATShortInfo shortInfo = new ATShortInfo(branch, pt.CurOperTarget, pt.RemainQty, ShortCategory.BOM.ToString(), "");
//        //            this.ShortManager.AddShortInfo(shortInfo, true);

//        //            pt.AddShortInfo(LateCategory.Tat, LateReason.NoBwBomPathShort.ToString(), pt.RemainQty, null, pt.TargetDateTime, pt.TargetDateTime);
//        //        }
//        //        else if (selectBom.SelectType == SelectType.Retry)
//        //        {
//        //            // Retry의 경우 CurPegPart를 모두 진행 후에 다시 진행 필요.
//        //            branch.PegPosition = PegPosition.SelectBom;
//        //            this._pegParts.Push(branch);
//        //        }
//        //    }

//        //    return branchList;
//        //}

//        //private void GetPrevStep(APEPegPart pegPart)
//        //{
//        //    var pegTarget = pegPart.SampleTarget;

//        //    if (BackwardCommonLogic.Instance.IsInputBuffer(pegPart.CurrentItemSiteBuffer, pegPart.CurrentStep))
//        //    {
//        //        var target = pegTarget.CurOperTarget.DeepCopy();
//        //        target.TargetQty = pegTarget.RemainQty;
//        //        this._factory.AddInTarget(target);

//        //        pegPart.CurrentStep = null;
//        //        pegPart.PegPosition = PegPosition.None;

//        //        return;
//        //    }

//        //    pegPart.CurrentStep = OboLogic.Instance.GetPrevOperation(pegPart);
//        //    pegPart.PegPosition = PegPosition.None;

//        //    if (pegPart.CurrentStep == null)
//        //    {
//        //        ATShortInfo info = new ATShortInfo(pegPart, pegTarget.CurOperTarget, pegTarget.Qty, ShortCategory.BOM.ToString(), "");
//        //        info.Bom = pegPart.NextBom;
//        //        ShortManager.AddShortInfo(info, true);

//        //        pegTarget.AddShortInfo(LateCategory.Tat, LateReason.NoBwBomPathShort.ToString(), pegTarget.RemainQty, null, pegTarget.TargetDateTime, pegTarget.TargetDateTime);
//        //        return;
//        //    }

//        //    BackwardCommonLogic.Instance.IsShortPegPart(pegPart);

//        //    if (OboInterfaces.PegControl.IsShortPegPart(pegPart))
//        //    {
//        //        ATShortInfo info = new ATShortInfo(pegPart, pegTarget.CurOperTarget, pegTarget.Qty, ShortCategory.ETC.ToString(), "IsShortPegPart");
//        //        info.Bom = pegPart.NextBom;
//        //        ShortManager.AddShortInfo(info, true);

//        //        pegPart.CurrentStep = null;
//        //    }
//        //}

//        //private void DoPegging(APEPegPart pegPart, bool isRun)
//        //{
//        //    ATElapsedTimeChecker.Instance.ResetTimer("DoPegging");
//        //    try
//        //    {
//        //        List<APEWip> candidateWips = new List<APEWip>();

//        //        var oper = pegPart.CurrentStep;
//        //        var pt = pegPart.Sample;
                
//        //        if (pt.RemainQty <= ATOption.Instance.MinimumAllocationQuantity)
//        //            return ;

//        //        APEPegContext context = new APEPegContext(oper, isRun, DefaultRuleSet, 1);
//        //        context.CurPegPart = pegPart;

//        //        List<string> keys = BackwardCommonLogic.Instance.GetPeggingKey(pegPart, context);
//        //        context.PeggingKeys = keys;

//        //        foreach (var key in keys)
//        //        {
//        //            var wips = oper.PlanWipQueue.GetWipSet(key, isRun);
//        //            candidateWips.AddRange(wips);
//        //        }

//        //        context.PlanWips = candidateWips;
//        //        OboInterfaces.PegControl.OnPrepareVirtualPegging(pegPart, pt, context.PlanWips, context, isRun);

//        //        if (context.PlanWips.Count == 0)
//        //            return ;

//        //        if (ATOption.Instance.ApplyReSortWip)
//        //            context.PlanWips.Sort(new ATPlanWipComparer(context));

//        //        var peggedwips = OboLogic.Instance.DoPegging(pegPart, pt, context.PlanWips, context);

//        //        foreach (var peggedWip in peggedwips)
//        //        {
//        //            this._factory.AddPeggedWip(peggedWip);
//        //            OboInterfaces.PegControl.OnCompleteVirtualWipPegging(pegPart, pt, peggedWip, context, isRun);
//        //        }

//        //        return ;
//        //    }
//        //    finally
//        //    {
//        //        ATElapsedTimeChecker.Instance.AddElapsedTime("DoPegging");
//        //    }
//        //}

//        public void Done()
//        {

          
//        }

//        internal OboFactory GetCurrentFactory()
//        {
//            return _factory;
//        }
//    }
//}
