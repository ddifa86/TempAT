using Mozart.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    [Mozart.Task.Execution.FEBaseClassAttribute(Root = "PKG", Category = "PKG", IsTypeBinding = true, Mandatory = true, Description = null)]
    public partial class ATAllocationGroup : IPropertyObject
    {
        #region Property
        /// <summary>
        /// Level이 속한 Stage 정보
        /// </summary>
        public string AllocationGroupID { get; private set; }

        public int Sequence { get; private set; }

        public AllocateType AllocateType { get; private set; }

        public Dictionary<string, ATBuffer> Buffers {get; private set;}

        public ATStage Stage { get; private set; }

        #endregion

        public List<ATResourceGroup> ResourceGroups { get; private set; }

        public ATAllocationGroup(string allocationGroupID, int sequence, ATStage stage, AllocateType type)
        {
            this.AllocationGroupID = allocationGroupID;
            this.Sequence = sequence;
            this.AllocateType = type;
            this.Stage = stage;

            this.Buffers = new Dictionary<string, ATBuffer>();
            this.ResourceGroups = new List<ATResourceGroup>();

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

        public void AddResourceGroup(ATResourceGroup resGroup)
        {
            int index = this.ResourceGroups.BinarySearch(resGroup, ATResourceGroupComparer.Default);
            if (index < 0)
                index = ~index;

            this.ResourceGroups.Insert(index, resGroup);
        }

        public void RemoveResourceGroup(ATResourceGroup resGroup)
        {
            this.ResourceGroups.Remove(resGroup);
        }

        internal class ATResourceGroupComparer : IComparer<ATResourceGroup>
        {
            public static ATResourceGroupComparer Default = new ATResourceGroupComparer();

            public int Compare(ATResourceGroup x, ATResourceGroup y)
            {
                if (object.ReferenceEquals(x, y))
                    return 0;

                var cmp = x.Sequence.CompareTo(y.Sequence);

                if (cmp != 0)
                    return cmp;

                return cmp;
            }
        }
    }
}
