using Mozart.Data.Entity;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.Simulation.Engine;
using Mozart.SeePlan.Aleatorik.Inputs;
using Mozart.SeePlan.Aleatorik.Outputs;
using Mozart.SeePlan.Pegging;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.DataActions;
using System.Reflection;

namespace Mozart.SeePlan.Aleatorik
{
    public partial class OutputWriter
    {
        public static OutputWriter Instance
        {
            get
            {
                return ServiceLocator.Resolve<OutputWriter>();
            }
        }

        public bool IsWriteOutput(string tableName)
        {
            if (ATOption.Instance.OutputTablesNotToLog.Contains(tableName))
                return false;

            return true;
        }

        public EXECUTION_TIME_LOG WriteExecutionTimeLog(string key, TimeSpan time)
        {
            if (IsWriteOutput("EXECUTION_TIME_LOG") == false)
                return null;

            var table = DerivedHelper.GetTable<EXECUTION_TIME_LOG>();
            var output = (EXECUTION_TIME_LOG)table.New();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.SECTION = key;
            output.ELAPSED_SEC = time.TotalSeconds;


            if (output != null)
                table.Add(output);

            return output;
        }
      
        public ELAPSED_TIME_LOG WriteElapsedTimeLog(string key, TimeInfo timeInfo)
        {
            if (IsWriteOutput("ELAPSED_TIME_LOG") == false)
                return null;

            var table = DerivedHelper.GetTable<ELAPSED_TIME_LOG>();
            var output = (ELAPSED_TIME_LOG)table.New();

            // 수정 필요
            var elapsedInfo = key.Split('^');
            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.STAGE_ID = "-";
            output.MODULE_ID = "-";

            if (elapsedInfo.Count() == 1)
            {
                output.RULE_POINT = elapsedInfo[0];
            }
            else
            {
                output.STAGE_ID = elapsedInfo[0];
                output.MODULE_ID = elapsedInfo[1];
                output.PHASE_NO = Int32.Parse(elapsedInfo[2]);
                output.RULE_POINT = elapsedInfo[3];
            }

            output.CALL_CNT = timeInfo.Count;
            output.ELAPSED_SEC = timeInfo.ElapseTimes.TotalSeconds;

            if (output != null)
                table.Add(output);

            return output;
        }

        internal INIT_DEMAND_LOG WriteInitDemandLog(APEPegPart pp, int seq)
        {
            if (IsWriteOutput("INIT_DEMAND_LOG") == false)
                return null;

            APETarget target = pp.SampleTarget;
            ATDemand demand = (target.MoPlan as ATMoPlan).Demand;
            var table = DerivedHelper.GetTable<INIT_DEMAND_LOG>();
            var output = (INIT_DEMAND_LOG)table.New();

            ATItemSiteBuffer soItemSiteBuffer = demand.ItemSiteBuffer;

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.MODULE_ID = ATExecutionContext.Instance.CurrentExecutionInfo.Key;
            output.PHASE_NO = ATExecutionContext.Instance.CurrentExecutionInfo.CurPhase;

            output.DEMAND_PEG_SEQ = seq;
            output.STAGE_ID = demand.StageID;
            output.SITE_ID = demand.SiteID;
            output.DEMAND_ID = demand.SoDemand.ID;
            output.MO_ID = demand.Moplan.ID;
            output.ITEM_ID = demand.ItemID;
            output.DUE_DATE = demand.DueDateTime.ToString(ATUtil.PlanDateFormat);
            output.DEMAND_QTY = demand.Qty;
            output.DEMAND_PRIORITY = demand.Priority;
            output.CUST_ID = demand.CustomerID;
            output.DEMAND_TYPE = string.Empty;
            output.MAX_LATENESS_DAY = demand.MaxLateDays;
            output.MAX_EARLINESS_DAY = demand.MaxLateDays;
            output.LATE_DATE = demand.MaxLateDueDateTime;
            output.DUE_WEEK = demand.Week;
            output.ITEM_GRADE = soItemSiteBuffer.Item.Grade;

            output.COMPARE_LOG = pp.FactorValues;
            output.FILTER_LOG = pp.FilterValues;

            if (output != null)
                table.Add(output);

            return output;
        }
      

