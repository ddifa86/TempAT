using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.TimeLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public class ATPMPeriod : APENonWorkingPeriod
    {

        public ATPMInfo PmInfo { get; private set; }

        internal int PMPriority { get; set; }
        internal PmPolicy PMPolicy { get; set; }
        internal double PMParameter { get; set; }

        public DateTime ExecutedStartTime { get; set; } // 실제 수행된 Start

        public PMFlag PmFlag { get; set; }


        public bool IsReviced
        {
            get
            {
                return this.PmInfo.StartTime != this.ExecutedStartTime || this.PmInfo.EndTime != this.End;
            }
        }

        public double ReservedKey { get; internal set; }

        public ATPMPeriod(ATPMInfo pmInfo, DateTime startTime, DateTime endTime)
            : base(pmInfo.Name, startTime, endTime, pmInfo.Option)
        {
            this.PmFlag = PMFlag.Waiting;
            this.PmInfo = pmInfo;
            this.SplitOption = pmInfo.Option;
            this.PMPolicy = pmInfo.PMPolicy;
            this.PMPriority = pmInfo.PMPriority;
            this.PMParameter = pmInfo.PMParameter;
            this.ExecutedStartTime = pmInfo.StartTime;
        }
    }
}
