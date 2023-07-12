using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class ATBomRouting : IPropertyObject, IConstraint
    {
        public ATBom Bom { get; private set; }

        public ATRoute Route { get; private set; }

        public double Priority { get; private set; }

        public string RouteID
        {
            get
            {
                return Route.RouteID;
            }
        }

        public ATBomRouting(ATBom bom, ATRoute route, double priority)
        {
            this.Bom = bom;
            this.Route = route;
            this.Priority = priority;
            this.Property = new DynamicDictionary();
            this.CalendarInfo = new ATCalendarManager();

            this.Constraints = new List<ATConstraint>();
            this.ConstraintInfos = new List<ATConstraintInfo>();

            List<ATProperty> reservedProps = ATInputData.Properties.GetPropertyValueByCategory(PropertyCategory.BomRouting.ToString());
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

        #region Inner Class
        internal class ATBomRoutingComparer : IComparer<ATBomRouting>
        {
            public static ATBomRoutingComparer Default = new ATBomRoutingComparer();

            public int Compare(ATBomRouting x, ATBomRouting y)
            {
                if (object.ReferenceEquals(x, y))
                    return 0;

                return x.Priority.CompareTo(y.Priority);
            }
        }
        #endregion
    }
}
