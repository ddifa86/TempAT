using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Outputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mozart.SeePlan.Aleatorik.ObyO.Data;
using Mozart.Task.Execution;
using Mozart.SeePlan.Aleatorik.Inputs;
using Mozart.Simulation.Engine;

namespace Mozart.SeePlan.Aleatorik
{
    public partial class OutputWriter
    {

        public SHORT_REPORT WriteShortReport(ATShortInfo shortInfo, int retryCount)
        {
            if (IsWriteOutput("SHORT_REPORT") == false) //불필요한 반복
                return null;

            var table = DerivedHelper.GetTable<SHORT_REPORT>();
            var output = (SHORT_REPORT)table.New();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;

            output.STAGE_ID = ATExecutionContext.Instance.CurrentStage.StageID;
            output.MODULE_ID = ATExecutionContext.Instance.CurrentExecutionInfo.Key;
            output.PHASE_NO = ATExecutionContext.Instance.CurrentExecutionInfo.CurPhase;

            var demand = shortInfo.OperTarget.SODemand;

            output.DEMAND_ID = demand.ID;
            output.DEMAND_ITEM_ID = demand.ItemID;
            output.LOT_ID = shortInfo.ShortLot != null ? shortInfo.ShortLot.LotID : null;

            output.DUE_MONTH = demand.Month.AddBarBetweenDate();
            output.DUE_WEEK = demand.Week.AddBarBetweenDate();
            output.DUE_DATE = demand.Date.AddBarBetweenDate();
            output.MAX_LATENESS_DAY = Convert.ToInt32(demand.MaxLateDays);
            output.EXTENDED_DUE_DATE = demand.MaxLateDueDateTime.ToString(ATUtil.PlanDateFormat);
            output.DEMAND_PRIORITY = demand.Priority;
            output.DEMAND_QTY = demand.Qty;
            output.SHORT_QTY = shortInfo.Qty;
            output.SHORT_UNIT_QTY = shortInfo.Qty / shortInfo.BCumChangeRatio;
            output.REASON_CATEGORY = string.IsNullOrEmpty(shortInfo.Category) ? "-" : shortInfo.Category;
            output.REASON_NAME = string.IsNullOrEmpty(shortInfo.Reason) ? "-" : shortInfo.Reason;
            output.ROUTING_ID = shortInfo.Bom == ATBom.NULL ? null : shortInfo.OperTarget.RouteID;
            output.OPER_ID = shortInfo.OperTarget.Oper.IsBuffer ? string.Empty : shortInfo.OperTarget.Oper.OperID;
            output.ITEM_ID = shortInfo.ItemSiteBuffer.ItemID;
            output.BUFFER_ID = shortInfo.ItemSiteBuffer.BufferID;
            output.SITE_ID = shortInfo.ItemSiteBuffer.SiteID;
            output.BOM_ID = shortInfo.Bom.BomID;
            output.RETRY_CNT = retryCount;

            output = ATOutputControl.Instance.WriteShortReport(output, shortInfo.ShortLot, shortInfo);
            if (output != null)
                table.Add(output);

            return output;
        }

        public SHORT_REPORT WriteShortReport(DEMAND demand, string reason)
        {
            if (IsWriteOutput("SHORT_REPORT") == false)
                return null;

            var table = DerivedHelper.GetTable<SHORT_REPORT>();
            var output = (SHORT_REPORT)table.New();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;

            ATBuffer buffer = ATInputData.ItemSiteBuffers.GetBuffer(demand.BUFFER_ID);
            output.STAGE_ID = buffer != null ? buffer.Stage.StageID : "";

            output.DEMAND_ID = demand.DEMAND_ID;
            output.DEMAND_ITEM_ID = demand.ITEM_ID;

            output.DUE_MONTH = ATUtil.ToMonth(demand.DUE_DATE, true).AddBarBetweenDate();
            output.DUE_WEEK = ATUtil.ToWeek(demand.DUE_DATE, true).AddBarBetweenDate();
            output.DUE_DATE = ATUtil.ToDate(demand.DUE_DATE, true).AddBarBetweenDate();
            output.MAX_LATENESS_DAY = Convert.ToInt32(demand.MAX_LATENESS_DAY);
            output.EXTENDED_DUE_DATE = ATUtil.ToDate(demand.DUE_DATE.AddDays(demand.MAX_LATENESS_DAY));
            output.DEMAND_PRIORITY = demand.DEMAND_PRIORITY;
            output.DEMAND_QTY = demand.DEMAND_QTY;
            output.SHORT_QTY = demand.DEMAND_QTY;
            output.SHORT_UNIT_QTY = demand.DEMAND_QTY;

            output.REASON_CATEGORY = ShortCategory.PERSIST.ToString();;
            output.REASON_NAME = reason;
            output.ITEM_ID = demand.ITEM_ID;
            output.BUFFER_ID = demand.BUFFER_ID;
            output.SITE_ID = demand.SITE_ID;

            output = ATOutputControl.Instance.WriteShortReportInLoading(output);
            if (output != null)
                table.Add(output);

            return output;
        }

