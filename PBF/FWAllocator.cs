using Mozart.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Manager;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public sealed partial class FWAllocator
    {
        public FWLine Line { get; internal set; }

        public FWFactory Factory
        {
            get { return this.Line.Factory; }
        }

        public FWAllocGroup AllocGroup { get; private set; }

        public DateTime NowDt { get; set; }

        public DateTime NextDt { get; private set; }

        public ATRuleSet RuleSet => ATRuleAgent.Instance.CurrentRuleSet;

        public PBFAllocateContext CurrentContext { get; private set; }

        public PBFModuleExecutionInfo CurrentExecutionInfo
        {
            get
            {
                return ATExecutionContext.Instance.CurrentExecutionInfo as PBFModuleExecutionInfo;
            }
        }
        public FWAllocator(FWLine line, FWAllocGroup group)
        {
            // 해당 부분은 추후 Line이 가질 수있도록 소스 수정 필요.
            this.NowDt = line.Factory.NowDT.StartTimeOfDayT();
            
            this.NextDt = line.Factory.NextStartTime;

            this.Line = line;
            this.AllocGroup = group;

            DoAllocateReservedLot();

            var buckets = this.AllocGroup.Buckets;

            // 소스리뷰 method로 분리
            foreach (var bucket in buckets)
            {
                bucket.DoPM(bucket.CurrentTime);
            }

            // RuleSet 설정에 따라서
            // 불필요한 소팅일수 있음 룰셋을 보고 소팅 여부 판단.
            // 옵션이 필요함. LotFirst or bucketFirst인지..
            if (ATOption.Instance.ApplyReSortLotInGroup)
                SortedBucketGroupQueue();

            FWInterface.AllocatorControl.OnInitialize(this, this.AllocGroup);
        }

        private void DoAllocateReservedLot()
        {
            var buckets = this.AllocGroup.Buckets;

            foreach (var bucket in buckets)
            {
                if (bucket.ReserveManager.IsReserved)
                {
                    if (bucket.Target.ResCategory != ResourceCategory.Resource)
                        continue;

                    var infos = bucket.ReserveManager.GetReserveInfos();

                    // 예약 할당 실패하면 다음 Lot은 예약 할당을 시도하지 않음
                    while (infos.Count() > 0)
                    {
                        if (bucket.CanOperation() == false)
                            break;

                        var info = bucket.ReserveManager.RemoveReserveInfo();
                        if (info == null)
                            break;

                        IAPELot targetLot = info.ReservedLot;
                        
                        APELotGroup lotGroup;
                        if (targetLot is APELot)
                        {
                            lotGroup = new APELotGroup(targetLot.LotGroupKey);
                            lotGroup.AddLot(targetLot, FWFactory.Instance.DefaultLotInGroupPreset);
                        }
                        else
                        {
                            lotGroup = targetLot as APELotGroup;
                        }

                        PBFAllocateContext context = new PBFAllocateContext(this, this.AllocGroup, AllocateType.Reserve, lotGroup.SampleLot.CurrentPlanInfo.Level);

                        context.SetDefaultContextPreset();
                        SetReserveInfo(info, lotGroup, context);

                        if (DoAllocate(lotGroup, bucket, context) == false)
                        {
                            // 예약 할당이 실패한 경우 우선 예약
                            if (targetLot.IsReserved == false)
                                bucket.ReserveManager.ReserveLot(lotGroup, true, context.AddArrInfos, context.SetupInfo);

                            break;
                        }
                    }
                }
            }
        }

        private void SetReserveInfo(ReserveInfo info, APELotGroup lotGroup, PBFAllocateContext context)
        {
            context.SelectedLotGroup = lotGroup;
            context.AllocationLog = lotGroup.SampleLot.CurrentPlanInfo.AllocationLog;
            context.AddArrInfos = info.AddArrInfos;
            context.AddBuckets = info.AddArrInfos.Select(x => x.Bucket).ToList();
            context.SetupInfo = info.SetupInfo;
        }

        private void SortedBucketGroupQueue()
        {
            var bucketGroups = this.AllocGroup.BucketGroups;
            foreach (var bucketGroup in bucketGroups)
            {
                // BucketGroup 내 LotGroup 추출
                var lotgroups = bucketGroup.Queue.GetLotGroups();

                // 옵션에 따라서 Sorting을 항상...
                lotgroups.ForEach(x => AllocatorLogic.Instance.SortedLotInGroup(x));
            }
        }

        private void SortedBucketQueue()
        {
            var buckets = this.AllocGroup.Buckets;
            foreach (var bucket in buckets)
            {
                var lotgroups = bucket.Queue.GetLotGroups();
                lotgroups.ForEach(x => AllocatorLogic.Instance.SortedLotInGroup(x));
            }
        }

        public void DoRun()
        {
            // RuleSet 정보 설정
            int maxLevel = FWFactory.Instance.DefaultRuleSet.Level;

            for (int curLevel = 1; curLevel <= maxLevel; curLevel++)
            {
                try
                {
                    PBFAllocateContext context = new PBFAllocateContext(this, this.AllocGroup, this.AllocGroup.AllocateType, curLevel);
                    this.CurrentContext = context;

                    FWInterface.AllocatorControl.OnStartLevel(curLevel, maxLevel, context);

                    context.SetDefaultContextPreset();

                    if (context.AllocateType == AllocateType.LotFirstSelection)
                        LotFirstSelected(context);
                    else
                        ResourceFirstSelected(context);

                    FWInterface.AllocatorControl.OnEndLevel(curLevel, maxLevel, context);
                }
                finally
                {
                    this.CurrentContext = null;
                }
            }
        }

        // LFS
        internal void LotFirstSelected(PBFAllocateContext context)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("Allocate_LotFirstSelected");
            try
            {
                // AllocGroup 내 Bucket 그룹 소팅
                var bucketGroups = context.SelectedAllocGroup.BucketGroups;

                foreach (var bucketGroup in bucketGroups)
                {
                    if (bucketGroup.Target.ResourceGroupType != ResourceCategory.Resource)
                        continue;

                    context.UpdateContextPreset(bucketGroup, AllocateType.LotFirstSelection);

                    // BucketGroup 내 LotGroup 추출
                    var lotgroups = bucketGroup.Queue.GetLotGroups();

                    int initLotGroupCnt = lotgroups.Count();
                    if (initLotGroupCnt == 0)
                        continue;
 
                    context.SelectedBucketGroup = bucketGroup;

                    // Context
                    FWInterface.AllocatorControl.OnPrepareAllocate(bucketGroup, context);

                    lotgroups.ForEach(x => x.InitFactorValue());

                    // 장비그룹 내 장비 가용확인 체크 => 
                    List<APELotGroup> filterLotGroups = new List<APELotGroup>();
                    var selectedLotGroups = GetFilterLotGroupInLevel(lotgroups, filterLotGroups, context);
                    selectedLotGroups.Sort(new FWLotGroupComparer(context, context.CompareLotGroupOnLFSPreset));

                    context.SelectedLotGroupInLevel = selectedLotGroups;

                    // FilterLotGroupInLevel 단계에서 필터된 정보가 있으면
                    if (filterLotGroups.Count() > 0)
                    {
                        var allocLog = new PBFAllocationLog(bucketGroup);
                        context.AllocationLog = allocLog;

                        allocLog.FilterLotGroups = filterLotGroups;
                        allocLog.CandidatedLotGroups.AddRange(context.SelectedLotGroupInLevel);
                        allocLog.AllocationSeq = CurrentExecutionInfo.AllocationKey++;
                        allocLog.LogType = "FilterLotGroupInLevel";
                        OutputWriter.Instance.WriteAllocationInfo(context, allocLog);
                    }

                    while (selectedLotGroups.Count() > 0)
                    {
                        var selectedlotgroup = selectedLotGroups.First();
                        selectedLotGroups.Remove(selectedlotgroup);

                        #region AllocLog 설정 부분
                        var allocLog = new PBFAllocationLog(bucketGroup);
                        context.AllocationLog = allocLog;

                        allocLog.CandidatedLotGroups.AddRange(context.SelectedLotGroupInLevel);
                        allocLog.AllocationSeq = CurrentExecutionInfo.AllocationKey++;
                        allocLog.LogType = "FilterResource";
                        #endregion

                        // 반복되는 과정에서 filter 조건이 맞아지는 경우.
                        if (AllocatorLogic.Instance.FilterLotGroupOnLFS(selectedlotgroup, context))
                        {
                            allocLog.FilterLotGroups.Add(selectedlotgroup);
                            allocLog.LogType = "FilterLotGroup";
                            OutputWriter.Instance.WriteAllocationInfo(context, allocLog);
                            continue;
                        }

                        allocLog.SetAllocatedLotGroup(selectedlotgroup);
                        context.AllocationLog = allocLog;
                        
                        var loadableBuckets = AllocatorLogic.Instance.GetLoadableBuckets(bucketGroup, selectedlotgroup, context);
                        loadableBuckets.Sort(new APEBucketComparer(context, selectedlotgroup));

                        // 아래 FEAction은 사용중인 사이트 확인 후 없으면 삭제
                        loadableBuckets = FWInterface.AllocatorControl.GetLoadableBuckets(loadableBuckets, selectedlotgroup, context);


                        if (loadableBuckets == null)
                            loadableBuckets = new List<PBFResource>();

                        allocLog.SelectedBuckets = loadableBuckets;

                        // 할당이 조금이라도 된 경우
                        bool isSuccess = false;
                        bool isStopAllocateLotGroup = false;

                        // for문으로 변경...
                        for (int i = 0; i < loadableBuckets.Count(); i++)
                        {
                            var bucket = loadableBuckets[i];
                            if (DoAllocate(selectedlotgroup, bucket, context))
                            {
                                isSuccess = true;

                                allocLog.LoadedBuckets.Add(bucket.BucketID);
                                allocLog.LogType = "Allocate";
                            }

                            // 모두 할당된 경우.
                            if (selectedlotgroup.CanAllocate() == false)
                                break;

                            if (FWInterface.AllocatorControl.IsStopAllocateLotGroup(loadableBuckets, selectedlotgroup, context))
                            {
                                isStopAllocateLotGroup = true;
                                break;
                            }

                            // 할당 과정에서 장비를 모두 소진하지 않은 경우.
                            // ex) NonSplit OffTime 등록한 경우에만 해당되는데.. 그 외에는 모두, 인위적으로 Stop 수행된 경우로 판단.
                            // 음.. 조건을 뽑아줘야하나...?
                            //if (isAlloc && bucket.CanOperation())
                            //{
                            //    loadableBucket.AddSort(bucket, new APEBucketComparer(context, selectedlotgroup));
                            //}
                        }

                        OutputWriter.Instance.WriteAllocationInfo(context, allocLog);

                        if (selectedlotgroup.Qty >= ATOption.Instance.MinimumAllocationQuantity)
                        {
                            var lots = FWInterface.AllocatorControl.OnInterceptOnExit(selectedlotgroup, context);
                            foreach (var lot in lots)
                            {
                                selectedlotgroup.RemoveLot(lot);
                                Line.MoveNext(lot);
                            }
                        }

                        // 할당을 성공했는데 수량이 남은 경우 다시 Add =>
                        // 할당을 하지 않은 경우에는 현 Phase에서는 할당할 일이 없음을 의미.
                        if (isSuccess && selectedlotgroup.CanAllocate() && isStopAllocateLotGroup == false)
                        {
                            if (selectedlotgroup.HasContents)
                            {
                                if (selectedLotGroups.Contains(selectedlotgroup) == false)
                                    selectedLotGroups.Add(selectedlotgroup);
                            }
                            else
                            {
                                //해당 케이스를 잡아야됨.
                            }
                        }

                        if (isSuccess)
                        {
                            if (bucketGroup.IsReSorting)
                                selectedLotGroups.Sort(new FWLotGroupComparer(context, context.CompareLotGroupOnLFSPreset));
                        }
                        else
                        {
                           
                        }
                    }
                }
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("Allocate_LotFirstSelected");
            }
        }

        private List<APELotGroup> GetFilterLotGroupInLevel(List<APELotGroup> lotGroups, List<APELotGroup> filterLotGroups, PBFAllocateContext context)
        {
            List<APELotGroup> selectedLotGroups = new List<APELotGroup>(); //new FWLotGroupComparer(context)

            foreach (var lotgroup in lotGroups)
            {
                // 현재 Level에서 대상 제거
                if (AllocatorLogic.Instance.FilterLotGroupInLevel(lotgroup, context))
                {
                    filterLotGroups.Add(lotgroup);
                    continue;
                }
                    
                selectedLotGroups.Add(lotgroup);
            }

            return selectedLotGroups;
        }

        // RFS
        internal void ResourceFirstSelected(PBFAllocateContext context)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("Allocate_ResourceFirstSelected");
            try
            {
                List<PBFResource> targetBuckets = new List<PBFResource>();

                // bucket 필터를 통해 해당 phase에서 진행가능한 bucket 추출
                // 초기 시점에 한번만하면 되지 않을까..?
                var buckets = context.SelectedAllocGroup.Buckets;

                foreach (var bucket in buckets)
                {
                    // LotGroup 내 소팅.
                    if (AllocatorLogic.Instance.FilterResourceOnRFS(bucket, context))
                    {
                        continue;
                    }

                    targetBuckets.Add(bucket);
                }

                // 소팅을 매번해야하나... addsort로도 처리가 될 것 같긴한데..
                AllocatorLogic.Instance.SortResourceOnRFS(targetBuckets, context);

                while (targetBuckets.Count() > 0)
                {
                    var bucket = targetBuckets.First();
                    targetBuckets.Remove(bucket);

                    context.UpdateContextPreset(bucket.BucketGroup, AllocateType.ResourceFirstSelection);

                    //Bucket이 가지고있는 ResourceGroup확인 

                    #region LotGroup Filter 로직 
                    List<APELotGroup> targetLotGroups = new List<APELotGroup>();
                    var lotGroups = bucket.Queue.GetLotGroups();
                    foreach (var lotGroup in lotGroups)
                    {
                        //// 반복되는 과정에서 filter 조건이 맞아지는 경우.
                        if (AllocatorLogic.Instance.FilterLotGroupOnRFS(lotGroup, bucket, context))
                        {
                            continue;
                        }

                        targetLotGroups.Add(lotGroup);
                        
                    }
                    #endregion

                    // 이번 Phase에서 할당할 Lot이 없는 경우
                    if (targetLotGroups.Count() == 0)
                        continue;

                    context.SelectedLotGroupInLevel = targetLotGroups;

                    // binarySearch 로 변경 필요. (속도 개선을 위한 조치)
                    // 하나의 LotGroup을 선택하는 이슈이므로..
                    // 단, 선택사유를 분석하기 위해서는 전체 정렬이 필요하긴 함..
                    if (bucket.BucketGroup.IsReSorting || bucket.IsFirstAllocate)
                    {
                        AllocatorLogic.Instance.SortLotGroupOnRFS(targetLotGroups, bucket, context);
                        bucket.IsFirstAllocate = false;
                    }

                    // SeletedLotGroup
                    var selectedlotgroup = targetLotGroups[0];

                    var allocLog = new PBFAllocationLog(bucket);
                    allocLog.CandidatedLotGroups.AddRange(targetLotGroups);
                    allocLog.AllocationSeq = CurrentExecutionInfo.AllocationKey++;
                    allocLog.LogType = "RFS";

                    allocLog.SetAllocatedLotGroup(selectedlotgroup);
                    context.AllocationLog = allocLog;

                    bool isAlloc = DoAllocate(selectedlotgroup, bucket, context);

                    // 할당 실패시. 해당 bucket은 현 Level에서는 할당 제외.
                    if (isAlloc == false)
                        continue;

                    OutputWriter.Instance.WriteAllocationInfo(context, allocLog);

                    // 장비가 추가할당이 가능한 경우에는 할당대상 장비로 재등록
                    if (bucket.CanOperation() == true)
                    {
                        targetBuckets.Add(bucket);
                        AllocatorLogic.Instance.SortResourceOnRFS(targetBuckets, context);
                    }
                }
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("Allocate_ResourceFirstSelected");
            }
        }

        internal double GetAllocableLotQty(double availBucketCapacity, double lotAvailAllocQty, double usagePer, double utilizationRatio, CapacityType capaType)
        {
            var needBucketCapacity = lotAvailAllocQty * usagePer / utilizationRatio;

            if (capaType == CapacityType.Time || capaType == CapacityType.Quantity)
            {
                var actBucketCapacity = Math.Min(needBucketCapacity, availBucketCapacity);

                if (usagePer > ATOption.Instance.MinimumAllocationQuantity)
                    lotAvailAllocQty = actBucketCapacity / usagePer * utilizationRatio;
            }

            return lotAvailAllocQty;
        }

        public bool DoAllocate(APELotGroup lotGroup, PBFResource bucket, PBFAllocateContext context)
        {
            // 예약된 Lot을 할당 실패한 경우, 어떻게 처리..?
            try
            {
                #region Properties
                DateTime now = this.Factory.NowDT;
                APELotGroup targetLotGroup;
                
                // Lot 투입 가능 시간과 Setup 가능 시간을 구분하기 위함
                DateTime loadableTime = bucket.CurrentTime;
                #endregion

                if (context.AllocateType != AllocateType.Reserve)
                {
                    #region 할당 관련 정보 세팅
                    targetLotGroup = AllocatorLogic.Instance.GetLoadableLots(bucket, lotGroup, context);

                    if (targetLotGroup == null || targetLotGroup.HasContents == false)
                        return false;

                    loadableTime = ATUtil.MaxTime(loadableTime, targetLotGroup.LastStepTime);
                    
                    context.SelectedLotGroup = targetLotGroup;
                    #endregion

                    #region Add Resource 정보 세팅
                    context.AddArrInfos = new List<ATOperResource>();
                    context.AddBuckets = new List<IBucket>();

                    bool hasAddArr = true;
                    var sampleArr = lotGroup.Sample.CurrentOper.GetArrange(bucket);
                    if (sampleArr != null && sampleArr.HasAddArrange)
                    {
                        foreach (var addArrGroupKey in sampleArr.AddArrangeInfo.Keys)
                        {
                            List<PBFResource> targetAddBuckets = new List<PBFResource>();
                            foreach (var arr in sampleArr.AddArrangeInfo[addArrGroupKey])
                            {
                                var arrBucket = arr.Bucket as PBFResource;

                                if (arrBucket.ReserveManager.IsReserved == false)
                                    targetAddBuckets.Add(arrBucket);
                            }

                            targetAddBuckets.Sort(new APEAddBucketComparer(context, lotGroup, bucket));

                            PBFResource addBucket = targetAddBuckets.FirstOrDefault();
                            if (addBucket == null || addBucket.CurrentCapaInfo.Remain == 0)
                            {
                                hasAddArr = false;
                                break;
                            }

                            ATOperResource addArr = sampleArr.GetAddArrangeInfo(addArrGroupKey, addBucket.BucketID);
                            if (addArr != null && context.AddArrInfos.Contains(addArr) == false)
                            {
                                context.AddArrInfos.Add(addArr);
                                context.AddBuckets.Add(addBucket);

                                double addUtilRate = addBucket.Target.GetUtilization(now);

                                if (context.AddUtilizationRate.ContainsKey(addBucket))
                                    context.AddUtilizationRate[addBucket] = addUtilRate;
                                else
                                    context.AddUtilizationRate.Add(addBucket, addUtilRate);

                                loadableTime = ATUtil.MaxTime(loadableTime, addBucket.CurrentTime);
                            }
                        }

                        if (hasAddArr == false || context.AddArrInfos.Count() == 0) 
                        {
                            foreach (APELot lot in targetLotGroup.GetLotList())
                            {
                                if (lot.Qty >= ATOption.Instance.MinimumAllocationQuantity)
                                    lot.AddVirtualLateInfo(LateCategory.Capacity, LateReason.NoOpAddResourceInfo.ToString(), null, Factory.NowDT, Factory.NextStartTime);
                            }

                            return false;
                        }
                    }
                    #endregion

                    #region Setup 정보 세팅
                    context.SetupInfo = AllocatorLogic.Instance.GetSetupInfo(bucket, targetLotGroup, context);
                    #endregion
                }
                else
                {
                    targetLotGroup = lotGroup;
                    loadableTime = ATUtil.MaxTime(loadableTime, targetLotGroup.LastStepTime);

                    if (context.AddArrInfos != null && context.AddArrInfos.Count() > 0)
                    {
                        foreach (PBFResource addBucket in context.AddBuckets)
                        {
                            loadableTime = ATUtil.MaxTime(loadableTime, (addBucket as PBFResource).CurrentTime);
                        }
                    }
                }

                #region Setup 시간 보정
                if (context.SetupInfo != null)
                {
                    if (context.SetupInfo.SetupResource != null)
                    {
                        var setupResource = context.SetupInfo.SetupResource;

                        if (setupResource.CurrentTime == setupResource.EndTime)
                        {
                            lotGroup.GetLotList().ForEach(x => (x as APELot).AddVirtualLateInfo(LateCategory.Capacity, LateReason.LackOfSetupResourceCapacity.ToString(), null, Factory.NowDT, Factory.NextStartTime));

                            return false;
                        }
                    }

                    bucket.SetupManager.AdjustSetupTime(loadableTime, context.SetupInfo, context.AllocateType);
                    loadableTime = ATUtil.MaxTime(loadableTime, context.SetupInfo.EndTime);
                }
                #endregion

                // 최후 로딩 여부 체크
                if (context.AllocateType != AllocateType.Reserve && AllocatorLogic.Instance.CheckForAvailableBeforeAllocation(bucket, targetLotGroup, context) == false)
                    return false;

                if (context.SetupInfo != null)
                {
                    if (AllocatorLogic.Instance.DoSetup(bucket, targetLotGroup, context.SetupInfo, context) == false)
                        return false;
                }

                double allocableQty = FWInterface.AllocatorControl.GetAllocableQty(targetLotGroup, bucket, loadableTime, context);
                
                // bundle 작업필요.
                List<IAPELot> lotList = targetLotGroup.GetLotList().ToList();
                List<IAPELot> allocatedLot = new List<IAPELot>();
                IAPELot lastLot = null;
                bool allocated = false;

                for (int i = 0; i < lotList.Count(); i++)
                {
                    #region Properties
                    var curLot = lotList[i];
                    double lotAvailAllocQty = curLot.CurrentQty;
                    
                    if (bucket.IsTimeCapa)
                        loadableTime = ATUtil.MaxTime(loadableTime, curLot.LastStepTime);
                    else
                        loadableTime = ATUtil.MaxTime(bucket.CurrentTime, curLot.LastStepTime);

                    #endregion

                    #region 예외처리
                    if (lotAvailAllocQty <= ATOption.Instance.MinimumAllocationQuantity)
                    {
                        allocatedLot.Add(curLot);
                        continue;
                    }

                    ATOperResource arr = curLot.CurrentOper.GetArrange(bucket);
                    if (arr == null)
                    {
                        // 할당 못한 사유 작성?
                        break;
                    }
                    #endregion

                    #region UsagePer, FlowTime 세팅
                    curLot.CurrentArrange = arr;
                    context.UsagePer = arr.GetUsagePer(now);
                    context.FlowTime = arr.GetFlowTime(now);
                    context.UtilizationRate = bucket.Target.GetUtilization(now);

                    // 이건 수정이 좀 필요 (Split 가능한 조건을 수량 제약(Constraint, MBS만 있다고 생각했는데 Fixed_Split의 PM도 있기 때문에 임시 변수 생성)
                    context.IsSplit = false; 

                    //context.BatchTime -> 엔진 스키마 정의 필요

                    FWInterface.AllocatorControl.OnSelectAllocateLot(targetLotGroup, bucket, curLot, loadableTime, context);

                    bool hasAddArr = true;
                    if (bucket.IsTimeCapa)
                    {
                        loadableTime = ATUtil.MaxTime(loadableTime, bucket.CurrentTime);

                        foreach (var addBucket in context.AddBuckets)
                        {
                            var addArr = arr.GetAddArrangeInfo(addBucket.Target.ResGroupID, addBucket.BucketID);

                            if (addArr == null)
                            {
                                hasAddArr = false;
                                break;
                            }

                            var addUsagePer = addArr.GetUsagePer(now);

                            context.UsagePer = Math.Max(context.UsagePer, addUsagePer);

                            double addUtilRate;
                            if (context.AddUtilizationRate.TryGetValue(addBucket, out addUtilRate) == false)
                            {
                                addUtilRate = addBucket.Target.GetUtilization(now);
                                context.AddUtilizationRate.Add(addBucket, addUtilRate);
                            }

                            context.UtilizationRate = Math.Min(context.UtilizationRate, addUtilRate);

                            loadableTime = ATUtil.MaxTime(loadableTime, (addBucket as PBFResource).CurrentTime);
                        }
                    }

                    if (hasAddArr == false)
                    {
                        foreach (APELot lot in lotList)
                        {
                            if (lot.CurrentQty >= ATOption.Instance.MinimumAllocationQuantity)
                                lot.AddVirtualLateInfo(LateCategory.Capacity, LateReason.NoOpAddResourceInfo.ToString(), null, Factory.NowDT, Factory.NextStartTime);
                        }
                        break;
                    }
                    #endregion

                    #region Constraint 세팅 및 적용
                    List<ATConstraintDetail> constraints = new List<ATConstraintDetail>();

                    // 아래 부분에서 제약을 중복으로 가져오는지 검증 필요 -> 어떻게 처리가 되어야 하는지 협의 필요
                    curLot.Apply((x, _) => constraints.AddRange((x.Sample as APELot).GetConstraintDetails(ATUtil.ToDate(now))));
                    constraints.AddRange(bucket.CurrentConstraintDetails);
                    constraints = constraints.OrderBy(x => x.RemainQty).ToList();

                    ATConstraintDetail minConstraint = constraints.FirstOrDefault();
                    double applyConstraintQty = double.MaxValue; // Constraint 리팩토링 시 삭제 검토

                    if (minConstraint != null)
                    {
                        applyConstraintQty = minConstraint.RemainQty;
                        context.IsSplit = true;
                    }

                    foreach (PBFResource addBucket in context.AddBuckets)
                    {
                        List<ATConstraintDetail> addConstraints = addBucket.CurrentConstraintDetails.OrderBy(x => x.RemainQty).ToList();
                        constraints.AddRange(addConstraints);

                        var addConstraint = addConstraints.FirstOrDefault();
                        if (addConstraint != null)
                        {
                            if (addConstraint.RemainQty < applyConstraintQty)
                            {
                                minConstraint = addConstraint;
                                applyConstraintQty = addConstraint.RemainQty;
                                context.IsSplit = true;
                            }
                        }
                    }

                    if (minConstraint != null)
                    {
                        lotAvailAllocQty = Math.Min(lotAvailAllocQty, minConstraint.RemainQty);
                        applyConstraintQty = minConstraint.RemainQty;
                        context.ConstraintQty = applyConstraintQty;

                        #region Late 사유 등록
                        string reason = LateReason.ItemConstraint.ToString();
                        string detail = string.Format("Constraint ID : {0}", minConstraint.Constraint.ConstraintID);

                        HashSet<IBucket> resources = new HashSet<IBucket>();

                        if (minConstraint.Constraint.Property.Category == PropertyCategory.Resource.ToString())
                            reason = LateReason.ResourceConstraint.ToString();

                        curLot.Apply((x, _) => (x.Sample as APELot).AddVirtualLateInfo(LateCategory.Constraint, reason, detail, Factory.NowDT, Factory.NextStartTime));
                        #endregion
                    }
                    else
                    {
                        string desc = string.Format("Calendar ID : {0}", bucket.CurrentCapaInfo.CapaInfo.CalAttr.CalendarID);

                        curLot.Apply((x, _) => (x.Sample as APELot).AddVirtualLateInfo(LateCategory.Capacity, LateReason.LackOfResourceCapacity.ToString(), desc, Factory.NowDT, Factory.NextStartTime, new HashSet<IBucket>() { bucket }));
                    }

                    #endregion

                    #region Constraint 부족으로 인한 Short 사유 등록
                    if (lotAvailAllocQty <= ATOption.Instance.MinimumAllocationQuantity)
                    {
                        if (minConstraint != null)
                        {
                            string reason = LateReason.ItemConstraint.ToString();
                            string detail = string.Format("Constraint ID : {0}, Apply Date : {1}, Remain Qty : {2}", minConstraint.Constraint.ConstraintID, minConstraint.Attribute.ApplyDate, minConstraint.RemainQty);

                            if (minConstraint.Constraint.Property.Category == PropertyCategory.Resource.ToString())
                                reason = LateReason.ResourceConstraint.ToString();

                            foreach (APELot lot in lotList)
                            {
                                if (lot.CurrentQty >= ATOption.Instance.MinimumAllocationQuantity)
                                    lot.AddVirtualLateInfo(LateCategory.Constraint, reason, detail, Factory.NowDT, Factory.NextStartTime);
                            }
                        }
                        
                        break;
                    }
                    #endregion

                    #region 할당 가능 Lot 수량 산출
                    double mainCapacity = double.MaxValue;
                    if (bucket.IsInfinite == false && bucket.Target.ResType != ResourceType.Batch)
                    {
                        var from = loadableTime;
                        var to = bucket.EndTime;

                        // Split 되기 전까지의 시간 (ReservationPolicy가 Split이 아닌경우, OffTime SplitOption을 Y로 하면 안됨 -> 추가 Validation이 필요할지?)
                        if (bucket.IsTimeCapa)
                        {
                            var nextSection = bucket.GetNextNonWorkingPeriod(loadableTime, bucket.EndTime, LotSplitOption.Split, true, true);
                            if (nextSection != APENonWorkingPeriod.NULL)
                            {
                                context.IsSplit = true;
                                to = loadableTime >= nextSection.Start ? loadableTime : nextSection.Start;
                            }
                        }

                        mainCapacity = bucket.CalculateCapacity(from, to);

                        lotAvailAllocQty = GetAllocableLotQty(mainCapacity, lotAvailAllocQty, context.UsagePer, context.UtilizationRate, bucket.CapacityType);
                    }

                    // TimeCapa가 아닌 경우만 AddResource 가용 Capa 산출
                    double addCapacity = double.MaxValue;
                    
                    foreach (var addArr in context.AddArrInfos)
                    {
                        var addBucket = addArr.Bucket as PBFResource;

                        if (addBucket.IsInfinite == true)
                            continue;

                        double usagePer = addArr.GetUsagePer(now);
                        addCapacity = addBucket.CurrentCapaInfo.Remain;

                        double addUtilRate;
                        if (context.AddUtilizationRate.TryGetValue(addBucket, out addUtilRate) == false)
                        {
                            addUtilRate = addBucket.Target.GetUtilization(now);
                            context.AddUtilizationRate.Add(addBucket, addUtilRate);
                        }

                        if (addBucket.IsTimeCapa)
                        {
                            addUtilRate = context.UtilizationRate;
                            usagePer = context.UsagePer;
                        }

                        lotAvailAllocQty = GetAllocableLotQty(addCapacity, lotAvailAllocQty, usagePer, addUtilRate, bucket.CapacityType);
                    }
                    #endregion

                    if (allocableQty < lotAvailAllocQty)
                    {
                        lotAvailAllocQty = allocableQty;

                        curLot.Apply((x, _) => (x.Sample as APELot).AddVirtualLateInfo(LateCategory.Capacity, LateReason.MultipleBatchSizeShort.ToString(), null, Factory.NowDT, Factory.NextStartTime, new HashSet<IBucket>() { bucket }));

                        if (lotAvailAllocQty <= ATOption.Instance.MinimumAllocationQuantity)
                            break;
                    }

                    allocableQty -= lotAvailAllocQty;

                    context.LotAvailAllocQty = lotAvailAllocQty;
                    context.AllocableQty = allocableQty;

                    #region Capa 부족으로 인한 Short 사유 등록
                    if (lotAvailAllocQty <= ATOption.Instance.MinimumAllocationQuantity)
                    {
                        string desc = string.Format("Calendar ID : {0}", bucket.CurrentCapaInfo.CapaInfo.CalAttr.CalendarID);

                        foreach (APELot lot in lotList)
                        {
                            if (lot.CurrentQty >= ATOption.Instance.MinimumAllocationQuantity)
                                lot.AddVirtualLateInfo(LateCategory.Capacity, LateReason.LackOfResourceCapacity.ToString(), desc, Factory.NowDT, Factory.NextStartTime, new HashSet<IBucket>() { bucket });
                        }

                        break;
                    }
                    #endregion

                    if (lastLot != null)
                    {
                        // Stop 여부를 판단할 때, Group 정보가 필요한가..?
                        if (FWInterface.AllocatorControl.IsStopAllocateInLotGroup(bucket, targetLotGroup, curLot, lastLot, context) == true)
                        {
                            break;
                        }
                    }

                    #region 장비 할당 수행
                    APEPlanInfo planInfo = bucket.Allocated(bucket, curLot, lotAvailAllocQty, context.UsagePer, context.FlowTime, context.UtilizationRate, loadableTime, context);

                    bucket.AddPlan(planInfo, curLot);
                    bucket.CurrentCapaInfo.UsedPlanInfos.Add(planInfo);

                    foreach (var detail in constraints)
                    {
                        APEPlanInfo cloneInfo = planInfo.Clone() as APEPlanInfo;
                        cloneInfo.AllocationInfo.Type = PlanInfoType.Constraint;
                        cloneInfo.Description = detail.Constraint.ConstraintID;

                        detail.UsedQty += lotAvailAllocQty;
                        detail.Constraint.Plans.Add(cloneInfo);
                    }

                    FWInterface.AllocatorControl.OnBucketAllocated(bucket, targetLotGroup, curLot, planInfo, context);

                    double mainAllocQty = lotAvailAllocQty;
                    curLot.Apply((x, _) =>
                    {
                        var sampleLot = x.Sample as APELot;
                        double allocQty = Math.Min(sampleLot.CurrentQty, mainAllocQty);

                        var currentPlanInfo = sampleLot.CurrentPlanInfo;
                        currentPlanInfo.UpdatePlanInfo(planInfo, allocQty);

                        mainAllocQty -= allocQty;
                    });

                    context.PlanInfo = planInfo;
                    #endregion

                    #region Add Resource 할당
                    foreach (var addArr in context.AddArrInfos)
                    {
                        var addBucket = addArr.Bucket as PBFResource;
                        var addUsagePer = addArr.GetUsagePer(now);

                        if (context.AddUtilizationRate.TryGetValue(addBucket, out double addResRate) == false)
                            addResRate = 1;

                        if (addBucket.IsTimeCapa)
                        {
                            addUsagePer = context.UsagePer;
                            addResRate = context.UtilizationRate;
                        }

                        APEPlanInfo addPlanInfo = addBucket.Allocated(bucket, curLot, lotAvailAllocQty, addUsagePer, 0, addResRate, loadableTime, context);

                        addBucket.AddPlan(addPlanInfo, curLot);
                        addBucket.CurrentCapaInfo.UsedPlanInfos.Add(addPlanInfo);
                        
                        FWInterface.AllocatorControl.OnBucketAllocated(addBucket, targetLotGroup, curLot, addPlanInfo, context);
                    }
                    #endregion

                    #region Lot 할당 수량 반영

                    double usedAllocQty = lotAvailAllocQty;
                    curLot.Apply((x, y) =>
                    {
                        var sampleLot = x.Sample as APELot;
                        double allocQty = Math.Min(sampleLot.CurrentQty, usedAllocQty);
                        sampleLot.CurrentQty -= allocQty;
                        usedAllocQty -= allocQty;

                        if (curLot is APELotGroup)
                            curLot.CurrentQty -= allocQty;
                    });

                    #endregion

                    bool isContinueAllocate = bucket.ProcessRemainLot(curLot, targetLotGroup, context);

                    if (curLot.CurrentQty <= ATOption.Instance.MinimumAllocationQuantity)
                    {
                        curLot.Apply((x, _) =>
                        {
                            var lot = (x.Sample as APELot);
                            FWInterface.AllocatorControl.OnAllocated(bucket, x, lot.CurrentPlanInfo, context);

                            allocatedLot.Add(lot);
                        });
                    }

                    if (isContinueAllocate == false)
                        break;

                    lastLot = curLot;

                    if (bucket.CanOperation() == false)
                        break;
                }

                // string allocKey = context.AllocationLog != null ? context.AllocationLog.AllocationKey : "";
                bucket.DoPM(bucket.CurrentTime);

                if (allocatedLot.Count() > 0)
                    allocated = true;

                // 할당된 Lot 혹은 Batch들 다음 Step으로 이동.
                foreach (var lot in allocatedLot)
                {
                    lotGroup.RemoveLot(lot);

                    lot.Apply((x, _) => this.Line.MoveNext(x.Sample as APELot));
                }

                return allocated;
            }
            finally
            {
                context.PlanInfo = null; // 필요한가?
            }
        }
    }
}
