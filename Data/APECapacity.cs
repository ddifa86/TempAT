using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class APECapacity: IPropertyObject
    {
        public ATCapacityInfo CapaInfo { get; } // 정적 Data

        public APECapacity Next { get; set; }

        public APECapacity Prev { get; set; }

        public double VirtualUsedQty { get; set; }

        public double Capacity { get; set; }

        public double UsedCapa { get; set; }

        public Dictionary<string, double> Infos { get; set; }

        public double PM { get; set; }

        public double SetupTime { get; set; }

        public double UsedRatio
        { 
            get
            {
                if (UsedCapa <= 0)
                    return 0;

                return Math.Round((UsedCapa / Capacity) * 100, 4);
            }
        }

        public List<APEPlanInfo> UsedPlanInfos { get; set; }

        public double OffTime { get; set; }

        public virtual double Remain 
        {
            get
            {
                return this.Capacity - this.UsedCapa;
            }
        }

        public IBucket Bucket { get; set; }

        public DateTime StartTime { get; private set; }

        public DateTime EndTime { get; private set; }

        public APECapacity(ATCapacityInfo info, IBucket bucket)
        {
            this.CapaInfo = info;
            this.Capacity = info.Capacity;
            this.UsedCapa = 0;
            this.Bucket = bucket;

            this.Property = new DynamicDictionary();
            this.CalendarInfo = new ATCalendarManager();
            this.UsedPlanInfos = new List<APEPlanInfo>();

            this.StartTime = ShopCalendar.GetShiftStartTime(info.StartTime, 1); // .StartTimeOfDayT(info.StartTime);
            this.EndTime = ShopCalendar.GetShiftStartTime(info.EndTime, 1);
        }

        #region IPropertyObject
        public dynamic Property { get; }

        public ATCalendarManager CalendarInfo { get; }

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
