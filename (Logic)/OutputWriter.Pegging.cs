using Mozart.Collections;
using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.ObyO;
using Mozart.SeePlan.Aleatorik.ObyO.Data;
using Mozart.SeePlan.Aleatorik.Outputs;
using Mozart.SeePlan.Simulation;
using Mozart.Simulation.Engine;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public partial class OutputWriter
    { 
        public void WritePeggableWipInfo(APETargetGroup targetGroup)
        {
            foreach (var pegPart in targetGroup.Items)
            {
                var itemSiteBuffers = ATInputData.ItemSiteBuffers.GetABomNetwork(true, pegPart.CurrentItemSiteBuffer.Key);
                var so = pegPart.SampleTarget.SoDemand;
                if (itemSiteBuffers == null)
                    return;

                if (so.IsWritePeggableWip)
                    return;

                string info = null;
                foreach (var item in itemSiteBuffers)
                {
                    var itemSiteBuffer = item.ItemSiteBuffer;
                    var qty = itemSiteBuffer.WipQueue.GetCumWipQty();

                    if (info == null)
                        info = itemSiteBuffer.Key + " : " + Math.Round(qty, 2) + "/" + Math.Round(item.ItemSiteBuffer.WipQueue.WipQty, 2);
                    else
                        info += "^ " + itemSiteBuffer.Key + " : " + Math.Round(qty, 2) + "/" + Math.Round(item.ItemSiteBuffer.WipQueue.WipQty, 2);
                }

                OutputWriter.Instance.WritePeggableWipInfo(so.ID, pegPart.CurrentItemSiteBufferID, info);
                so.IsWritePeggableWip = true;
            }
        }
 

        internal void WritePSIReport()
        {
            foreach (ATItemSiteBuffer isb in ATInputData.ItemSiteBuffers.ItemSiteBuffers.Values)
            {
                double boh = 0;
                foreach (var summary in isb.PSISummarys)
                {
                    var key = summary.Key;
                    var value = summary.Value;

                    if (key <= ATOption.Instance.PlanStartTime)
                        boh += value.BOH;

                    if (boh <= ATOption.Instance.MinimumAllocationQuantity)
                        boh = 0;

                    double inQty = value.In;
                    double outQty = value.Out;
                    double eoh = boh + inQty - outQty;

                    if (eoh <= ATOption.Instance.MinimumAllocationQuantity)
                        eoh = 0;

                    WritePSIReport(isb, key, boh, inQty, outQty, eoh);

                    boh = eoh;
                }
            }
        }

        internal PSI_REPORT WritePSIReport(ATItemSiteBuffer itemSiteBuffer, DateTime date, double boh, double inQty, double outQty, double eoh)
        {
            if (IsWriteOutput("PSI_REPORT") == false) //불필요한 반복
                return null;

            var table = DerivedHelper.GetTable<PSI_REPORT>();
            var output = (PSI_REPORT)table.New();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.STAGE_ID = ATExecutionContext.Instance.CurrentStage.StageID;
            output.MODULE_ID = ATExecutionContext.Instance.CurrentExecutionInfo.Key;

            output.PLAN_DATE = date.ToString("yyyyMMdd").AddBarBetweenDate();
            output.ITEM_ID = itemSiteBuffer.ItemID;
            output.ITEM_TYPE = itemSiteBuffer.Item.ItemType.ToString();
            output.SITE_ID = itemSiteBuffer.SiteID;
            output.BUFFER_ID = itemSiteBuffer.BufferID;

            output.BOH_QTY = boh;
            output.IN_QTY = inQty;
            output.OUT_QTY = outQty;
            output.EOH_QTY = eoh;

            output = ATOutputControl.Instance.WritePsiReport(output);
            if (output != null)
                table.Add(output);

            return output;
        }

        public TARGET_PLAN WriteTargetPlan(APEPegPart pegPart, ATOperTarget operTarget, bool isOut, double targetQty)
        {
            ATMoPlan mp = operTarget.MoPlan;
            var oper = operTarget.Oper;

            // Write 옵션에 따라 처리
            if (oper.OperType != OperType.Buffer && ATOption.Instance.ApplyTargetPlanDetail == false)
                return null;

            if (IsWriteOutput("TARGET_PLAN") == false)
                return null;

            var table = DerivedHelper.GetTable<TARGET_PLAN>();
            var output = (TARGET_PLAN)table.New();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.STAGE_ID = ATExecutionContext.Instance.CurrentStage.StageID;
            output.MODULE_ID = ATExecutionContext.Instance.CurrentExecutionInfo.Key;
            output.PHASE_NO = ATExecutionContext.Instance.CurrentExecutionInfo.CurPhase;

            output.DEMAND_ID = mp.SODemand.ID;
            output.MO_ID = mp.Demand.ID;
            output.ITEM_ID = operTarget.Item.ItemID;
            output.ITEM_TYPE = operTarget.Item.ItemType.ToString();

            output.BUFFER_ID = oper.IsBuffer ? operTarget.CurrentItemSiteBuffer.BufferID : string.Empty;
            output.ROUTING_ID = oper.IsBuffer ? string.Empty : oper.RouteID;
            output.OPER_ID = oper.IsBuffer ? string.Empty : oper.StepID;

            output.CATEGORY = oper.OperType == OperType.Buffer ? oper.OperType.ToString() : OperType.Operation.ToString();
            output.IN_OUT = isOut ? "Out" : "In";
            output.SITE_ID = operTarget.CurrentItemSiteBuffer.SiteID;

            output.TARGET_QTY = targetQty;
            output.TARGET_UNIT_QTY = targetQty / operTarget.BCumChangeRatio;
            output.TARGET_DATETIME = operTarget.TargetDateTime;
            output.TARGET_DATE = operTarget.TargetDate.AddBarBetweenDate();
            output.TARGET_WEEK = operTarget.TargetWeek.AddBarBetweenDate();
            output.TARGET_MONTH = operTarget.TargetMonth.AddBarBetweenDate();

            output.MO_DUE_DATETIME = mp.Demand.DueDateTime;
            output.MO_DUE_WEEK = mp.Demand.Week.AddBarBetweenDate();
            output.MO_DUE_MONTH = mp.Demand.Month.AddBarBetweenDate();
            output.MO_ITEM_ID = mp.Demand.ItemID;
            output.MO_ITEM_TYPE = mp.Demand.ItemSiteBuffer.Item.ItemType.ToString();
            output.MO_QTY = mp.Qty;

            output.DEMAND_ID = mp.SODemand.ID;
            output.DUE_DATE = mp.SODemand.DueDateTime.ToString(ATUtil.PlanDateFormat);
            output.DEMAND_WEEK = mp.SODemand.Week.AddBarBetweenDate();
            output.DEMAND_MONTH = mp.SODemand.Month.AddBarBetweenDate();
            output.DEMAND_ITEM_ID = mp.SODemand.ItemID;
            output.DEMAND_ITEM_TYPE = mp.SODemand.ItemSiteBuffer.Item.ItemType.ToString();
            output.DEMAND_QTY = mp.SODemand.Qty;

            output.BOM_ID = oper.IsBuffer ? string.Empty : operTarget.CurrentBom.BomID;

            output.OPER_YIELD = operTarget.Yield;
            output.TAT = operTarget.Tat.SecondToUom(ATOption.Instance.TimeUOM);
            output.CUM_TAT = operTarget.BCumTat.SecondToUom(ATOption.Instance.TimeUOM);
            output.TARGET_SEQ = AleatorikOutputMart.Instance.TARGET_PLAN.TotalCount + 1;

            // Demand Sorting 검증 용 컬럼 추가
            if (oper.OperType == OperType.Buffer && ATExecutionContext.Instance.CurrentStage.BufferRoute.LastOper.OperID == oper.OperID)
            {
                output.PEG_PART_KEY = mp.Mo.Key.ToString();
                output.DEMAND_MAX_LATENESS_DAY = Convert.ToInt32(mp.SODemand.MaxLateDays);
                output.DEMAND_ITEM_GRADE = mp.SODemand.ItemSiteBuffer.Item.Grade;
                output.DEMAND_PRIORITY = mp.SODemand.Priority;
                output.DEMAND_MIN_CUM_TAT = mp.SODemand.ItemSiteBuffer.GetMinCumTAT(mp.SODemand.MaxLateDueDateTime);
                if (pegPart != null)
                    output.SUM_DEMAND_QTY = pegPart.Targets.Sum(x => x.Qty);
            }

            if (pegPart != null)
                output.PATH_ID = pegPart.AllocPathID + "_" + mp.Demand.ID + pegPart.PathID;

            output.ORG_TARGET_DATETIME = operTarget.OrgTargetDateTime;
            output.FW_TARGET_DATETIME = operTarget.FWTargetDateTime == DateTime.MinValue ? ATUtil.DateMinValue : operTarget.FWTargetDateTime;

            output = ATOutputControl.Instance.WriteTargetPlan(output, pegPart, operTarget);
            if (output != null)
                table.Add(output);

            return output;
        }

        public INTARGET_PLAN WriteInTargetPlan(ATOperTarget target, APEPegPart pegPart = null)
        {
            if (IsWriteOutput("INTARGET_PLAN") == false)
                return null;

            ATMoPlan mp = target.MoPlan;

            var table = DerivedHelper.GetTable<INTARGET_PLAN>();
            var output = (INTARGET_PLAN)table.New();

            output.TARGET_SEQ = AleatorikOutputMart.Instance.INTARGET_PLAN.TotalCount + 1;

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.STAGE_ID = ATExecutionContext.Instance.CurrentStage.StageID;
            output.MODULE_ID = ATExecutionContext.Instance.CurrentExecutionInfo.Key;
            output.PHASE_NO = ATExecutionContext.Instance.CurrentExecutionInfo.CurPhase;

            output.DEMAND_ID = target.SODemand.ID;
            output.DEMAND_PRIORITY = target.SODemand.Priority;
            output.MO_ID = mp.Demand.ID;

            output.ITEM_ID = target.CurrentItemSiteBuffer.ItemID;
            output.ITEM_TYPE = target.CurrentItemSiteBuffer.Item.ItemType.ToString();
            output.BUFFER_ID = target.OperID;
            output.SITE_ID = target.CurrentItemSiteBuffer.SiteID;

            output.TARGET_QTY = target.TargetQty;
            output.TARGET_UNIT_QTY = target.TargetQty / target.BCumChangeRatio;

            output.TARGET_DATETIME = target.TargetDateTime;
            output.TARGET_DATE = target.TargetDate.AddBarBetweenDate();
            output.TARGET_WEEK = target.TargetWeek.AddBarBetweenDate();
            output.TARGET_MONTH = target.TargetMonth.AddBarBetweenDate();

            output.MO_DUE_DATETIME = mp.Demand.DueDateTime;
            output.MO_ITEM_ID = mp.Demand.ItemID;
            output.MO_QTY = mp.Qty;

            output.DEMAND_ID = mp.SODemand.ID;
            output.DUE_DATETIME = mp.SODemand.DueDateTime;
            output.DEMAND_ITEM_ID = mp.SODemand.ItemID;
            output.DEMAND_QTY = mp.SODemand.Qty;

            output = ATOutputControl.Instance.WriteIntargetPlan(output, pegPart, target);
            if (output != null)
                table.Add(output);

            return output;
        }
 
        public PEG_INFO WritePegInfo(ATOperTarget target, APEWip wip, double pegQty, double sumQty, APEPegContext context)
        {
            ATExecutionContext.Instance.CurrentExecutionInfo.Kpi?.AddPegResult(pegQty);

            if (IsWriteOutput("PEG_INFO") == false)
                return null;

            ATMoPlan moplan = target.MoPlan;
            ATOperation oper = context.Oper;

            var table = DerivedHelper.GetTable<PEG_INFO>();
            var output = (PEG_INFO)table.New();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.STAGE_ID = ATExecutionContext.Instance.CurrentStage.StageID;
            output.MODULE_ID = ATExecutionContext.Instance.CurrentExecutionInfo.Key;
            output.LEVEL_NO = context.Level;
            output.DEMAND_ID = moplan.SODemand.ID;
            output.MO_ID = moplan.ID;
            output.PHASE_NO = ATExecutionContext.Instance.CurrentExecutionInfo.CurPhase;

            output.WIP_ID = wip.WipInfo.LotID;
            output.WIP_QTY = wip.WipInfo.UnitQty;
            output.AVAILABLE_DATETIME = wip.AvailableTime;
            output.PEG_QTY = pegQty;
            output.WIP_TYPE = wip.WipInfo.LotType.ToString();
            output.CREATION_TYPE = wip.CreationType.ToString();
            output.ITEM_ID = wip.ItemID;
            output.ITEM_TYPE = wip.Item.ItemType.ToString();
            output.ITEM_GRADE = wip.Item.Grade.ToString();

            output.TARGET_ITEM_ID = target.Item.ItemID;
            output.TARGET_ITEM_TYPE = target.Item.ItemType.ToString();
            output.SITE_ID = wip.ItemSiteBuffer.SiteID;
            output.TARGET_SITE_ID = target.SiteID;
            output.WIP_STATUS = wip.State.ToString();

            output.BUFFER_ID = oper.IsBuffer ? wip.ItemSiteBuffer.BufferID : string.Empty;
            output.BUFFER_SEQ = oper.IsBuffer ? wip.ItemSiteBuffer.Buffer.Sequence : 0;
            output.ROUTING_ID = oper.IsBuffer ? string.Empty : oper.RouteID;
            output.OPER_ID = oper.IsBuffer ? string.Empty : oper.OperID;

            output.TARGET_DATETIME = target.TargetDateTime;
            output.TARGET_DATE = target.TargetDate.AddBarBetweenDate();
            output.TARGET_WEEK = target.TargetWeek.AddBarBetweenDate();
            output.TARGET_MONTH = target.TargetMonth.AddBarBetweenDate();
            output.TARGET_QTY = target.TargetQty;

            output.MO_QTY = moplan.Qty;
            output.MO_ITEM_ID = moplan.Demand.ItemID;
            output.MO_ITEM_TYPE = moplan.Demand.ItemSiteBuffer.Item.ItemType.ToString();
            output.MO_DUE_DATETIME = moplan.Demand.DueDateTime;
            output.MO_WEEK = moplan.Demand.Week;
            output.MO_MONTH = moplan.Demand.Month;

            output.DEMAND_QTY = moplan.SODemand.Qty;
            output.DEMAND_ITEM_ID = moplan.SODemand.ItemID;
            output.DEMAND_SITE_ID = moplan.SODemand.SiteID;
            output.DEMAND_ITEM_TYPE = moplan.SODemand.ItemSiteBuffer.Item.ItemType.ToString();
            output.DUE_DATETIME = moplan.SODemand.DueDateTime;
            output.DUE_WEEK = moplan.SODemand.Week;
            output.DUE_MONTH = moplan.SODemand.Month;
            output.DEMAND_MAX_LATENESS_DAY = Convert.ToInt32(moplan.SODemand.MaxLateDays);

            output.PEG_SEQ = AleatorikOutputMart.Instance.PEG_INFO.TotalCount + 1;
            output.PEGGING_GROUP_KEY = context.PeggingGroupKey;
            output.TARGET_GROUP_KEY = context.TargetGroupKey;
            output.PEGGING_KEY = context.PeggingKey;

            output.TARGET_SUM_QTY = sumQty;

            if (target.PegTarget.PegPart.CurRefPlan != null)
                output.REF_PLAN_VER = target.PegTarget.PegPart.CurRefPlan.ID;
            
            output = ATOutputControl.Instance.WritePegInfo(output, target, wip, pegQty, sumQty, context);
            if (output != null)
                table.Add(output);

            return output;
        }
 
        public LOT_SPLIT_LOG WriteSplitLog(ATSplitInfo splitInfo, APELot fromLot, APEWip splitWip, ATBomDetail detail, double splitRatio, double toQty)
        {
            if (IsWriteOutput("LOT_SPLIT_LOG") == false)
                return null;

            var table = DerivedHelper.GetTable<LOT_SPLIT_LOG>();
            var output = (LOT_SPLIT_LOG)table.New();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.STAGE_ID = ATExecutionContext.Instance.CurrentStage.StageID;
            output.MODULE_ID = ATExecutionContext.Instance.CurrentExecutionInfo.Key;
            output.PHASE_NO = ATExecutionContext.Instance.CurrentExecutionInfo.CurPhase;

            var sourceTarget = splitWip.SourceTarget;

            output.DEMAND_ID = sourceTarget != null ? (sourceTarget.MoPlan as ATMoPlan).SODemand.ID : "-";
            output.DEMAND_ITEM_ID = sourceTarget != null ?  (splitWip.SourceTarget.MoPlan as ATMoPlan).SODemand.ItemID : "-";

            output.MO_ID = sourceTarget != null ? splitWip.SourceTarget.MoPlan.ID : "-";
            output.MO_DUE_DATETIME = sourceTarget != null ?  splitWip.SourceTarget.MoPlan.DueDate : ATOption.Instance.PlanEndTime;

            var bom = detail.Bom;
            output.ORG_LOT_ID = splitInfo.OrgLot.LotID;
            output.ORG_LOT_QTY = splitInfo.Qty;
            output.BOM_ID = bom.BomID;
            output.BOM_TYPE = bom.BomType.ToString();
            output.TO_QTY = splitRatio;
            output.COBY_LOT_ID = splitWip.WipInfo.LotID;

            output.CONFIRMED_LOT_ID = fromLot.LotID;
            output.CONFIRMED_LOT_QTY = toQty; // 확인 필요
            output.ORG_ITEM_ID = splitInfo.CurrentItemSiteBuffer.ItemID;
            output.ORG_SITE_ID = splitInfo.CurrentItemSiteBuffer.SiteID;
            output.ORG_ITEM_GRADE = splitInfo.CurrentItemSiteBuffer.Item.Grade;

            output.COBY_ITEM_ID = splitWip.ItemID;
            output.COBY_SITE_ID = splitWip.ItemSiteBuffer.SiteID;
            output.COBY_GRADE = splitWip.Item.Grade;

            output.COBY_LOT_QTY = splitWip.WipInfo.UnitQty;

            output.BUFFER_ID = splitWip.Buffer.BufferID;

            output.TO_LOT_AVAILABLE_DATETIME = splitWip.AvailableTime;

            output.CALENDAR_ID = detail.Calendar != null ? detail.Calendar.CalendarID : "";

            output = ATOutputControl.Instance.WriteLotSplitLog(output, splitInfo);
            if (output != null)
                table.Add(output);

            return output;
        }

        public UNPEG_INFO WriteUnpegInfo(Inputs.WIP wip, ATUnpegReason reason)
        {
            ATInputData.Wips.AddUnpegWips(wip);

            if (IsWriteOutput("UNPEG_INFO") == false)
                return null;

            var table = DerivedHelper.GetTable<UNPEG_INFO>();
            var output = (UNPEG_INFO)table.New();

            var item = ATInputData.ItemSiteBuffers.GetItem(wip.ITEM_ID);
            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;

            ATBuffer buffer = ATInputData.ItemSiteBuffers.GetBuffer(wip.BUFFER_ID);
            if (buffer != null)
                output.STAGE_ID = buffer.Stage.StageID;


            output.MODULE_ID = "-";   // ATContext.Instance.CurrentExecutionInfo.Key;
            output.AVAILABLE_DATETIME = wip.AVAILABLE_DATETIME;

            output.WIP_ID = wip.WIP_ID;
            output.CREATION_TYPE = LotCreateType.Wip.ToString();

            output.ITEM_TYPE = item == null ? string.Empty : item.ItemType.ToString();
            output.WIP_QTY = wip.WIP_QTY;

            output.UNPEG_QTY = wip.WIP_QTY;

            output.UNPEG_CATEGORY = reason.Category.ToString();
            output.UNPEG_REASON = reason.Reason;
            output.REASON_DETAIL = reason.ReasonDetail;

            output.ITEM_ID = wip.ITEM_ID;
            output.BUFFER_ID = wip.BUFFER_ID;

            output.WIP_STATUS = wip.WIP_STATUS;
            output.ROUTING_ID = wip.ROUTING_ID;
            output.OPER_ID = wip.OPER_ID;
            output.SITE_ID = wip.SITE_ID;

            output = ATOutputControl.Instance.WriteUnpegInfoInLoading(output, wip, reason);
            if (output != null)
                table.Add(output);

            return output;
        }

        public UNPEG_INFO WriteUnpegInfo(APEWip wip, ATUnpegReason reason)
        {
            if (IsWriteOutput("UNPEG_INFO") == false)
                return null;

            var table = DerivedHelper.GetTable<UNPEG_INFO>();
            var output = table.New();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.STAGE_ID = ATExecutionContext.Instance.CurrentStage.StageID;
            output.MODULE_ID = ATExecutionContext.Instance.CurrentExecutionInfo.Key;

            output.WIP_ID = wip.WipInfo.LotID;
            output.AVAILABLE_DATETIME = wip.AvailableTime;
            output.CREATION_TYPE = wip.CreationType.ToString();

            // 원본 수량이 아닌 초기 InitPlan 시점에 이동하여 완성된 수량 정보가 필요함.
            output.WIP_QTY = wip.WipInfo.UnitQty;
            output.UNPEG_QTY = wip.RemainQty;

            output.UNPEG_CATEGORY = reason.Category.ToString();
            output.UNPEG_REASON = reason.Reason;
            output.REASON_DETAIL = reason.ReasonDetail;

            output.ITEM_ID = wip.Item.ItemID;
            output.ITEM_TYPE = wip.Item.ItemType.ToString();
            output.BUFFER_ID = wip.ItemSiteBuffer.BufferID;
            output.SITE_ID = wip.ItemSiteBuffer.SiteID;

            output.WIP_STATUS = wip.State.ToString();
            output.ROUTING_ID = wip.Oper.IsBuffer ? string.Empty : wip.Oper.RouteID;
            output.OPER_ID = wip.Oper.IsBuffer ? string.Empty : wip.OperID;

            output = ATOutputControl.Instance.WriteUnpegInfo(output, wip, reason);
            if (output != null)
                table.Add(output);

            return output;
        }

        public COMPARE_BOM_LOG WriteCompareBomLog(APEPegPart pegPart, List<ATBom> boms, List<ATBom> sortBoms, List<ATBom> filterBoms, ATBom selectedBom, APESelectBomContext context)
        {
            if (filterBoms.Count == 0 && sortBoms.Count <= 1)
                return null;
            
            if (IsWriteOutput("COMPARE_BOM_LOG") == false)
                return null;

            var table = DerivedHelper.GetTable<COMPARE_BOM_LOG>();
            var output = (COMPARE_BOM_LOG)table.New();

            var pegTarget = pegPart.SampleTarget;

            string bomIDs = string.Join(", ", boms.Select(x => x.BomID));
            string filterLog = filterBoms.ToList<IFactorObject>().GetFactorLog();
            string compareLog = sortBoms.ToList<IFactorObject>().GetFactorLog();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.STAGE_ID = ATExecutionContext.Instance.CurrentStage.StageID;
            output.MODULE_ID = ATExecutionContext.Instance.CurrentExecutionInfo.Key;
            output.PHASE_NO = ATExecutionContext.Instance.CurrentExecutionInfo.CurPhase;

            output.DEMAND_ID = pegTarget.SoDemand.ID;
            output.DEMAND_ITEM_ID = pegTarget.SoDemand.ItemID;

            output.INIT_BOM = bomIDs;
            output.ISB_ID = pegPart.CurrentItemSiteBufferID;

            output.USED_BOM_ID = selectedBom?.BomID;

            output.FILTERED_BOM = filterLog;
            output.USABLE_BOM = compareLog;

            output = ATOutputControl.Instance.WriteCompareBomLog(output, context);
            if (output != null)
                table.Add(output);

            return output;
        }

        public BOM_PATH_SEARCH_LOG WriteBomPathSearchInfo(string soID, string path)
        {
            if (IsWriteOutput("BOM_PATH_SEARCH_LOG") == false)
                return null;

            var table = DerivedHelper.GetTable<BOM_PATH_SEARCH_LOG>();
            var output = (BOM_PATH_SEARCH_LOG)table.New();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.DEMAND_ID = soID;
            output.BOM_PATH = path;

            table.Add(output);

            return output;
        }

        public ITEM_SITE_BUFFER_WIP_LOG WriteItemSiteBufferWipLog(APEWip wip, string type, ATItemSiteBuffer itemSiteBuffer, double pegWipQty = 0)
        {
            if (wip.WipInfo.LotID.Contains(ATConstants.BIN_WIP_PREFIX))
                return null;

            if (IsWriteOutput("ITEM_SITE_BUFFER_WIP_LOG") == false)
                return null;

            var table = DerivedHelper.GetTable<ITEM_SITE_BUFFER_WIP_LOG>();
            var output = (ITEM_SITE_BUFFER_WIP_LOG)table.New();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.WIP_ID = wip.WipInfo.LotID;
            output.ISB_ID = itemSiteBuffer.Key;
            output.LOG_TYPE = type;
            output.AVAILABLE_DATETIME = wip.AvailableTime;
            output.LOG_SEQ = AleatorikOutputMart.Instance.ITEM_SITE_BUFFER_WIP_LOG.TotalCount + 1;
            //output.QTY = type == "Peg" ? (wip.RemainQty - wip.PegContext.ConfirmPegQty) + wip.PegContext.PeggedWip.RemainQty : wip.CumWipQty;
            output.WIP_QTY = type == "Peg" ? wip.RemainQty : wip.CumWipQty;
            table.Add(output);

            return output;
        }

        private PEGGABLE_WIP_INFO WritePeggableWipInfo(string soID, string soItemSiteBuffer, string info)
        {
            if (IsWriteOutput("PEGGABLE_WIP_INFO") == false) //불필요한 반복
                return null;

            var table = DerivedHelper.GetTable<PEGGABLE_WIP_INFO>();
            var output = (PEGGABLE_WIP_INFO)table.New();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.PHASE_NO = ATExecutionContext.Instance.CurrentExecutionInfo.CurPhase;
            output.DEMAND_ID = soID;
            output.DEMAND_ISB_ID  = soItemSiteBuffer;
            output.WIP_INFO = info;

            table.Add(output);

            return output;
        }
       
    }
}
