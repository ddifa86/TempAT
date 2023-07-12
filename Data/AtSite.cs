using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class ATSite : IPropertyObject, IConstraint
    {
        public string SiteID { get; private set; }
        public string SiteName { get; private set; }

        public ATSite(string siteID, string siteName)
        {
            this.SiteID = siteID;
            this.SiteName = siteName;

            this.Property = new DynamicDictionary();
            this.CalendarInfo = new ATCalendarManager();

            this.Constraints = new List<ATConstraint>();
            this.ConstraintInfos = new List<ATConstraintInfo>();

            List<ATProperty> reservedProps = ATInputData.Properties.GetPropertyValueByCategory(PropertyCategory.Site.ToString());
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
