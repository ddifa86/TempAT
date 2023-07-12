 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    [Mozart.Task.Execution.FEBaseClassAttribute(Root = "PKG", Category = "PKG", IsTypeBinding = true, Mandatory = true, Description = null)]
    public class ATWipInfo : IPropertyObject, ICloneable, ISetupProperty, IConstraint
    {
 
        public string LotID { get; private set; }
 
        public double UnitQty { get; private set; }

 
        public WipType LotType { get; private set; }

       
        public LotState LotState { get; private set; }

        public ATSite Site { get; private set; }

        public string SiteID
        {
            get
            {
                return this.Site.SiteID;
            }
        }

        public DateTime AvailableTime { get; private set; }

        public DateTime TrackInTime { get; private set; }

        public ATItem Item { get; internal set; }

        public ATRoute Route { get; internal set; }

        public ATOperation Oper { get; internal set; }

        public ATResource Resource { get; internal set; }

        public ATItemSiteBuffer ItemSiteBuffer { get; set; }

        public ATStage Stage { get; set; }

        public Dictionary<string, string> SetupProperty { get; set; }

        public ATWipInfo(
            string lotID, double unitQty, WipType wipType, LotState lotState,
            ATSite site, ATItem item, ATRoute route, ATOperation oper,
            ATResource resource, DateTime availableTime, DateTime trackintime,
            ATStage stage
            )
        {
            this.LotID = lotID;
            this.UnitQty = unitQty;
            this.LotType = wipType;
            this.LotState = lotState;
            this.Site = site;
            this.Item = item;
            this.Route = route;
            this.Oper = oper;       
            this.Resource = resource;
            this.AvailableTime = availableTime;
            this.TrackInTime = trackintime;
            this.Stage = stage;
            this.SetupProperty = new Dictionary<string, string>();

            this.Property = new DynamicDictionary();
            this.CalendarInfo = new ATCalendarManager();

            this.Constraints = new List<ATConstraint>();
            this.ConstraintInfos = new List<ATConstraintInfo>();

            List<ATProperty> reservedProps = ATInputData.Properties.GetPropertyValueByCategory(PropertyCategory.Wip.ToString());
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

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
