using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class ATInputErrorConfig : IPropertyObject
    {
        public string TableName { get; private set; }
        public string ColumnName { get; private set; }
        public ErrorReasonCode ReasonCode { get; private set; }
        public ErrorSeverity LogLevel { get; private set; }
        public string RefTableName { get; private set; }
        public string RefColumnName { get; private set; }
        public string Parameter { get; private set; }

        // 여기에 체크를 위한 함수들이 List형태로 붙어있어야할 것 같은데..?
        //ConfigDelegate ConfigMethod 

        public ATInputErrorConfig(string tableName, string columnName, ErrorReasonCode reasonCode, ErrorSeverity logLevel, string refTableName, string refColumnName, string parameter)
        {
            this.TableName = tableName;
            this.ColumnName = columnName;
            this.ReasonCode = reasonCode;
            this.LogLevel = logLevel;
            this.RefTableName = refTableName;
            this.RefColumnName = refColumnName;
            this.Parameter = parameter;
        }

        #region DataObject Interface
        public dynamic Property { get; internal set; }

        public ATCalendarManager CalendarInfo { get; internal set; }

        public void SetProperty(string propertyID, object value)
        {
            this.Property[propertyID] = value;
        }
        public void SetCalendar(string propertyID, ATCalendar calendar)
        {
            this.CalendarInfo.AddCalendar(calendar);
        }
        #endregion

    }
}
