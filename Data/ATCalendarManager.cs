using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class ATCalendarManager // : ATDataManager
    {
        Dictionary<string, List<ATCalendar>> _calendars;
        HashSet<ATCalendarAttribute> _calendarAttribute;
        public ATCalendarManager()
        {
            this._calendars = new Dictionary<string, List<ATCalendar>>();
            this._calendarAttribute = new HashSet<ATCalendarAttribute>();
        }

        public void AddCalendar(ATCalendar calendar)
        {
            if (calendar == null)
                return;

            if (this._calendars.TryGetValue(calendar.CalendarType, out var value))
                value.Add(calendar);
            else
                this._calendars.Add(calendar.CalendarType, new List<ATCalendar>() { calendar });

            // Attribute 별도로 등록.
            var attributes = calendar.GetAttributes();
            
            //attributes.ForEach(x => this.ImportRow<ATCalendarAttribute>(x));
            attributes.ForEach(x => _calendarAttribute.Add(x));
        }

        /// <summary>
        /// 커스텀으로 추가된 Calendar의 Attribute 정보를 가져오는 경우.
        /// </summary>
        /// <param name="calendarType"></param>
        /// <param name="now"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public ATCalendarAttribute GetCalendarAttribute(string calendarType, DateTime now, string attribute)
        {
            ATCalendar calendar;
            if (this._calendars.TryGetValue(calendarType, out var calendars))
                calendar = calendars.FirstOrDefault();
            else
                return null;

            if (calendar == null)
                return null;

            // Custom 로직인 경우
            var detail = calendar.GetDetail(now);
            if (detail == null)
                return null;

            var attrlst = detail.GetAttribte( attribute);

            return attrlst.FirstOrDefault();
        }

        /// <summary>
        /// 내부 로직으로 약속된 Calendar를 사용하는 경우
        /// </summary>
        /// <param name="calendarType"></param>
        /// <param name="interanlPatternSeq"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        internal ATCalendarAttribute GetCalendarAttribute(string calendarType, string interanlPatternSeq , string attribute)
        {
            ATCalendarAttribute result;

            if (string.IsNullOrEmpty(attribute))
            {
                result = this._calendarAttribute.Where(x => x.CalendarType == calendarType && x.ApplyDate == interanlPatternSeq).OrderBy(x => x.Priority).FirstOrDefault();
            }
            else
            {
                result = this._calendarAttribute.Where(x => x.CalendarType == calendarType && x.ApplyDate == interanlPatternSeq && x.Attribute == attribute).OrderBy(x => x.Priority).FirstOrDefault();
            }

            return result;
        }

        internal List<ATCalendarAttribute> GetCalendarAttributes(string calendarType, string attribute)
        {
            var results = this._calendarAttribute.Where(x => x.CalendarType == calendarType && x.Attribute == attribute).OrderBy(x => x.EffectiveStartTime);

            return results.ToList();
        }
    }
}
