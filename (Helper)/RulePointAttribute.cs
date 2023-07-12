using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    [AttributeUsage(AttributeTargets.Delegate)]
    public class RulePointAttribute : Attribute
    {
        public string Name { get; set; }

        public RulePointAttribute(string name)
        {
            this.Name = name;
        }
    }
}
