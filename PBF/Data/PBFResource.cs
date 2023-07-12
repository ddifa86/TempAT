using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Manager;
using Mozart.SeePlan.TimeLibrary;
using Mozart.Simulation.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public class PBFResource : IAPEQueueManager, IBucket, IFactorObject, IConstraintManager
    {
        #region Values

        public APECapacityManager CapacityManager { get; set; }

        public APECapacity CurrentCapaInfo { get; set; }

        public ATOffTimeManager OffTimeManager { get; set; }

        public ATPMManager PMManager { get; set; }

        public bool IsInfinite { get; private set; }

        public PBFResourceGroup BucketGroup { get; internal set; }

        public List<ATConstraintDetail> CurrentConstraintDetails { get; internal set; }

        public bool IsFirstAllocate { get; internal set; }
        #endregion

        #region properties
        public DateTime StartTime
        {
            get
            {
                return this.CurrentCapaInfo.StartTime;
            }
        }

        public DateTime EndTime
        {
            get
            {
                return this.CurrentCapaInfo.EndTime;
            }
        }

        public List<APEPlanInfo> Plans { get; private set; }

        public List<APEPlanInfo> CapaPlans { get; private set; }

        public APEPlanInfo LastPlan { get; set; }

        public APEPlanInfo LastCapaPlan { get; set; }

        public ATCalendarManager CalendarInfo
        {
            get
            {
                return this.Target.CalendarInfo;
            }
        }

        public DateTime CurrentTime
        {
            get
            {
                if (this.CurrentCapaInfo is ATTimeCapacity)
                    return (this.CurrentCapaInfo as ATTimeCapacity).CurrentTime;
                
                return FWFactory.Instance.NowDT;

            }
            set
            {
                if (this.CurrentCapaInfo is ATTimeCapacity)
                    (this.CurrentCapaInfo as ATTimeCapacity).CurrentTime = value;
            }
        }

        public bool IsTimeCapa
        {
            get
            {
                return this.CapacityType == CapacityType.Time;
            }
        }

        public bool IsQtyCapa
        {
            get
            {
                return this.CapacityType == CapacityType.Quantity;
            }
        }

        public bool IsCountCapa
        {
            get
            {
                return this.CapacityType == CapacityType.Count;
            }
        }

        public APELot LastAllocatedLot { get; private set; }

        #endregion

        #region 생성자
        public PBFResource(ATResource resource)
        {
            //IBucket
            this.Target = resource;
            this.CapacityManager = new APECapacityManager(this, this.CapacityType);

            //Values
            this.OffTimeManager = new ATOffTimeManager(this);
            this.PMManager = new ATPMManager(this);
            this.IsInfinite = resource.IsInfinite;

            //IAPEQueueManager
            this.Queue = new FWWipQueue();

            //IFactorObject
            this.FactorInfos = new Dictionary<string, ATFactorValue>();
            this.FilterInfos = new Dictionary<string, ATFilterValue>();
            this.FactorValues = string.Empty;
            this.FilterValues = string.Empty;

            this.Plans = new List<APEPlanInfo>();
            this.CapaPlans = new List<APEPlanInfo>();

            this.ReserveManager = new ReserveManager(this);
            this.SetupManager = new SetupManager(this);
            this.AllocateManager = new AllocateManager(this);

            InitWorkCalendar();

            APEWipAgent.Instance.RegistQueue(this);
        }

        internal PBFResource()
        {
            ATResource resource = new ATResource();

            this.Target = resource;
            this.IsInfinite = true;

            this.CapacityManager = new APECapacityManager(this, this.CapacityType);

            DateTime starttime = ATOption.Instance.PlanStartTime.AddDays(-100);
            DateTime endtime = ATOption.Instance.PlanEndTime.AddDays(100);

            this.OffTimeManager = new ATOffTimeManager(this);
            this.PMManager = new ATPMManager(this);

            ATCapacityInfo capaInfo = new ATCapacityInfo(resource, 86400, starttime, endtime);
            this.CurrentCapaInfo = new ATTimeCapacity(capaInfo, this);

            //IFactorObject
            this.FactorInfos = new Dictionary<string, ATFactorValue>();
            this.FilterInfos = new Dictionary<string, ATFilterValue>();
            this.FactorValues = string.Empty;
            this.FilterValues = string.Empty;

            this.Plans = new List<APEPlanInfo>();
            this.CapaPlans = new List<APEPlanInfo>();

            this.ReserveManager = new ReserveManager(this); // Dummy Bucket이 필요한가??
            this.AllocateManager = new AllocateManager(this);
            this.SetupManager = new SetupManager(this);

            APEWipAgent.Instance.RegistQueue(this);
        }
        #endregion

        #region IBucket

        public ATResource Target { get; private set; }

        public string BucketID
        {
            get
            {
                return this.Target.ResourceID;
            }
        }

        public CapacityType CapacityType
        {
            get
            {
                return this.Target.CapaType;
            }
        }

        // public List<APEPlanInfo> UsedPlanInfos { get; }

        #endregion

        #region IAPEQueueManager
        public string QueueName
        {
            get
            {
                return this.Target.ResourceID;
            }
        }

        public FWWipQueue Queue{ get; private set; }
        #endregion

        #region IFactorObject
        public string FactorObjectKey
        { 
            get
            {
                return this.Target.ResourceID;
            }
        }

        public Dictionary<string, ATFactorValue> FactorInfos { get; set; }

        public Dictionary<string, ATFilterValue> FilterInfos { get; set; }

        public string FactorValues { get; set; }

        public string FilterValues { get; set; }

        public string InitFilterLogs()
        {
            return GetCurrentState();
        }

        public string InitFactorLogs()
        {
            return GetCurrentState();
        }

        internal string GetCurrentState()
        {
            return string.Format("{0},{1},{2}/{3}", this.Target.ResourceID, this.CurrentTime.DbToString(true)
                , this.CurrentCapaInfo.Capacity - this.CurrentCapaInfo.UsedCapa, this.CurrentCapaInfo.Capacity);
        }
        #endregion

        #region Cycle
        public virtual void StartCycle(DateTime now, DateTime nextStartTime)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("Bucket_StartCycle");
            try
            {
                this.IsFirstAllocate = true;
                // 장비의 CapaInfo 정보가 필요한 경우
                // 1. 첫 설정
                // 2. 장비 Rolling 
                if (this.CurrentCapaInfo == null)
                {
                    // 장비의 GetCapaInfo

                    var curCapaInfo = this.CapacityManager.GetCapacity(now, nextStartTime, this.CapacityType); //this.CapaInfos.Where(x => x.StartTime <= now && now < nextStartTime).FirstOrDefault();

                    this.CurrentCapaInfo = curCapaInfo;
                    this.CurrentConstraintDetails = this.GetConstraintDetails(ATUtil.ToDate(now));

                    if (this.IsTimeCapa)
                    {
                        ATTimeCapacity timeCapaInfo = this.CurrentCapaInfo as ATTimeCapacity;

                        timeCapaInfo.UseTo(ATUtil.MaxTime(this.CurrentTime, now));

                        if (timeCapaInfo.CapaInfo.IsNonWorkingCapa == true)
                        {
                            // 연결관계 재정립 -> CurrentCapa가 null이므로, 지나간 capa를 저장해서 지정하도록 해야함
                            //curCapaInfo.Next = lastCapaInfo.Next;
                            //lastCapaInfo.Next.Prev = curCapaInfo;
                            //curCapaInfo.Prev = lastCapaInfo;
                            //lastCapaInfo.Next = curCapaInfo;

                            // timeType인데 Capa Calendar가 정의되지않아 새로 만들어 정의해준경우, NonWorkingTime으로 만들어서 넣어줌
                            this.OffTimeManager.AddOffTime(ATReservedCode.OFF_TIME, now, nextStartTime, LotSplitOption.Split);
                            timeCapaInfo.CurrentTime = nextStartTime;
                        }
                    }

                    this.OffTimeManager.UpdateOffTimePeriods(now, this.EndTime, this.CurrentCapaInfo);
                }
                else
                {
                    if (this.IsTimeCapa)
                    {
                        // 해야하나..?
                        ATTimeCapacity timeCapaInfo = this.CurrentCapaInfo as ATTimeCapacity;
                        timeCapaInfo.UseTo(ATUtil.MaxTime(this.CurrentTime, now));
                    }
                }
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("Bucket_StartCycle");
            }
        }

        public virtual void EndCycle(DateTime now, DateTime nextStartTime)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("Bucket_EndCycle");
            try
            {
                this.DoPM(nextStartTime, true);

                foreach (var planInfo in this.Plans)
                {
                    OutputWriter.Instance.WriteResPlan(planInfo);
                }

                this.Plans = null;
                this.CapaPlans = null;

                this.Plans = new List<APEPlanInfo>();
                this.CapaPlans = new List<APEPlanInfo>();

                //OutputWriter.Instance.WriteCapacityAllocInfo(this, now); 위치를 Executor 마지막으로 수정
                this.CurrentCapaInfo = null;
                this.CurrentConstraintDetails = null;
                FWInterface.BucketControl.OnEndCycle(this, now, nextStartTime);

            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("Bucket_EndCycle");
            }
        }

        #endregion

        #region PM
        internal void DoPM(DateTime datetime, bool isEndCycle = false) 
        {
            // isEndcycle Parameter로 받지 않고 처리하는 방법에 대한 고민이 필요함
            if (this.IsTimeCapa == false)
                return;

            List <ATPMPeriod> pms = this.PMManager.GetCurrPMList(datetime);  // date 이전의 PMList
            List<ATPMPeriod> selectedPMs = this.PMManager.SelectPM(pms, isEndCycle); // pmList에서 pm 선택

            for (int i = 0; i < selectedPMs.Count(); i++)
            {
                ATPMPeriod pm = selectedPMs[i];

                #region 시간보정
                var pmPeriod = (pm.End - pm.Start).TotalSeconds;
                DateTime start = pm.Start;
                DateTime end = pm.End;

                if (pm.PMPolicy != PmPolicy.Fix_None && pm.PMPolicy != PmPolicy.Fix_Split)
                {
                    if (start < this.CurrentTime)
                    {
                        start = this.CurrentTime;
                        end = this.CurrentTime.AddSeconds(pmPeriod);
                    }
                }

                // GetNonWorkingTime
                TimeSpan offTimes = this.GetCumNonWorkingTime(start, this.EndTime < end ? this.EndTime : end, false);
                end = end + offTimes;
                #endregion

                if (pm.PmFlag != PMFlag.Reserved)
                    pm.ExecutedStartTime = start;

                double usedCapa = pmPeriod;
                if (this.EndTime < end) // reserve
                {
                    TimeSpan rollingOffTime = TimeSpan.Zero;
                    rollingOffTime = this.GetCumNonWorkingTimeNoExtend(start, this.EndTime, LotSplitOption.None, false);
                    end = this.EndTime;
                    usedCapa = (end - start).TotalSeconds - rollingOffTime.TotalSeconds;

                    double reserveCapa = pmPeriod - usedCapa;

                    DateTime reservedEnd = end.AddSeconds(reserveCapa);
                    pm.Start = this.EndTime;
                    pm.End = reservedEnd;
                    pm.PmFlag = PMFlag.Reserved;
                }
                else
                {
                    // PM 할당 끝
                    pm.End = end;
                    pm.PmFlag = PMFlag.Executed;
                }

                #region 집계
                this.CurrentCapaInfo.PM += usedCapa;

                (this.CurrentCapaInfo as ATTimeCapacity).UseTo(end);

                // Executed가 아닐때도 매번 Add할 예정
                if (pm.PmFlag == PMFlag.Executed)
                {
                    AllocationInfo allocationInfo = new AllocationInfo(PlanInfoType.PM, pm.ExecutedStartTime, pm.End, pm.End, this, this, 0, 0, 0, 0, 0, 0, null, null);

                    APEPlanInfo pmInfo = new APEPlanInfo(null, allocationInfo);
                    pmInfo.Description = pm.Name + "@" + pm.PMPriority;

                    this.AddPlan(pmInfo, null);
                }
                #endregion

                FWInterface.AllocatorControl.OnPM(this, pm);

                //지나간 PM Update하고 Write -> 수행한건 수행했다고 바로 적자. 수행하지 않은(waited) fixedPM은 Return해서 selectedPMs에 넣기. Fixed가 아니면 지우고 적기.
                List<ATPMPeriod> todoFixedPM = this.PMManager.UpdatePM(pm.End);
                selectedPMs.AddRange(todoFixedPM);
            }
        }
        #endregion

        /// <summary>
        /// fence 내에서 대상 시간 기준 다음 WorkingTime 시작 시간을 반환합니다.
        /// 대상 시간이 WorkingTime에 포함된 경우 현재 시간을 반환합니다.
        /// 현재 Period 내에서 대상 시간 이후 더이상 WorkingTime이 존재하지 않는 경우 EndTime을 반환합니다.
        /// </summary>
        /// <param name="at">대상 시간입니다.</param>
        /// <returns></returns>
        public DateTime GetNextWorkingTime(DateTime at, DateTime fence, bool containPM = true)
        {
            var dt = at;
            var nextSection = this.GetNextNonWorkingPeriod(dt, fence, LotSplitOption.None, containPM);
            while (nextSection != APENonWorkingPeriod.NULL)
            {
                if (nextSection.Start >= fence)
                    break;

                if (dt < nextSection.Start)
                    return dt;

                dt = nextSection.End;
                nextSection = this.GetNextNonWorkingPeriod(dt, fence, LotSplitOption.None, containPM);
            }

            if (dt > fence)
                dt = fence;

            return dt;
        }

        /// <summary>
        /// at, fence사이의 첫번째 OffTime
        /// </summary>
        /// <param name="at"></param>
        /// <param name="fence"></param>
        /// <param name="option"></param>
        /// <param name="containPM"></param>
        /// <returns></returns>
        public APENonWorkingPeriod GetNextNonWorkingPeriod(DateTime at, DateTime fence, LotSplitOption option = LotSplitOption.None, bool containPM = true, bool containFence = false)
        {
            APENonWorkingPeriod offTime = this.OffTimeManager.GetNextNonWorkingPeriod(at, fence, containFence, option);
            if (containPM == false)
                return offTime;

            APENonWorkingPeriod pm = this.PMManager.GetNextNonWorkingPeriod(at, fence, containFence, option);
            APENonWorkingPeriod res = APENonWorkingPeriod.NULL;

            if (offTime == APENonWorkingPeriod.NULL && pm == APENonWorkingPeriod.NULL)
                return res;
            else if (pm == APENonWorkingPeriod.NULL)
                return offTime;
            else if (offTime == APENonWorkingPeriod.NULL)
                return pm;

            if (pm.Overlaps(offTime) == true)
            {
                TimeSpan duration = pm.Duration + offTime.Duration;
                DateTime nonWorkingStart = ATUtil.MinTime(pm.Start, offTime.Start);
                DateTime nonWorkingEnd = nonWorkingStart + duration;
                res = new APENonWorkingPeriod("Merged", nonWorkingStart, nonWorkingEnd);
            }
            else
            {
                res = pm.End > offTime.End ? offTime : pm; // 첫번째 OffTime을 선택
            }

            return res;
        }

        /// <summary>
        /// start, end까지의 nonWorkingTime (OffTime, PM)을 조회,. 해당 nonWorkingTime만큼 늘어난 기간에 겹친 nonWorkingTime까지의 합
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>

        public TimeSpan GetCumNonWorkingTime(DateTime start, DateTime end, bool containPM = true)
        { // NoExtend 와 합치기
            var fence = end;
            var t = start;

            while (t < fence)
            {
                var section = this.GetNextNonWorkingPeriod(t, fence, LotSplitOption.None, containPM);

                if (section == APENonWorkingPeriod.NULL)
                    break;

                if (section.Start >= fence)
                    break;

                if (section.End <= fence)
                {
                    var st = t < section.Start ? section.Start : t;
                    fence = fence.Add(section.End - st);
                }
                else
                {
                    fence = section.End.Add(fence - section.Start);
                }

                t = section.End;
            }

            return fence - end; // 기간 내의 NonWorkingTime만큼 늘려준 fence - 원래의 end (nonWorkingTime)
        }

        /// <summary>
        /// start, end 사이의 offTime의 합 (NonWorkingTime의 기간을 늘려 검색하지 않음)
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public TimeSpan GetCumNonWorkingTimeNoExtend(DateTime start, DateTime end, LotSplitOption option = LotSplitOption.None, bool containPM = true)
        {
            
            TimeSpan res = TimeSpan.Zero;

            DateTime at = start;
            while (at < end)
            {
                var section = this.GetNextNonWorkingPeriod(at, end, option, containPM);

                if (section == APENonWorkingPeriod.NULL)
                    break;

                if (section.Start >= end)
                    break;

                DateTime sectionStart = section.Start < start ? start : section.Start;
                DateTime sectionEnd = section.End < end ? section.End : end;

                if (sectionEnd <= end)
                    at = sectionEnd;

                res = res + (sectionEnd - sectionStart);
            }

            return res;
        }

        /// <summary>
        /// 현재 Period 내에서 대상 시간 기준 다음 NonWorkingTime 구간 시작 시간을 반환합니다. 
        /// 대상 시간이 NonWorkingTime 구간 내에 포함된 경우 현재 시간을 반환합니다.
        /// 현재 Period 내에서 대상 시간 이후 더이상 NonWorkingTime 구간이 존재하지 않는 경우 EndTime을 반환합니다.
        /// </summary>
        /// <param name="at">대상 시간입니다.</param>
        /// <param name="option">검색 대상 작업물 분할 방식입니다.</param>
        /// <returns>다음 NonWorkingTime 구간 시작 시간입니다.</returns>
        public DateTime GetNextNonWorkingTime(DateTime at, DateTime fence, LotSplitOption option)
        {
            var nextSection = this.GetNextNonWorkingPeriod(at, fence, option);
            if (nextSection != APENonWorkingPeriod.NULL)
            {
                var dt = at >= nextSection.Start ? at : nextSection.Start;
                return dt;
            }

            return fence;
        }

        #region workCalendar

        internal void InitWorkCalendar()
        {
            // OffTime 등록 작업
            // SupplyChainMaster (SCM) 캘린더로 관리 를 이용한 설정
            // Site Calendar 관리
            // 장비 그룹 WorkTimeTable 로 관리 
            // ResourceCalendar 로 관리
            if (this.CapacityType == CapacityType.Time)
            {
                //this.WorkCalendar.AddNonWorkingPeriods(this.Target.NonWorkingInfos);
                this.OffTimeManager.AddOffTimes();
                this.PMManager.AddPMInfos();
            }
        }

        public double CalculateCapacity(DateTime from, DateTime to)
        {
            if (this.IsInfinite)
                return double.MaxValue;

            if (this.IsTimeCapa == false)
                return (this.CurrentCapaInfo).Remain;

            var st = from;

            var secs = 0d;

            // 할당가능 시간 정보 산출
            while (st < to)
            {
                var t1 = this.GetNextWorkingTime(st, to);
                if (t1 >= to)
                    break;

                var t2 = this.GetNextNonWorkingTime(t1, to, LotSplitOption.None);
                if (t2 > to)
                    t2 = to;

                secs += (t2 - t1).TotalSeconds;

                st = t2;
            }

            return secs;
        }

        /// <summary>
        /// 기간 내의 NonWorkingTime 정보를 반환합니다.
        /// </summary>
        /// <param name="start">검색 대상의 시작 시간</param>
        /// <param name="end">검색 대상 종료 시간 </param>
        /// <returns></returns>
        public List<APENonWorkingPeriod> GetNonWorkingPeriods(DateTime start, DateTime end)
        {
            SortedList<APENonWorkingPeriod, APENonWorkingPeriod> nonWorkingPeriod = new SortedList<APENonWorkingPeriod, APENonWorkingPeriod>(ATNonWorkingPeriodComparer.Default);

            // nextStartTime이 nonWorkingTime의 start인 것은 가져오면 안되므로 <=를 <로 변경
            var lst = this.OffTimeManager.GetNonWorkingPeriods(start, end);
            foreach (var tmp in lst)
                nonWorkingPeriod.Add(tmp, tmp);
            
            var pmPeriods = this.PMManager.GetNonWorkingPeriods(start, end).Where(x => (x as ATPMPeriod).PMPolicy == PmPolicy.Fix_None || (x as ATPMPeriod).PMPolicy == PmPolicy.Fix_Split).ToList();
            foreach (var tmp in pmPeriods)
                nonWorkingPeriod.Add(tmp, tmp);

            return nonWorkingPeriod.Values.ToList();
        }
        #endregion

        #region Allocate
        public APEPlanInfo DummyAllocated(IAPELot lot, double tat)
        {
            var sampleLot = lot.Sample as APELot;
            DateTime start = this.CurrentTime;
            DateTime lot_end_time = start.AddSeconds(tat);

            LotInfo lotInfo = new LotInfo(sampleLot);

            PBFModuleExecutionInfo executionInfo = ATExecutionContext.Instance.CurrentExecutionInfo as PBFModuleExecutionInfo;

            AllocationInfo allocationInfo = new AllocationInfo(PlanInfoType.Allocate, start, lot_end_time, lot_end_time, this, this, executionInfo.AllocationKey++, 0, lot.Qty, 0, 0, 0, null, null);
            allocationInfo.TotalTat = tat;

            APEPlanInfo info = new APEPlanInfo(lotInfo, allocationInfo);

            sampleLot.CurrentPlanInfo = info;
            sampleLot.AddVirtualLateInfo(LateCategory.Tat, LateReason.DummyOperationLate.ToString(), null, lot.LastStepTime, lot.LastStepTime, null);

            return info;
        }

        public bool CanOperation()
        {
            if (this.IsInfinite)
                return true;
       
            if (this.CurrentCapaInfo.Remain > ATOption.Instance.MinimumAllocationQuantity)
                return true;

            return false;
        }

        public APEPlanInfo Allocated(IBucket mainBucket, IAPELot batch, double allocQty, double usagePer, double flowTime, double utilizationRate, DateTime start , PBFAllocateContext context)
        {
            APELot lot;
            if (batch is APELot)
                lot = batch as APELot;
            else
                lot = (batch as APELotGroup).SampleLot;

            DateTime bucket_end_time = this.CurrentTime;
            DateTime lot_end_time = this.CurrentTime;
            double load = allocQty * usagePer / utilizationRate;

            if (this.Target.ResType == ResourceType.Batch)
                load = context.BatchTime;

            if (this.IsTimeCapa == true)
            {
                // 1초보다 적은 할당 로드인 경우에는, 1초로 보정.
                double loadTime = Math.Max(1, load);
                if (usagePer == 0)
                    loadTime = 0;

                (this.CurrentCapaInfo as ATTimeCapacity).UseTo(start);
                
                bucket_end_time = start.AddSeconds(loadTime);
                lot_end_time = bucket_end_time.AddSeconds(flowTime);

                // NonworkingTime이 고려된 시간정보 반환.
                // start ~ end 사이 구간에 존재한 NonWorkingTime으로 인해 지연되어 하는 시간 정보를 반환
                if (this.IsInfinite == false)
                {
                    TimeSpan sumNonWorkingTime = this.GetCumNonWorkingTime(start, bucket_end_time);
                    bucket_end_time = bucket_end_time.Add(sumNonWorkingTime);
                    lot_end_time = lot_end_time.Add(sumNonWorkingTime);
                }
            }
            else if (this.IsQtyCapa == true)
            {
                bucket_end_time = start;
                lot_end_time = start;
            }
            else if (this.IsCountCapa == true)
            {
                load = 1;
            }

            double allocationKey = context.AllocationLog.AllocationSeq;
            
            LotInfo lotInfo = lot.CurrentPlanInfo.LotInfo;
            AllocationInfo allocationInfo = new AllocationInfo(PlanInfoType.Allocate, start, lot_end_time, bucket_end_time, this, mainBucket, allocationKey, context.Level, allocQty, load, usagePer, utilizationRate, this.CurrentCapaInfo, context.AllocationLog);

            APEPlanInfo info = new APEPlanInfo(lotInfo, allocationInfo);

            this.CurrentCapaInfo.UsedCapa += load;

            if(this.IsTimeCapa == true)
                (this.CurrentCapaInfo as ATTimeCapacity).UseTo(bucket_end_time);

            return info;
        }
        #endregion

        #region ReserveManager
        public ReserveManager ReserveManager { get; set; }
        #endregion

        #region SetupManager
        public SetupManager SetupManager { get; set; }
        #endregion

        #region AllocateManager
        public AllocateManager AllocateManager { get; set; }

        public bool ProcessRemainLot(IAPELot lot, APELotGroup lotGroup, PBFAllocateContext context)
        {
            return this.AllocateManager.ProcessRemainLot(lot, lotGroup, context);
        }
        #endregion

        public void AddPlan(APEPlanInfo info, IAPELot lot)
        {
            info.PrevPlanInfo = this.LastPlan; // this.Plans.LastOrDefault();
            this.Plans.Add(info);
            this.LastPlan = info;

            if (info.Type == PlanInfoType.Allocate)
            {
                this.CapaPlans.Add(info);
                this.LastCapaPlan = info;

                this.LastAllocatedLot = (lot.Sample as APELot).DeepCopy();
            }
        }
    }
}
