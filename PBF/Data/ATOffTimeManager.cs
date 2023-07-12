using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.TimeLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    /// <summary>
    /// 설비의 OffTime을 등록하고 관리하는 Manager 입니다.
    /// </summary>
    public class ATOffTimeManager : ATNonWorkingManager
    {
        public ATOffTimeManager(PBFResource bucket)
            : base(bucket)
        {

        }

        /// <summary>
        /// 설비에 OffTime을 등록합니다
        /// </summary>
        /// <param name="name">OffTime의 Name 입니다.</param>
        /// <param name="start">OffTime의 시작 시간입니다</param>
        /// <param name="end">OffTime의 종료 시간입니다.</param>
        /// <param name="option">OffTime의 Split OPtion 입니다.</param>
        internal void AddOffTime(string name, DateTime start, DateTime end, LotSplitOption option)
        {
            ATOffTimeInfo info = new ATOffTimeInfo(start, end, null, name);
            var period = new ATOffTimePeriod(info,start, end);
            AddNonWorkingTimes(period);
        }

        /// <summary>
        /// 설비에 OffTime을 등록합니다
        /// </summary>
        /// <param name="info">OffTime 정보 입니다.</param>
        /// <param name="start">OffTime 시작 시간 입니다.</param>
        /// <param name="end">OffTime 종료 시간 입니다.</param>
        internal void AddOffTime(ATOffTimeInfo info, DateTime start, DateTime end)
        {
            var period = new ATOffTimePeriod(info, start, end);
            AddNonWorkingTimes(period);
        }

        internal void AddOffTimes()
        {
            List<ATNonWorkingInfo> nonWorkingInfos = this._bucket.Target.OffTimeInfoManager.NonWorkingInfos;

            foreach (ATNonWorkingInfo info in nonWorkingInfos)
            {
                // 태룡 추가 확인 필요.
                var target = Mozart.SeePlan.FactoryConfiguration.Current;
                var startTime = info.StartTime + target.StartOffset;
                var endTime = info.EndTime + target.StartOffset;

                this.AddOffTime(info as ATOffTimeInfo, startTime, endTime);
            }
        }

        /// <summary>
        /// 지나간 WorkCalendar 정보 정리하고 별도 History 테이블로 관리합니다
        /// </summary>
        /// <param name="fence">해당 날짜 이전의 NonWorkingTime에 대해 관리합니다.</param>
        internal void UpdateCurrentOffTIme(DateTime fence)
        {
            var lst = _nonWorkingTimes.Values.Where(x => x.End <= fence).ToList();

            foreach (var info in lst)
            {
                ////PmRefactoring Prev 연결관계..
                //if (capa.Prev != null)
                //    capa.Prev.OffTime += info.Duration.TotalSeconds;

                this._nonWorkingTimesHistory.Add(info);
                this._nonWorkingTimes.Remove(info);
            }
        }

        /// <summary>
        /// 현재의 Capacity 정보에 해당 Bucket 구간의 OffTime을 집계합니다.
        /// </summary>
        /// <param name="fence">해당 날짜 이전의 NonWorkingTime에 대해 집계합니다.</param>
        /// <param name="capa">현재의 Capacity 입니다.</param>
        internal void CalculateCurrentOffTIme(DateTime fence, APECapacity capa)
        {
            var lst = _nonWorkingTimes.Values.Where(x => x.End <= fence).ToList();

            foreach (var info in lst)
            {
                capa.OffTime += info.Duration.TotalSeconds;
            }
        }

        /// <summary>
        /// 이전 OffTime을 OffTime 목록에서 제거하고, 이번 Bucket 구간의 OffTime을 집계하고 기록합니다.
        /// </summary>
        /// <param name="now">현재 Rolling 시간 입니다.</param>
        /// <param name="nextStartTime">다음 Rolling 시간 입니다.</param>
        /// <param name="capa">이번 Bucket 구간의 Capacity 입니다.</param>
        internal void UpdateOffTimePeriods(DateTime now, DateTime nextStartTime, APECapacity capa)
        {
            UpdateCurrentOffTIme(now); // 이전 날짜의 OffTime을 지워줌
            CalculateCurrentOffTIme(nextStartTime, capa); // 오늘 날짜의 OffTime을 집계함

            var lst = this.GetNonWorkingPeriods(now, nextStartTime);

            // 이번 Rolling 구간에서의 NonWorkingTime 출력
            var range = new DateTimeInterval(now, nextStartTime);
            foreach (var period in lst)
            {
                ATOffTimePeriod off = period as ATOffTimePeriod;

                //PmRefactoring 꼭 필요한지?
                if (off.Contains(range))
                {
                    // NoneWorkingTime이 현 구간을 포함하는 경우, 장비 사용하지 못하도록 설정.
                    this._bucket.CurrentTime = ATUtil.MaxTime(this._bucket.CurrentTime, nextStartTime);
                }

                if (off.IsWrite == false)
                {
                    OutputWriter.Instance.WriteNonWorkingPeriodResPlan(this._bucket, off, "NonWorking", off.Name);
                    off.IsWrite = true;
                }
            }
        }


        /// <summary>
        /// at, fence 사이에 걸쳐있는 NonWorking Time을 조회, 그중 가장 빠른 nonWorkingTime을 Return
        /// </summary>
        /// <param name="at"></param>
        /// <param name="fence"></param>
        /// <returns></returns>
        public APENonWorkingPeriod GetNextNonWorkingPeriod(DateTime at, DateTime fence, bool containFence, LotSplitOption option = LotSplitOption.None)
        {
            //at ~ fence 사이에 걸쳐져있는 NonWorkingTime
            foreach (var section in this._nonWorkingTimes.Values)
            {
                if ((containFence && fence < section.Start) || (containFence == false && fence <= section.Start))
                        return APENonWorkingPeriod.NULL;

                if (section.End <= at)
                    continue;

                var cinterval = section as APENonWorkingPeriod;
                if (option != LotSplitOption.None && cinterval.SplitOption != option)
                    continue;

                return section;
            }

            return APENonWorkingPeriod.NULL;
        }
    }
}
