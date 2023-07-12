using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    /// <summary>
    /// 설비의 PM 정보 입니다.
    /// </summary>
    public class ATPMInfo : ATNonWorkingInfo
    {
        internal int PMPriority { get; set; }
        internal PmPolicy PMPolicy { get; set; }
        internal double PMParameter { get; set; }

        public ATPMInfo(DateTime startTime, DateTime endTime, ATCalendarAttribute attr, string name, LotSplitOption option, int pmPriority, PmPolicy pmPolicy, double pmParameter)
            : base(startTime, endTime, attr, name, option)
        {
            this.PMPriority = pmPriority;
            this.PMPolicy = pmPolicy;
            this.PMParameter = pmParameter;
        }
    }
}
