using Mozart.SeePlan.Aleatorik.Planner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    /// <summary>
    /// 설비의 NonWorkingTime을 등록하고 관리하는 Manager 입니다.
    /// </summary>
    public class ATNonWorkingInfoManager
    {
        public ATResource Resource { get; set; }

        public NonWorkingType Type { get; private set; }

        public List<ATNonWorkingInfo> NonWorkingInfos { get; private set; }

        public Dictionary<string, List<ATNonWorkingInfo>> DailyNonWorkingInfos { get; private set; }

        public ATNonWorkingInfoManager(ATResource res, NonWorkingType type)
        {
            this.Resource = res;
            this.Type = type;
            this.NonWorkingInfos = new List<ATNonWorkingInfo>();
            this.DailyNonWorkingInfos = new Dictionary<string, List<ATNonWorkingInfo>>();
        }

        /// <summary>
        /// 설비에 PM정보를 등록합니다.
        /// </summary>
        /// <param name="startTime">PM의 시작 시간입니다.</param>
        /// <param name="endTime">PM의 종료 시간입니다.</param>
        /// <param name="attr">PM 스케줄 정보를 담고있는 Attribute 입니다.</param>
        /// <param name="name">PM의 이름입니다.</param>
        /// <param name="splitOption">PM의 Split Option 입니다.</param>
        /// <param name="priority">PM의 우선순위입니다.</param>
        /// <param name="policy">PM의 정책입니다.</param>
        /// <param name="parameter">PM의 Parameter 입니다.</param>
        public void AddPMInfo(DateTime startTime, DateTime endTime, ATCalendarAttribute attr, string name, LotSplitOption splitOption, int priority, PmPolicy policy, double parameter)
        { 
            ATPMInfo info = new ATPMInfo(startTime, endTime, attr, name, splitOption, priority, policy, parameter);

            //Setting시 겹치는 PM 미리 제거 (미리 제거하지 않으면, 조회시마다 겹치는지 조회해야함)
            var firstPm = this.NonWorkingInfos.Where(x => x.StartTime <= info.StartTime && info.StartTime < x.EndTime).FirstOrDefault();
            if (firstPm == null)
            {
                this.NonWorkingInfos.Add(info);
            }
            else
            {
                var first = firstPm as ATPMInfo;

                if (first.PMPriority > info.PMPriority)
                {
                    this.NonWorkingInfos.Add(info);
                    this.NonWorkingInfos.Remove(firstPm);

                    ATPMPeriod canceledPM = new ATPMPeriod(first, firstPm.StartTime, firstPm.EndTime);
                    canceledPM.PmFlag = PMFlag.Canceled;

                    OutputWriter.Instance.WritePmPlanLog(this.Resource.ResourceID, canceledPM);
                }
                else
                {
                    ATPMPeriod canceledPM = new ATPMPeriod(info, info.StartTime, info.EndTime);
                    canceledPM.PmFlag = PMFlag.Canceled;

                    OutputWriter.Instance.WritePmPlanLog(this.Resource.ResourceID, canceledPM);
                }
            }
        }

        /// <summary>
        /// 설비의 OffTime을 등록합니다.
        /// </summary>
        /// <param name="startTime">OffTime의 시작 시간입니다.</param>
        /// <param name="endTime">OffTime의 종료 시간입니다.</param>
        /// <param name="attr">OffTime 스케줄 정보를 담고있는 Attribute 입니다.</param>
        /// <param name="name">OffTime의 name입니다.</param>
        /// <param name="splitOption">OffTime의 Split Option 입니다.</param>
        public void AddOffTimeInfo(DateTime startTime, DateTime endTime, ATCalendarAttribute attr, string name, LotSplitOption splitOption)
        {
            ATOffTimeInfo info = new ATOffTimeInfo(startTime, endTime, attr, name, splitOption);
            this.NonWorkingInfos.Add(info);

            if (this.DailyNonWorkingInfos.TryGetValue(info.CalAttr.ApplyDate, out var infos) == false)
            {
                infos = new List<ATNonWorkingInfo>();
                this.DailyNonWorkingInfos.Add(info.CalAttr.ApplyDate, infos);
            }

            infos.Add(info);
        }
    }
}
