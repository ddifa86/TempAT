using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public static class ErrDesc
    {
        public static string NotFoundKeyInput(string targetTable)
        {
            return targetTable + " table is mandatory.";
        }

        public static string NotFoundReferredData(string targetColumn, string referredTable)
        {
            return string.Format("{0} must be defined in {1}", targetColumn, referredTable);
        }

        public static string MismatchReservedWord(string targetColumn, string reservedWord)
        {
            return string.Format("{0} must be allowed only for reserved words ({1})", targetColumn, reservedWord);
        }

        public static string Null(string targetColumn)
        {
            return string.Format("{0} must be not Null", targetColumn);
        }

        public static string DataTypeMisMatch(string targetColumn, string dataType)
        {
            return string.Format("VALUE of {0} does not satisfied for data type {1}", targetColumn, dataType);
        }

        public static string OutOfRange_above(string targetColumn)
        {
            return string.Format("{0} must be greater than or equal to 0.", targetColumn);
        }

        public static string OutOfRange_greater(string targetColumn)
        {
            return string.Format("{0} must be greater than 0", targetColumn);
        }

        public static string OutOfRange_datetime(string startColumn, string endColumn)
        {
            return string.Format("{0} must be less than {1}", startColumn, endColumn);
        }

        public static string OutOfRange_between(string targetColumn, string start, string end)
        {
            return string.Format("{0} must be between {1} and {2}", targetColumn, start, end);
        }

        public static string ChainError()
        {
            return "This error is a chain error caused by another error";
        }


    }

    public static class ErrKey
    {
        // 1. EXECUTION
        public static string AllocationGroup = "ALLOCATION_GROUP_ID";
        public static string StageMaster = "STAGE_ID";
        public static string ScenarioOptionConfig = "SCENARIO_ID,MODULE_ID,OPTION_ID,PHASE_NO";
        public static string ScenarioConfig = "SCENARIO_ID,MODULE_ID";
        public static string FactoryConfig = "FACTORY_START_HOUR,START_DAY_OF_WEEK";
        public static string PlanConfig = "PLAN_VER";
        public static string ExecutionResult = "SCENARIO_ID,TABLE_NAME,MODULE_KEY,PHASE";

        // 2. INT
        public static string PropMaster = "PROP_ID";

        // 3. DEMAND
        public static string Demand = "DEMAND_ID";
        public static string DemandPropValue = "SO_ID,PROPERTY_ID";
        public static string CustPropValue = "CUST_ID,PROP_ID";
        public static string RefProdPlan = "REF_PLAN_VER,SITE_ID,ITEM_ID,BUFFER_ID";

        // 4. ITEM_SITE_BUFFER
        public static string BufferMaster = "BUFFER_ID";
        public static string BufferPropValue = "BUFFER_ID,PROP_ID";
        public static string ItemMaster = "ITEM_ID";
        public static string ItemPropValue = "ITEM_ID,PROP_ID";
        public static string ItemSiteBufferMaster = "ITEM_ID,SITE_ID,BUFFER_ID";
        public static string ItemSiteBufferPropValue = "ITEM_ID,SITE_ID,BUFFER_ID,PROP_ID";
        public static string SiteMaster = "SITE_ID";
        public static string SitePropValue = "SITE_ID,PROP_ID";//

        // 5. BOM
        public static string BomDetail = "BOM_ID,FROM_ITEM_ID,FROM_SITE_ID,FROM_BUFFER_ID,TO_ITEM_ID,TO_SITE_ID,TO_BUFFER_ID";
        public static string BomDetailAlt = "BOM_ID,ITEM_ID,SITE_ID,BUFFER_ID,ALT_ITEM_ID,ALT_SITE_ID,ALT_BUFFER_ID";
        public static string BomMaster = "BOM_ID";
        public static string BomPropValue = "BOM_ID,PROP_ID";
        public static string BomRouting = "ROUTING_ID,BOM_ID";
        public static string BomRoutingPropValue = "BOM_ID,ROUTING_ID,PROP_ID";
        public static string RoutingMaster = "ROUTING_ID";
        public static string RoutingOper = "ROUTING_ID,OPER_ID";
        public static string RoutingOperPropValue = "ROUTING_ID,OPER_ID,PROP_ID";

        // 6. RESOURCE
        public static string Constraint = "CONSTRAINT_ID";
        public static string OperAddRes = "ROUTING_ID,OPER_ID,RES_ID,ADD_RES_ID";
        public static string OperAddPropValue = "ROUTING_ID,OPER_ID,RES_ID,ADD_RES_ID,PROP_ID";
        public static string OperRes = "ROUTING_ID,OPER_ID,RES_ID";
        public static string OperResPropValue = "ROUTING_ID,OPER_ID,RES_ID,PROP_ID";
        public static string ResGroupMaster = "RES_GROUP_ID";
        public static string ResMaster = "RES_ID";
        public static string ResPropValue = "RES_ID,PROP_ID";
        public static string Setup = "SETUP_ID,SETUP_CONDITION,FROM_CONDITION_VALUE,TO_CONDITION_VALUE";

        // 7. WIP
        public static string Wip = "WIP_ID";
        public static string WipPropValue = "WIP_ID,PROP_ID";

        // 8. RULE
        public static string ScenarioRulesetConfig = "SCENARIO_ID,MODULE_ID,PHASE_NO,TARGET_CATEGORY,TARGET_ID";
        public static string FactorMaster = "FACTOR_ID,RULE_POINT";
        public static string RuleFactor = "RULE_ID,FACTOR_ID";
        public static string RuleMaster = "RULE_ID,RULE_POINT";
        public static string RuleSetConfig= "RULESET_ID,RULE_POINT,LEVEL_NO,RULE_ID";
        public static string RulePointMaster = "RULE_POINT";
        public static string RuleSetMaster = "RULESET_ID";

        // 9. CALENDAR
        public static string CalendarBasedAttr = "CALENDAR_ID,PATTERN_ID,ATTR_TYPE";
        public static string CalendarDetail = "CALENDAR_ID,PATTERN_ID";
        public static string CalendarMaster = "CALENDAR_ID";
    }
}
