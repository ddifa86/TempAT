using Mozart.SeePlan.TimeLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    /// <summary>
    /// 설비의 NonWorkingTime을 등록하고 관리하는 Manager 입니다.
    /// </summary>
    public class ATNonWorkingManager
    {
        public  PBFResource _bucket { get; private set; }
        internal SortedList<APENonWorkingPeriod, APENonWorkingPeriod> _nonWorkingTimes { get; set; }
        internal List<APENonWorkingPeriod> _nonWorkingTimesHistory { get; set; }


        public ATNonWorkingManager(PBFResource bucket)
        {
            this._bucket = bucket;
            this._nonWorkingTimes = new SortedList<APENonWorkingPeriod, APENonWorkingPeriod>(ATNonWorkingPeriodComparer.Default);
            this._nonWorkingTimesHistory = new List<APENonWorkingPeriod>();
        }

        public void AddNonWorkingTimes(APENonWorkingPeriod period)
        {
            if (this._nonWorkingTimes.ContainsKey(period) == false)
                this._nonWorkingTimes.Add(period, period);
            else
            {

            }
        }

        /// <summary>
        /// 기간 내에 StartTime이 있는 NonWorkingTime 정보를 반환합니다.
        /// </summary>
        /// <param name="start">검색 대상의 시작 시간</param>
        /// <param name="end">검색 대상 종료 시간 </param>
        /// <returns></returns>
        public List<APENonWorkingPeriod> GetNonWorkingPeriods(DateTime start, DateTime end)
        {
            // nextStartTime이 nonWorkingTime의 start인 것은 가져오면 안되므로 <=를 <로 변경
            var lst = this._nonWorkingTimes.Values.Where(x => x.Start >= start && x.Start < end).ToList();
            if (lst == null)
                return new List<APENonWorkingPeriod>();

            return lst;
        }

        /// <summary>
        /// NonWorkingTime List에서 Parameter값 (end)와 빠르거나 같은 시간에 시작하는 NonWorkingTime을 반환합니다
        /// </summary>
        /// <param name="end"></param>
        /// <returns>NonWorkingTime List에서 Parameter값 (end)와 빠르거나 같은 시간에 시작하는 NonWorkingTime입니다.</returns>
        public List<APENonWorkingPeriod> GetNonWorkingTimes(DateTime end)
        {
            var lst = this._nonWorkingTimes.Values.Where(x => x.Start <= end).ToList();
            return lst;
        }

        /// <summary>
        /// NonWorkingTimeHistory에서 Start,End 사이에 StartTime이 있는 NonWorkingTime List를 반환합니다.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns>NonWorkingTimeHistory에서 Start,End 사이에 StartTime이 있는 NonWorkingTime List입니다</returns>
        public List<APENonWorkingPeriod> GetNonWorkingPeriodhistory(DateTime start, DateTime end)
        {//LX사용중
            var lst = this._nonWorkingTimesHistory.Where(x => x.Start >= start && x.Start < end).ToList();
            return lst;
        }


        /// <summary>
        /// 전체 NonWorkingHistory 반환합니다
        /// </summary>
        /// <returns>전체 NonWorkingHistory입니다.</returns>
        public List<APENonWorkingPeriod> GetAllNonWorkingPeriodHistory()
        {
            return this._nonWorkingTimesHistory;
        }
    }

    public class ATNonWorkingPeriodComparer : IComparer<APENonWorkingPeriod>
    {
        public static ATNonWorkingPeriodComparer Default = new ATNonWorkingPeriodComparer();

        public int Compare(APENonWorkingPeriod x, APENonWorkingPeriod y)
        {
            var cmp = x.Start.CompareTo(y.Start);

            // 시작시간이 동일한 경우, 휴무기간이 긴 정보 우선 처리.
            if (cmp == 0)
                cmp = y.End.CompareTo(x.End);

            return cmp;
        }
    }
}
