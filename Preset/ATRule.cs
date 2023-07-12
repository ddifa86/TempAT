using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class ATRule
    {
        /// <summary>
        /// Rule 호출 위치
        /// </summary>
        public RulePoint RulePoint { get; private set; }

        /// <summary>
        /// Rule 이름
        /// </summary>
        public string RuleName { get; private set; }

        /// <summary>
        /// Operation, Init, Level 등 Rule이 호출되는 단위
        /// </summary>
        public CallType CallType { get; private set; }

        public RulePointType RulePointType { get; private set; }

        public bool IsActive { get; private set; }

        public string Description { get; private set; }

        public string RulePointID
        {
            get
            {
                return this.RulePoint.ToString();
            }
        }

        public ATRule(RulePoint rulepoint, string rulename, CallType calltype, RulePointType rulePointType, bool isactive, string description)
        {
            this.RulePoint = rulepoint;
            this.RuleName = rulename;
            this.CallType = calltype;
            this.RulePointType = rulePointType;
            this.IsActive = isactive;
            this.Description = description;
        }
    }
}
