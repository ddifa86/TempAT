using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class ATScenarioRuleSetConfig
    {
        public string ModuleID {get; private set;}

        public string TargetType { get; private set; }

        public string TargetID { get; private set; }

        public ATRuleSet RuleSet { get; private set; }

        public int Phase { get; set; }

        public string RuleSetID
        {
            get
            {
                return this.RuleSet.RulesetID;
            }
        }

        public ATScenarioRuleSetConfig(string modulekey, string targettype, string targetid, int phase, ATRuleSet ruleset)
        {
            this.ModuleID = modulekey;
            this.TargetType = targettype;
            this.TargetID = targetid;
            this.RuleSet = ruleset;
            this.Phase = phase;
        }
    }
}
