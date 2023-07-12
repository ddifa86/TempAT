using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class ATConstraint
    {
        public string ConstraintID { get; set; }

        public string Condition { get; set; }

        public ATProperty Property { get; set; }

        public string PropertyID { get; set; }

        public ATCalendar Calendar { get; set; }

        public double UsagePer { get; set; }

        public double UsedQty { get; internal set; }

        public double VirtualUsedQty { get; internal set; }

        public ConstraintPolicy Policy { get; internal set; }

        public Dictionary<string, ATConstraintDetail> ConstraintDetails { get; set; }

        public List<APEPlanInfo> Plans { get; private set; }

        public ATConstraint(ATConstraintInfo info)
        {
            this.ConstraintID = info.ConstraintID;
            this.Condition = info.Condition;
            this.Property = info.Property;
            this.PropertyID = info.PropertyID;
            this.Calendar = info.Calendar;
            this.Policy = info.Policy;
            this.UsagePer = 1;

            this.ConstraintDetails = new Dictionary<string, ATConstraintDetail>();
            this.Plans = new List<APEPlanInfo>();

            double cumQty = 0;
            foreach (var attribute in info.Calendar.GetAttributes(ATReservedCode.CONSTRAINT))
            {
                ATConstraintDetail detail = new ATConstraintDetail(this, attribute);

                if (this.Policy == ConstraintPolicy.Cumulative)
                {
                    detail.Qty += cumQty;
                    cumQty = detail.Qty;
                }

                var startTime = attribute.EffectiveStartTime;
                var endTime = attribute.EffectiveEndTime;

                while (startTime < endTime)
                {
                    string applyDate = startTime.ToString(ATUtil.DateFormat);
                    this.ConstraintDetails.Add(applyDate, detail);

                    startTime = startTime.AddDays(1);
                }
            }
        }
    }
}
