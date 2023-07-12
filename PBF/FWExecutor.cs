using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mozart.Task.Execution;
using System.Diagnostics;
using Mozart.Simulation.Engine;
using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Planner.Data;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    //[ProvideExecutor("Cbs", Priority = 1)]
    public class FWExecutor : ExperimentSimulation, IModelExecutor
    {
        /// <summary>Get the currently activated Planner.</summary>
        public static FWExecutor Current => ServiceLocator.Resolve<FWExecutor>();

        FWFactory _factory;
        IModelLog logger;
        ModelContext context;

        public ModelContext Context
        {
            get { return context; }
        }

        public new FWFactory AoFactory
        {
            get { return this._factory; }
        }

        public IModelLog Logger
        {
            get { return this.logger; }
        }

        internal List<IAgent> Agents;

        /// <summary>
        /// Constructor.
        /// </summary>
        public FWExecutor()
        {
            if (!ServiceLocator.IsRegistered<FWExecutor>())
            {
                ServiceLocator.RegisterInstance<FWExecutor>(this);

                Agents = new List<IAgent>();
                Agents.Add(ATRuleAgent.Instance);
                Agents.Add(APEWipAgent.Instance);
                Agents.Add(APEReleaseAgent.Instance);
                Agents.Add(ATConstraintAgent.Instance);
                Agents.Add(ATLateInfoTrackerAgent.Instance);
                Agents.Add(ATDummyAgent.Instance);
                Agents.Add(ATResourceAgent.Instance);
            }
        }

        protected override ActiveObject CreateRoot(Coordinator c)
        {
            this._factory = new FWFactory(c);
            return this._factory;
        }


        #region IModelExecutor 멤버
        /// <summary>
        /// Perform initialization to execute planner.
        /// </summary>
        /// <param name="context">Model execution context.</param>
        public virtual void Initialize(ModelContext context)
        {
            var executeInfo = ATExecutionContext.Instance.CurrentExecutionInfo as PBFModuleExecutionInfo;

            string key = string.Format("{0}_Initialize", executeInfo.Key);
            ATElapsedTimeChecker.Instance.StartCustomTimer(key);

            try
            {
                // 해당 부분은항상 필요한가..
                this.context = context;
                this.logger = context.GetLog(MConstants.LoggerMain);

                ATOption.Instance.SetDefaultOptionValue();

                Agents.ForEach(x => x.Initialize());

                #region ModuleInit

                executeInfo.Kpi = new APEKpiSummary();

                #region Demand Kpi 집계
                if (executeInfo.Kpi != null)
                {
                    foreach (var so in ATInputData.Demands._rowDemand)
                    {
                        if (so.DUE_DATE >= ATOption.Instance.PlanEndTime)
                            continue;

                        executeInfo.Kpi.AddDictionary(KpiCategory.RTF, KpiIndex.DEMAND_QTY, so.DUE_DATE, true, so.DEMAND_QTY, CalcType.Sum);
                    }
                }
                #endregion

                FWInterface.ModuleControl.OnStartModule(executeInfo);

                if (string.IsNullOrEmpty(executeInfo.RefKey) == false)
                {
                    string[] refKeys = executeInfo.RefKey.Replace(" ", "").Split(',');

                    foreach (var refkey in refKeys)
                    {
                        var refModule = ATExecutionContext.Instance.GetExecutionInfo(refkey);

                        if (refModule == null)
                            continue;

                        executeInfo.RefModules.Add(refModule);

                        if (refModule is PBBModuleExecutionInfo)
                        {
                            // PreDefine함수로 정의해야하나..?
                            var refPBBModule = refModule as PBBModuleExecutionInfo;

                            // BW 결과를 그대로 받기.                    
                            executeInfo.Wips = refPBBModule.PeggedWips;
                            executeInfo.InTargets = refPBBModule.StageInTargets;
                            executeInfo.OperTargets = refPBBModule.OperTargets;
                            executeInfo.BinPeggedInfos = refPBBModule.BinPeggedInfos;
                            executeInfo.AssemblyTargets = refPBBModule.StageInAssemblyTargets;
                            executeInfo.UnpeggedWips = refPBBModule.UnpeggedWips;
                        }
                        
                        if (refModule is PBOModuleExecutionInfo)
                        {
                            var refPBOModule = refModule as PBOModuleExecutionInfo;

                            executeInfo.Wips = refPBOModule.PeggedWips;
                            executeInfo.InTargets = refPBOModule.StageInTargets;
                            executeInfo.OperTargets = refPBOModule.OperTargets;
                            //executeInfo.BinPeggedInfos = refPBOModule.BinPeggedInfos;
                            executeInfo.AssemblyTargets = refPBOModule.StageInAssemblyTargets;
                            executeInfo.UnpeggedWips = refPBOModule.UnpeggedWips;
                        }
                    }

                    var inTargets = FWInterface.InitControl.PrepareInTarget(executeInfo, executeInfo.RefModules);
                    if (inTargets != null && inTargets.Count > 0)
                    {
                        executeInfo.InTargets = inTargets;
                    }

                    var wips = FWInterface.InitControl.PrepareWip(executeInfo, executeInfo.RefModules);
                    if (wips != null && wips.Count > 0)
                    {
                        executeInfo.Wips = wips;
                    }

                    var operTargets = FWInterface.InitControl.PrepareOperTarget(executeInfo, executeInfo.RefModules);
                    if (operTargets != null && operTargets.Count > 0)
                    {
                        executeInfo.OperTargets = operTargets;
                    }

                    var incomingWips = FWInterface.InitControl.PrepareIncomingWip(executeInfo, executeInfo.RefModules);
                    if (incomingWips != null && incomingWips.Count > 0)
                    {
                        executeInfo.IncomingWips = incomingWips;
                    }

                    FWInterface.InitControl.OnPrepareCustomInput(executeInfo, executeInfo.RefModules);
                }

                #endregion
            }
            finally
            {
                ATElapsedTimeChecker.Instance.StopCustomTimer(key, false);
            }
        }

        /// <summary>Executes order-by-order forward engine.</summary>
        /// <param name="context">Model execution context.</param>
        public virtual void Execute(ModelContext context)
        {
            try
            {
                var executeInfo = ATExecutionContext.Instance.CurrentExecutionInfo as PBFModuleExecutionInfo;

                this.Logger.MonitorInfo("\t Forward-Init..." + DateTime.Now.ToSortableString());

                // CreateAllocGroup / Bucket
                var allocGroup = CreateAllocationGroup();

                FWInterface.ModuleControl.OnCreateAllocationGroup(allocGroup);

                // CreateLot
                var lots = CreateLot();
                var orgLots = lots.Values.ToList();

                int intargetWipCnt = lots.Count() > 0 ? lots.Where(r => r.Value.IsWipLot == true).Count() : 0;
                this.logger.MonitorInfo("\t Input lot count : from intarget = {0}, from pegged wip = {1}", lots.Count() - intargetWipCnt, intargetWipCnt);

                //if(allocGroup.)
                // Lot이 없으면 에러 처리하고 종료?
                // Target이 없어도 에러 처리하고 종료.
                // 그 외 기타 필수 정보 없는 경우 종료.??
                // FilterLot
                executeInfo.CurPhase = 0;
                executeInfo.CurRetryCount = 0;

                List<APELot> remainLots = new List<APELot>();
                while (executeInfo.CurPhase < executeInfo.PhaseCount)
                {
                    remainLots = new List<APELot>();
                    // 옵션 설정.
                    executeInfo.CurPhase++;

                    string key = string.Format("{0}_PHASE_{1}", executeInfo.Key, executeInfo.CurPhase);
                    ATElapsedTimeChecker.Instance.StartCustomTimer(key);

                    /// Phase별 Option 초기화
                    var option = executeInfo.GetOption(executeInfo.CurPhase);
                    ATOption.Instance.Update(option);

                    /// Factory 생성 작업
                    this.Start();
                    if (this.Engine.Exception != null)
                        throw new Exception(Strings.EXCEPTION_AT_START, this.Engine.Exception);

                    /// 장비 그룹 설정
                    this.AoFactory.Initialize(allocGroup);

                    /// FilterLot
                    var filterPreset = ATRuleAgent.Instance.CurrentRuleSet.GetRule(RulePoint.FilterLot, CallType.Phase);
                    List<APELot> selectedLots = FWFactoryLogic.Instance.FilterLot(lots, filterPreset);
                    FWPhaseInfo phaseInfo = new FWPhaseInfo(orgLots, selectedLots, executeInfo.CurPhase);

                    // ReleaseLot 등록 작업.
                    APEReleaseAgent.Instance.ReleaseLots.AddRange(selectedLots);

                    FWInterface.ModuleControl.OnStartPhase(phaseInfo);

                    Logger.MonitorInfo(string.Format("Start the current phase of the module({0}/{1})", executeInfo.CurPhase, executeInfo.PhaseCount));
                    Logger.MonitorInfo(string.Format("Default RuleSetID : {0}", ATRuleAgent.Instance.CurrentRuleSet.RulesetID));

                    this.RunSync();
                    if (this.Engine.Exception != null)
                        throw new Exception(Strings.EXCEPTION_AT_RUNFAST, this.Engine.Exception);

                    this.Stop();

                    FWInterface.ModuleControl.OnEndPhase(phaseInfo);

                    remainLots.AddRange(APEReleaseAgent.Instance.ReleaseLots);
                    APEReleaseAgent.Instance.ReleaseLots.Clear();

                    ATElapsedTimeChecker.Instance.StopCustomTimer(key, true);
                }

                foreach (var lot in remainLots)
                {
                    lot.ShortCategory = "unreleased lot";
                    APEWipAgent.Instance.AddRemainLot(lot);

                    string reason;

                    if (lot.CurrentItemSiteBuffer.IsMaterialItemSiteBuffer == false)
                    {
                        reason = LateReason.UnReleasedFERTMaterialShort.ToString();
                        lot.AddShortInfo(LateCategory.Material, reason, lot.Qty, null, lot.LastStepTime, lot.LastStepTime);
                    }
                }

                this.Engine.Done();

                var resources = executeInfo.Stage.Resources;
                foreach (var resource in resources)
                {
                    PBFResource bucket = resource.Bucket as PBFResource;

                    var infos = bucket.CapacityManager.GetOrgCapaInfos();
                    foreach (var info in infos)
                    {
                        // 통일 시기기 (PBO, PBF)
                        OutputWriter.Instance.WriteCapaAllocationPlan(bucket, info);
                    }
                }

                foreach (var constraint in ATConstraintAgent.Instance.GetContraints())
                {
                    foreach (var planInfo in constraint.Plans)
                    {
                        // ResourceID에 ConstraintID가 기록되어야함 -> Constraint로 FWBucket을 생성해야 할까??
                        OutputWriter.Instance.WriteResPlan(planInfo);
                    }

                    foreach (var detail in constraint.ConstraintDetails)
                    {
                        OutputWriter.Instance.WriteCapaAllocationPlan(detail.Value.Constraint, detail.Value);
                    }
                }

                OutputWriter.Instance.WriteRemainLotLog(this._factory);
                OutputWriter.Instance.WritePlanningResult();
                OutputWriter.Instance.WritePSIReport();
                //OutputWriter.Instance.WritePlanIndex(executeInfo.Key, executeInfo.Kpi);

                FWInterface.ModuleControl.OnEndModule(executeInfo);
            }
            finally
            {
                Agents.ForEach(x => x.Dispose());
            }
        }

        /// <summary>
        /// Line 내 AllocatoionGroupo / BucketGroup / Bucket 정보들을 생성
        /// </summary>
        /// <param name="solver"></param>
        public List<FWAllocGroup> CreateAllocationGroup()
        {
            ATElapsedTimeChecker.Instance.ResetTimer("CraeteAllocationGroup");
            try
            {
                List<FWAllocGroup> allocGroups = new List<FWAllocGroup>();
                var stage = ATExecutionContext.Instance.CurrentStage;

                foreach (var allocGrp in stage.AllocationGroups.Values)
                {
                    FWAllocGroup allocGroup = ObjectMapper.CreateAllocGroup(allocGrp);

                    allocGroups.AddSort(allocGroup, APEAllocGroupComparer.Default);
                    //this.DefaultLine.AllocGroups.Add(allocGrp.AllocationGroupID, allocGroup);

                    foreach (var resGrp in allocGrp.ResourceGroups)
                    {
                        PBFResourceGroup bucketgroup = ObjectMapper.CreateBucketGroup(resGrp);

                        // 더하는 시점에 AddSort 작업.
                        allocGroup.BucketGroups.AddSort(bucketgroup, APEBucketGroupComparer.Default);

                        foreach (var resource in resGrp.Resources.Values)
                        {
                            PBFResource bucket = ObjectMapper.CreateBucket(resource);

                            bucketgroup.Buckets.Add(bucket);
                            allocGroup.Buckets.Add(bucket);
                            bucket.BucketGroup = bucketgroup;

                            resource.Bucket = bucket;

                            // 리소스의 arrange 정보를 이용해서 오퍼별 사용가능 장비와 arrange 정보를 등록해주는 작업 필요.
                            // Oper -> Loadable Bukcet / Arrange 정보 등록                            
                            foreach (var arrange in resource.ArrangeInfos.Values)
                            {
                                arrange.Oper.ArrangeInfos.Add(bucket, arrange);
                                arrange.Oper.BucketGroup = bucketgroup;
                                //res.ArrangeInfos.ForEach(x => x.Oper.ArrangeInfos.Add(bucket, x));
                            }

                            FWInterface.BucketControl.OnCreateBucket(bucket);
                        }
                    }
                }

                foreach (var resource in stage.Resources.Where(r => r.Bucket == null))
                {
                    PBFResource bucket = ObjectMapper.CreateBucket(resource);
                    resource.Bucket = bucket;

                    FWInterface.BucketControl.OnCreateBucket(bucket);

                    // Main 
                    // 장비 그룹 있음. => AllocGroup

                    // Add

                    // Constraint

                }

                // Bucket에 Arrange 정보 연결 작업 

                return allocGroups;
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("CraeteAllocationGroup");
            }

        }

        public APELot CreateLot(string lotID, double batchQty, ATOperation oper, ATOperTarget target, APEWip wip, LotCreateType type)
        {
            var lot = ObjectMapper.CreateLot(lotID, batchQty, oper, target, wip, type);

            if (lot.InitOperTarget.PegTarget?.PegPart?.CurRefPlan != null)
                lot.RefPlans.Add(lot.InitOperTarget.PegTarget.PegPart.CurRefPlan);

            return lot;
        }

        public Dictionary<APELot, APELot> CreateLot()
        {
            // Pegging된 Wip만 로드할지 결정 필요.
            // 전체 Wip을 모두 로드할지 결정 필요.
            ATElapsedTimeChecker.Instance.ResetTimer("CreateLot");
            try
            {
                var executionInfo = ATExecutionContext.Instance.CurrentExecutionInfo as PBFModuleExecutionInfo;

                Dictionary<APELot, APELot> targetLots = new Dictionary<APELot, APELot>();

                foreach (var target in executionInfo.InTargets)
                {
                    double lotSize = target.CurrentItemSiteBuffer.GetInputBatchSize();

                    double lotQty = target.InTargetQty;

                    while (lotQty > 0)
                    {
                        var batchQty = Math.Min(lotSize, lotQty);

                        string lotID = LotHelper.GeneratLotID(ATConstants.TARGET_BATCH_PREFIX, target.TargetID);
                        var lot = CreateLot(lotID, batchQty, target.Oper, target, null, LotCreateType.Normal);

                        targetLots.Add(lot,lot);

                        lotQty -= lot.Qty;

                        lot.OnCreateLot(lot, null, LotCreateType.Creation);
                    }
                }

                foreach (var wip in executionInfo.Wips)
                {
                    string lotID = LotHelper.GeneratLotID(ATConstants.WIP_BATCH_PREFIX, wip.WipInfo.LotID);
                    var lot = CreateLot(lotID, wip.RemainQty, wip.Oper, wip.MapTarget, wip, wip.CreationType);

                    targetLots.Add(lot, lot);

                    // Pegging 된 재공만 PSI에 집계되는 상황 (UnPeg 재공은 집계되지 않음)
                    // PBF만 돌렸을 때 결과 집계가 관건이라면 Obo의 Wip 집계 위치는 수정하지 않아도 될지?
                    if (wip.CreationType != LotCreateType.SplitByBom)
                        wip.ItemSiteBuffer.AddBufferPSISummaryPlanWip(wip);

                    lot.OnCreateLot(lot, null, LotCreateType.Creation);
                }

                // Wips 랑 차이가 멀까.....
                foreach (var wip in executionInfo.IncomingWips)
                {
                    string lotID = LotHelper.GeneratLotID(ATConstants.WIP_BATCH_PREFIX, wip.WipInfo.LotID);
                    var lot = CreateLot(lotID, wip.RemainQty, wip.Oper, wip.MapTarget, wip, LotCreateType.Incoming);
                    
                    targetLots.Add(lot, lot);

                    lot.OnCreateLot(lot, null, LotCreateType.Creation);
                }

                foreach (var wip in executionInfo.UnpeggedWips)
                {
                    if (wip.CreationType != LotCreateType.SplitByBom)
                        wip.ItemSiteBuffer.AddBufferPSISummaryPlanWip(wip);
                }

                return targetLots;
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("CreateLot");
            }
        }


        //protected override void OnFinished(Coordinator c)
        //{
        //    //base.OnFinished(c);
        //}

        public override void Reset()
        {
            base.Reset();
            //ysh
            this.Engine.StartTime = ATOption.Instance.PlanStartTime;// this.Context.StartTime;
            this.Engine.StopTime = ATOption.Instance.PlanEndTime;//.AddDays(- 1); //this.Context.EndTime;

            var notif = FWInterface.ModuleControl;
            if (Mozart.Task.Execution.Helper.IsOverridenMethod(typeof(FWModuleControl), notif, "CompareSameTimeEvent"))
                this.Engine.SameTimeComparison = notif.CompareSameTimeEvent;
	
        }

        //public override bool OnFinished(Coordinator c, int run)
        //{
        //    return true;
        //}

        //public override bool ContinueRunning(int run)
        //{
        //    var executeInfo = ATExecutionContext.Instance.CurrentExecutionInfo as PBFModuleExecutionInfo;
        //    executeInfo.CurPhase++;

        //    return executeInfo.CurPhase < executeInfo.PhaseCount;
        //}

        

        #endregion
    }
}
