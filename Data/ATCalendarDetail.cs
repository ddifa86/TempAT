using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class ATCalendarDetail
    {
        public ATCalendar Calendar { get; private set; }

        public string CalendarID
        {
            get { return Calendar.CalendarID; }
        }
     

        public string PatternSeq { get; private set; }

        public DateTime EffectiveStartTime { get; private set; }
        public DateTime EffectiveEndTime { get; private set; }

        /// <summary>
        /// EveryDay
        /// DaysOfWeek  
        /// DaysOfMonth 
        /// DaysOfYear  
        /// EveryNDays  
        /// </summary>
        public CalendarPatternType PatternType { get; private set; }

        /// <summary>
        /// EveryDay
        /// DaysOfWeek  : Sunday = 0, Monday = 1, Tuesday = 2, Wednesday = 3, Thursday = 4, Friday = 5, Saturday = 6
        /// DaysOfMonth : 1~31
        /// DaysOfYear  : 1 ~ 365
        /// EveryNDays  : 1 ~
        /// </summary>
        public string PatternValue { get; private set; }

        private Dictionary<string, string> _internalNDays = new Dictionary<string, string>();

        public double Priority { get; private set; }

        /// <summary>
        /// Key : Attribute
        /// </summary>
        public Dictionary<string, List<ATCalendarAttribute>> Attributes;
        //ATDataManager Attributes;

        public ATCalendarDetail(ATCalendar calendar, string patternSeq, 
            DateTime startTime, DateTime endTime, CalendarPatternType patternType, string patternValue, double priority)
        {
            this.Calendar = calendar;
            this.PatternSeq = patternSeq;
            this.EffectiveStartTime = startTime;
            this.EffectiveEndTime = endTime;

            this.PatternType = patternType;

            this.PatternValue = patternValue;

            if (PatternType == CalendarPatternType.DaysOfWeek)
            {
                var weekdays = PatternValue.Split(',');
                var adjustValue = string.Empty;
                foreach (var x in weekdays)
                {
                    DayOfWeek res;
                    if (ATUtil.StringToDayOfWeek(x, out res) == false)
                        continue;

                    if (string.IsNullOrEmpty(adjustValue))
                        adjustValue = res.ToString();
                    else
                        adjustValue += "," + res.ToString();
                }

                this.PatternValue = adjustValue;
            }
 

            this.Priority = priority;

            // this.Attributes = new ATDataManager();\
            this.Attributes = new Dictionary<string, List<ATCalendarAttribute>>();
        }

        public bool AddAttribute(ATCalendarAttribute attribute)
        {
            string key = attribute.Attribute;
            List<ATCalendarAttribute> lst;
            if (this.Attributes.TryGetValue(key, out lst) == false)
            {
                lst = new List<ATCalendarAttribute>();
                this.Attributes.Add(key, lst);
            }
            lst.Add(attribute);

            return true;
        }

        public DateTime GetEffectiveEndTime(DateTime targetTime)
        {
            DateTime effectiveEndTime = targetTime;
            if (this.PatternType == CalendarPatternType.EveryNDays)
            {
                if (string.IsNullOrEmpty(this.PatternValue))
                {
                    return this.EffectiveEndTime;
                }

                var ndays = Convert.ToInt32(this.PatternValue);
                effectiveEndTime = effectiveEndTime.AddDays(ndays);
            }
            //else if (this.PatternType == CalendarPatternType.Period)
            //{
            //    effectiveEndTime = this.EffectiveEndTime;
            //}
            else
            {
                effectiveEndTime = effectiveEndTime.AddDays(1);
            }

            return effectiveEndTime;
        }
        
        /// <summary>
        /// 유효한 Calendar 정보인지 체크
        /// </summary>
        /// <param name="targetTime"></param>
        /// <returns></returns>
        public bool IsEffectiveTime(DateTime targetTime)
        {
            if (this.PatternType == CalendarPatternType.DaysOfWeek)
            {
                var daysofweek = targetTime.DayOfWeek;

                if (this.PatternValue.Contains(daysofweek.ToString()) == false)
                    return false;
            }
            else if (this.PatternType == CalendarPatternType.DaysOfMonth)
            {
                var daysofmonth = targetTime.FirstDayOfMonth();

                string[] patterns = PatternValue.Split(',');
                HashSet<DateTime> dayList = new HashSet<DateTime>();
                foreach (string value in patterns)
                {
                    int day = Convert.ToInt32(value.Trim()) - 1;
                    dayList.Add(daysofmonth.AddDays(day));
                }

                if (dayList.Contains(targetTime) == false)
                    return false;

            }
            else if (this.PatternType == CalendarPatternType.EveryNDays)
            {             
                if (string.IsNullOrEmpty(this.PatternValue))
                {
                    return this.EffectiveStartTime <= targetTime && this.EffectiveEndTime > targetTime;
                }

                var ndays = Convert.ToInt32(this.PatternValue);

                var days = Math.Ceiling((targetTime - this.EffectiveStartTime).TotalDays); //targetTime.ToString(ATUtil.DateFormat);

                if (days % ndays != 0)
                    return false;
            }
            //else if (this.PatternType == CalendarPatternType.Period)
            //{
            //    if (this.EffectiveStartTime == targetTime)
            //        return true;

            //    return false;
            //}

            if (this.EffectiveStartTime <= targetTime && this.EffectiveEndTime >= targetTime)
            {
                return true;
            }

            return false;
        }


        public List<ATCalendarAttribute> GetAttribte(string attribute = null )
        {
            if (string.IsNullOrEmpty(attribute) == true)
            {
                List<ATCalendarAttribute> lst = new List<ATCalendarAttribute>();

                foreach (var attrs in this.Attributes.Values)
                {
                    lst.AddRange(attrs);
                }

                return lst;
            }
            else
            {

                List<ATCalendarAttribute> lst;
                if (this.Attributes.TryGetValue(attribute, out lst) == false)
                    lst = new List<ATCalendarAttribute>();

                 return lst;
            }
        }
    }
}
