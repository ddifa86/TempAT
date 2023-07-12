using Mozart.DataActions.Metadata;
using Mozart.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik
{
    public partial class Stage
    {


        //private void PrepareForwardInput(PBFModuleExecutionInfo info)
        //{
        //    //var info = ATExecutionContext.Instance.CurrentExecutionInfo as PeggingModuleExecutionInfo;

        //    if (info == null)
        //        return;

        //    var stage = ATExecutionContext.Instance.CurrentStage;
        //    if (stage == null)
        //        return;

        //    Planner.PlanningLogic.Instance.Reset(stage);

        //    var refInfo = ATExecutionContext.Instance.GetExecutionInfo(info.RefKey);

        //    //ModuleExecutionInfo refInfo = null;
        //    //if (string.IsNullOrEmpty(info.RefKey) == false)
        //    //    refInfo = BCPContext.Instance.GetExecutionInfo(info.RefKey);

        //    //// 투입 타겟 준비 부분.
        //    //var intargets = ModuleControl.Instance.PrepareInTargets(info, refInfo);
        //    //if (intargets != null && intargets.Count() > 0)
        //    //{
        //    //    this.StockInTargets.AddRange(intargets);
        //    //}

        //    //// 공정별 타겟 준비 부분.
        //    //var targets = ModuleControl.Instance.PrepareOperTargets(info, refInfo);
        //    //if (targets != null && targets.Count() > 0)
        //    //{
        //    //    foreach (var ot in targets)
        //    //        AddOperTarget(ot);
        //    //}

        //    // InStockWip 준비 부분.
        //    var stock = ExecutionControl.Instance.PrepareInStockWips(info, refInfo);
        //    if (stock != null && stock.Count() > 0)
        //    {
        //        this.InStock.Wips.AddRange(stock);
        //    }
        //}

        //private void PreparePBOInput(PBOModuleExecutionInfo info)
        //{
        //    if (info == null)
        //        return;

        //    var stage = ATExecutionContext.Instance.CurrentStage;
        //    if (stage == null)
        //        return;

        //    var refInfo = ATExecutionContext.Instance.GetExecutionInfo(info.RefKey);

        //    this.Demands = ExecutionControl.Instance.PrepareDemands(info, refInfo);
        //}

        //internal void PrepareOutput(ModuleExecutionInfo info)
        //{

        //    if (info is PBOModuleExecutionInfo)
        //    {
        //        this.PrepareBackwardOutput(info as PBOModuleExecutionInfo);
        //    }

        //    if (info is PBFModuleExecutionInfo)
        //        this.PrepareForwardOutput(info as PBFModuleExecutionInfo);

        //}

        //internal void PrepareBackwardOutput(PBOModuleExecutionInfo info)
        //{
        //}

        //internal void PrepareForwardOutput(PBFModuleExecutionInfo info)
        //{
        //}


    }
}
