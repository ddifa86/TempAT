using Mozart.Data;
using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Inputs;
using Mozart.SeePlan.Aleatorik.Outputs;
using Mozart.SeePlan.Aleatorik.Planner;
using Mozart.SeePlan.TimeLibrary;
using Mozart.Simulation.Engine;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public partial class OutputWriter
    {
        private int _resourcePlanSequence = 0;

        public LOT_HISTORY WriteLotHistory(APELot lot, double qty, string lotevent, DateTime now, string newbatchs)
        {
            if (IsWriteOutput("LOT_HISTORY") == false)
                return null;

            var table = DerivedHelper.GetTable<LOT_HISTORY>();
            var output = (LOT_HISTORY)table.New();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.STAGE_ID = ATExecutionContext.Instance.CurrentStage.StageID;
            output.MODULE_ID = ATExecutionContext.Instance.CurrentExecutionInfo.Key;
            output.EVENT_DATETIME = now;
            output.EVENT_TYPE = lotevent;
            output.LOT_ID = lot.LotID;
            output.LOT_QTY = qty;
            
            output.ITEM_ID = lot.CurrentItemSiteBuffer.ItemID;
            output.SITE_ID = lot.CurrentItemSiteBuffer.SiteID;
            output.BUFFER_ID = lot.CurrentItemSiteBuffer.BufferID;

            if (lot.CurrentBom != null && lot.CurrentBom != ATBom.NULL)
                output.BOM_ID = lot.CurrentBom.BomID;

            if (lot.CurrentOper != null)
            {
                output.ROUTING_ID = lot.CurrentOper.IsBuffer ? "-" : lot.CurrentRoute.RouteID;
                output.OPER_ID = lot.CurrentOper.IsBuffer ? "-" : lot.CurrentOperID;
            }

            if (lot.CurrentTarget != null)
            {
                output.DEMAND_ID = lot.CurrentTarget.SODemand.ID;
                output.MO_ID = lot.CurrentTarget.MoPlan.ID;
            }

            output.ADD_INFO = newbatchs;

            if (lotevent == LifeCycle.Release.ToString())
            {
                if (lot.AssemblyInfo != null)
                    output.ORG_LOT_ID = lot.AssemblyInfo.PartLotIds;

                if (lot.OrgLot != null)
                    output.ORG_LOT_ID = lot.OrgLot.LotID;
            }

            if (lot.Wip != null && lotevent == LotCreateType.Creation.ToString())
                output.WIP_ID = lot.Wip.WipInfo.LotID;

            if (lotevent == LifeCycle.Remain.ToString())
                output.ADD_INFO = lot.ShortCategory;

            output.CREATION_TYPE = lot.LotType.ToString();

            output = ATOutputControl.Instance.WriteLotHistory(output, lot);
            if(output != null)
                table.Add(output);

            return output;
        }

        public LOT_ASSEMBLY_LOG WriteLotAssemblyLog(APELot assyLot, ATBomDetail detail, APELot partlot, double qty, DateTime now)
        {
            if (IsWriteOutput("LOT_ASSEMBLY_LOG") == false)
                return null;

            var table = DerivedHelper.GetTable<LOT_ASSEMBLY_LOG>();
            var output = (LOT_ASSEMBLY_LOG)table.New();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.STAGE_ID = ATExecutionContext.Instance.CurrentStage.StageID;
            output.MODULE_ID = ATExecutionContext.Instance.CurrentExecutionInfo.Key;
            output.PHASE_NO = ATExecutionContext.Instance.CurrentExecutionInfo.CurPhase;

            output.EVENT_DATETIME = now;
            output.AVAILABLE_DATETIME = partlot.LastStepTime;

            output.FROM_LOT_ID = partlot.LotID;
            output.FROM_ITEM_ID = partlot.Item.ItemID;
            output.FROM_ITEM_TYPE = partlot.Item.ItemType.ToString();
            output.FROM_LOT_QTY = qty;
            output.FROM_TARGET_DATETIME = partlot.CurrentTarget.TargetDateTime;
            output.FROM_DEMAND_ID = partlot.CurrentTarget.SODemand.ID;
            output.FROM_SITE_ID = detail.FromSiteID;
            output.FROM_BUFFER_ID = detail.FromBufferID;

            output.TO_LOT_ID = assyLot.LotID;
            output.TO_ITEM_ID = assyLot.Item.ItemID;
            output.TO_ITEM_TYPE = assyLot.Item.ItemType.ToString();
            output.TO_SITE_ID = assyLot.CurrentItemSiteBuffer.SiteID;
            output.TO_LOT_QTY = assyLot.Qty;

            output.FROM_QTY = detail.FromQty;
            output.BOM_ID = detail.BomID;
            
            output.TO_BUFFER_ID = detail.ToBufferID;

            output.ROUTING_ID = assyLot.CurrentRoute.RouteID;
            output.OPER_ID = assyLot.InitOper.OperID;

            if (assyLot.CurrentTarget != null)
            {
                output.TO_TARGET_DATETIME = assyLot.CurrentTarget.TargetDateTime;
                output.TO_DEMAND_ID = assyLot.CurrentTarget.SODemand.ID;
            }

            output = ATOutputControl.Instance.WriteLotAssemblyLog(output, assyLot, partlot, detail);
            if (output != null)
                table.Add(output);

            return output;
        }

        public SHORT_REPORT WriteShortReport(APELot lot)
        {
            if (lot.IsNoTargetSplitLot)
                return null;

            if (IsWriteOutput("SHORT_REPORT") == false)
                return null;

            var table = DerivedHelper.GetTable<SHORT_REPORT>();
            var output = (SHORT_REPORT)table.New();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;

            output.STAGE_ID = ATExecutionContext.Instance.CurrentStage.StageID;
            output.MODULE_ID = ATExecutionContext.Instance.CurrentExecutionInfo.Key;
            output.PHASE_NO = ATExecutionContext.Instance.CurrentExecutionInfo.CurPhase;

            var demand = lot.CurrentTarget.SODemand;

            output.DEMAND_ID = demand.ID;
            output.DEMAND_ITEM_ID = demand.ItemID;
            output.LOT_ID = lot.LotID;

            output.DUE_MONTH = ATUtil.ToMonth(demand.DueDateTime, true).AddBarBetweenDate();
            output.DUE_WEEK = ATUtil.ToWeek(demand.DueDateTime, true).AddBarBetweenDate();
            output.DUE_DATE = ATUtil.ToDate(demand.DueDateTime, true).AddBarBetweenDate();
            output.MAX_LATENESS_DAY = Convert.ToInt32(demand.MaxLateDays);
            output.EXTENDED_DUE_DATE = demand.MaxLateDueDateTime.ToString("yyyyMMdd");

            output.DEMAND_PRIORITY = demand.Priority;
            output.DEMAND_QTY = demand.Qty;
            output.SHORT_QTY = lot.Qty;
            output.SHORT_UNIT_QTY = lot.Qty / lot.CurrentTarget.BCumChangeRatio;
            output.REASON_CATEGORY = lot.ShortCategory;
            output.REASON_NAME = lot.ReasonName;
            output.ROUTING_ID = lot.CurrentBom == ATBom.NULL ? null : lot.CurrentTarget.RouteID;
            output.OPER_ID = lot.CurrentTarget.Oper.IsBuffer ? string.Empty : lot.CurrentTarget.Oper.OperID;
            output.ITEM_ID = lot.CurrentItemSiteBuffer.ItemID;
            output.BUFFER_ID = lot.CurrentItemSiteBuffer.BufferID;
            output.SITE_ID = lot.CurrentItemSiteBuffer.SiteID;
            output.BOM_ID = lot.CurrentBom.BomID;

            output = ATOutputControl.Instance.WriteShortReport(output, lot, null);
            if (output != null)
                table.Add(output);

            return output;
        }

        public REF_PROD_PLAN_LOG WriteRefProdPlanLog(APERefPlan refPlan, ATDemand demand, string reason, int retryCount)
        {
            if (IsWriteOutput("REF_PROD_PLAN_LOG") == false) //불필요한 반복
                return null;

            // 마지막
            var table = DerivedHelper.GetTable<REF_PROD_PLAN_LOG>();
            var output = (REF_PROD_PLAN_LOG)table.New();

            output.PLAN_VER = AleatorikGlobalParameters.Instance.PlanScenario;
            output.PROJECT_ID = AleatorikGlobalParameters.Instance.ProjectID;
            output.PHASE_NO = ATExecutionContext.Instance.CurrentExecutionInfo.CurPhase;
            output.REF_PLAN_VER = refPlan.ID;
            output.DEMAND_ID = refPlan.SoID;
            output.MO_ID = demand.Moplan.ID;

            output.STAGE_ID = refPlan.Stage.StageID;
            output.TARGET_DEMAND_ID = demand.ID;
            output.REASON = reason;
            output.ITEM_ID = refPlan.ItemSiteBuffer.ItemID;
            output.SITE_ID = refPlan.ItemSiteBuffer.SiteID;
            output.BUFFER_ID = refPlan.ItemSiteBuffer.BufferID;
            output.DUE_DATE = refPlan.DueDate;
            output.BOM_ID = refPlan.Bom?.BomID;
            output.ROUTING_ID = refPlan.Route?.RouteID;
            output.OPER_ID = refPlan.Operation?.OperID;
            output.RES_ID = refPlan.Resource?.ResourceID;
            output.REF_TYPE = refPlan.Type;
            output.DEMAND_QTY = refPlan.Qty;
            output.REMAIN_QTY = refPlan.RemainQty;

            output = ATOutputControl.Instance.WriteRefProdPlan(output);
            if (output != null)
                table.Add(output);

            return output;
        }

        public PROD_PLAN WriteProdPlan(APELot lot, APEPlanInfo planInfo)
        {
            planInfo.IsWritePlanInfo = true;

            if (IsWriteOutput("PROD_PLAN") == false)
                return null;

            if (planInfo.AllocationInfo.AllocLotQty <= ATOption.Instance.MinimumAllocationQuantity)
                return null;

            if (planInfo.LotInfo == null)
                return null; // 버그

            var table = DerivedHelper.GetTable<PROD_PLAN>();
            var output = (PROD_PLAN)table.New();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.STAGE_ID = ATExecutionContext.Instance.CurrentStage.StageID;
            output.MODULE_ID = ATExecutionContext.Instance.CurrentExecutionInfo.Key;
            output.PHASE_NO = ATExecutionContext.Instance.CurrentExecutionInfo.CurPhase;

            var lotInfo = planInfo.LotInfo;
            var allocationInfo = planInfo.AllocationInfo;

            output.PLAN_SEQ = ++_resourcePlanSequence; // planInfo.PlanSeq;
            output.LEVEL_NO = planInfo.Level;
            output.ALLOCATION_SEQ = allocationInfo.AllocSeq;

            output.REF_PLAN_VER = allocationInfo.RefPlan?.ID;

            output.LOT_ID = lot.LotID;
            output.ORG_LOT_ID = lotInfo.OrgLotID;
            output.LOT_GROUP_KEY = lotInfo.LotGroupKey;
            output.BOM_ID = lotInfo.Bom.BomID;

            output.ITEM_ID = lotInfo.Item.ItemID;
            output.ITEM_TYPE = lotInfo.Item.ItemType.ToString();
            output.SITE_ID = lotInfo.ItemSiteBuffer.SiteID;
            output.BUFFER_ID = lotInfo.Bom == ATBom.NULL ? lotInfo.Operation.OperID : lotInfo.Bom.FromBuffer.BufferID;

            output.ROUTING_ID = lotInfo.Route == null ? string.Empty : lotInfo.Route.RouteID;
            output.OPER_ID = lotInfo.Operation.IsBuffer ? string.Empty : lotInfo.Operation.OperID;

            output.IN_PLAN_QTY = allocationInfo.AllocLotQty;
            output.IN_PLAN_UNIT_QTY = allocationInfo.AllocLotQty / lotInfo.OperTarget.BCumChangeRatio;

            output.OUT_PLAN_QTY = allocationInfo.AllocLotQty * lotInfo.Yield;
            output.OUT_PLAN_UNIT_QTY = allocationInfo.AllocLotQty / lotInfo.OperTarget.BCumChangeRatio;

            output.RES_ID = planInfo.Bucket.BucketID;
            output.ARRIVAL_DATETIME = planInfo.ArrivalTime;

            bool isTimePlan = allocationInfo.Bucket.CapacityType == CapacityType.Time;
            output.USED_CAPA = isTimePlan ? allocationInfo.UsedCapaQty.SecondToUom(ATOption.Instance.TimeUOM) : allocationInfo.UsedCapaQty;
            output.USAGE_PER = isTimePlan ? allocationInfo.UsagePer.SecondToUom(ATOption.Instance.TimeUOM) : allocationInfo.UsagePer;
            output.TOTAL_TAT = allocationInfo.TotalTat.SecondToUom(ATOption.Instance.TimeUOM);
            output.UTIL_RATE = allocationInfo.UtilizationRate;

            output.START_DATETIME = planInfo.StartTime;
            output.END_DATETIME = planInfo.EndTime;
            output.RES_END_DATETIME = allocationInfo.BucketEndTime;

            output.CREATION_TYPE = lotInfo.LotType;
            output.WIP_TYPE = lotInfo.WipType;

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
            output.DEMAND_SITE_ID = lotInfo.OperTarget.SODemand.SiteID;
            output.DEMAND_BUFFER_ID = lotInfo.OperTarget.SODemand.BufferID;
            output.DUE_DATE = lotInfo.OperTarget.SODemand.DueDateTime.ToString(ATUtil.PlanDateFormat);
            output.DEMAND_PRIORITY = lotInfo.OperTarget.SODemand.Priority;
           
            output.MAX_LATENESS_DAY = Convert.ToInt32(lotInfo.OperTarget.SODemand.MaxLateDays);
            output.EXTENDED_DUE_DATETIME = lotInfo.OperTarget.SODemand.CalcDueDateTime.AddDays(lotInfo.OperTarget.SODemand.MaxLateDays);
            output.EXTENDED_TARGET_DATETIME = lotInfo.OperTarget.TargetDateTime.AddDays(lotInfo.OperTarget.SODemand.MaxLateDays);
            output.DUE_DATETIME = planInfo.OperTarget.SODemand.CalcDueDateTime;
            output.OPER_YIELD = lotInfo.Yield;
            output.CHANGE_RATIO = lotInfo.OperTarget.BCumChangeRatio;
            
            output = ATOutputControl.Instance.WriteProdPlan(output, planInfo);
            if (output != null)
                table.Add(output);

            return output;
        }

        public RES_PLAN WriteResPlan(APEPlanInfo planInfo)
        {
            planInfo.IsWritePlanInfo = true;

            if (IsWriteOutput("RES_PLAN") == false)
                return null;

            var table = DerivedHelper.GetTable<RES_PLAN>();
            var output = (RES_PLAN)table.New();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.STAGE_ID = ATExecutionContext.Instance.CurrentStage.StageID;
            output.MODULE_ID = ATExecutionContext.Instance.CurrentExecutionInfo.Key;
            output.PHASE_NO = ATExecutionContext.Instance.CurrentExecutionInfo.CurPhase;

            var allocationInfo = planInfo.AllocationInfo;

            output.LEVEL_NO = planInfo.Level;
            output.PLAN_SEQ = ++_resourcePlanSequence;

            output.ALLOCATION_SEQ = allocationInfo.AllocSeq;

            output.RES_ID = planInfo.Bucket.BucketID;
            output.MAIN_RES_ID = allocationInfo.MainBucket.BucketID;

            output.CREATION_TYPE = planInfo.Type.ToString();
            output.ALLOCATION_TYPE = planInfo.Type.ToString();

            output.ARRIVAL_DATETIME = planInfo.ArrivalTime;
            output.START_DATETIME = planInfo.StartTime;
            output.END_DATETIME = planInfo.EndTime;
            output.RES_END_DATETIME = planInfo.AllocationInfo.BucketEndTime;

            output.PLAN_DATE = ATUtil.ToDate(planInfo.StartTime).AddBarBetweenDate();
            output.PLAN_WEEK = ATUtil.ToWeek(planInfo.StartTime).AddBarBetweenDate();
            output.PLAN_MONTH = ATUtil.ToMonth(planInfo.StartTime).AddBarBetweenDate();

            output.DUE_DATE = planInfo.StartTime.ToString(ATUtil.PlanDateFormat);
            output.TARGET_DATETIME = planInfo.StartTime;
            output.EXTENDED_DUE_DATETIME = planInfo.StartTime;
            output.EXTENDED_TARGET_DATETIME = planInfo.StartTime;
            output.DUE_DATETIME = planInfo.StartTime;

            output.DESCRIPTION = planInfo.Description;

            if (allocationInfo.IsAllocatedInfo)
            {
                output.TOTAL_TAT = allocationInfo.TotalTat.SecondToUom(ATOption.Instance.TimeUOM);

                bool isTimePlan = allocationInfo.Bucket.CapacityType == CapacityType.Time;
                output.USED_CAPA = isTimePlan ? allocationInfo.UsedCapaQty.SecondToUom(ATOption.Instance.TimeUOM) : allocationInfo.UsedCapaQty;
                output.USAGE_PER = isTimePlan ? allocationInfo.UsagePer.SecondToUom(ATOption.Instance.TimeUOM) : allocationInfo.UsagePer;
                output.UTIL_RATE = allocationInfo.UtilizationRate; 
            }

            if (planInfo.LotInfo != null)
            {
                var lotInfo = planInfo.LotInfo;
                output.MO_ID = lotInfo.OperTarget.MoPlan.ID;
                output.DEMAND_ID = lotInfo.OperTarget.SODemand.ID;
                output.LOT_ID = lotInfo.OrgLotID;
                output.BOM_ID = lotInfo.Bom.BomID;
                output.DEMAND_ITEM_ID = lotInfo.OperTarget.SODemand.ItemID;
                output.DEMAND_SITE_ID = lotInfo.OperTarget.SODemand.SiteID;
                output.DEMAND_BUFFER_ID = lotInfo.OperTarget.SODemand.BufferID;
                output.DUE_DATE = lotInfo.OperTarget.SODemand.DueDateTime.ToString(ATUtil.PlanDateFormat);
                output.DEMAND_PRIORITY = lotInfo.OperTarget.SODemand.Priority;
                output.LOT_GROUP_KEY = lotInfo.LotGroupKey;
                output.ITEM_ID = lotInfo.Item.ItemID;
                output.ITEM_TYPE = lotInfo.Item.ItemType.ToString();
                output.SITE_ID = lotInfo.ItemSiteBuffer.SiteID;
                output.BUFFER_ID = lotInfo.ItemSiteBuffer.BufferID;
                output.ROUTING_ID = lotInfo.Operation.RouteID;
                output.OPER_ID = lotInfo.Operation.OperID;

                output.PLAN_QTY = allocationInfo.AllocLotQty;
                output.PLAN_UNIT_QTY = allocationInfo.AllocLotQty / lotInfo.OperTarget.BCumChangeRatio;

                output.CREATION_TYPE = lotInfo.LotType;
                output.WIP_TYPE = lotInfo.WipType;

                output.TARGET_DATETIME = lotInfo.OperTarget.TargetDateTime;
                output.TARGET_DATE = lotInfo.OperTarget.TargetDate.AddBarBetweenDate();
                output.TARGET_WEEK = lotInfo.OperTarget.TargetWeek.AddBarBetweenDate();
                output.TARGET_MONTH = lotInfo.OperTarget.TargetMonth.AddBarBetweenDate();
                output.LPST_GAP_DAY = (lotInfo.OperTarget.TargetDateTime - planInfo.StartTime).TotalDays;
                output.MAX_LATENESS_DAY = Convert.ToInt32(lotInfo.OperTarget.SODemand.MaxLateDays);
                output.EXTENDED_DUE_DATETIME = lotInfo.OperTarget.SODemand.CalcDueDateTime.AddDays(lotInfo.OperTarget.SODemand.MaxLateDays);
                output.EXTENDED_TARGET_DATETIME = lotInfo.OperTarget.TargetDateTime.AddDays(lotInfo.OperTarget.SODemand.MaxLateDays);
                output.DUE_DATETIME = planInfo.OperTarget.SODemand.CalcDueDateTime;

                output.OPER_YIELD = lotInfo.Yield;
                output.B_CHG_RATIO = lotInfo.OperTarget.BCumChangeRatio;
                output.ORG_LOT_ID = lotInfo.OrgLotID;
            }

            // 소스 리뷰 (원석) 
            if (planInfo.Type == PlanInfoType.PM)
                output.OPER_ID = planInfo.Description;

            if (planInfo.Type == PlanInfoType.Setup)
                output.OPER_ID = PlanInfoType.Setup.ToString();

            if (planInfo.Type == PlanInfoType.Constraint)
                output.RES_ID = planInfo.Description;

            output = ATOutputControl.Instance.WriteResPlan(output, planInfo.Bucket, null, planInfo, planInfo.Type);
            if (output != null)
                table.Add(output);

            return output;
        }

        public RES_PLAN WriteNonWorkingPeriodResPlan(PBFResource bucket, DateTimeInterval period, string allocType, string operID)
        {
            if (IsWriteOutput("RES_PLAN") == false)
                return null;

            DateTime startTime = period.Start;
            DateTime endTime = period.End;

            var table = DerivedHelper.GetTable<RES_PLAN>();
            var output = (RES_PLAN)table.New();
            DateTime planDate = FWFactory.Instance.NowDT;

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.STAGE_ID = ATExecutionContext.Instance.CurrentStage.StageID;
            output.MODULE_ID = ATExecutionContext.Instance.CurrentExecutionInfo.Key;

            // output.LOT_ID = bucket.BucketID;

            output.ALLOCATION_SEQ = 0;

            output.OPER_ID = operID;

            output.RES_ID = bucket.BucketID;
            output.MAIN_RES_ID = bucket.BucketID;

            output.START_DATETIME = startTime;
            output.END_DATETIME = endTime;
            output.RES_END_DATETIME = endTime;

            output.CREATION_TYPE = allocType;

            output.PLAN_DATE = ATUtil.ToDate(planDate).AddBarBetweenDate();
            output.PLAN_WEEK = ATUtil.ToWeek(planDate).AddBarBetweenDate();
            output.PLAN_MONTH = ATUtil.ToMonth(planDate).AddBarBetweenDate();

            output.PLAN_SEQ = ++_resourcePlanSequence;
            output.ALLOCATION_TYPE = allocType;

            output.DUE_DATE = startTime.ToString(ATUtil.PlanDateFormat);
            output.ARRIVAL_DATETIME = startTime;
            output.TARGET_DATETIME = startTime;
            output.EXTENDED_DUE_DATETIME = startTime;
            output.EXTENDED_TARGET_DATETIME = startTime;
            output.DUE_DATETIME = startTime;

            output = ATOutputControl.Instance.WriteResPlan(output, bucket, null, null, PlanInfoType.NonWorking);
            if (output != null)
                table.Add(output);

            return output;
        }

        public PM_PLAN_LOG WritePmPlanLog(string bucketID, ATPMPeriod pm)
        {
            if (pm.IsWrite)
                return null;

            if (IsWriteOutput("PM_PLAN_LOG") == false)
                return null;

            var table = DerivedHelper.GetTable<PM_PLAN_LOG>();
            var output = (PM_PLAN_LOG)table.New();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.RES_ID = bucketID;
            output.PM_ID = pm.Name;
            output.PM_POLICY = pm.PMPolicy.ToString();
            output.ORG_START_DATETIME = pm.PmInfo.StartTime;
            output.ORG_END_DATETIME = pm.PmInfo.EndTime;
            output.REV_START_DATETIME = pm.ExecutedStartTime;
            output.REV_END_DATETIME = pm.End;
            output.REVISED_YN = pm.IsReviced == true ? "Y" : "N";
            output.EXECUTED_YN = pm.PmFlag == PMFlag.Executed ? "Y" : "N";
            output.PM_PRIORITY = pm.PMPriority;

            pm.IsWrite = true;

            if (output != null)
                table.Add(output);

            return output;
        }

        public STAGE_OUT_PLAN WriteStageOutPlan(APELot lot)
        {
            ATExecutionContext.Instance.CurrentExecutionInfo.Kpi?.AddRtfQty(lot);

            var so = lot.CurrentTarget.SODemand;
            so.AddShipmentPlan(lot);

            if (IsWriteOutput("STAGE_OUT_PLAN") == false)
                return null;

            var table = DerivedHelper.GetTable<STAGE_OUT_PLAN>();
            var output = (STAGE_OUT_PLAN)table.New();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.STAGE_ID = ATExecutionContext.Instance.CurrentStage.StageID;
            output.SITE_ID = lot.CurrentItemSiteBuffer.SiteID;
            output.MODULE_ID = ATExecutionContext.Instance.CurrentExecutionInfo.Key;
            output.PHASE_NO = ATExecutionContext.Instance.CurrentExecutionInfo.CurPhase;

            output.LOT_ID = lot.LotID;
            output.BUFFER_ID = lot.CurrentItemSiteBuffer.BufferID;

            output.OPER_YIELD = 1;
            output.PLAN_QTY = lot.CurrentQty;
            output.PLAN_UNIT_QTY = lot.CurrentQty;

            output.ARRIVAL_DATETIME = lot.LastStepTime;
            output.START_DATETIME = lot.LastStepTime;
            output.END_DATETIME = lot.LastStepTime;

            output.ITEM_ID = lot.Item.ItemID;

            output.CREATION_TYPE = lot.LotType.ToString();
            output.WIP_TYPE = lot.IsWipLot ? lot.Wip.WipInfo.LotType.ToString() : "Intarget";

            output.PLAN_DATE = ATUtil.ToDate(lot.LastStepTime).AddBarBetweenDate();
            output.PLAN_WEEK = ATUtil.ToWeek(lot.LastStepTime).AddBarBetweenDate();
            output.PLAN_MONTH = ATUtil.ToMonth(lot.LastStepTime).AddBarBetweenDate();

            output.TARGET_DATETIME = lot.CurrentTarget.TargetDateTime;
            output.TARGET_DATE = lot.CurrentTarget.TargetDate.AddBarBetweenDate();
            output.TARGET_WEEK = lot.CurrentTarget.TargetWeek.AddBarBetweenDate();
            output.TARGET_MONTH = lot.CurrentTarget.TargetMonth.AddBarBetweenDate();

            output.LPST_GAP_DAY = (lot.CurrentTarget.TargetDateTime - lot.LastStepTime).TotalDays;

            output.DEMAND_ID = lot.CurrentTarget.SODemand.ID;
            output.DEMAND_ITEM_ID = lot.CurrentTarget.SODemand.ItemID;
            output.DEMAND_BUFFER_ID = lot.CurrentTarget.SODemand.BufferID;
            output.DEMAND_SITE_ID = lot.CurrentTarget.SODemand.SiteID;
            output.DUE_DATE = lot.CurrentTarget.SODemand.DueDateTime.ToString(ATUtil.PlanDateFormat);
            output.DEMAND_PRIORITY = lot.CurrentTarget.SODemand.Priority;
            output.PLAN_SEQ = AleatorikOutputMart.Instance.STAGE_OUT_PLAN.TotalCount + 1;

            output.MAX_LATENESS_DAY = Convert.ToInt32(lot.CurrentTarget.SODemand.MaxLateDays);
            output.EXTENDED_DUE_DATETIME = lot.CurrentTarget.SODemand.DueDateTime.AddDays(lot.CurrentTarget.SODemand.MaxLateDays);
            output.EXTENDED_TARGET_DATETIME = lot.CurrentTarget.TargetDateTime.AddDays(lot.CurrentTarget.SODemand.MaxLateDays);

            output = ATOutputControl.Instance.WriteStageOutPlan(output, lot);
            if (output != null)
                table.Add(output);

            return output;
        }

        internal void WritePlanningResult()
        {
            foreach (var demand in ATInputData.Demands.GetDemands())
            {
                if (demand.StageID == ATExecutionContext.Instance.CurrentStage.StageID)
                {
                    double soQty = demand.Qty;
                    if (demand.ShipmentPlans.Count > 0)
                    {
                        List<KeyValuePair<DateTime, double>> list = demand.ShipmentPlans.ToList();
                        list.Sort((x, y) => x.Key.CompareTo(y.Key));

                        for (int i = 0; i < list.Count; i++)
                        {
                            KeyValuePair<DateTime, double> info = list.ElementAt(i);

                            double qtyOpen = i == list.Count - 1 ? soQty : info.Value;

                            OutputWriter.Instance.WriteShipmentPlan(demand, info.Key, qtyOpen, info.Value);

                            soQty -= info.Value;
                        }
                    }
                    else
                    {
                        OutputWriter.Instance.WriteShipmentPlan(demand, demand.DueDateTime, soQty, 0);
                    }

                    demand.ShipmentPlans.Clear();
                }

                foreach (var info in demand.LateInfoManager.LateInfos.Values)
                {
                    WriteShortLog(info);
                }

                demand.LateInfoManager.LateInfos.Clear();
                demand.ShortManager.Clear();
            }
        }

        internal void WriteShortLog()
        {
            foreach (var demand in ATInputData.Demands.GetDemands())
            {
                if (demand.StageID != ATExecutionContext.Instance.CurrentStage.StageID)
                    continue;

                foreach (var info in demand.LateInfoManager.LateInfos.Values)
                {
                    WriteShortLog(info);
                }
            }
        }

        internal SHORT_LOG WriteShortLog(ATLateInfo info)
        {
            if (IsWriteOutput("SHORT_LOG") == false) //불필요한 반복
                return null;

            var table = DerivedHelper.GetTable<SHORT_LOG>();
            var output = (SHORT_LOG)table.New();

            var so = info.Demand;

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;

            output.STAGE_ID = ATExecutionContext.Instance.CurrentStage.StageID;
            output.MODULE_ID = ATExecutionContext.Instance.CurrentExecutionInfo.Key;
            output.PHASE_NO = info.Phase;
            output.RETRY_CNT = info.RetryCount;

            output.DEMAND_ID = so.ID;
            output.DEMAND_ITEM_ID = so.ItemID;
            output.DUE_DATE = so.DueDateTime.ToString(ATUtil.PlanDateFormat);
            output.DEMAND_QTY = so.Qty;

            output.SHORT_SEQ = AleatorikOutputMart.Instance.SHORT_LOG.TotalCount + 1;
            output.SHORT_TYPE = info.ShortType.ToString();
            output.SHORT_CATEGORY = info.Category.ToString();
            output.SHORT_REASON = info.Reason.ToString();
            output.SHORT_DETAIL_INFO = info.GetLateDetailInfo();
            output.SHORT_QTY = info.ShortQty;
            output.ISB_ID = info.ItemSiteBuffer.Key;
            
            if (info.Bom != ATBom.NULL)
                output.BOM_ID = info.Bom.BomID;

            if (info.Oper != null && info.Oper.IsBuffer == false)
            {
                output.ROUTING_ID = info.Oper.RouteID;
                output.OPER_ID = info.Oper.OperID;
            }

            if (info.Resources != null && info.Resources.Count > 0)
                output.RES_ID = string.Join(", ", info.Resources.Select(x => x.Target.ResourceID));

            output.REF_PLAN_VER = info.RefPlanID;
            output.FROM_LATE_DATETIME = info.FromDate;
            output.TO_LATE_DATETIME = info.ToDate;
            output.SHORT_CNT = info.Count;
            
            output = ATOutputControl.Instance.WriteShortLog(output, info);
            if (output != null)
                table.Add(output);

            return output;
        }

        internal SHORT_LOG WriteShortLog(DEMAND entity, string reason, string reasonDetail)
        {
            if (IsWriteOutput("SHORT_LOG") == false) //불필요한 반복
                return null;

            var table = DerivedHelper.GetTable<SHORT_LOG>();
            var output = (SHORT_LOG)table.New();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            
            ATBuffer buffer = ATInputData.ItemSiteBuffers.GetBuffer(entity.BUFFER_ID);
            string stage = "";
            if (buffer != null && buffer.Stage != null)
            {
                stage = buffer.Stage.StageID;
            }
            output.STAGE_ID = stage;
            output.MODULE_ID = "Persist";
            output.PHASE_NO = 0;

            output.DEMAND_ID = entity.DEMAND_ID;
            output.DEMAND_ITEM_ID = entity.ITEM_ID;
            output.DUE_DATE = entity.DUE_DATE.ToString(ATUtil.PlanDateFormat);
            output.DEMAND_QTY = entity.DEMAND_QTY;

            output.SHORT_SEQ = AleatorikOutputMart.Instance.SHORT_LOG.TotalCount + 1;
            output.SHORT_TYPE = Status.Short.ToString();
            output.SHORT_CATEGORY = LateCategory.StdData.ToString();
            output.SHORT_REASON = reason;
            output.SHORT_DETAIL_INFO = reasonDetail;
            output.SHORT_QTY = entity.DEMAND_QTY;
            output.ISB_ID = entity.ITEM_ID + "@" + entity.SITE_ID + "@" + entity.BUFFER_ID;

            output.BOM_ID = string.Empty;
            output.ROUTING_ID = string.Empty;
            output.OPER_ID = string.Empty;
            output.RES_ID = string.Empty;
            output.REF_PLAN_VER = string.Empty;
            output.FROM_LATE_DATETIME = AleatorikGlobalParameters.Instance.start_time;
            output.TO_LATE_DATETIME = AleatorikGlobalParameters.Instance.start_time;
            output.SHORT_CNT = 0;

            output = ATOutputControl.Instance.WriteShortLog(output, null);
            if (output != null)
                table.Add(output);

            return output;
        }

        internal void WriteRemainLotLog(FWFactory factory)
        {
            foreach (var line in factory.Lines)
            {
                foreach (var groups in line.Value.AllocGroups)
                {
                    foreach (var bucketGroup in groups.BucketGroups)
                    {
                        if (bucketGroup.Queue.HasLotGroup)
                        {
                            var lotGroups = bucketGroup.Queue.GetLotGroups();

                            while (lotGroups.Count > 0)
                            {
                                var lotGroup = lotGroups.First();
                                var lots = lotGroup.GetLotList();
                                while (lots.Count > 0)
                                {
                                    var lot = lots.First() as APELot;

                                    SetRemainLotLog(lot, factory.NextStartTime);
                                   
                                    APEWipAgent.Instance.AddRemainLot(lot);
                                    APEWipAgent.Instance.RemoveLot(lot);
                                }

                                lotGroups = bucketGroup.Queue.GetLotGroups();
                            }
                        }
                    }

                    foreach (var bucket in groups.Buckets)
                    {
                        var reserveInfos = bucket.ReserveManager;
                        if (reserveInfos.IsReserved)
                        {
                            var infos = reserveInfos.GetReserveInfos();
                            foreach (var info in infos)
                            {
                                info.ReservedLot.Apply((x, _) =>
                                {
                                    var sampleLot = x as APELot;
                                    SetRemainLotLog(sampleLot, factory.NextStartTime);
                                    APEWipAgent.Instance.AddRemainLot(sampleLot);
                                });
                            }
                        }
                    }
                }
            }

            var buffers = ATInputData.ItemSiteBuffers.GetBuffers();
            foreach (var buffer in buffers)
            {
                var logic = APEAssemblyAgent.Instance.GetAssemblyLogic(buffer);
                if (logic != null)
                {
                    var partLotInfos = logic.GetPartLotInfos();

                    foreach (var assyLots in logic.AssemblyLots)
                    {
                        foreach (var assyLot in assyLots.Value)
                        {
                            Tuple<ATBom, ATDemand> key = new Tuple<ATBom, ATDemand>(assyLot.CurrentBom, assyLot.CurrentTarget.SODemand);

                            var partLots = partLotInfos.GetDictionary(key);
                            if (partLots == null)
                                break;

                            double max = assyLot.CurrentTarget.RemainQty;
                            var demand = assyLot.CurrentTarget.SODemand;

                            foreach (var partLot in partLots)
                            {
                                double qty = partLot.Value.Sum(x => x.Qty);
                                double needQty = max.ConvertValue(partLot.Key.FromQty, partLot.Key.ToQty, PlanType.Backward);
                                double shortQty = needQty - qty;

                                LateReason reason;
                                if (partLot.Key.FromItemSiteBuffer.IsMaterialItemSiteBuffer)
                                    reason = LateReason.HAWAMaterialShort;
                                else
                                    reason = LateReason.FERTMaterialShort;

                                if (shortQty >= ATOption.Instance.MinimumAllocationQuantity)
                                {
                                    demand.LateInfoManager.AddShortInfo(partLot.Key.FromItemSiteBuffer, partLot.Key.Bom, null, null, LateCategory.Material, reason.ToString(), shortQty, null, assyLot.CurrentTarget.TargetDateTime, assyLot.CurrentTarget.TargetDateTime, assyLot.CurrentTarget.PegTarget?.PegPart?.CurRefPlan?.ID);
                                }
                            }
                        }
                    }

                    foreach (var lot in logic.GetRemainAssemblyLots())
                    {
                        lot.ShortCategory = "wait in assembly agent";
                        APEWipAgent.Instance.AddRemainLot(lot);
                    }
                }
            }

            foreach (var lot in APEWipAgent.Instance.GetRemainLots())
            {
                OutputWriter.Instance.WriteLotHistory(lot, lot.Qty, LifeCycle.Remain.ToString(), factory.NowDT, string.Empty);

                FWFactoryLogic.Instance.WritePlanInfos(lot);

                OutputWriter.Instance.WriteShortReport(lot);

                lot.Dispose();
            }
        }

        private void SetRemainLotLog(APELot lot, DateTime dt)
        {
            var status = lot.CurrentStatus(dt);

            switch (status)
            {
                case Status.Normal:
                    break;
                case Status.Late:
                    lot.UpdateLateInfos(Status.Late);
                    break;
                case Status.Short:
                    lot.UpdateLateInfos(Status.Short);
                    break;
                default:
                    break;
            }
            
            lot.ShortCategory = "wait in resource";
        }

        private SHIPMENT_PLAN WriteShipmentPlan(ATDemand so, DateTime date, double soQty, double planQty)
        {
            if (IsWriteOutput("SHIPMENT_PLAN") == false) //불필요한 반복
                return null;

            var table = DerivedHelper.GetTable<SHIPMENT_PLAN>();
            var output = (SHIPMENT_PLAN)table.New();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;

            output.ITEM_ID = so.ItemID;
            output.SITE_ID = so.SiteID;

            output.STAGE_ID = ATExecutionContext.Instance.CurrentStage.StageID;
            output.MODULE_ID = ATExecutionContext.Instance.CurrentExecutionInfo.Key;

            output.DEMAND_ID = so.ID;
            output.DUE_DATE = so.DueDateTime.ToString(ATUtil.PlanDateFormat);
            output.SHIPMENT_DATE = date.ToString(ATUtil.PlanDateFormat); //factoryTime 고려한 date
            output.DEMAND_PRIORITY = so.Priority;

            output.DEMAND_QTY = soQty;
            output.PROD_QTY = planQty;

            output.MAX_LATENESS_DAY = Convert.ToInt32(so.MaxLateDays);
            output.MAX_EARLINESS_DAY = Convert.ToInt32(so.MaxEarlyDays);
            output.DUE_WEEK = so.Week.AddBarBetweenDate(); // string으로 변경하면 같이 변경하기
            output.ON_TIME_QTY = date <= so.DueDateTime ? planQty : 0;
            output.LATE_QTY = date > so.DueDateTime ? planQty : 0;

            output = ATOutputControl.Instance.WriteShipmentPlan(output, so);
            if (output != null)
                table.Add(output);

            return output;
        }

        public CAPA_ALLOCATION_INFO WriteCapaAllocationPlan(ATConstraint info, ATConstraintDetail detail)
        {
            if (detail.IsWriteDetail)
                return null;

            if (IsWriteOutput("CAPA_ALLOCATION_INFO") == false) //불필요한 반복
                return null;

            var table = DerivedHelper.GetTable<CAPA_ALLOCATION_INFO>();
            var output = (CAPA_ALLOCATION_INFO)table.New();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;

            output.STAGE_ID = ATExecutionContext.Instance.CurrentStage.StageID;
            output.MODULE_ID = ATExecutionContext.Instance.CurrentExecutionInfo.Key;
            output.BUCKET_DATE = detail.Attribute.ApplyDate.AddBarBetweenDate();

            output.TARGET_ID = info.ConstraintID;
            output.TARGET_TYPE = ATReservedCode.CONSTRAINT;

            var effStartTime = detail.Attribute.EffectiveStartTime;
            var effEndTime = detail.Attribute.EffectiveEndTime;

            output.CALENDAR_ID = detail.Attribute.CalendarID;
            output.EFF_START_DATE = effStartTime.Date.ToString(ATUtil.PlanDateFormat);
            output.EFF_END_DATE = effEndTime.Date.ToString(ATUtil.PlanDateFormat);
            output.PATTERN_ID = detail.Attribute.PatternSeq;

            output.CAPA_TYPE = ATReservedCode.CONSTRAINT;

            output.ON_TIME_CAPA = detail.Qty;
            output.CAPA_MODE = "Finite";
            output.REMAIN_CAPA = detail.RemainQty;
            output.ALLOCATION_CAPA = detail.UsedQty;
            output.ALLOCATION_RATIO = detail.UsedRatio;

            output = ATOutputControl.Instance.WriteCapaAllocationInfo(output);
            if (output != null)
                table.Add(output);

            detail.IsWriteDetail = true;

            return output;
        }

        public CAPA_ALLOCATION_INFO WriteCapaAllocationPlan(IBucket bucket, APECapacity capacity)
        {
            if (IsWriteOutput("CAPA_ALLOCATION_INFO") == false) //불필요한 반복
                return null;

            var table = DerivedHelper.GetTable<CAPA_ALLOCATION_INFO>();
            var output = (CAPA_ALLOCATION_INFO)table.New();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.STAGE_ID = ATExecutionContext.Instance.CurrentStage.StageID;
            output.MODULE_ID = ATExecutionContext.Instance.CurrentExecutionInfo.Key;
            output.PHASE_NO = ATExecutionContext.Instance.CurrentExecutionInfo.CurPhase;

            var now = ShopCalendar.SplitDate(capacity.StartTime);
            output.BUCKET_DATE = now.ToString(ATUtil.PlanDateFormat);

            output.TARGET_ID = bucket.BucketID;
            output.TARGET_TYPE = "Resource";

            var effStartTime = capacity.StartTime;
            var effEndTime = capacity.EndTime;

            if (capacity.CapaInfo.CalAttr != null)
            {
                output.CALENDAR_ID = capacity.CapaInfo.CalAttr.CalendarID;
                output.PATTERN_ID = capacity.CapaInfo.CalAttr.PatternSeq;
                output.EFF_START_DATE = capacity.CapaInfo.CalAttr.EffectiveStartTime.ToString(ATUtil.PlanDateFormat);
                output.EFF_END_DATE = capacity.CapaInfo.CalAttr.EffectiveEndTime.ToString(ATUtil.PlanDateFormat);
            }
            else
            {
                output.EFF_START_DATE = effStartTime.Date.ToString(ATUtil.PlanDateFormat);
                output.EFF_END_DATE = effEndTime.Date.ToString(ATUtil.PlanDateFormat);
            }

            output.CAPA_TYPE = bucket.CapacityType.ToString();

            output.TOTAL_CAPA = capacity.Capacity;
            output.ON_TIME_CAPA = capacity.Capacity;
            output.CAPA_MODE = bucket.Target.IsInfinite ? "Infinite" : "Finite";
            output.ALLOCATION_CAPA = capacity.UsedCapa;
            output.REMAIN_CAPA = capacity.Capacity - capacity.UsedCapa;

            if (capacity is ATTimeCapacity)
            {
                var capa = capacity as ATTimeCapacity;

                output.TOTAL_CAPA = capacity.Capacity.SecondToUom(ATOption.Instance.TimeUOM);
                output.ALLOCATION_CAPA = capacity.UsedCapa.SecondToUom(ATOption.Instance.TimeUOM);

                output.OFF_TIME_CAPA = capa.OffTime.SecondToUom(ATOption.Instance.TimeUOM);
                output.SETUP_CAPA = capa.SetupTime.SecondToUom(ATOption.Instance.TimeUOM);
                output.PM_CAPA = capa.PM.SecondToUom(ATOption.Instance.TimeUOM);

                output.ON_TIME_CAPA = (capacity.Capacity - capa.OffTime).SecondToUom(ATOption.Instance.TimeUOM);
                output.REMAIN_CAPA = (capacity.Capacity - capacity.UsedCapa - capa.OffTime - capa.SetupTime - capa.PM).SecondToUom(ATOption.Instance.TimeUOM);
            }

            output.ALLOCATION_RATIO = output.ON_TIME_CAPA == 0 ? 0 : Math.Round((output.ALLOCATION_CAPA / output.ON_TIME_CAPA) * 100, 4);

            output = ATOutputControl.Instance.WriteCapaAllocationInfo(output);
            if (output != null)
                table.Add(output);

            if(bucket.Target.ResCategory == ResourceCategory.Resource)
                ATExecutionContext.Instance.CurrentExecutionInfo.Kpi?.AddUtil(bucket.Target.ResGroupID, capacity.StartTime, output.ALLOCATION_RATIO);

            return output;
        }

        public ALLOCATION_LOG WriteAllocationInfo(PBFAllocateContext context, PBFAllocationLog allocLog)
        {
            if (context.NowDt >= context.CurrentAllocator.Factory.EngineStartTime)
                return null;

            if (IsWriteOutput("ALLOCATION_LOG") == false)
                return null;

            var table = DerivedHelper.GetTable<ALLOCATION_LOG>();
            var output = (ALLOCATION_LOG)table.New();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.PHASE_NO = ATExecutionContext.Instance.CurrentExecutionInfo.CurPhase;
            output.STAGE_ID = ATExecutionContext.Instance.CurrentStage.StageID;
            output.MODULE_ID = ATExecutionContext.Instance.CurrentExecutionInfo.Key;
            output.LEVEL_NO = context.Level;

            output.PLAN_DATE = context.NowDt.ToString(ATUtil.PlanDateFormat);

            // ResourceGroup 별 누적 정보
            output.TARGET_ID = allocLog.TargetResourceID;

            int selectCnt = allocLog.SelectedLotGroup == null ? 0 : 1;
            int filterCnt = allocLog.FilterLotGroups.Count();
            int candidateCnt = allocLog.CandidatedLotGroups.Count();

            output.INIT_LOT_GROUP_CNT = selectCnt + filterCnt + candidateCnt;
            output.FILTERED_LOT_GROUP_CNT = filterCnt;
            output.USABLE_LOT_GROUP_CNT = selectCnt + candidateCnt;

            output.FILTERED_LOT_GROUP = allocLog.FilterLotGroups.ToList<IFactorObject>().GetFilterLog();

            var sr = new StringBuilder();
            if (allocLog.SelectedLotGroup != null)
            {
                sr.Append(allocLog.SelectedLotGroup.FactorValues);
                sr.AppendLine();
            }

            sr.Append(allocLog.CandidatedLotGroups.ToList<IFactorObject>().GetFactorLog());
            output.USABLE_LOT_GROUP = sr.ToString();

            // LotGroup 선택 후 변경되는 정보.
            output.USED_LOT_GROUP = allocLog.AllocatedLotGroupInfo;
            output.ALLOCATION_SEQ = allocLog.AllocationSeq;
            output.USABLE_RES = allocLog.SelectedBuckets.ToList<IFactorObject>().GetFactorLog();
            output.FILTERED_RES = allocLog.FilterResources.ToList<IFactorObject>().GetFilterLog();

            output.LOG_TYPE = allocLog.LogType;
            output.USED_RES = string.Join(",", allocLog.LoadedBuckets);
            output.ALLOCATION_TYPE = context.AllocateType.ToString();

            output = ATOutputControl.Instance.WriteAllocationLog(output, context);
            if (output != null)
                table.Add(output);

            return output;
        }
    }
}
