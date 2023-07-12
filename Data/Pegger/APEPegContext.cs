using Mozart.SeePlan.Pegging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class APEPegContext : BaseContext, ICloneable
    {
        /// <summary>
        /// Pegging 대상 Operation
        /// </summary>
        public ATOperation Oper { get; private set; }

        /// <summary>
        /// Pegging 단계 정보
        /// </summary>
        public int Level { get; private set; }
        
        /// <summary>
        /// 현 Phase Pegging 시 Wip Sort Preset 정보
        /// </summary>
        public ATWeightPreset CompareWipPreset { get; private set; }

        /// <summary>
        /// 현 Phase Pegging 시 TaretSortPreset 정보
        /// </summary>
        public ATWeightPreset CompareTargetInGroupPreset { get; private set; }

        /// <summary>
        /// 현 Phas Pegging 시 TargetGroup Sort Preset 정보
        /// </summary>
        public ATWeightPreset CompareTargetGroupPreset { get; private set; }       

        /// <summary>
        /// Pegging 시 FilterGroup Preset 정보
        /// </summary>
        public ATWeightPreset FilterTargetGroupPreset { get; private set; }

        /// <summary>
        /// Pegging 시 Wip / Target 이 확정된 시점에 필터 가능 여부 확인
        /// </summary>
        public ATWeightPreset FilterWipPreset { get; private set; }

        public ATWeightPreset PeggingKeyPreset { get; private set; }

        /// <summary>
        /// 현 Phase의 Pegging Type : Target First Select, Wip First Select
        /// </summary>
        public PegProcedureType PegType { get; set; }

        public bool IsRun { get; set; }
                
        /// <summary>
        /// Pegging 대상이 되는 PegPart 혹은 TargetGroup
        /// </summary>
        public ITargetGroup CurPegPart { get; set; }
        
        /// <summary>
        /// Pegging Wip Kit ID 정보
        /// </summary>
        public string PeggingKey { get; set; }

        /// <summary>
        /// Pegging Group Key 정보
        /// </summary>
        public string PeggingGroupKey { get; set; }

        /// <summary>
        /// Target Group Key 정보
        /// </summary>
        public string TargetGroupKey { get; set; }

        /// <summary>
        /// Peg Wip 호출 횟수
        /// </summary>
        public int PegSequence { get; set; }

        /// <summary>
        /// Pegging 된 수량 정보
        /// </summary>
        public double PegQty { get; set; }

        public List<APEWip> PlanWips { get; set; }

        public List<string> PeggingKeys { get; set; }

        public APEPegContext(int level, ATOperation oper, PegProcedureType pegType, ATRuleSet ruleSet, bool isRun, string peggingGroupKey)
        {
            this.Oper = oper;
            this.Level = level;
            this.PegType = pegType;
            this.IsRun = isRun;
            this.PegSequence = 0;
            this.PegQty = 0;

            this.PeggingGroupKey = peggingGroupKey;
            this.PeggingKeys = new List<string>();
            this.PlanWips = new List<APEWip>();

            this.CompareWipPreset = ruleSet.GetRule(RulePoint.CompareWip, CallType.Level, level);
            this.CompareTargetInGroupPreset = ruleSet.GetRule(RulePoint.CompareTargetInGroup, CallType.Level, level);
            this.CompareTargetGroupPreset = ruleSet.GetRule(RulePoint.CompareTargetGroup, CallType.Level, level);
            this.FilterTargetGroupPreset = ruleSet.GetRule(RulePoint.FilterTargetGroup, CallType.Level, level);
            this.FilterWipPreset = ruleSet.GetRule(RulePoint.FilterWip, CallType.Level, level);
            this.PeggingKeyPreset = ruleSet.GetRule(RulePoint.PeggingKey, CallType.Level, level);
        }

        public APEPegContext(ATOperation oper, bool isRun, ATRuleSet ruleSet, int level)
        {
            this.Oper = oper;
            this.Level = 1;

            this.IsRun = isRun;
            this.PegSequence = 0;
            this.PegQty = 0;
            
            this.CompareWipPreset = ruleSet.GetRule(RulePoint.CompareWip, CallType.Level, level);
            this.PeggingKeyPreset = ruleSet.GetRule(RulePoint.PeggingKey, CallType.Level, level);
            this.FilterWipPreset = ruleSet.GetRule(RulePoint.FilterWip, CallType.Level, level);

            this.PeggingKeys = new List<string>();
        }

        public virtual APEPegContext DeepCopy()
        {
            APEPegContext clone = (APEPegContext)this.MemberwiseClone();

            return clone;
        }

        public virtual object Clone()
        {
            return this.DeepCopy();
        }
    }
}
