using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public static partial class ATUtil
    {
        #region Variables

        /// <summary>
        /// 월 문자열 서식입니다. 
        /// 기본값은 'yyyyMM' 입니다.
        /// </summary>
        public static string MonthFormat = "yyyyMM";

        /// <summary>
        /// 주 문자열 서식입니다. 
        /// 기본값은 'yyyyWW'입니다.
        /// </summary>
        public static string WeekFormat = "yyyyWW";

        public static DateTime DateMinValue = "1800-01-01 00:00:00".DbToDateTime();

        public static string PlanDateFormat = "yyyy-MM-dd";

        #endregion

        #region Properties

        /// <summary>
        /// 날짜 문자열 서식입니다. 
        /// FactoryConfiguration.DbDateFormat과 동일합니다.
        /// </summary>
        public static string DateFormat
        {
            get { return FactoryConfiguration.Current.DbDateFormat; }
            set { FactoryConfiguration.Current.DbDateFormat = value; }
        }

        /// <summary>
        /// 날짜와 시간 문자열 서식입니다. 
        /// FactoryConfiguration.DbDateTimeFormat과 동일합니다.
        /// </summary>
        public static string DateTimeFormat
        {
            get { return FactoryConfiguration.Current.DbDateTimeFormat; }
            set { FactoryConfiguration.Current.DbDateTimeFormat = value; }
        }

        #endregion

        #region Date

        /// <summary>
        /// 날짜 문자열 서식을 사용하여 대상 날짜와 시간 있는 그대로에 해당하는 날짜 문자열 표현으로 변환합니다.
        /// </summary>
        /// <param name="value">대상 시간입니다.</param>
        /// <param name="withTime">시간을 고려할지 여부입니다.</param>
        /// <returns>대상 시간에 대한 날짜 문자열 표현입니다.</returns>
        public static string ToString(DateTime value, bool withTime)
        {
            return value.ToString(withTime ? DateTimeFormat : DateFormat);
        }

        /// <summary>
        /// 날짜 문자열 서식을 사용하여 대상 날짜와 시간을 해당하는 날짜 문자열 표현으로 변환합니다. 
        /// 시간을 고려하는 경우 공장 일 경계는 당일로 취급합니다. 
        /// </summary>
        /// <param name="value">대상 시간입니다.</param>
        /// <param name="withTime">시간을 고려할지 여부입니다.</param>
        /// <returns>대상 시간에 대한 날짜 문자열 표현입니다.</returns>
        public static string ToDate(DateTime value, bool withTime = false)
        {
            var dt = withTime ? value : value.StartTimeOfDayT();
            dt = dt.SplitDate();

            return dt.ToString(DateFormat);
        }

        /// <summary>
        /// 날짜 문자열 서식을 사용하여 대상 날짜와 시간을 해당하는 날짜 문자열 표현으로 변환합니다.
        /// 시간을 고려하는 경우 공장 일 경계는 전일로 취급합니다.
        /// </summary>
        /// <param name="value">대상 시간입니다.</param>
        /// <param name="withTime">시간을 고려할지 여부입니다.</param>
        /// <returns>대상 시간에 대한 날짜 문자열 표현입니다.</returns>
        public static string ToDateEnd(DateTime value, bool withTime = false)
        {
            var dt = withTime ? value : value.EndTimeOfDayT();
            dt = dt.SplitDateEnd();

            return dt.ToString(DateFormat);
        }

        #endregion Date

        #region Month

        /// <summary>
        /// 월 문자열 서식을 사용하여 대상 날짜와 시간을 해당하는 월 문자열 표현으로 변환합니다. 
        /// 시간을 고려하는 경우 공장 일 경계는 당일로 취급합니다.
        /// </summary>
        /// <param name="value">대상 시간입니다.</param>
        /// <param name="withTime">시간을 고려할지 여부입니다.</param>
        /// <returns>대상 시간에 대한 월 문자열 표현입니다.</returns>
        public static string ToMonth(DateTime value, bool withTime = false)
        {
            var dt = withTime ? value : value.StartTimeOfDayT();
            dt = dt.SplitDate();

            return dt.ToString(MonthFormat);
        }

        /// <summary>
        /// 월 문자열 서식을 사용하여 대상 날짜와 시간을 해당하는 월 문자열 표현으로 변환합니다. 
        /// 시간을 고려하는 경우 공장 일 경계는 전일로 취급합니다.
        /// </summary>
        /// <param name="value">대상 시간입니다.</param>
        /// <param name="withTime">시간을 고려할지 여부입니다.</param>
        /// <returns>대상 시간에 대한 월 문자열 표현입니다.</returns>
        public static string ToMonthEnd(DateTime value, bool withTime = false)
        {
            var dt = withTime ? value : value.EndTimeOfDayT();
            dt = dt.SplitDateEnd();

            return dt.ToString(MonthFormat);
        }

        #endregion

        #region Week

        /// <summary>
        /// 주 문자열 서식을 사용하여 대상 날짜와 시간을 해당하는 주 문자열 표현으로 변환합니다. 
        /// 시간을 고려하는 경우 공장 일 경계는 당일로 취급합니다.
        /// </summary>
        /// <param name="value">대상 시간입니다.</param>
        /// <param name="withTime">시간을 고려할지 여부입니다.</param>
        /// <returns>대상 시간에 대한 주 문자열 표현입니다.</returns>
        public static string ToWeek(DateTime value, bool withTime = false)
        {
            var dt = withTime ? value : value.StartTimeOfDayT();

            dt = dt.StartTimeOfWeekF();
            dt = dt.SplitDate();

            var weekNo = DateUtility.GetIso8601WeekOfYear(dt, null, FactoryConfiguration.Current.StartWeek);

            var year = dt.Year;
            var str = year.ToString("D4") + weekNo.ToString("D2");

            return str;
        }

        public static int WeekOfYear(this DateTime value, bool withTime = false)
        {
            var week = ToWeek(value, withTime);
            return Int32.Parse(week);
        }

        /// <summary>
        /// 주 문자열 서식을 사용하여 대상 날짜와 시간을 해당하는 주 문자열 표현으로 변환합니다. 
        /// 시간을 고려하는 경우 공장 일 경계는 전일로 취급합니다.
        /// </summary>
        /// <param name="value">대상 시간입니다.</param>
        /// <param name="withTime">시간을 고려할지 여부입니다.</param>
        /// <returns>대상 시간에 대한 주 문자열 표현입니다.</returns>
        public static string ToWeekEnd(DateTime value, bool withTime = false)
        {
            var dt = withTime ? value : value.EndTimeOfDayT();
            dt = dt.SplitDateEnd();

            dt = dt.StartTimeOfWeekF();

            var weekNo = DateUtility.GetIso8601WeekOfYear(dt, null, FactoryConfiguration.Current.StartWeek);

            var year = dt.Year;
            var str = year.ToString("D4") + weekNo.ToString("D2");

            return str;
        }

        /// <summary>
        /// 공장시간 기준 주차의 지정 요일의 날짜 정보 반환
        /// </summary>
        /// <param name="time"></param>
        /// <param name="daysofweek"></param>
        /// <returns></returns>
        public static DateTime GetDaysOfWeekDateF(this DateTime time, DayOfWeek daysofweek)
        {
            var dt = time.StartTimeOfDayT();
            dt = dt.SplitDate();
            DateTime startofWeek = dt.StartTimeOfWeekF();


            int curDays = Convert.ToInt32(startofWeek.DayOfWeek); //현재
            int shipDays = Convert.ToInt32(daysofweek); //원하는 일


            DateTime targetDate = startofWeek.AddDays(shipDays - curDays); //오늘 + (원하는 일 - 오늘 )
            if (targetDate < startofWeek)
                targetDate = targetDate.AddDays(7);

            return targetDate;
        }

        /// <summary>
        /// 해당월의 첫날
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime FirstDayOfMonth(this DateTime dateTime)
        {
            DateTime today = dateTime;

            DateTime firstDay = today.AddDays(1 - today.Day);

            return firstDay;
        }

        /// <summary>
        /// 해당 월의 마지막 날
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime LastDayOfMonth(this DateTime dateTime)
        {

            DateTime today = dateTime;

            DateTime firstDay = today.AddDays(1 - today.Day);

            DateTime lastDay = firstDay.AddMonths(1).AddDays(-1);

            return lastDay;
        }


        public static DateTime MinTime(DateTime x, DateTime y)
        {
            if (x <= y)
                return x;
            else
                return y;
        }

        public static DateTime MaxTime(params DateTime[] times)
        {
            return times.Max();
        }

        public static DateTime MaxTime(DateTime x, DateTime y)
        {
            if (x >= y)
                return x;
            else
                return y;
        }

        public static string AddBarBetweenDate(this string target)
        {
            if (target.Length == 8)
            {
                return target.Substring(0, 4) + "-" + target.Substring(4, 2) + "-" + target.Substring(6, 2);
            }
            else if (target.Length == 6)
            {
                return target.Substring(0, 4) + "-" + target.Substring(4, 2);
            }

            return string.Empty;
        }

        #endregion
    }
}
