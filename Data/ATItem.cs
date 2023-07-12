using Mozart.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    [Mozart.Task.Execution.FEBaseClassAttribute(Root = "PKG", Category = "PKG", IsTypeBinding = true, Mandatory = true, Description = null)]
    public class ATItem : ISetupProperty, IPropertyObject, IConstraint
    {
        public ItemType ItemType { get; private set; }

        public string ItemID { get; private set; }

        public int Grade { get; set; }


        public Dictionary<string, string> SetupProperty { get; set; }

        public bool HasConstraint { get; set; }

        public List<ATConstraintInfo> ConstraintInfos { get; set; }

        public List<ATConstraint> Constraints { get; set; }

        public string ItemName { get; set; }
        public string ItemGroup { get; set; }
        public string ItemUnit { get; set; }
        public string ProcurementType { get; set; }
        public string ProdType { get; set; }
        public string ItemSize { get; set; }

        public ATItem(string itemID, ItemType itemType, int grade, string name, string group, string unit, string procurementType, string prodType, string itemSize)
        {
            this.ItemID = itemID;
            this.ItemType = itemType;

            this.SetupProperty = new Dictionary<string, string>();

            this.Grade = grade;

            this.ItemName = name;
            this.ItemGroup = group;
            this.ItemUnit = unit;
            this.ProcurementType = procurementType;
            this.ProdType = prodType;
            this.ItemSize = itemSize;

            this.Property = new DynamicDictionary();
            this.CalendarInfo = new ATCalendarManager();

            List<ATProperty> reservedProps = ATInputData.Properties.GetPropertyValueByCategory(PropertyCategory.Item.ToString());
            if (reservedProps != null)
                reservedProps.ForEach(x => this.SetProperty(x.PropertyID, x.DefaultValue));

            this.ConstraintInfos = new List<ATConstraintInfo>();
            this.Constraints = new List<ATConstraint>();
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
