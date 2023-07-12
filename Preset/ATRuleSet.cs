using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class ATRuleSet
    {
        public ModuleType ModuleType { get; private set; }
        public string RulesetID { get; private set; }

        public int Level { get; private set; }

        public string Description { get; private set; }

        public HashedSet<RulePoint> _registRulePoints { get; private set; }
        

        private  Dictionary<string, ATRuleSetConfig> _ruleActionMap;

        public ATRuleSet(string id, int level, string desc, ModuleType moduletype)
        {
            this.ModuleType = moduletype;
            this.RulesetID = id;
            this.Level = level;
            this.Description = desc;
            this._registRulePoints = new HashedSet<RulePoint>();
            this._ruleActionMap = new Dictionary<string, ATRuleSetConfig>();
        }

        public void AddRuleSetConfig(string key, ATRuleSetConfig action)
        {
            //string key = action.Rule.CallType  == CallType.Level ? action.Rule.RuleName + action.Level : action.Rule.RuleName;
            if (_ruleActionMap.ContainsKey(key) == false)
            {
                _ruleActionMap.Add(key, action);
                _registRulePoints.Add(action.Rule.RulePoint);
            }
        }

        public ATWeightPreset GetRule(RulePoint rulepoint, CallType callType, int level = 0)
        {
            ATRuleSetConfig ruleMap;

            string key = callType == CallType.Level ? rulepoint.ToString() + level : rulepoint.ToString();

            if (_ruleActionMap.TryGetValue(key, out ruleMap) == false)
            {
                if (ruleMap == null)
                {
                    return null; 
                }
            }

            return ruleMap.Target as ATWeightPreset;
        }

        //public SortedSet<ATWeightFactor> GetFactors(RulePoint rulepoint, CallType callType, int level = 0)
        //{
        //    ATRuleSetConfig ruleMap;

        //    string key = callType == CallType.Level ? rulepoint.ToString() + level : rulepoint.ToString();

        //    if (_ruleActionMap.TryGetValue(key, out ruleMap) == false)
        //    {
        //        if (ruleMap == null)
        //        {
        //            if (callType == CallType.Level && level != 1)
        //            {
        //                return GetFactors(rulepoint, callType, 1);
        //            }

        //            return null;
        //        }
        //    }

        //    var preset = ruleMap.Target as ATWeightPreset;
        //    if (preset != null && preset.FactorList.Count != 0)
        //        return preset.FactorList;

        //    return null;
        //}

        public string GetInvalidLog()
        {
            List<string> nonMappingRule = new List<string>();

            var checkInfo = string.Empty;
            bool isValid;

            if (this.ModuleType == ModuleType.PBB)
            {
                isValid = _registRulePoints.Contains(RulePoint.WipKey);
                if (isValid == false)
                    nonMappingRule.Add(RulePoint.WipKey.ToString());

                isValid = _registRulePoints.Contains(RulePoint.PeggingKey);
                if (isValid == false)
                    nonMappingRule.Add(RulePoint.PeggingKey.ToString());

                isValid = _registRulePoints.Contains(RulePoint.PeggingGroupKey); 
                if (isValid == false)
                    nonMappingRule.Add(RulePoint.PeggingGroupKey.ToString());

                isValid = _registRulePoints.Contains(RulePoint.TargetGroupKey);
                if (isValid == false)
                    nonMappingRule.Add(RulePoint.TargetGroupKey.ToString());
            }
            else if (this.ModuleType == ModuleType.PBF)
            {
                isValid = _registRulePoints.Contains(RulePoint.LotGroupKey);
                if (isValid == false)
                    nonMappingRule.Add(RulePoint.LotGroupKey.ToString());

            }
            else if (this.ModuleType == ModuleType.PBO)
            {
                isValid = _registRulePoints.Contains(RulePoint.WipKey);
                if (isValid == false)
                    nonMappingRule.Add(RulePoint.WipKey.ToString());

                isValid = _registRulePoints.Contains(RulePoint.PeggingKey);
                if (isValid == false)
                    nonMappingRule.Add(RulePoint.PeggingKey.ToString());
            }

            if (nonMappingRule.Count() > 0)
                checkInfo = "Missing require rulepoint : " + string.Join(", ", nonMappingRule);

            return checkInfo;
        }
    }
}
