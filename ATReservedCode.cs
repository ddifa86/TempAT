using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    
    public struct ATReservedCode
    {

        public const string RESERVED_PREFIX_CODE = "#";

        #region Option 명

        public const string DEMANDITEMS = "DemandItems";

        public const string DEMANDDUEDATE = "DemandDueDate";

        public const string DEFAULT_RULE_SET = "DefaultRuleSet";

        public const string BUCKET_CYCLETIME_MINUTES = "BucketCycleTimeMinutes";

        public const string APPLY_EXCESS_LOGGING = "ApplyExcessLogging";

        public const string APPLY_TARGET_PLAN_DETAIL = "ApplyTargetPlanDetail";

        public const string DISCARD_WIP = "DiscardWip";

        public const string MINIMUM_ALLOCATION_QUANTITY = "MinimumAllocationQuantity";

        public const string DISCARD_CARRY_OVER = "DiscardCarryOver";

        public const string RETRY_COUNT = "RetryCount";

        public const string WRITE_PERFORMANCE_LOG = "WritePerformanceLog";

        public const string APPLY_OVER_CAPACITY = "ApplyOverCapacity";

        public const string BLOCK_PRODUCT_SUPPLY = "BlockProductSupply";

        public const string FROZEN_HORIZON = "FrozenHorizon";

        public const string APPLY_NO_ARRNAGE_TO_DUMMY = "ApplyNoArrangeToDummy";

        public const string APPLY_INFINITE_MATERIAL = "ApplyInfiniteMaterial";

        public const string TARGET_REFERENCE_PLAN_TYPE = "TargetReferencePlanType";

        public const string TRANSFER_SHORT_TO_NEXT_PHASE = "TransferShortToNextPhase";

        public const string DISCARD_MULTIPLE_BATCH_SIZE = "DiscardMultipleBatchSize";

        public const string APPLY_RESORT_LOT_IN_GROUP = "ApplyReSortLotInGroup ";

        public const string APPLY_RESORT_WIP = "ApplyReSortWip";

        public const string APPLY_RESORT_TARGET_GROUPT = "ApplyReSortTargetGroup";

        public const string OUTPUT_TABLES_NOT_TO_LOG = "OutputTablesNotToLog";

        public const string SETUP_POLICY = "SetupPolicy";

        public const string DISCARD_CALENDAR_TAT = "DiscardCalendarTat";

        public const string DISCARD_CALENDAR_YIELD = "DiscardCalendarYield";

        public const string APPLY_WIP_RELEASE_ON_AVAILABLE_TIME = "ApplyWipReleaseOnAvailableTime";

        public const string TIME_UOM = "TimeUOM";

        public const string APPLY_FIRST_TARGET = "ApplyFirstTarget";
        #endregion

        #region Calendar
        public const string YIELD = "#Yield";
        public const string TAT = "#Tat";

        public const string RUNTAT = "#RunTat";
        public const string WAITTAT = "#WaitTat";
        public const string WORK_TIME = "#WorkTime";
        public const string HOLIDAY = "#Holiday";
        public const string BREAK_TIME = "#BreakTime";
        public const string OFF_TIME = "#OffTime";
        public const string CAPACITY = "#Capacity";

        public const string SETUP_ID = "#SetupID";
        public const string MIN_ON_HAND = "#MinOnHand";

        public const string FLOW_TIME = "#FlowTime";
        public const string USAGE_PER = "#UsagePer";
        public const string MULTIPLE_BATCH_SIZE = "#MultipleBatchSize";
        public const string CONSTRAINT = "#Constraint";
        #endregion

        #region Resource Property
        public const string PM = "#PM";
        public const string UTILIZATION_RATE = "#UtilizationRate";
        #endregion

        #region ViewKey

        public const string OPERTARGET_KEY = "#OPERTARGET_KEY";

        #endregion

        #region Setup Reserved Word
        public const string ITEM_ID = "#ITEM_ID";
        public const string BOM_ID = "#BOM_ID";
        public const string ROUTING_ID = "#ROUTING_ID";
        public const string OPER_ID = "#OPER_ID";
        #endregion

        public const string ITEM = "Item";

        public const string WIP = "Wip";
    }
}
