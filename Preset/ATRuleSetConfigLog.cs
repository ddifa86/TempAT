using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class ATRuleSetConfigLog 
    {
        public string VersionNo { get; private set; }

        public string ModuleKey { get; private set; }

        public string TargetType { get; private set; }

        public string TargetID { get; private set; }

        public string RulesetID { get; private set; }

        public int Phase { get; private set; }

        public ATRuleSetConfigLog(string versionNo, string modulekey, string targettype, string targetid, string rulesetid, int phase)
        {
            this.VersionNo = versionNo;
            this.ModuleKey = modulekey;
            this.TargetType = targettype;
            this.TargetID = targetid;
            this.RulesetID = rulesetid;
            this.Phase = phase;
        }
    }
}
