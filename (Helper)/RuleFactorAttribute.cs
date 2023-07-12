using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RuleFactorAttribute : Attribute
    {
        public Type RulePoint { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class FEActionAttribute : Attribute
    {
        public string RuleID { get; set; }
        public Type RulePoint { get; set; }
    }
}