        public SCENARIO_RULESET_CONFIG_DATA WriteExecutionRulSetConfigLog(string moduleKey, string targetType, string targetId, string rulesetId, int phase) 
        {
            if (IsWriteOutput("SCENARIO_RULESET_CONFIG_DATA") == false)
                return null;

            var table = DerivedHelper.GetTable<SCENARIO_RULESET_CONFIG_DATA>();
            var output = (SCENARIO_RULESET_CONFIG_DATA)table.New();

            string key = moduleKey + targetType + targetId + rulesetId;
            var log = ATRuleAgent.Instance.GetRuleSetConfigLogs(key);

            if (log == null)
            {
                ATRuleSetConfigLog obj = ObjectMapper.CreateRuleSetConfigLog("-", moduleKey, targetType, targetId, rulesetId, phase);
                ATRuleAgent.Instance.AddRuleSetConfigLog(obj);

                output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
                output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
                output.MODULE_ID = moduleKey;
                output.TARGET_ID = targetId;
                output.TARGET_CATEGORY = targetType;
                output.RULESET_ID = rulesetId;

                if (output != null)
                    table.Add(output);
            }

            return output;
        }
 
        public SCENARIO_OPTION_CONFIG_DATA WriteExecutionOptionConfigLog(SCENARIO_OPTION_CONFIG entity)
        {
            if (IsWriteOutput("SCENARIO_OPTION_CONFIG_DATA") == false)
                return null;

            var table = DerivedHelper.GetTable<SCENARIO_OPTION_CONFIG_DATA>();
            var output = (SCENARIO_OPTION_CONFIG_DATA)table.New();

            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;

            output.SCENARIO_ID = entity.SCENARIO_ID;
            output.MODULE_ID = entity.MODULE_ID;
            output.OPTION_ID = entity.OPTION_ID;
            output.OPTION_VALUE = entity.OPTION_VALUE;
            output.CALENDAR_ID = entity.CALENDAR_ID;
            output.DESCRIPTION = entity.DESCRIPTION;
            output.PHASE_NO = entity.PHASE_NO;

            if (output != null)
                table.Add(output);
            
            return output;
        }

        public RULE_FACTOR_DATA WritePresetFactorMapLog(RULE_FACTOR entity)
        {
            if (IsWriteOutput("RULE_FACTOR_DATA") == false)
                return null;

            var table = DerivedHelper.GetTable<RULE_FACTOR_DATA>();
            var output = (RULE_FACTOR_DATA)table.New();

            output.PLAN_VER= AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.RULE_ID = entity.RULE_ID;
            output.FACTOR_ID = entity.FACTOR_ID;
            output.FACTOR_SEQ = entity.FACTOR_SEQ;
            output.FACTOR_WEIGHT = entity.FACTOR_WEIGHT;
            output.FACTOR_VALUE = entity.FACTOR_VALUE;

            if (output != null)
                table.Add(output);

            return output;
        }

        public RULESET_CONFIG_DATA WriteRuleActionMapLog(RULESET_CONFIG entity)
        {
            if (IsWriteOutput("RULESET_CONFIG_DATA") == false)
                return null;

            var table = DerivedHelper.GetTable<RULESET_CONFIG_DATA>();
            var output = (RULESET_CONFIG_DATA)table.New();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.RULESET_ID = entity.RULESET_ID;
            output.RULE_POINT = entity.RULE_POINT;
            output.LEVEL_NO = entity.LEVEL_NO;
            output.RULE_ID = entity.RULE_ID;

            if (output != null)
                table.Add(output);

            return output;
        }



