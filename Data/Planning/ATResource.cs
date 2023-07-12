
using Mozart.SeePlan.Aleatorik.Inputs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    [Mozart.Task.Execution.FEBaseClassAttribute(Root = "PKG", Category = "PKG", IsTypeBinding = true, Mandatory = true, Description = null)]
    public partial class ATResource : IPropertyObject, IConstraint
    {
        public string StageID { get; private set; }

        public string ResourceID { get; private set; }

        /// <summary>
        /// Additional 장비 여부
        /// </summary>
        public ResourceCategory ResCategory { get; private set; }

        /// <summary>
        /// 장비 타입 (Inline, Table, Batch, Dummy, None)
        /// </summary>
        public ResourceType ResType { get; private set; }

        public string SiteID { get; private set; }

        public bool IsInfinite { get; internal set; }

        public ATNonWorkingInfoManager OffTimeInfoManager { get; internal set; }

        public ATNonWorkingInfoManager PMTimeInfoManager { get; internal set; }

        /// <summary>
        /// Calendar로 인해 주기별 CapaInfo를 관리하는 경우
        /// </summary>
        public List<ATCapacityInfo> CapaInfos;

        /// <summary>
        /// Time, Quantity, Count
        /// </summary>
        public CapacityType CapaType { get; private set; }

        public UomType CapacityPeriod { get; private set; }

        public UomType UomType { get; private set; }

        public ATResourceGroup ResGroup { get; private set; }

        public Dictionary<string, ATOperResource> ArrangeInfos { get; internal set; }

        public bool IsSetupResource 
        {
            get
            {
                return this.SetupInfo != null;
            }
        }

        public ATSetupInfo SetupInfo { get; internal set; }

        public bool HasConstraint { get; set; }

        public List<ATConstraintInfo> ConstraintInfos { get; set; }

        public List<ATConstraint> Constraints { get; set; }

        public double Utilization { get; set; }

        public Dictionary<string, double> UtilizationSets { get; private set; }

        //추후 IResource로 분리 고려
        public string ResGroupID { get; set; }
 
        public ATResource(string resourceID, ResourceCategory category, ResourceType resourcetype, string siteID
            , CapacityType capatype, CapacityMode capaMode, ATResourceGroup resgroup, string resGroupID, double utilRate)
        {
            this.ResourceID = resourceID;
            this.ResCategory = category;
            this.ResType = resourcetype;
            this.SiteID = siteID;
            this.CapaType = capatype;
            this.IsInfinite = capaMode == CapacityMode.Infinite ? true : false;
            this.ResGroup = resgroup;
            this.ResGroupID = resGroupID;
            this.Utilization = utilRate;

            this.Property = new DynamicDictionary();
            this.ArrangeInfos = new Dictionary<string, ATOperResource>();
            this.CalendarInfo = new ATCalendarManager();
            this.CapaInfos = new List<ATCapacityInfo>();
            this.ConstraintInfos = new List<ATConstraintInfo>();
            this.Constraints = new List<ATConstraint>();
            this.UtilizationSets = new Dictionary<string, double>();

            this.OffTimeInfoManager = new ATNonWorkingInfoManager(this, NonWorkingType.OffTime);
            this.PMTimeInfoManager = new ATNonWorkingInfoManager(this, NonWorkingType.PM);

            List<ATProperty> reservedProps = ATInputData.Properties.GetPropertyValueByCategory(PropertyCategory.Resource.ToString());
            if (reservedProps != null)
                reservedProps.ForEach(x => this.SetProperty(x.PropertyID, x.DefaultValue));
        }

        public ATResource(string resourceID, ResourceCategory category)
        {
            this.ResourceID = resourceID;
            this.ResCategory = category;

            // 아래 기본 세팅 중 필요없는 멤버는 세팅 X (예. SetupInfos)
            this.Property = new DynamicDictionary();
            this.ArrangeInfos = new Dictionary<string, ATOperResource>();
            this.CalendarInfo = new ATCalendarManager();
            this.CapaInfos = new List<ATCapacityInfo>();
            this.OffTimeInfoManager = new ATNonWorkingInfoManager(this, NonWorkingType.OffTime);
            this.PMTimeInfoManager = new ATNonWorkingInfoManager(this, NonWorkingType.PM);
            this.Property = new DynamicDictionary();
            this.CalendarInfo = new ATCalendarManager();
            this.UtilizationSets = new Dictionary<string, double>();

            List<ATProperty> reservedProps = ATInputData.Properties.GetPropertyValueByCategory(PropertyCategory.Resource.ToString());
            if (reservedProps != null)
                reservedProps.ForEach(x => this.SetProperty(x.PropertyID, x.DefaultValue));
        }

        internal ATResource()
        {
            this.ResourceID = ResourceType.Dummy.ToString();
            this.ResType = ResourceType.Dummy;

            this.CapaType = CapacityType.None; 
            this.Property = new DynamicDictionary();
            this.CapaInfos = new List<ATCapacityInfo>();
            this.OffTimeInfoManager = new ATNonWorkingInfoManager(this, NonWorkingType.OffTime);
            this.PMTimeInfoManager = new ATNonWorkingInfoManager(this, NonWorkingType.PM);
            this.Property = new DynamicDictionary();
            this.CalendarInfo = new ATCalendarManager();
            this.UtilizationSets = new Dictionary<string, double>();
        }

        #region DataObject Interface
        public dynamic Property { get; internal set; }

        public ATCalendarManager CalendarInfo { get; internal set; }

        public void SetProperty(string propertyID, object value)
        {
            this.Property[propertyID] = value;
        }

        public void SetSetupInfo(ATSetupInfo setup)
        {
            if (setup == null)
                return;

            this.SetupInfo = setup;
        }


        public void SetPmInfo(ATCalendar calendar)
        {
            if (calendar == null)
                return;

            // this.PMTimeInfoManager.SetPMPlans(calendar);
            List<ATCalendarAttribute> attrs = new List<ATCalendarAttribute>();

            attrs.AddRange(calendar.GetAttributes(ATReservedCode.PM));

            attrs = attrs.OrderBy(x => x.EffectiveStartTime).ToList();

            foreach (var attr in attrs)
            {
                string attrValue = Convert.ToString(attr.Value);
                string policy = attrValue.Split('@')[1];
                string policyValue = attrValue.Split('@')[2];
                int pmStartHour = attrValue.Split('@')[3] == "" ? 0 : Convert.ToInt32(attrValue.Split('@')[3].ToString());

                DateTime start = attr.EffectiveStartTime.Date.AddHours(pmStartHour);


                LotSplitOption splitOption = policy.Contains("Split") ? LotSplitOption.Split : LotSplitOption.KeepAndDelay;
                double pmTime = attr.Value == null ? 0 : Convert.ToDouble(attrValue.Split('@')[0]);

                DateTime end = attr.EffectiveEndTime;

                if (attr.Detail.PatternType == CalendarPatternType.EveryNDays && string.IsNullOrEmpty(attr.Detail.PatternValue) == true)
                    end = attr.EffectiveEndTime;
                else
                    end = start.AddHours(pmTime);

                // PMTime이 30시간이면?? 다음날까지 지정이 안될것으로 보이는데 ?
                //if (end > attr.EffectiveEndTime)
                //    end = attr.EffectiveEndTime;

                    int pmPriority = Convert.ToInt32(attr.PatternSeq);
                    PmPolicy pmPolicy = ATUtil.StringToEnum<PmPolicy>(policy, PmPolicy.Fix_None);
                    double pmParameter = string.IsNullOrEmpty(policyValue) ? 0 : Convert.ToDouble(policyValue);

                PMTimeInfoManager.AddPMInfo(start, end, attr, calendar.CalendarID, splitOption, pmPriority, pmPolicy, pmParameter);

            }
        }
        public void SetCalendar(string propertyID, ATCalendar calendar)
        {
            this.CalendarInfo.AddCalendar(calendar);
            
        }

        public double GetUtilization(DateTime dateTime)
        {
            double utilization = this.Utilization;

            string key = dateTime.ToString(ATUtil.DateFormat);
            if (this.UtilizationSets.TryGetValue(key, out double value))
                utilization = value;

            return utilization;
        }

        public void SetUtilRateCal(ATCalendar calendar)
        {
            if (calendar == null)
                return;

            foreach (ATCalendarDetail detail in calendar.Details.Values)
            {
                var cal = detail.GetAttribte(calendar.CalendarType);

                foreach (var info in cal)
                {
                    string key = info.ApplyDate;
                    if (this.UtilizationSets.ContainsKey(key) == false)
                    {
                        double value = info.Value;
                        if (value > 1) value = 1;
                        else if (value < 0) value = 0;

                        this.UtilizationSets.Add(key, value);
                    }

                }
            }
        }

        public void CreateCapaInfo(ATCalendar calendar)
        {
            if (calendar == null)
                return;

            List<ATCalendarAttribute> attrs = new List<ATCalendarAttribute>();

            #region Get Attribute
            if (this.CapaType == CapacityType.Time)
            {
                // WorkTime은 ApplyDate의 Priority 첫번째 Attr을 적용
                attrs = calendar.GetAttributes(ATReservedCode.WORK_TIME)
                                .GroupBy(x => x.ApplyDate)
                                .ToDictionary(g => g.Key, g => g.OrderBy(r => r.Priority).First())
                                .Values.ToList();

                // OffTime은 중복과 관계없이 모든 Attr을 적용
                attrs.AddRange(calendar.GetAttributes(ATReservedCode.OFF_TIME));

                attrs = attrs.OrderBy(x => x.EffectiveStartTime).ToList();
            }
            else if (this.CapaType == CapacityType.Quantity || this.CapaType == CapacityType.Count)
            {
                // ApplyDate의 Priority 첫번째 Attr을 적용
                attrs = calendar.GetAttributes(ATReservedCode.CAPACITY)
                                .OrderBy(x => x.ApplyDate)
                                .GroupBy(x => x.ApplyDate)
                                .ToDictionary(g => g.Key, g => g.OrderBy(r => r.Priority).First())
                                .Values.ToList();

                attrs = attrs.OrderBy(x => x.EffectiveStartTime).ToList();
            }
            #endregion

            #region Set nonWorkingTime / Capainfos
            foreach (var attr in attrs)
            {
                if (attr.Attribute == ATReservedCode.OFF_TIME)
                {
                    if (this.ResCategory == ResourceCategory.AddResource)
                        continue;

                    var now = attr.EffectiveStartTime;
                    //now = ShopCalendar.StartTimeOfDay(now);

                    string formats = Convert.ToString(attr.Value);

                    var timeInfos = formats.Split(';');

            foreach (string timeinfo in timeInfos)
            {
                var infos = timeinfo.Split(',');
                if (infos.Count() < 4)
                {
                    continue;
                }

                        string name = string.IsNullOrEmpty(infos[2]) == false ? infos[2] : attr.Attribute;

                        LotSplitOption option = LotSplitOption.KeepAndDelay;
                        if (infos[3] == "Y")
                            option = LotSplitOption.Split;

                        if (TimeSpan.TryParse(infos[0], out TimeSpan startTime) == false)
                            continue;

                        if (TimeSpan.TryParse(infos[1], out TimeSpan endTime) == false)
                            continue;

                        // endTime에 24:00:00 들어오면, 24일로 설정되어서, 하루로 보정해줌
                        endTime = endTime >= TimeSpan.Parse("1.00:00:00") ? TimeSpan.Parse("1.00:00:00") : endTime;

                        DateTime start = now.Add(startTime);
                        DateTime end = now.Add(endTime);

                        OffTimeInfoManager.AddOffTimeInfo(start, end, attr, name, option);
                    }
                }
                else if (attr.Attribute == ATReservedCode.WORK_TIME)
                {
                    SetWorkingTime(attr);
                }
                else if (attr.Attribute == ATReservedCode.CAPACITY)
                {
                    SetCapainfos(attr);
                }
            }
            #endregion
        }

        #region SetCapa
       
        private void SetWorkingTime(ATCalendarAttribute attr)
        {
            var factoryStartTime = attr.EffectiveStartTime;
            //factoryStartTime = ShopCalendar.StartTimeOfDay(factoryStartTime);

            var factoryEndTime = attr.EffectiveEndTime;
            //factoryEndTime = ShopCalendar.StartTimeOfDay(factoryEndTime);

            /// 10:00:00,20:00:00 형태로만 입력됨.
            /// 시작시간의 상대적인 시간 정보
            string timeinfo = Convert.ToString(attr.Value);

            var start_end_time = timeinfo.Split(',');

            // 입력값이 잘못된 경우 예외처리.
            if (start_end_time.Count() < 2)
                return;

            if (TimeSpan.TryParse(start_end_time[0], out TimeSpan startTime) == false)
                return;

            if (TimeSpan.TryParse(start_end_time[1], out TimeSpan endTime) == false)
                return;

            if (attr.Detail.PatternType != CalendarPatternType.EveryNDays && this.ResCategory != ResourceCategory.AddResource)
            {
                // endTime에 24:00:00 들어오면, 24일로 설정되어서, 하루로 보정해줌
                endTime = endTime >= TimeSpan.Parse("1.00:00:00") ? TimeSpan.Parse("1.00:00:00") : endTime;

                var workStartTime = factoryStartTime.Add(startTime);
                var workEndTime = factoryStartTime.Add(endTime);

                // 24시간 수행되지 않는 공장의 경우, 일하지 않는 시간을 NonWorkingTime으로 등록.
                if (factoryStartTime != workStartTime)
                {
                    this.OffTimeInfoManager.AddOffTimeInfo(factoryStartTime, workStartTime, attr, ATReservedCode.OFF_TIME, LotSplitOption.Split);
                }

                if (factoryEndTime != workEndTime)
                {
                    this.OffTimeInfoManager.AddOffTimeInfo(workEndTime, factoryEndTime, attr, ATReservedCode.OFF_TIME, LotSplitOption.Split);
                }
            }

            double capacity = (factoryEndTime - factoryStartTime).TotalSeconds;

            ATCapacityInfo capa = new ATCapacityInfo(this, capacity, factoryStartTime, factoryEndTime, attr);
            this.AddCapacity(capa);
        }

        private void SetCapainfos(ATCalendarAttribute attr)
        {
            double capacity = attr.Value;

            var startTime = attr.EffectiveStartTime;  // applyDate.StartTimeOfDay();
            var endTime = attr.EffectiveEndTime; //startTime.AddDays(period);

            ATCapacityInfo capa = new ATCapacityInfo(this, capacity, startTime, endTime, attr);

            this.AddCapacity(capa);
        }
        #endregion


        #endregion

        public override string ToString()
        {
            return ATUtil.CreateKey(this.SiteID, this.ResourceID);
        }

        public void AddArrange(ATOperResource arrange)
        {
            string key = ATUtil.CreateKey(arrange.RouteID, arrange.OperID);

            if (this.ArrangeInfos.ContainsKey(key) == false)
            {
                this.ArrangeInfos.Add(key, arrange);
            }
        }

        public ATOperResource GetArrange(string routingID, string operID)
        {
            string key = ATUtil.CreateKey(routingID, operID);
            ATOperResource info;

            if (this.ArrangeInfos.TryGetValue(key, out info) == false)
                return null;

            return info;
        }

        /// <summary>
        /// Capa 정보 등록
        /// </summary>
        /// <param name="info"></param>
        public void AddCapacity(ATCapacityInfo info)
        {
            CapaInfos.Add(info);
        }
    }
}

