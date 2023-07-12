using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public interface IPropertyObject
    {
        dynamic Property { get; }

        ATCalendarManager CalendarInfo { get; }

        void SetProperty(string propertyID, object value);

        void SetCalendar(string propertyID, ATCalendar calendar);

        // 추후 필요 여부 확인 후 다시 복구 작업.
        //Dictionary<string, object> Properety { get; }

        //bool TryGetProperty(string key, out object result);

        //bool TrySetProperty(string key, object result);
    }
}
