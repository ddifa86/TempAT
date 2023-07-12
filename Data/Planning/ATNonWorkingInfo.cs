using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    /// <summary>
    /// 설비의 NonWorking시간의 정보입니다.
    /// </summary>
    public class ATNonWorkingInfo
    {
        public string Name { get; set; }
        public LotSplitOption Option { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public ATCalendarAttribute CalAttr;

        public ATNonWorkingInfo(DateTime startTime, DateTime endTime, ATCalendarAttribute attr, string name = "", LotSplitOption option = LotSplitOption.KeepAndDelay)
        {
            this.StartTime = startTime;
            this.EndTime = endTime;
            this.CalAttr = attr;

            this.Name = string.IsNullOrEmpty(name) == false ? name : attr?.Attribute;
            this.Option = option;

        }
    }
}