        public SHORT_PROD_PLAN WriteShortProductPlan(APELot lot, APEPlanInfo planInfo)
        {
            planInfo.IsWritePlanInfo = true;

            var table = DerivedHelper.GetTable<SHORT_PROD_PLAN>();
            var output = (SHORT_PROD_PLAN)table.New();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.STAGE_ID = ATExecutionContext.Instance.CurrentStage.StageID;
            output.MODULE_ID = ATExecutionContext.Instance.CurrentExecutionInfo.Key;
            output.PHASE_NO = ATExecutionContext.Instance.CurrentExecutionInfo.CurPhase;

            var lotInfo = planInfo.LotInfo;
            var allocationInfo = planInfo.AllocationInfo;
            
            output.PLAN_SEQ = planInfo.PlanSeq;
            output.LEVEL_NO = planInfo.Level;

            output.LOT_ID = lot.LotID;
            output.LOT_GROUP_KEY = lotInfo.LotGroupKey;
            output.BOM_ID = lotInfo.Bom.BomID;

            output.FROM_ITEM_ID = lotInfo.Item.ItemID;
            output.FROM_SITE_ID = lotInfo.ItemSiteBuffer.SiteID;
            output.FROM_BUFFER_ID = lotInfo.Bom == ATBom.NULL ? lotInfo.Operation.OperID : lotInfo.Bom.FromBuffer.BufferID;

            output.ROUTING_ID = lotInfo.Bom == ATBom.NULL ? string.Empty : lotInfo.Route.RouteID;
            output.OPER_ID = lotInfo.Operation.IsBuffer ? string.Empty : lotInfo.Operation.OperID;

            output.TO_BUFFER_ID = lotInfo.Bom == ATBom.NULL ? string.Empty : lotInfo.Bom.ToBuffer.BufferID;

            if (lotInfo.OperTarget.CurrentBomDetail != null)
            {
                output.TO_ITEM_ID = lotInfo.Bom == ATBom.NULL ? string.Empty : lotInfo.OperTarget.CurrentBomDetail.ToItemSiteBuffer.ItemID;
                output.TO_SITE_ID = lotInfo.Bom == ATBom.NULL ? string.Empty : lotInfo.OperTarget.CurrentBomDetail.ToItemSiteBuffer.SiteID;
            }
            else
            {
                output.TO_ITEM_ID = lotInfo.Bom == ATBom.NULL ? string.Empty : output.FROM_ITEM_ID;
                output.TO_SITE_ID = lotInfo.Bom == ATBom.NULL ? string.Empty : output.FROM_SITE_ID;
            }

            output.OPER_YIELD = lotInfo.Yield;
            output.PLAN_QTY = allocationInfo.AllocLotQty;
            output.PLAN_UNIT_QTY = allocationInfo.AllocLotQty / lotInfo.OperTarget.BCumChangeRatio;

            output.RES_ID = allocationInfo.Bucket == null ? "Dummy" : allocationInfo.Bucket.BucketID;

            output.ARRIVAL_DATETIME = lotInfo.ArrivalTime;
            output.START_DATETIME = allocationInfo.StartTime;
            output.END_DATETIME = allocationInfo.EndTime;
            output.RES_END_DATETIME = allocationInfo.BucketEndTime == DateTime.MinValue ? allocationInfo.EndTime : allocationInfo.BucketEndTime;

            output.CREATION_TYPE = lot.LotType.ToString();
            output.WIP_TYPE = lot.IsWipLot ? lot.Wip.WipInfo.LotType.ToString() : "Intarget";

            output.PLAN_DATE = ATUtil.ToDate(planInfo.StartTime).AddBarBetweenDate();
            output.PLAN_WEEK = ATUtil.ToWeek(planInfo.StartTime).AddBarBetweenDate();
            output.PLAN_MONTH = ATUtil.ToMonth(planInfo.StartTime).AddBarBetweenDate();

            output.TARGET_DATETIME = lotInfo.OperTarget.TargetDateTime;
            output.TARGET_DATE = lotInfo.OperTarget.TargetDate.AddBarBetweenDate();
            output.TARGET_WEEK = lotInfo.OperTarget.TargetWeek.AddBarBetweenDate();
            output.TARGET_MONTH = lotInfo.OperTarget.TargetMonth.AddBarBetweenDate();

            output.LPST_GAP_DAY = (lotInfo.OperTarget.TargetDateTime - planInfo.StartTime).TotalDays;

            output.MO_ID = lotInfo.OperTarget.MoPlan.ID;
            output.DEMAND_ID = lotInfo.OperTarget.SODemand.ID;
            output.DEMAND_ITEM_ID = lotInfo.OperTarget.SODemand.ItemID;
            output.DUE_DATE = lotInfo.OperTarget.SODemand.DueDateTime.ToString(ATUtil.PlanDateFormat);
            output.DEMAND_PRIORITY = lotInfo.OperTarget.SODemand.Priority;

            output.MAX_LATENESS_DAY = lotInfo.OperTarget.SODemand.MaxLateDays;
            output.EXTENDED_DUE_DATETIME = lotInfo.OperTarget.SODemand.DueDateTime.AddDays(lotInfo.OperTarget.SODemand.MaxLateDays);
            output.EXTENDED_TARGET_DATETIME = lotInfo.OperTarget.TargetDateTime.AddDays(lotInfo.OperTarget.SODemand.MaxLateDays);

            output.TOTAL_TAT = allocationInfo.TotalTat.SecondToUom(ATOption.Instance.TimeUOM);
            output.B_CHG_RATIO = lotInfo.OperTarget.BCumChangeRatio;

            output.ORG_LOT_ID = lotInfo.OrgLotID;

            output = ATOutputControl.Instance.WriteShortProdPlan(output, planInfo);
            if (output != null)
                table.Add(output);
             
            return output;
        }
    }
}
