using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    /// <summary>
    /// 정적 정보를 관리하는 클래스가 아님.
    /// </summary>
    public class ATOperResource : IPropertyObject, IConstraint
    {
        public ATRoute Route { get; private set; }

        public ATOperation Oper { get; private set; }

        public ATResource Resource { get; private set; }

        public string RouteID
        {
            get
            {
                return this.Route.RouteID;
            }
        }

        public string OperID
        {
            get
            {
                return this.Oper.OperID;
            }
        }

        public string ResourceID
        {
            get
            {
                return this.Resource.ResourceID;
            }
        }

        public IBucket Bucket
        {
            get
            {
                return this.Resource.Bucket;
            }
        }

        public bool HasAddArrange
        {
            get
            {
                return AddArrangeInfo.Count() > 0;
            }
        }

        public double FlowTime { get; private set; }

        public double UsagePer { get; private set; }

        public Dictionary<string, double> UsagePerSets { get; private set; }

        public Dictionary<string, double> FlowTimeSets { get; private set; }

        public double Priority { get; private set; }

        // 추가로 봐야할 Arrange 정보
        public Dictionary<string, List<ATOperResource>> AddArrangeInfo;

        public ArrangeType Type;        

        // Calendar 처리...필요?

        public ATOperResource(ATRoute route , ATOperation oper, ATResource resource, double flowTime, double usagePer, double priority, ArrangeType type, PropertyCategory propertyCat)
        {
            this.Route = route;
            this.Oper = oper;
            this.Resource = resource;
            this.UsagePer = usagePer;
            this.FlowTime = flowTime;
            this.Priority = priority;
            this.Type = type;
            this.AddArrangeInfo = new Dictionary<string, List<ATOperResource>>();
            this.FlowTimeSets = new Dictionary<string, double>();
            this.UsagePerSets = new Dictionary<string, double>();

            this.Property = new DynamicDictionary();
            this.CalendarInfo = new ATCalendarManager();

            this.ConstraintInfos = new List<ATConstraintInfo>();
            this.Constraints = new List<ATConstraint>();

            if (this.Resource.CapaType == CapacityType.Time)
            {
                this.FlowTime = flowTime.UomToSecond(ATOption.Instance.TimeUOM);
                this.UsagePer = usagePer.UomToSecond(ATOption.Instance.TimeUOM);
            }

            List<ATProperty> reservedProps = ATInputData.Properties.GetPropertyValueByCategory(propertyCat.ToString());
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


        public void SetUsagePerCal(ATCalendar calendar)
        {
            if (calendar == null)
                return;

            var cal = calendar.GetAttributes();

            foreach (var info in cal)
            {
                string key = info.ApplyDate + "@" + info.Attribute;

                if (this.UsagePerSets.ContainsKey(key) == false)
                {
                    double usagePer = info.Value;
                    if (this.Resource.CapaType == CapacityType.Time)
                        usagePer = usagePer.UomToSecond(ATOption.Instance.TimeUOM);

                    this.UsagePerSets.Add(key, usagePer);
                }
            }
        }

        public void SetFlowTimeCal(ATCalendar calendar)
        {
            if (calendar == null)
                return;

            var cal = calendar.GetAttributes();

            foreach (var info in cal)
            {
                string key = info.ApplyDate + "@" + info.Attribute;

                if (this.FlowTimeSets.ContainsKey(key) == false)
                {
                    double flowTime = info.Value;
                    if (this.Resource.CapaType == CapacityType.Time)
                        flowTime = flowTime.UomToSecond(ATOption.Instance.TimeUOM);

                    this.FlowTimeSets.Add(key, flowTime);
                }
            }
        }

        public void SetCalendar(string propertyID, ATCalendar calendar)
        {
            this.CalendarInfo.AddCalendar(calendar);
        }
        #endregion

        public double GetUsagePer(DateTime time)
        {
            string usagePerKey = string.Concat(time.ToString(ATUtil.DateFormat), "@", ATReservedCode.USAGE_PER);

            double usagePer;
            if (this.UsagePerSets.TryGetValue(usagePerKey, out double usage))
                usagePer = usage;
            else
                usagePer = this.UsagePer;

            return usagePer;
        }

        public double GetFlowTime(DateTime time)
        {
            string processTimeKey = string.Concat(time.ToString(ATUtil.DateFormat), "@", ATReservedCode.FLOW_TIME);

            double flowTime;
            if (this.FlowTimeSets.TryGetValue(processTimeKey, out double value))
                flowTime = value;
            else
                flowTime = this.FlowTime;

            return flowTime;
        }

        public void SetAddArrangeInfo(string resGroupID, ATOperResource arr)
        {
            List<ATOperResource> arrs;
            if (this.AddArrangeInfo.TryGetValue(resGroupID, out arrs) == false)
                this.AddArrangeInfo.Add(resGroupID, arrs = new List<ATOperResource>()); 

            if (arrs.Where(x => x.ResourceID == arr.ResourceID).FirstOrDefault() == null)
                arrs.Add(arr);

        }

        public ATOperResource GetAddArrangeInfo(string resGroupID, string addResourceID)
        {
            List<ATOperResource> arr;
            if (this.AddArrangeInfo.TryGetValue(resGroupID, out arr) == true)
                return arr.Where(x => x.ResourceID == addResourceID).FirstOrDefault();
            else
                return null;
        }
    }
     
}
