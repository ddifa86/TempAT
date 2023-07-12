using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class ATResourceGroup : IPropertyObject
    {
        public string GroupID { get; private set; }

        public int Sequence { get; private set; }

        public ResourceCategory ResourceGroupType { get; private set; }

        public bool IsInvalidResourceGroup { get; private set; }

        public ATAllocationGroup AllocationGroup { get; private set; }

        public bool IsReSorting { get; private set; }

        public Dictionary<string, ATResource> Resources { get; private set; }

        public ATResourceGroup(string groupid,  int sequence, ATAllocationGroup allocationGroup, bool isReSorting)
        {
            this.GroupID = groupid;
            this.Sequence = sequence;
            this.AllocationGroup = allocationGroup;

            this.IsReSorting = isReSorting;

            this.ResourceGroupType = ResourceCategory.None;
            this.IsInvalidResourceGroup = false;

            this.Resources = new Dictionary<string, ATResource>();
            this.Property = new DynamicDictionary();
            this.CalendarInfo = new ATCalendarManager();
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

        public bool AddResource(ATResource resource)
        {
            string key = resource.ResourceID;
            bool isFirst = this.Resources.Count <= 0;

            if (this.Resources.ContainsKey(key) == false)
            {
                if (isFirst == false)
                {
                    if (this.ResourceGroupType != resource.ResCategory || this.IsInvalidResourceGroup)
                    {
                        this.IsInvalidResourceGroup = true;
                        return false;
                    }
                }

                this.Resources.Add(key, resource);
                this.ResourceGroupType = resource.ResCategory;

                return true;
            }

            return false;
        }

    }
}
