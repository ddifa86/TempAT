
using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.EngineBase;
using Mozart.SeePlan.Aleatorik.Inputs;
using Mozart.SeePlan.Aleatorik.ObyO.Data;
using Mozart.SeePlan.Aleatorik.Pegger;
using Mozart.Simulation.Engine;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.ObyO
{
    public class OboExecutor : IModelExecutor
    {
        public static OboExecutor Current => ServiceLocator.Resolve<OboExecutor>();

        public PBOShortManager ShortManager { get; set; }

        private OboFactory _factory { get; set; }

        internal List<IAgent> Agents;

        public OboExecutor()
        {
            if (!ServiceLocator.IsRegistered<OboExecutor>())
            {
                ServiceLocator.RegisterInstance(this);

                Agents = new List<IAgent>();
                Agents.Add(ATRuleAgent.Instance);
                Agents.Add(ReleaseAgent.Instance);
                Agents.Add(ATISBQueueAgent.Instance);
                Agents.Add(APEWipQueueAgent.Instance);
                Agents.Add(ATConstraintAgent.Instance);
                Agents.Add(ATLateInfoTrackerAgent.Instance);
                Agents.Add(ATDummyAgent.Instance);
                Agents.Add(ATResourceAgent.Instance);
            }
        }

        void IModelExecutor.Initialize(ModelContext context)
        {
            var executeInfo = ATExecutionContext.Instance.CurrentExecutionInfo as PBOModuleExecutionInfo;
            string key = string.Format("{0}_Initialize", executeInfo.Key);
            ATElapsedTimeChecker.Instance.StartCustomTimer(key);

            BackwardCommonLogic.Instance.PegControl = PBOInterface.PegControl; // LWS ??

            try
            {
                ATOption.Instance.SetDefaultOptionValue();

                Agents.ForEach(x => x.Initialize());

                #region InitModule
                Logger.MonitorInfo(" Start PrepareInput");

                executeInfo.Kpi = new APEKpiSummary();

                #region Demand Kpi 집계
                if (executeInfo.Kpi != null)
                {
                    foreach (var so in ATInputData.Demands._rowDemand)
                    {
                        executeInfo.Kpi.AddDictionary(KpiCategory.RTF, KpiIndex.DEMAND_QTY, so.DUE_DATE, true, so.DEMAND_QTY, CalcType.Sum);
                    }
                }
                #endregion

                PBOInterface.ModuleControl.OnStartModule(executeInfo);

                executeInfo.Demands.AddRange(executeInfo.Stage.Demands.Values);

                if (string.IsNullOrEmpty(executeInfo.RefKey) == false)
                {
                    string[] refKeys = executeInfo.RefKey.Split(',');

                    foreach (var refkey in refKeys)
                    {
                        var refModule = ATExecutionContext.Instance.GetExecutionInfo(refkey);

                        if (refModule == null)
                            continue;

                        executeInfo.RefModules.Add(refModule);

                        if (refModule is PBBModuleExecutionInfo)
                        {

                        }
                    }

                    // Demand
                    executeInfo.Demands = PBOInterface.InitControl.PrepareDemand(executeInfo, executeInfo.RefModules);
                    executeInfo.Demands.ForEach(x => ATInputData.Demands.AddDemand(x));

                    PBOInterface.InitControl.OnPrepareCustomInput(executeInfo, executeInfo.RefModules);
                }

                Logger.MonitorInfo(" End PrepareInput");
                #endregion

            }
            catch (Exception e)
            {
                // => 이렇게해도 끝나지않고 Executor로 넘어가는데, 끝내는 다른 방법이 있는지?
                Logger.MonitorInfo(e.Message);
                return;
            }
            finally
            {
                ATElapsedTimeChecker.Instance.StopCustomTimer(key, false);
            }
        }

        private void CreateOboBuckets(ATStage stage)
        {
            foreach (var resource in stage.Resources)
            {
                PBOResource bucket = new PBOResource(resource);

                resource.Bucket = bucket;

                foreach (var arrange in resource.ArrangeInfos.Values)
                {
                    arrange.Oper.OboArrangeInfos.Add(arrange, bucket);
                }
            }
        }

        void IModelExecutor.Execute(ModelContext context)
        {
            try
            {
                var executeInfo = ATExecutionContext.Instance.CurrentExecutionInfo as PBOModuleExecutionInfo;
                var demands = executeInfo.Demands;

                // Demand Smoothing 혹은 보정작업 ? 
                demands = PBOInterface.InitControl.DemandSmoothing(demands);

                if (demands.Count() == 0)
                {
                    Logger.MonitorInfo("Please set up demand information.");

#warning 에러 처리 추가 필요.
                    // 에러 정보를 MODULE EXECUTOR 에서 받아서 처리 가능하도록 구조를 잡고 진행 필요.
                    // errKey
                    OutputWriter.Instance.WriteErrorLog(ModuleType.PBO, executeInfo.Key, ErrorSeverity.Critical, ErrorReasonCode.IncompatibleConfig, null, "", "SALES_ORDER@SO_ID", "No Sales Order Available");

                    return;
                }

                // InitWip => 공정에 Wip 정보를 붙이는 작업..
                Logger.MonitorInfo("Start creating PLANWIP information");
                
                BackwardCommonLogic.Instance.CreatePlanWip();

                // VirtualPegging 작업.
                Logger.MonitorInfo("Start creating PEGPART information");
                var moMasters = BackwardCommonLogic.Instance.CreateMoMaster(demands);
                var pegParts = BackwardCommonLogic.Instance.CreatePegPart(moMasters.Values);

                // OboBucket 생성 및 Resource Mapping 작업
                Logger.MonitorInfo("Start creating BUCKET information");
                CreateOboBuckets(executeInfo.Stage);

                executeInfo.CurPhase = 0;

                while (executeInfo.CurPhase < executeInfo.PhaseCount)
                {
                    pegParts.ForEach(x => x.InitFactorValue());

                    executeInfo.CurPhase++;

                    string key = string.Format("{0}_PHASE_{1}", executeInfo.Key, executeInfo.CurPhase);
                    ATElapsedTimeChecker.Instance.StartCustomTimer(key);

                    // Phase별 Option 초기화
                    var option = executeInfo.GetOption(executeInfo.CurPhase);
                    ATOption.Instance.Update(option);

                    // Phase별 PegPart Filter
                    var filterPreset = ATRuleAgent.Instance.CurrentRuleSet.GetRule(RulePoint.FilterTarget, CallType.Operation);
                    HashSet<APEPegPart> selPegParts = BackwardCommonLogic.Instance.FilterTarget(pegParts, filterPreset);

                    OboPhaseInfo phaseInfo = new OboPhaseInfo(pegParts, selPegParts, executeInfo.CurPhase);
                    PBOInterface.ModuleControl.OnStartPhase(phaseInfo, executeInfo);

                    #region Phase별 Pegging Group Sorting

                    // Pegging Group으로 바꾸고 모든 내용이 변경되어야 할 것으로 보이는데..
                    // VOC 들어온 내용과 엮어서 7월에 다시 진행 필요.
                    // PBB의 흐름과 동기화를 하기위해서는 PeggingGroup으로 모델링.
                    
                    var targetGroups = PBOBackwardLogic.Instance.GetTargetGroups(selPegParts);
                    List<APETargetGroup> selTargetGroups = targetGroups.Values.ToList();

                    foreach (var targetgroup in selTargetGroups)
                    {
                        BackwardCommonLogic.Instance.SortedTargetInTargetGroup(targetgroup, null, ATRuleAgent.Instance.CurrentRuleSet);
                    }

                    // CompareTargetGroup
                    var comparePreset = ATRuleAgent.Instance.CurrentRuleSet.GetRule(RulePoint.CompareTargetGroup, CallType.Level, 1);
                    if (comparePreset != null)
                        selTargetGroups.Sort(new APETargetGroupComparer(comparePreset));

                    // Sorting 이력 출력을 위해 정보 필요.
                    int seq = 1;
                    foreach (var targetgroup in selTargetGroups)
                    {
                        targetgroup.Targets.ForEach(x => OutputWriter.Instance.WriteInitDemandLog((x as APETarget).PegPart, seq++));
                    }
                    #endregion

                    int targetGroupCount = selTargetGroups.Count();
                    int perCount = Math.Max(targetGroupCount / 10, 1);
                    int currentTargetGroupCount = 0;

                    while (selTargetGroups.Count() > 0)
                    {
                        var targetGroup = selTargetGroups.First();
                        targetGroup.Targets.ForEach(x => pegParts.Remove((x as APETarget).PegPart));
                        selTargetGroups.Remove(targetGroup);
                        targetGroups.Remove(targetGroup.Key);
                        string targetGroupKey = targetGroup.Key.ToString();

                        currentTargetGroupCount++;

                        PBOInterface.ModuleControl.OnSelectPegPartInPhase(targetGroup);

                        this.ShortManager = new PBOShortManager(targetGroup.Items);

                        OboFactory factory = new OboFactory();
                        this._factory = factory;

                        bool isRetryInPhase = true;
                        executeInfo.CurRetryCount = 0;

                        while (isRetryInPhase)
                        {
                            isRetryInPhase = false;
                            
                            // 추후 Phase별 Retry 별 Count 정보가 필요
                            OutputWriter.Instance.WritePeggableWipInfo(targetGroup);

                            DoExecute(targetGroup, factory);

                            this.ShortManager.WriteShortReport(executeInfo.CurRetryCount);

                            var retryPegParts = this.ShortManager.CreateRetryPegPart(ShortType.InPhase, null);
                            if (retryPegParts != null && retryPegParts.Count > 0)
                            {
                                var isRetry = this.ShortManager.IsRetryInPhase(targetGroups, targetGroup, null, executeInfo.CurRetryCount);
                                if (isRetry)
                                {
                                    foreach (var retryPegPart in retryPegParts)
                                    {
                                        string retryTargetGroupKey = PBOInterface.ModuleControl.GetRetryTargetGroupKey(targetGroups, retryPegPart, targetGroupKey, executeInfo);

                                        bool isSameKey = retryTargetGroupKey == targetGroupKey;

                                        if (targetGroups.TryGetValue(retryTargetGroupKey, out var retryTargetGroup) == false)
                                        {
                                            retryTargetGroup = new APETargetGroup(retryTargetGroupKey);
                                            retryTargetGroup.CurrentOperation = retryPegParts.First().CurrentOperation;

                                            if (isSameKey)
                                            {
                                                isRetryInPhase = true;
                                                targetGroup = retryTargetGroup;
                                            }
                                            else
                                            {
                                                selTargetGroups.Add(retryTargetGroup);
                                            }
                                        }

                                        retryTargetGroup.Merge(retryPegPart);
                                        BackwardCommonLogic.Instance.SortedTargetInTargetGroup(retryTargetGroup, null, ATRuleAgent.Instance.CurrentRuleSet);
                                    }

                                    this.ShortManager.ClearShortInfo(ShortType.InPhase);
                                    executeInfo.CurRetryCount++;
                                }
                                else
                                {
                                    foreach (var retryPegPart in retryPegParts)
                                    {
                                        // KTR 추후 Short 난 정보들만 수행하여 OperTarget 정보를 출력하는 기능 추가할 때 해당 정보를 활용
                                        if (retryPegPart.Sample.Qty > ATOption.Instance.MinimumAllocationQuantity)
                                            executeInfo.AddShortDemand(retryPegPart.SampleTarget.Demand, retryPegPart.Sample.Qty);
                                    }
                                }
                            }
                        }

                        var nextRetryPegParts = this.ShortManager.CreateRetryPegPart(ShortType.NextPhase, null);
                        if (nextRetryPegParts != null && nextRetryPegParts.Count > 0)
                            nextRetryPegParts.ForEach(x => pegParts.Add(x));

                        this.ShortManager.ClearShortInfo();

                        if (ATOption.Instance.ApplyReSortTargetGroup)
                        {
                            comparePreset = ATRuleAgent.Instance.CurrentRuleSet.GetRule(RulePoint.CompareTargetGroup, CallType.Operation);
                            if (comparePreset != null)
                                selTargetGroups.Sort(new APETargetGroupComparer(comparePreset));
                        }
                    }

                    // 현재 확정계획 롤백의 경우 Confirm이 되는 경우에만 확정을 하고, Short인 경우 Rollback은 Phase 종료시점에 일괄적으로 진행
                    // 추후 수정 필요
                    foreach (var refPlan in ATInputData.RefPlans.GetRefPlans())
                    {
                        refPlan.VirtualUsedQty = 0;
                    }

                    PBOInterface.ModuleControl.OnEndPhase(phaseInfo, executeInfo);

                    Logger.MonitorInfo(string.Format(""));

                    ATElapsedTimeChecker.Instance.StopCustomTimer(key, true);
                }

                // Done 처리
                // 임시로 PegLogic의 WriteUnpeg 사용 -> 공용으로 사용 고려 필요

                foreach (var pegPart in pegParts)
                {
                    var pegTarget = pegPart.SampleTarget;
                    var so = pegTarget.SoDemand;
                    executeInfo.AddShortDemand(so, pegTarget.Qty);

                    so.LateInfoManager.AddShortInfo(so.ItemSiteBuffer, pegPart.CurrentBom, null, null, LateCategory.Etc, LateReason.RemainPegPart.ToString(), pegTarget.Qty, null, pegTarget.TargetDateTime, pegTarget.TargetDateTime, null);
                }

                var resources = executeInfo.Stage.Resources;
                foreach (var resource in resources)
                {
                    if (resource.Bucket == null)
                        continue;

                    PBOResource bucket = resource.Bucket as PBOResource;

                    foreach (var planInfo in bucket.Plans)
                    {
                        OutputWriter.Instance.WriteResPlan(planInfo);
                    }

                    var infos = bucket.CapacityManager.GetOrgCapaInfos();
                    foreach (var info in infos)
                    {
                        OutputWriter.Instance.WriteCapaAllocationPlan(bucket, info);
                    }
                }

                foreach (var constraint in ATConstraintAgent.Instance.GetContraints())
                {
                    foreach (var planInfo in constraint.Plans)
                    {
                        // Description에 ConstraintID 기록?
                        OutputWriter.Instance.WriteResPlan(planInfo);
                    }

                    foreach (var detail in constraint.ConstraintDetails)
                    {
                        OutputWriter.Instance.WriteCapaAllocationPlan(detail.Value.Constraint, detail.Value);
                    }
                }

                BackwardCommonLogic.Instance.WriteUnpeg(executeInfo);
                OutputWriter.Instance.WritePlanningResult();
                OutputWriter.Instance.WritePSIReport();
                //OutputWriter.Instance.WritePlanIndex(executeInfo.Key, executeInfo.Kpi);

                PBOInterface.ModuleControl.OnEndModule(executeInfo);
            }
            finally
            {
                Agents.ForEach(x => x.Dispose());
            }
        }

        public void DoExecute(ITargetGroup part, OboFactory factory)
        {
            var executeInfo = ATExecutionContext.Instance.CurrentExecutionInfo as PBOModuleExecutionInfo;

            factory.Init();

            HashSet<IAlignQueue> queues = new HashSet<IAlignQueue>();
            BackwardCommonLogic.Instance.SetAlignQueue(queues, executeInfo.Stage.Buffers);

            PBOBackwardPlannerControl control = new PBOBackwardPlannerControl();
            BackwardPlanner planner = new BackwardPlanner(queues, control);

            var targetGroup = part as APETargetGroup;

            targetGroup.Targets.Reverse();
            var pegParts = targetGroup.Targets.Select(x => x.Group); 

            // KTR : TargetGroup이 동작이 기존과 다를 것 같은데
            // 마지막 Buffer에서 PeggingGroup으로 들어가서 한번에 Pegging 하고
            planner.Initialize(pegParts);

            planner.DoExecute();

            // KTR Planner.Done();

            // KTR factory Initialize()
            factory.DoExecute();
            factory.Done();

            planner.AlignManager.Dispose();
        }

        public OboFactory GetCurrentFactory()
        {
            return _factory;
        }
    }
}
