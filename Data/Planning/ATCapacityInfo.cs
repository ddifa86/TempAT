using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{

    public class ATCapacityInfo 
    {

        #region value
        public double Capacity { get;  internal set; }

        public ATResource Resource { get; internal set; }

        public ATCalendarAttribute CalAttr { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        #endregion

        #region property
        public bool IsNonWorkingCapa
        {
            get { return this.CalAttr == null; }
        }
        #endregion

        public ATCapacityInfo(ATResource resource, double capacity, DateTime startTime, DateTime endTime, ATCalendarAttribute attr = null)
        {
            this.Capacity = capacity;
            this.StartTime = startTime;
            this.EndTime = endTime;

            this.CalAttr = attr;

            this.Resource = resource;
        }

        #region method
        public ATCapacityInfo ShallowCopy(double qty)
        {
            var copy = (ATCapacityInfo)this.MemberwiseClone();

            copy.Capacity = qty;

            return copy;
        }
        #endregion

    }

}
