using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    /// <summary>
    /// 설비의 OffTime 정보입니다.
    /// </summary>
    public class ATOffTimeInfo : ATNonWorkingInfo
    {
        public ATOffTimeInfo(DateTime startTime, DateTime endTime, ATCalendarAttribute attr, string name = "", LotSplitOption option = LotSplitOption.KeepAndDelay)
            :base(startTime, endTime, attr, name, option)
        {
            
        }
    }
}