        public BOM_NETWORK_INFO WriteBomNetwork(ATBomDetail detail, ATItemSiteBuffer sop)
        {
            if (IsWriteOutput("BOM_NETWORK_INFO") == false) // 불필요한 반복
                return null;

            var table = DerivedHelper.GetTable<BOM_NETWORK_INFO>();
            var output = (BOM_NETWORK_INFO)table.New();            

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.DEMAND_ITEM_ID = sop.ItemID;
            output.DEMAND_SITE_ID = sop.SiteID;
            output.DEMAND_BUFFER_ID = sop.BufferID;

            output.BOM_ID = detail.BomID;
            output.BOM_TYPE = detail.Bom.BomType.ToString();

            output.FROM_SITE_ID = detail.FromSiteID;
            output.FROM_ITEM_ID = detail.FromItemID;
            output.FROM_BUFFER_ID = detail.FromBufferID;

            output.FROM_BUFFER_SEQ = detail.FromBuffer.Sequence;
            output.FROM_QTY = detail.FromQty;

            output.TO_SITE_ID = detail.ToSiteID;
            output.TO_ITEM_ID = detail.ToItemID;
            output.TO_BUFFER_ID = detail.ToBufferID;
            output.TO_BUFFER_SEQ = detail.ToBuffer.Sequence;

            output.TO_QTY = detail.ToQty;

            output.USABLE_DETAIL_YN = detail.IsUsableDetail.ToString();
            output.USABLE_BOM_YN = detail.Bom.IsUsableBom.ToString();

            string routeIDs = string.Empty;
            string resourceIDs = string.Empty;

            foreach (var route in detail.Bom.BomRoutes)
            {
                if (string.IsNullOrEmpty(routeIDs))
                {
                    routeIDs += route.RouteID;
                }
                else
                {
                    routeIDs += "/" + route.RouteID;
                }

                if (!string.IsNullOrEmpty(route.Route.ResourceIDs))
                {
                    if (string.IsNullOrEmpty(resourceIDs))
                    {
                        resourceIDs += route.Route.ResourceIDs;
                    }
                    else
                        resourceIDs += "/" + route.Route.ResourceIDs;
                }
            }

            output.ROUTING_ID = routeIDs;
            output.RES_LIST = resourceIDs;
            output.BOM_PRIORITY = Convert.ToInt32(detail.Bom.Priority);

            output.ROUTING_TAT = detail.Bom.MinTat.SecondToUom(ATOption.Instance.TimeUOM);
            output.MIN_CUM_TAT = detail.ToItemSiteBuffer.GetMinCumTAT(DateTime.MinValue).SecondToUom(ATOption.Instance.TimeUOM); 
            output.MAX_CUM_TAT = detail.ToItemSiteBuffer.MaxCumTat.SecondToUom(ATOption.Instance.TimeUOM); 
            output.LATE_CUM_TAT = detail.ToItemSiteBuffer.LateCumTat.SecondToUom(ATOption.Instance.TimeUOM);

            output.FROM_WIP_QTY = detail.FromItemSiteBuffer.WipQueue.WipQty;
            output.TO_WIP_QTY = detail.ToItemSiteBuffer.WipQueue.WipQty;

            output.TO_WIP_QTY_SUM = detail.ToItemSiteBuffer.WipQueue.CumWipQty;
            output.FROM_WIP_QTY_SUM = detail.FromItemSiteBuffer.WipQueue.CumWipQty;

            output.ALL_RES_LIST = detail.Bom.All_ResIDs;
            output.PREV_ISB_LIST = detail.Bom.PrevIsbs;
            output.NEXT_ISB_LIST = detail.Bom.NextIsbs;

            output.MAX_CUM_YIELD = detail.FromItemSiteBuffer.MaxCumYield;

            output = ATOutputControl.Instance.WriteBomNetwork(output, detail, sop);
            if (output != null)
                table.Add(output);

            return output;
        }

        public ITEM_SITE_BUFFER_ALT_INFO WriteItemSiteBufferAltInfo(ATItemSiteBuffer item, ATBomDetailAlt alt, string altKey)
        {
            if (IsWriteOutput("ITEM_SITE_BUFFER_ALT_INFO") == false) // 불필요한 반복
                return null;

            var table = DerivedHelper.GetTable<ITEM_SITE_BUFFER_ALT_INFO>();
            var output = (ITEM_SITE_BUFFER_ALT_INFO)table.New();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.ALT_ISB_ID = altKey;
            output.ITEM_ID = item.ItemID;
            output.ITEM_GRADE = item.Item.Grade;
            output.SITE_ID = item.SiteID;
            output.BUFFER_ID = item.BufferID;
            output.ALT_ITEM_ID = alt.AltItemID;
            output.ALT_ITEM_GRADE = alt.AltItem.Grade;
            output.ALT_SITE_ID = alt.AltSiteID;
            output.ALT_BUFFER_ID = alt.AltBufferID;
            output.ALT_PRIORITY = Convert.ToInt32(alt.Priority);

            output = ATOutputControl.Instance.WriteItemSiteBufferAltInfo(output);
            if (output != null)
                table.Add(output);

            return output;
        }

