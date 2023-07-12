using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class ATCustomer : IPropertyObject, IConstraint
    {
        public string CustomerID { get; private set; }

        public string CustomerName { get; private set; }

        public double Priority { get; private set; }

        public ATCustomer(string customerID, string customerName, double priority)
        {
            this.CustomerID = customerID;
            this.CustomerName = customerName;
            this.Priority = priority;

            this.Property = new DynamicDictionary();
            this.CalendarInfo = new ATCalendarManager();

            this.ConstraintInfos = new List<ATConstraintInfo>();
            this.Constraints = new List<ATConstraint>();

            List<ATProperty> reservedProps = ATInputData.Properties.GetPropertyValueByCategory(PropertyCategory.Customer.ToString());
            if (reservedProps != null)
                reservedProps.ForEach(x => this.SetProperty(x.PropertyID, x.DefaultValue));
        }

        #region DataObject Interface
        public dynamic Property { get; internal set; }

        public ATCalendarManager CalendarInfo { get; internal set; }

        public List<ATConstraintInfo> ConstraintInfos { get; set; }

        public List<ATConstraint> Constraints { get; set; }

        public bool HasConstraint { get; set; }

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
