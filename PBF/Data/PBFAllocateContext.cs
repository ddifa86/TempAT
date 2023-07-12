using Mozart.Simulation.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public partial class PBFAllocateContext : BaseContext
    {
        public FWAllocator CurrentAllocator { get; internal set; }
        /// <summary>
        /// Pegging 시 사용될 RuleSet 정보
        /// </summary>
        /// 
        public ATRuleSet RuleSet => ATRuleAgent.Instance.CurrentRuleSet;

        /// <summary>
        /// 할당대상 그룹 정보
        /// </summary>
        public FWAllocGroup SelectedAllocGroup { get; private set; }

        /// <summary>
        /// 현 Cycle Start Time
        /// </summary>
        public DateTime NowDt => CurrentAllocator.NowDt;

        /// <summary>
        /// Next Cycle Start Time
        /// </summary>
        public DateTime NextDt => CurrentAllocator.NextDt;

        /// <summary>
        /// 현재 할당 Phase 정보
        /// </summary>
        public int Level { get; private set; }
                 
        public AllocateType AllocateType { get; set; }

        public PBFAllocationLog AllocationLog { get; set; }
   
        #region 할당 과정에서 필요한 정보 => 필요한가..?

        public FWSetupInfo SetupInfo { get; set; }

        public double FlowTime { get; set; }

        public double BatchTime { get; set; }

        public double UsagePer { get; set; }

        public double UtilizationRate { get; set; }

        public double LotAvailAllocQty { get; set; }

        public double AllocableQty { get; internal set; }

        public bool IsSplit { get; set; }

        public Dictionary<IBucket, double> AddUtilizationRate { get; set; }

        public APEPlanInfo PlanInfo { get; set; }

        public List<ATOperResource> AddArrInfos { get; set; }

        public List<IBucket> AddBuckets { get; set; }

        public APELotGroup SelectedLotGroup { get; internal set; }
        
        public Dictionary<APELotGroup, FWSetupInfo> SetupInfoDict { get; set; }

        public double MaxSetupHrs { get; set; }

        public double ConstraintQty { get; internal set; }

        /// <summary>
        /// 할당 후 Lot에 잔여량이 있는 경우 수량 제약에 의한 잔여 여부
        /// MBS(GetAllocableQty) or Constraint에 의한 잔여
        /// </summary>
        public bool IsQuantityConstraint
        {
            get
            {
                // IsSplit을 제외한 조건 삭제 고려
                return this.AllocableQty <= ATOption.Instance.MinimumAllocationQuantity || this.ConstraintQty >= ATOption.Instance.MinimumAllocationQuantity || this.IsSplit;
            }
        }
        #endregion

        public PBFAllocateContext(FWAllocator allocator, FWAllocGroup allocGroup, AllocateType allocType, int level)
        {
            this.CurrentAllocator = allocator;
            this.SelectedAllocGroup = allocGroup;
            this.AllocateType = allocType;
            this.Level = level;

            this.AddArrInfos = new List<ATOperResource>();
            this.AddBuckets = new List<IBucket>();
            this.AddUtilizationRate = new Dictionary<IBucket, double>();
            this.SetupInfoDict = new Dictionary<APELotGroup, FWSetupInfo>();
        }

        internal void SetDefaultContextPreset()
        {
            if (this.AllocateType == AllocateType.ResourceFirstSelection)
            {
                this.CompareLotGroupOnRFSPreset = this.RuleSet.GetRule(RulePoint.CompareLotGroupOnRFS, CallType.Level, this.Level);
                this.CompareResourceOnRFSPreset = this.RuleSet.GetRule(RulePoint.CompareResourceOnRFS, CallType.Level, this.Level);
                this.FilterResourceOnRFSPreset = this.RuleSet.GetRule(RulePoint.FilterResourceOnRFS, CallType.Level, this.Level);
                this.FilterLotGroupOnRFSPreset = this.RuleSet.GetRule(RulePoint.FilterLotGroupOnRFS, CallType.Level, this.Level);
            }
            else
            {
                this.CompareLotGroupOnLFSPreset = this.RuleSet.GetRule(RulePoint.CompareLotGroupOnLFS, CallType.Level, this.Level);
                this.CompareResourceOnLFSPreset = this.RuleSet.GetRule(RulePoint.CompareResourceOnLFS, CallType.Level, this.Level);
                this.CompareAddResourcePreset = this.RuleSet.GetRule(RulePoint.CompareAddResource, CallType.Level, this.Level);
                this.FilterLotGroupOnLFSPreset = this.RuleSet.GetRule(RulePoint.FilterLotGroupOnLFS, CallType.Level, this.Level);
                this.FilterLotGroupInLevelPreset = this.RuleSet.GetRule(RulePoint.FilterLotGroupInLevel, CallType.Level, this.Level);
                this.FilterResourceOnLFSPreset = this.RuleSet.GetRule(RulePoint.FilterResourceOnLFS, CallType.Level, this.Level);
            }
        }

        internal void UpdateContextPreset(PBFResourceGroup bucketGroup, AllocateType allocType)
        {
            ATRuleSet ruleSet = ATRuleAgent.Instance.GetRuleSet(bucketGroup.BucketGroupID, RuleSetType.ResourceGroup);

            if (allocType == AllocateType.ResourceFirstSelection)
            {
                this.CompareLotGroupOnRFSPreset = ruleSet.GetRule(RulePoint.CompareLotGroupOnRFS, CallType.Level, this.Level);
                this.CompareResourceOnRFSPreset = ruleSet.GetRule(RulePoint.CompareResourceOnRFS, CallType.Level, this.Level);
                this.FilterLotGroupOnRFSPreset = ruleSet.GetRule(RulePoint.FilterLotGroupOnRFS, CallType.Level, this.Level);
                this.FilterResourceOnRFSPreset = ruleSet.GetRule(RulePoint.FilterResourceOnRFS, CallType.Level, this.Level);
            }
            else
            {
                this.CompareLotGroupOnLFSPreset = ruleSet.GetRule(RulePoint.CompareLotGroupOnLFS, CallType.Level, this.Level);
                this.CompareResourceOnLFSPreset = ruleSet.GetRule(RulePoint.CompareResourceOnLFS, CallType.Level, this.Level);
                this.CompareAddResourcePreset = ruleSet.GetRule(RulePoint.CompareAddResource, CallType.Level, this.Level);
                this.FilterLotGroupOnLFSPreset = ruleSet.GetRule(RulePoint.FilterLotGroupOnLFS, CallType.Level, this.Level);
                this.FilterLotGroupInLevelPreset = ruleSet.GetRule(RulePoint.FilterLotGroupInLevel, CallType.Level, this.Level);
                this.FilterResourceOnLFSPreset = ruleSet.GetRule(RulePoint.FilterResourceOnLFS, CallType.Level, this.Level);
            }

            this.AddBuckets = new List<IBucket>();
            this.AddArrInfos = new List<ATOperResource>();
            this.AddUtilizationRate = new Dictionary<IBucket, double>();
            this.SetupInfoDict = new Dictionary<APELotGroup, FWSetupInfo>();
            this.MaxSetupHrs = 0;
        }
    }
}