        public void WriteErrorLog(ERROR_LOG err)
        {
            bool isWriteOutput = IsWriteOutput("ERROR_LOG");
            var table = DerivedHelper.GetTable<ERROR_LOG>();
            
            if (err != null && isWriteOutput)
                table.Add(err);
        }

        public ERROR_LOG WriteErrorLog(ModuleType module, string moduleKey, ErrorSeverity severity, ErrorReasonCode reasonCode,
            IEntityObject entity, string columns, string referredData = "", string detail = "")
        {
            bool isWriteOutput = IsWriteOutput("ERROR_LOG");

            var table = DerivedHelper.GetTable<ERROR_LOG>();

            var output = (ERROR_LOG)table.New();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.CATEGORY = module == ModuleType.None ? "Input" : "Engine";
            output.MODULE_ID = module == ModuleType.None ? "" : moduleKey;
            output.TARGET_KEY = entity != null ? entity.GetType().Name + "@" + columns : columns;

            string targetData = entity != null ?  GetErrorTargetData(entity, columns.Split(',')) : columns;
            output.TARGET_DATA = targetData;
            output.REFERRED_KEY = referredData;
            output.SEVERITY = severity.ToString();

            output.REASON_CODE = reasonCode.ToString();
            output.REASON_DETAIL = detail;

            if (output != null && isWriteOutput)
                table.Add(output);

            if (severity == ErrorSeverity.Critical)
            {
                ATPersistHelper.HasErrorData = true;
                ATPersistHelper.SetErrorStr(string.Format("Error : {0}\t\t{1}\t\t{2}\r\n",output.TARGET_KEY, output.TARGET_DATA, output.REASON_CODE));
            }

            return output;
        }

