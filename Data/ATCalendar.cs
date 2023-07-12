using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class ATCalendar
    {
        public string CalendarID { get; private set; }


        public string CalendarType { get; private set; }

        /// <summary>
        /// Key : PatternSeq
        /// </summary>
        public SortedDictionary<string, ATCalendarDetail> Details;

        public ATCalendar(string calendarID, string calendarType)
        {
            this.CalendarID = calendarID;
            this.CalendarType = calendarType;

            this.Details = new SortedDictionary<string, ATCalendarDetail>();
        }

        public bool AddDetail(ATCalendarDetail detail)
        {
            if (Details.ContainsKey(detail.PatternSeq) == true)
                return false;

            Details.Add(detail.PatternSeq, detail);

            return true;
        }

        public ATCalendarDetail GetDetail(string patternSeq)
        {
            if (string.IsNullOrEmpty(patternSeq))
                return null;

            ATCalendarDetail detail;

            if (Details.TryGetValue(patternSeq, out detail) == false)
                return null;

            return detail;
        }


        public ATCalendarDetail GetDetail(DateTime now)
        {
            
            if (this.Details.Values.Count() == 0)
                return null;

            var lst = this.Details.Values.Where(x => x.IsEffectiveTime(now) == true).ToList();

            if (lst.Count() == 0)
                return null;

            lst.Sort(new ATCalendarDetailComparer(now));

            var selectDetails = lst.First();

            return selectDetails;
        }

        public List<ATCalendarAttribute> GetAttributes()
        {
            List<ATCalendarAttribute> lst = new List<ATCalendarAttribute>();

            foreach (var detail in Details.Values)
            {
                var attrs = detail.GetAttribte();
                lst.AddRange(attrs);
            }

            return lst;
        }

        public List<ATCalendarAttribute> GetAttributes(string attribute)
        {
            List<ATCalendarAttribute> lst = new List<ATCalendarAttribute>();

            foreach (var detail in Details.Values)
            {
                var attrs = detail.GetAttribte(attribute);
                lst.AddRange(attrs);
            }

            return lst;

        }


        public bool IsNeedDailyPatternCode()
        {
            // Holiday / PM / BreakTime
            if (this.CalendarType.StartsWith(ATReservedCode.RESERVED_PREFIX_CODE))
                return true;

            return false;
        }
    }
}
