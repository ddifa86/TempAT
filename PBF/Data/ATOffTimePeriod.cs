using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.TimeLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public class ATOffTimePeriod : APENonWorkingPeriod
    {
        ATOffTimeInfo OffTimeInfo { get; set; }

        public ATOffTimePeriod(ATOffTimeInfo Info, DateTime startTime, DateTime endTime)
            : base(Info.Name, startTime, endTime, Info.Option)
        {
            this.OffTimeInfo = Info;
        }
    }
}
