using Mozart.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    [Mozart.Task.Execution.FEBaseClassAttribute(Root = "PKG", Category = "PKG", IsTypeBinding = true, Mandatory = true, Description = null)]
    public partial class ATDemand : IPropertyObject, IConstraint
    {
        #region Property

        public string StageID { get; private set; }

        public string ID { get; private set; }

        public string SiteID { get; private set; }

        public DateTime DueDateTime { get; private set; }

        public DateTime MaxLateDueDateTime
        {
            get
            {
                return DueDateTime.AddDays(this.MaxLateDays);
            }
        }
        /// <summary>
        /// 후행 가능 시간 정보
        /// </summary>
        public double MaxLateDays { get; private set; }

        /// <summary>
        /// 선행 가능 시간 정보
        /// </summary>
        public double MaxEarlyDays { get; private set; }

        public double Qty { get; internal set; }

        public int Priority { get; private set; }

        public string CustomerID { get; private set; }

        public ATItemSiteBuffer ItemSiteBuffer { get; private set; }

        public ATCustomer Customer { get; private set; }

        public string IsRtfTarget { get; internal set; }

        public string ItemSiteBufferKey
        {
            get
            {
                return ItemSiteBuffer.Key;
            }
        }

        private ATDemand _soDemand;
        public ATDemand SoDemand
        {
            get
            {
                if (_soDemand == null)
                {
                    return this;
                }
                return _soDemand;
            }
            set
            {
                _soDemand = value;
            }
        }

        public ATMoPlan Moplan { get; set; }

        public string ItemID
        {
            get
            {
                return this.ItemSiteBuffer.ItemID;
            }
        }

        public string BufferID
        {
            get
            {
                return this.ItemSiteBuffer.BufferID;
            }
        }

        public string DemandType;

        public string Date { get { return ATUtil.ToDate(this.DueDateTime, true); } }

        public string Week { get { return ATUtil.ToWeek(this.DueDateTime, true); } }

        public string Month { get { return ATUtil.ToMonth(this.DueDateTime, true); } }

        public ATLateInfoManager LateInfoManager { get; set; }

        public ATShortManager ShortManager { get; set; } // 추후 LateInfoManager와 통합 가능

        public HashSet<string> SoPathLog { get; set; }

        public bool IsWritePeggableWip { get; set; }

        public DateTime CalcDueDateTime
        {
            get 
            {
                return this.DueDateTime.AddHours(FactoryConfiguration.Current.StartTime.Hours).AddSeconds(86399);
            }
        }
        #endregion

        public Dictionary<DateTime, double> ShipmentPlans = new Dictionary<DateTime, double>();


        public ATDemand(string stageID, string soID, string siteID, ATItemSiteBuffer itemBuffer, ATCustomer customer, DateTime duedate, double qty, int priority, string customerID, double maxlateday, double maxearlyday)
        {
            this.StageID = stageID;
            this.ID = soID;
            this.SiteID = siteID;
            this.ItemSiteBuffer = itemBuffer;
            this.DueDateTime = duedate;
            this.Qty = qty;
            this.Priority = priority;
            this.Customer = customer;
            this.CustomerID = customerID;

            this.MaxLateDays = maxlateday;
            this.MaxEarlyDays = maxearlyday;
            this.IsWritePeggableWip = false;

            this.IsRtfTarget = "Y";

            this.Property = new DynamicDictionary();
            this.CalendarInfo = new ATCalendarManager();

            this.ConstraintInfos = new List<ATConstraintInfo>();
            this.Constraints = new List<ATConstraint>();
            this.LateInfoManager = new ATLateInfoManager(this);
            this.ShortManager = new ATShortManager();
            this.SoPathLog = new HashSet<string>();

            List<ATProperty> reservedProps = ATInputData.Properties.GetPropertyValueByCategory(PropertyCategory.SalesOrder.ToString());
            if (reservedProps != null)
                reservedProps.ForEach(x => this.SetProperty(x.PropertyID, x.DefaultValue));
        }

        #region DataObject Interface
        public dynamic Property { get; set; }

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

        #region ToString

        public override string ToString()
        {
            return this.ItemSiteBuffer.Key;
        }

        #endregion //ToString

        public ATDemand Clone()
        {
            var copy = this.MemberwiseClone() as ATDemand;

            return copy;
        }

        public void AddShipmentPlan(APELot lot)
        {
            DateTime planShipDate = lot.LastStepTime.Date.StartTimeOfDayT();

            if (this.StageID != ATExecutionContext.Instance.CurrentStage.StageID)
                return;

            if (this.ShipmentPlans.ContainsKey(planShipDate) == false)
                this.ShipmentPlans.Add(planShipDate, 0);

            this.ShipmentPlans[planShipDate] += lot.CurrentQty;
        }
    }
}