        public ERROR_LOG SetErrorLog(ModuleType module, string moduleKey, ErrorSeverity severity, ErrorReasonCode reasonCode,
            IEntityObject entity, string columns, string referredData = "", string detail = "")
        {
            var table = DerivedHelper.GetTable<ERROR_LOG>();

            var output = (ERROR_LOG)table.New();

            output.PLAN_VER = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
            output.PROJECT_ID = AleatorikInputMart.Instance.GlobalParameters.ProjectID;
            output.CATEGORY = module == ModuleType.None ? "Input" : "Engine";
            output.MODULE_ID = module == ModuleType.None ? "" : moduleKey;
            output.TARGET_KEY = entity != null ? entity.GetType().Name + "@" + columns : columns;

            string targetData = entity != null ? GetErrorTargetData(entity, columns.Split(',')) : columns;
            output.TARGET_DATA = targetData;
            output.REFERRED_KEY = referredData;
            output.SEVERITY = severity.ToString();

            output.REASON_CODE = reasonCode.ToString();
            output.REASON_DETAIL = detail;

            if (severity == ErrorSeverity.Critical)
            {
                ATPersistHelper.HasErrorData = true;
                ATPersistHelper.SetErrorStr(string.Format("Error : {0}\t\t{1}\t\t{2}\r\n", output.TARGET_KEY, output.TARGET_DATA, output.REASON_CODE));
            }

            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="target">TABLE@KEY1_COLUMN,KEY1_COLUMN,KEY1</param>
        /// <returns></returns>
        public static string GetErrorTargetData(IEntityObject entity, string[] columns)
        {
            Type type = entity.GetType();
            string targetData = string.Empty;

            foreach (var col in columns)
            {
                try
                {
                    var columnProperty = type.GetProperty(col);
                    if (columnProperty == null)
                    {
                        if (string.IsNullOrEmpty(targetData))
                            targetData = "ERR_ColName";
                        else
                            targetData += "@" + "ERR_ColName";

                        continue;
                    }

                    var columnValue = columnProperty.GetValue(entity);
                    var value = columnValue == null ? string.Empty : columnValue.ToString();

                    if (string.IsNullOrEmpty(value))
                        columnValue = "Null";

                    if (string.IsNullOrEmpty(targetData))
                        targetData = value;
                    else
                        targetData += "@" + columnValue;
                }
                catch
                {

                    if (string.IsNullOrEmpty(targetData))
                        targetData = "ERR_ColName";
                    else
                        targetData += "@" + "ERR_ColName";
                }
            }

            return targetData;
        }

        /// <summary>
        /// ReasonCode에 따라 테이블의 컬럼 정보를 조합하여 ErrorReason 정보 반환.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="reason"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public static string GetErrorReasonDetail(EntityObject entity, ErrorReasonCode reason, List<string> column)
        {
            //Type type = entity.GetType();
            //string reasonDetail = null;


            //switch (reason)
            //{
            //    case ErrorReasonCode.NotFoundReferredData:
            //    case ErrorReasonCode.Null:
            //        break;
            //    case ErrorReasonCode.MismatchReservedWord:
            //        string keyColumn = config.Parameter.Replace(" ", "").Split(',').FirstOrDefault();
            //        string keyValue = type.GetProperty(keyColumn).GetValue(entity).ToString();
            //        string key = config.RefTableName + "@" + keyColumn + "@" + keyValue;
            //        string reservedWord = null;

            //        if (ATPersistHelper.ReservedDictionary.TryGetValue(key, out string[] reservedWords))
            //            reservedWord = string.Join(", ", reservedWords);

            //        reasonDetail += string.Format("(Non Reserved Value : {0} = {1}), (Reserved Words : {2})", config.ColumnName, type.GetProperty(config.ColumnName).GetValue(entity), reservedWord);
            //        break;
            //    case ErrorReasonCode.DataTypeMisMatch:
            //        reasonDetail += string.Format("(Data Type : {0}), (Data Type Mismatch Value : {1})", type.GetProperty(config.RefColumnName).GetValue(entity), type.GetProperty(config.ColumnName).GetValue(entity));
            //        break;
            //    default:
            //        break;
            //}

            //return reasonDetail;
            return string.Empty;
        }

        //public PLAN_INDEX WritePlanIndex(string moduleKey, string category, string Index, string timeKey, string timeUnit, double value)
        //{
        //    bool isWriteOutput = IsWriteOutput("PLAN_INDEX");
        //    var table = DerivedHelper.GetTable<PLAN_INDEX>();
        //    var output = (PLAN_INDEX)table.New();

        //    output.PLAN_VERSION = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;
        //    output.MODULE_KEY = moduleKey;
        //    output.CATEGORY = category.ToString();
        //    output.INDEX = Index.ToString();
        //    output.TIME_KEY = timeKey;
        //    output.TIME_UNIT = timeUnit;
        //    output.VALUE = value;

        //    if (output != null && isWriteOutput)
        //        table.Add(output);

        //    return output;
        //}

        //public void WritePlanIndex(string moduleKey, APEKpiSummary kpiSummary)
        //{
        //    if (kpiSummary == null)
        //        return;

        //    foreach (var kpi in kpiSummary.KpiDic_Sum)
        //    {
        //        string category = kpi.Key.Split('@')[0];
        //        string index = kpi.Key.Split('@')[1];
        //        string timeKey = kpi.Key.Split('@')[2];

        //        WritePlanIndex(moduleKey, category, index, timeKey, TimeUnit.Month.ToString(), kpi.Value);
        //    }

        //    foreach (var kpi in kpiSummary.KpiDic_Avg)
        //    {
        //        string category = kpi.Key.Split('@')[0];
        //        string index = kpi.Key.Split('@')[1];
        //        string timeKey = kpi.Key.Split('@')[2];
        //        double count = double.Parse(kpi.Value.Split('@')[0]);
        //        double sum = double.Parse(kpi.Value.Split('@')[1]);
        //        double avg = count == 0 ? 0 : Math.Round(sum / count, 4);

        //        WritePlanIndex(moduleKey, category, index, timeKey, TimeUnit.Month.ToString(), avg);
        //    }
        //}
    }
}
