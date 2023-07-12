using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik
{
    public class CalendarHelper
    {
        private Dictionary<string, ATCalendar> _calendars = new Dictionary<string, ATCalendar>();

        public bool AddCalendar(string calendarID, ATCalendar calendar)
        {
            if (_calendars.ContainsKey(calendarID) == true)
                return false;

            _calendars.Add(calendarID, calendar);

            return true;
        }

        public ATCalendar GetCalendar(string calendarID)
        {
            if (string.IsNullOrEmpty(calendarID))
                return null;

            ATCalendar calendar;
            if (_calendars.TryGetValue(calendarID, out calendar) == false)
                return null;

            return calendar;
        }

        public ATCalendarDetail GetCalendarDetail(string calendarID, string patternSeq)
        {
            var calendar = GetCalendar(calendarID);

            if (calendar == null)
                return null;

            var detail = calendar.GetDetail(patternSeq);

            return detail;
        }

        public Dictionary<string, ATCalendar> GetCalendars()
        {
            return _calendars;
        }
    }
}
