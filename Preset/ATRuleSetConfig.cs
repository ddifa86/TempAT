using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class ATRuleSetConfig
    {
        public ATRuleSet RuleSet { get; private set; }

        public int Level { get; private set; }

        public ATRule Rule { get; private set; }

        public object Target { get; private set; }

        public string RulePointID
        {
            get
            {
                return this.Rule.RulePointID;
            }
        }

        public ATRuleSetConfig(ATRuleSet ruleSet, int level, ATRule rule, object target)
        {
            this.RuleSet = ruleSet;
            this.Level = level;
            this.Rule = rule;
            this.Target = target;
        }
    }
}
