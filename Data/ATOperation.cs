using Mozart.Extensions;
using Mozart.SeePlan.Cbsim;
using Mozart.SeePlan.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    [Mozart.Task.Execution.FEBaseClassAttribute(Root = "PKG", Category = "PKG", IsTypeBinding = true, Mandatory = true, Description = null)]
    public partial class ATOperation : Step, IComparer<ATOperation>, IPropertyObject, IConstraint
    {
        public int Sequence { get; private set; }

        public double WaitTat { get; private set; }

        public double RunTat { get; private set; }

        public double TotalTat { get; private set; }

        public double Yield { get; private set; }

        public double CumYield { get; internal set; }

        public double CumWaitTat { get; internal set; }

        public double CumRunTat { get; internal set; }

        public OperType OperType { get; set; }

        public Dictionary<string, double> TatSets { get; private set; }

        public Dictionary<string, double> YieldSets { get; private set; }

        public bool IsBuffer
        {
            get
            {
                return this.OperType == OperType.Buffer;
            }
        }

        public ATBuffer CurrentBuffer
        {
            get
            {
                return (this.Route as ATRoute).Bom.FromBuffer;
            }
        }

        public string OperID
        {
            get
            {
                return this.StepID;
            }
        }

        public bool IsFirstResOper;

        public ATCalendar TatCalendar;
        public ATCalendar YieldCalendar;

        public ATRuleSet RuleSet;



        public HashSet<ATResource> Resources { get; private set; }

        public List<ATOperResource> Arranges;

        public APEWipQueue PlanWipQueue;

        public ATOperation(string id, int seq, OperType operType, double waitTAT, double runTAT, double yield, double multiLotSize)
            : base(id)
        {
            this.Sequence = seq;
            this.OperType = operType;
            this.Yield = yield;
            this.CumYield = 1;
            this.MBSValue = multiLotSize;

            this.WaitTat = ATUtil.UomToSecond(waitTAT, ATOption.Instance.TimeUOM);
            this.RunTat = ATUtil.UomToSecond(runTAT, ATOption.Instance.TimeUOM);
            this.TotalTat = waitTAT + runTAT;

            this.Property = new DynamicDictionary();
            this.CalendarInfo = new ATCalendarManager();


            this.TatSets = new Dictionary<string, double>();
            this.YieldSets = new Dictionary<string, double>();

            this.Arranges = new List<ATOperResource>();
            this.Resources = new HashSet<ATResource>();

            this.PlanWipQueue = new APEWipQueue(this);

            this.ConstraintInfos = new List<ATConstraintInfo>();
            this.Constraints = new List<ATConstraint>();

            List<ATProperty> reservedProps = ATInputData.Properties.GetPropertyValueByCategory(PropertyCategory.RoutingOperation.ToString());
            if (reservedProps != null)
                reservedProps.ForEach(x => this.SetProperty(x.PropertyID, x.DefaultValue));
        }

        public dynamic Property { get; internal set; }

        public ATCalendarManager CalendarInfo { get; internal set; }
        public List<ATConstraintInfo> ConstraintInfos { get; set; }
        public List<ATConstraint> Constraints { get; set; }
        public bool HasConstraint { get; set; }

        public virtual void SetProperty(string propertyID, object value)
        {
            this.Property[propertyID] = value;
        }

        public void SetTatCal(ATCalendar calendar)
        {
            if (calendar == null)
                return;

            foreach (ATCalendarDetail detail in calendar.Details.Values)
            {
                List<ATCalendarAttribute> attributes = new List<ATCalendarAttribute>();
                attributes.AddRange(detail.GetAttribte(ATReservedCode.RUNTAT));
                attributes.AddRange(detail.GetAttribte(ATReservedCode.WAITTAT));

                foreach (var attribute in attributes)
                {
                    string key = attribute.ApplyDate + "@" + attribute.Attribute;

                    if (this.TatSets.ContainsKey(key) == false)
                        this.TatSets.Add(key, ATUtil.UomToSecond(attribute.Value, ATOption.Instance.TimeUOM));
                }

                this.TatCalendar = calendar;
            }
        }

        public void SetYieldCal(ATCalendar calendar)
        {
            if (calendar == null)
                return;

            foreach (ATCalendarDetail detail in calendar.Details.Values)
            {
                var attributes = detail.GetAttribte(calendar.CalendarType);

                foreach (var attribute in attributes)
                {
                    string key = attribute.ApplyDate;

                    if (this.YieldSets.ContainsKey(key) == false)
                        this.YieldSets.Add(key, attribute.Value);
                }

                this.YieldCalendar = calendar;
            }
        }

        public void SetCalendar(string propertyID, ATCalendar calendar)
        {
            this.CalendarInfo.AddCalendar(calendar);
        }

      

        public string GetRuleSetID()
        {
            if (this.IsBuffer == false)
            {
                string key = this.Route.RouteID + "@" + this.OperID;
                return key;
            }

            return this.OperID;
        }

        public double GetTat(DateTime dateTime, bool isOut)
        {
            double tat = isOut ? this.RunTat : this.WaitTat;

            if (ATOption.Instance.DiscardCalendarTat == true)
                return tat;
            
            string subAttribute = isOut ? ATReservedCode.RUNTAT : ATReservedCode.WAITTAT;
            
            string key = dateTime.ToString(ATUtil.DateFormat) + "@" + subAttribute;
            if (this.TatSets.TryGetValue(key, out double value))
                tat = value;

            return tat;
        }

        public double GetYield(DateTime dateTime)
        {
            double yield = this.Yield;

            if (ATOption.Instance.DiscardCalendarYield == true)
                return yield;

            string key = dateTime.ToString(ATUtil.DateFormat);
            if (this.YieldSets.TryGetValue(key, out double value))
                yield = value;

            if (yield <= 0)
                yield = 1;

            return yield;
        }

        #region ToString

        public override string ToString()
        {
            return base.ToString();
        }

        public int Compare(ATOperation x, ATOperation y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            int cmp = x.Sequence.CompareTo(y.Sequence);

            return cmp;
        }

        #endregion //ToString

    }

}
