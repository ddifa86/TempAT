using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class ATConstraintInfo
    {
        public string ConstraintID { get; set; }

        public string Condition { get; set; }

        public ATProperty Property { get; set; }

        public string PropertyID { get; set; }

        public ATCalendar Calendar { get; set; }

        public ConstraintPolicy Policy { get; set; }

        public ATConstraintInfo(string constraintID, ATProperty property, string condition, ATCalendar calendar, ConstraintPolicy policy)
        {
            this.ConstraintID = constraintID;
            this.Property = property;
            this.PropertyID = property.PropertyID;
            this.Condition = condition;
            this.Calendar = calendar;
            this.Policy = policy;
        }
    }
}
