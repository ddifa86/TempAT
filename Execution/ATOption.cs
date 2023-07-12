 
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class ATOption
    {
        public static ATOption Instance
        {
            get
            {
                return ServiceLocator.Resolve<ATOption>();
            }
        }

        public DateTime PlanStartTime { get; set; }

        public DateTime PlanEndTime { get; set; }

        public int PlanPeriod { get; set; }

        public double MinimumAllocationQuantity = 0.00000001; //10의 마이너스 8승

        public int MinimumAllocationValue = 8;

        /// <summary>
        /// Bucket Rolling 시간
        /// </summary>
        public float BucketCycleTimeMinutes = 1440;

        public bool ApplyThreadedLineAllocation { get; set; }

        #region Global

        /// <summary>
        /// 
        /// </summary>
        public string DemandItems = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public DateTime DemandDueDate = DateTime.MaxValue;

        /// <summary>
        /// 
        /// </summary>
        public bool DiscardWip = false;

        public bool WritePerformanceLog = true;

        public bool DiscardCalendarTat = false;

        public bool DiscardCalendarYield = false;

        public UomType TimeUOM = UomType.Second;

        #endregion

        #region Module 별
        public HashSet<string> OutputTablesNotToLog = new HashSet<string>();

        public bool ApplyExcessLogging = false;


        //Operation //Buffer
        public bool ApplyTargetPlanDetail = false;

        public bool ApplyInfiniteMaterial = false;

        /// <summary>
        /// 
        /// </summary>
        public string DefaultRuleSet = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public int RetryCount = 0;

        // 장비에 무한 캐파 설정.
        public bool ApplyOverCapacity = false;

        /// <summary>
        /// NoCarryover 로직 적용 옵션
        /// </summary>
        public bool DiscardCarryOver = false;

        /// <summary>
        /// 
        /// </summary>
        public bool BlockProductSupply = false;

        /// <summary>
        /// 
        /// </summary>
        public bool ApplyNoArrangeToDummy = false;

        public int FrozenHorizon = 0;

        public string TargetReferencePlanType;
              
        public bool DiscardMultipleBatchSize = false;

        public bool TransferShortToNextPhase = false;

        public bool ApplyReSortLotInGroup = false;

        public bool ApplyReSortWip = false;

        public bool ApplyReSortTargetGroup = false;

        public string SetupPolicy = "Extend";

        public bool ApplyWipReleaseOnAvailableTime = false;

        public bool ApplyFirstTarget = false;

        #endregion

        public ATOption()
        {
            var args = AleatorikInputMart.Instance.GlobalParameters;
            this.PlanStartTime = args.start_time;

            this.PlanStartTime = this.PlanStartTime.AddSeconds(-this.PlanStartTime.Second);
            this.PlanStartTime = this.PlanStartTime.AddMilliseconds(-this.PlanStartTime.Millisecond);

            this.PlanPeriod = args.period;
            this.PlanEndTime = this.PlanStartTime.AddDays(this.PlanPeriod);
        }

        public bool SetGlobalOption(string name, string value)
        {
            if (name == ATReservedCode.DEMANDITEMS)
            {
                this.DemandItems = value;
                return true;
            }
            else if (name == ATReservedCode.DEMANDDUEDATE)
            {
                if (string.IsNullOrEmpty(value) == false)
                {
                    this.DemandDueDate = Convert.ToDateTime(value);// info.Get<DateTime>("DemandDueDate", DateTime.MaxValue);
                    return true;
                }
            }
            else if (name == ATReservedCode.MINIMUM_ALLOCATION_QUANTITY)
            {
                this.MinimumAllocationQuantity = Convert.ToDouble(value);
                this.MinimumAllocationValue = Convert.ToInt32(Math.Abs(Math.Floor(Math.Log10(MinimumAllocationQuantity))));
                return true;
            }
            else if (name == ATReservedCode.WRITE_PERFORMANCE_LOG)
            {
                this.WritePerformanceLog = ATUtil.BoolYN(value, true);
                return true;
            }
            else if (name == ATReservedCode.DISCARD_WIP)
            {
                this.DiscardWip = ATUtil.BoolYN(value, false);
                return true;
            }
            else if (name == ATReservedCode.OUTPUT_TABLES_NOT_TO_LOG)
            {
                this.OutputTablesNotToLog = new HashSet<string>(value.Replace(" ", "").Split(','));
                return true;
            }
            else if (name == ATReservedCode.DISCARD_CALENDAR_TAT)
            {
                this.DiscardCalendarTat = ATUtil.BoolYN(value, false);
                return true;
            }
            else if (name == ATReservedCode.DISCARD_CALENDAR_YIELD)
            {
                this.DiscardCalendarYield = ATUtil.BoolYN(value, false);
                return true;
            }
            else if (name == ATReservedCode.TIME_UOM)
            {
                this.TimeUOM = ATUtil.StringToEnum<UomType>(value, UomType.Second);
                return true;
            }
         
            return false;
        }

        public void Update(ModuleExecutionOption info)
        {
            foreach (var option in info.Keys)
            {
                switch (option)
                {
                    case ATReservedCode.DEFAULT_RULE_SET:
                        this.DefaultRuleSet = info.Get<string>(ATReservedCode.DEFAULT_RULE_SET, string.Empty);

                        var ruleSet = ATRuleAgent.Instance.GetRuleSet(ATOption.Instance.DefaultRuleSet);
                        ATRuleAgent.Instance.SetCurrentRuleSet(ruleSet);
                        break;
                    case ATReservedCode.BUCKET_CYCLETIME_MINUTES:
                        this.BucketCycleTimeMinutes = float.Parse(info.Get<string>(ATReservedCode.BUCKET_CYCLETIME_MINUTES, "1440"));
                        break;
                    case ATReservedCode.APPLY_EXCESS_LOGGING:
                        this.ApplyExcessLogging = ATUtil.BoolYN(info.Get<string>(ATReservedCode.APPLY_EXCESS_LOGGING, "N"), true);
                        break;
                    case ATReservedCode.APPLY_TARGET_PLAN_DETAIL:
                        this.ApplyTargetPlanDetail = ATUtil.BoolYN(info.Get<string>(ATReservedCode.APPLY_TARGET_PLAN_DETAIL, "N"), true);
                        break;
                    case ATReservedCode.DISCARD_CARRY_OVER:
                        this.DiscardCarryOver = ATUtil.BoolYN(info.Get<string>(ATReservedCode.DISCARD_CARRY_OVER, "N"), true);
                        break;
                    case ATReservedCode.RETRY_COUNT:
                        this.RetryCount = Int32.Parse(info.Get<string>(ATReservedCode.RETRY_COUNT, "0"));
                        break;
                    case ATReservedCode.APPLY_OVER_CAPACITY:
                        this.ApplyOverCapacity = ATUtil.BoolYN(info.Get<string>(ATReservedCode.APPLY_OVER_CAPACITY, "N"), false);
                        break;
                    case ATReservedCode.BLOCK_PRODUCT_SUPPLY:
                        this.BlockProductSupply = ATUtil.BoolYN(info.Get<string>(ATReservedCode.BLOCK_PRODUCT_SUPPLY, "N"), false);
                        break;
                    case ATReservedCode.FROZEN_HORIZON:
                        this.FrozenHorizon = Int32.Parse(info.Get<string>(ATReservedCode.FROZEN_HORIZON, "0"));
                        break;
                    case ATReservedCode.APPLY_NO_ARRNAGE_TO_DUMMY:
                        this.ApplyNoArrangeToDummy = ATUtil.BoolYN(info.Get<string>(ATReservedCode.APPLY_NO_ARRNAGE_TO_DUMMY, "N"), true);
                        break;
                    case ATReservedCode.APPLY_INFINITE_MATERIAL:
                        this.ApplyInfiniteMaterial = ATUtil.BoolYN(info.Get<string>(ATReservedCode.APPLY_INFINITE_MATERIAL, "N"), false);
                        break;
                    case ATReservedCode.TARGET_REFERENCE_PLAN_TYPE:
                        this.TargetReferencePlanType = info.Get<string>(ATReservedCode.TARGET_REFERENCE_PLAN_TYPE, string.Empty);
                        break;
                    case ATReservedCode.TRANSFER_SHORT_TO_NEXT_PHASE:
                        this.TransferShortToNextPhase = ATUtil.BoolYN(info.Get<string>(ATReservedCode.TRANSFER_SHORT_TO_NEXT_PHASE, "N"), false);
                        break;
                    case ATReservedCode.DISCARD_MULTIPLE_BATCH_SIZE:
                        this.DiscardMultipleBatchSize = ATUtil.BoolYN(info.Get<string>(ATReservedCode.DISCARD_MULTIPLE_BATCH_SIZE, "N"), false);
                        break;
                    case ATReservedCode.APPLY_RESORT_LOT_IN_GROUP:
                        this.ApplyReSortLotInGroup = ATUtil.BoolYN(info.Get<string>(ATReservedCode.APPLY_RESORT_LOT_IN_GROUP, "N"), false);
                        break;
                    case ATReservedCode.APPLY_RESORT_WIP:
                        this.ApplyReSortWip = ATUtil.BoolYN(info.Get<string>(ATReservedCode.APPLY_RESORT_WIP, "N"), false);
                        break;
                    case ATReservedCode.APPLY_RESORT_TARGET_GROUPT:
                        this.ApplyReSortTargetGroup = ATUtil.BoolYN(info.Get<string>(ATReservedCode.APPLY_RESORT_TARGET_GROUPT, "N"), false);
                        break;
                    case ATReservedCode.SETUP_POLICY:
                        this.SetupPolicy = info.Get<string>(ATReservedCode.SETUP_POLICY, "Extend");
                        break;
                    case ATReservedCode.APPLY_WIP_RELEASE_ON_AVAILABLE_TIME:
                        this.ApplyWipReleaseOnAvailableTime = ATUtil.BoolYN(info.Get<string>(ATReservedCode.APPLY_WIP_RELEASE_ON_AVAILABLE_TIME, "N"), false);
                        break;
                    case ATReservedCode.APPLY_FIRST_TARGET:
                        this.ApplyFirstTarget = ATUtil.BoolYN(info.Get<string>(ATReservedCode.APPLY_FIRST_TARGET, "N"), false);
                        break;
                    default:
                        ATInputControl.Instance.SetCustomOption(info, option);
                        break;
                }
            }
        }

        public void SetDefaultOptionValue()
        {
            this.ApplyExcessLogging = false;
            this.ApplyTargetPlanDetail = false;
            this.ApplyInfiniteMaterial = false;
            this.DefaultRuleSet = string.Empty;
            this.RetryCount = 0;
            this.ApplyOverCapacity = false;
            this.DiscardCarryOver = false;
            this.BlockProductSupply = false;
            this.ApplyNoArrangeToDummy = false;
            this.FrozenHorizon = 0;
            this.TargetReferencePlanType = string.Empty;
            
            this.TransferShortToNextPhase = false;
            this.DiscardMultipleBatchSize = false;
            this.ApplyWipReleaseOnAvailableTime = false;
        }
    }
}
