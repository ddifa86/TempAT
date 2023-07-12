using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Planner;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    /// <summary>
    /// RuleAgent로 변경 필요
    /// </summary>
    public class ATRuleAgent : IAgent
    {
        public static ATRuleAgent Instance
        {
            get { return ServiceLocator.Resolve<ATRuleAgent>(); }
        }

        private static Dictionary<string, ATRuleSet> _ruleSets = new Dictionary<string, ATRuleSet>();
        private static Dictionary<string, ATScenarioRuleSetConfig> _ruleSetConfigs = new Dictionary<string, ATScenarioRuleSetConfig>();
        private static Dictionary<string, ATFactor> _factors = new Dictionary<string, ATFactor>();
        private static Dictionary<string, ATWeightPreset> _presets = new Dictionary<string, ATWeightPreset>();
        private static Dictionary<string, ATRuleSetConfigLog> _ruleSetConfigLogs = new Dictionary<string, ATRuleSetConfigLog>();
        private static Dictionary<string, ATRule> _rules = new Dictionary<string, ATRule>();

        private ATRuleSet _currentDefaultRuleSet;

        public ATRuleSet CurrentRuleSet
        {
            get
            {
                return _currentDefaultRuleSet;
            }
        }
 

        public ATRuleAgent()
        {
          
        }
        

        public void Initialize()
        {
            var executeInfo = ATExecutionContext.Instance.CurrentExecutionInfo;
            var option = executeInfo.GetOption(1);

            string defaultRuleSet = option.Get<string>(ATReservedCode.DEFAULT_RULE_SET, string.Empty);

            _currentDefaultRuleSet = ATRuleAgent.Instance.GetRuleSet(defaultRuleSet);

            if (_currentDefaultRuleSet == null)
            {
                //OutputWriter.Instance.WriteInputErrorLog("RULE_MASTER", "EXECUTION_OPTION_CONFIG", ErrorSeverity.Error, ErrorType.ConfigError.ToString(),
                //    "The “DefaultRuleSet” option of the module has invalid value (EXECUTION_OPTION_CONFIG). Check if the option value exists in the RULESET_ID column of the RULESET_MASTER table.");
                    
                throw new Exception(ATPersistHelper.ErrorStr.ToString());
            }
            
        }

        public void Dispose()
        {
            _currentDefaultRuleSet = null;
        }

        internal void SetCurrentRuleSet(ATRuleSet ruleSet)
        {
            _currentDefaultRuleSet = ruleSet;
        }


        public ATRuleSet GetRuleSet(ATOperation oper)
        {
            string targetID = oper.GetRuleSetID();
            RuleSetType targetType = oper.IsBuffer ? RuleSetType.Buffer : RuleSetType.Operation;

            return GetRuleSet(targetID, targetType);
        }

        public ATRuleSet GetRuleSet(string targetID, RuleSetType targetType)
        {
            string moduleKey = ATExecutionContext.Instance.CurrentExecutionInfo.Key;
            int phase = ATExecutionContext.Instance.CurrentExecutionInfo.CurPhase;

            string key = moduleKey + targetType + targetID + phase;

            var ruleSet = ATRuleAgent.Instance.GetScenarioRuleSetConfig(key);

            if (ruleSet == null)
            {
                var defaultRuleSet = ATRuleAgent.Instance.CurrentRuleSet;

                // if (targetType != RuleSetType.Operation.ToString())
                OutputWriter.Instance.WriteExecutionRulSetConfigLog(moduleKey, targetType.ToString(), targetID, defaultRuleSet.RulesetID, phase);

                return defaultRuleSet;
            }
            else
            {
                OutputWriter.Instance.WriteExecutionRulSetConfigLog(moduleKey, targetType.ToString(), targetID, ruleSet.RuleSetID, phase);
                return ruleSet.RuleSet;
            }
        }

        public ATRuleSetConfigLog GetRuleSetConfigLogs(string key)
        {
            if (_ruleSetConfigLogs.TryGetValue(key, out var value))
                return value;
            else
                return null;
        }

        public bool AddRuleSetConfigLog(ATRuleSetConfigLog obj)
        {
            string key = obj.ModuleKey + obj.TargetType + obj.TargetID + obj.RulesetID;
            if (_ruleSetConfigLogs.ContainsKey(key))
                return false;

            _ruleSetConfigLogs.Add(key, obj);
            return true;
        }

        public ATRule GetRule(string key)
        {
            if (_rules.TryGetValue(key, out var value))
                return value;
            else
                return null;
        }

        public bool AddRule(ATRule obj)
        {
            if (_rules.ContainsKey(obj.RulePointID))
                return false;

            _rules.Add(obj.RulePointID, obj);
            return true;
        }


        public ATWeightPreset GetPreset(string key)
        {
            if (_presets.TryGetValue(key, out var value))
                return value;
            else
                return null;
        }

        public bool AddWeightPreset(ATWeightPreset obj)
        {
            if (_presets.ContainsKey(obj.RuleID))
                return false;

            _presets.Add(obj.RuleID, obj);

            return true;
        }

        public IEnumerable<ATFactor> GetFactor()
        {
            return _factors.Values;
        }

        public ATFactor GetFactor(string key)
        {
            if (_factors.TryGetValue(key, out var value))
                return value;
            else
                return null;
        }

        public  bool AddFactor(ATFactor obj)
        {
            string key = obj.RulePointID + obj.FactorID;
            if (_factors.ContainsKey(key))
                return false;

            _factors.Add(key, obj);
            return true;
        }

        public ATScenarioRuleSetConfig GetScenarioRuleSetConfig(string key)
        {
            if (_ruleSetConfigs.TryGetValue(key, out var value))
                return value;
            else
                return null;
        }

        public  bool AddScenarioRuleSetConfig(ATScenarioRuleSetConfig obj)
        {
            string key = obj.ModuleID + obj.TargetType + obj.TargetID + obj.Phase;

            if (_ruleSetConfigs.ContainsKey(key))
                return false;

            _ruleSetConfigs.Add(key, obj);
            return true;
        }

        public  IEnumerable<ATRuleSet> GetRuleSet()
        {
            return _ruleSets.Values;
        }

        public  ATRuleSet GetRuleSet(string key)
        {
            if (string.IsNullOrEmpty(key))
            { 
            }
            if (_ruleSets.TryGetValue(key, out var value))
                return value;
            else
                return null;
        }

        internal void RemoveRuleSet(string key)
        {
            if (_ruleSets.ContainsKey(key))
                _ruleSets.Remove(key);
        }

        public  bool AddRuleSet(ATRuleSet obj)
        {
            if (_ruleSets.ContainsKey(obj.RulesetID))
                return false;

            _ruleSets.Add(obj.RulesetID, obj);
            return true;
        }

      
    }
}
