using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.EngineBase;
using Mozart.SeePlan.Aleatorik.Inputs;
using Mozart.SeePlan.Aleatorik.Pegger.Data;
using Mozart.SeePlan.DataModel;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Pegger
{
    public class BWExecutor : IModelExecutor
    {
        public static BWExecutor Current => ServiceLocator.Resolve<BWExecutor>();

        internal HashSet<IAgent> Agents;

        public PBBShortManager ShortManager { get; set; }

        public BWExecutor()
        {
            if (!ServiceLocator.IsRegistered<BWExecutor>())
            {
                ServiceLocator.RegisterInstance(this);

                Agents = new HashSet<IAgent>();
                Agents.Add(ATRuleAgent.Instance);
                Agents.Add(ATISBQueueAgent.Instance);
                Agents.Add(APEWipQueueAgent.Instance);
                Agents.Add(ATLateInfoTrackerAgent.Instance);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        void IModelExecutor.Initialize(ModelContext context)
        {
            var executeInfo = ATExecutionContext.Instance.CurrentExecutionInfo as PBBModuleExecutionInfo;
            BackwardCommonLogic.Instance.PegControl = PBBInterface.PegControl;

            string key = string.Format("{0}_Initialize", executeInfo.Key);
            ATElapsedTimeChecker.Instance.StartCustomTimer(key);

            try
            {
                ATOption.Instance.SetDefaultOptionValue();

                Agents.ForEach(x => x.Initialize());

                #region InitModule

                // KTR: 해당 KPI는 추후 삭제.
                executeInfo.Kpi = new APEKpiSummary();

                PBBInterface.ModuleControl.OnStartModule(executeInfo);

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
                    }

                    // Demand
                    executeInfo.Demands = PBBInterface.InitControl.PrepareDemand(executeInfo, executeInfo.RefModules);
                    executeInfo.Demands.ForEach(x => ATInputData.Demands.AddDemand(x));

                    PBBInterface.InitControl.OnPrepareCustomInput(executeInfo, executeInfo.RefModules);
                }
                #endregion
            }
            finally
            {
                ATElapsedTimeChecker.Instance.StopCustomTimer(key, false);
            }
        }

        private HashSet<APEPegPart> CreatePegPart(List<ATDemand> demands)
        {
            var moMasters = BackwardCommonLogic.Instance.CreateMoMaster(demands);
            var pegParts = BackwardCommonLogic.Instance.CreatePegPart(moMasters.Values);

            return pegParts;
        }

        void IModelExecutor.Execute(ModelContext context)
        {
            try
            {
                var executeInfo = ATExecutionContext.Instance.CurrentExecutionInfo as PBBModuleExecutionInfo;

                var demands = executeInfo.Demands;

                if (demands.Count == 0)
                {
                    Logger.MonitorInfo("-----------Please set up demand information.-----------");

#warning 에러 처리 추가 필요.
                    // 에러 정보를 MODULE EXECUTOR 에서 받아서 처리 가능하도록 구조를 잡고 진행 필요.
                    // errKey
                    OutputWriter.Instance.WriteErrorLog(ModuleType.PBB, executeInfo.Key, ErrorSeverity.Critical, ErrorReasonCode.IncompatibleConfig, null, "", "SALES_ORDER@SO_ID", "No Sales Order Available");
                    return;
                }

                demands = PBBInterface.InitControl.DemandSmoothing(demands);

                var pegParts = CreatePegPart(demands);
                executeInfo.PegParts = pegParts;

                Logger.MonitorInfo(string.Format("ModuleKey : {0}, ModuleType : {1}, PegParts : {2}", executeInfo.Key, executeInfo.ModuleType, pegParts.Count));
                
                BackwardCommonLogic.Instance.CreatePlanWip();

                executeInfo.CurPhase = 0;
                executeInfo.CurRetryCount = 0;

                while (executeInfo.CurPhase < executeInfo.PhaseCount)
                {
                    executeInfo.CurPhase++;

                    string key = string.Format("{0}_PHASE_{1}", executeInfo.Key, executeInfo.CurPhase);
                    ATElapsedTimeChecker.Instance.StartCustomTimer(key);

                    var option = executeInfo.GetOption(executeInfo.CurPhase);
                    ATOption.Instance.Update(option);

                    #region Phase별 PegPart Filter
                    var preset = ATRuleAgent.Instance.CurrentRuleSet.GetRule(RulePoint.FilterTarget, CallType.Operation);
                    HashSet<APEPegPart> selPegParts = BackwardCommonLogic.Instance.FilterTarget(executeInfo.PegParts, preset);

                    BWPhaseInfo phaseInfo = new BWPhaseInfo(executeInfo.PegParts, selPegParts, executeInfo.CurPhase);

                    PBBInterface.ModuleControl.OnStartPhase(phaseInfo);

                    Logger.MonitorInfo(string.Format("\t\t+ # Phase {0} start => Target PegParts : {1}, now : {2}", executeInfo.CurPhase, selPegParts.Count, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")));

                    if (selPegParts.Count == 0)
                    {
                        ATElapsedTimeChecker.Instance.StopCustomTimer(key, true);
                        PBBInterface.ModuleControl.OnEndPhase(phaseInfo);

                        continue;
                    }
                    #endregion

                    foreach (var pegPart in selPegParts)
                    {
                        executeInfo.PegParts.Remove(pegPart);
                    }

                    this.ShortManager = new PBBShortManager(selPegParts);

                    bool isRetryInPhase = true;
                    executeInfo.CurRetryCount = 0;

                    while (isRetryInPhase)
                    {
                        isRetryInPhase = false;

                        this.DoExecute(selPegParts, context);

                        
                        var remainPegParts = this.ShortManager.GetInTargetPegParts();
                        var retryPegParts = this.ShortManager.CreateRetryPegPart(ShortType.InPhase, remainPegParts);

                        if (retryPegParts != null && retryPegParts.Count > 0)
                        {
                            // KTR : null 을 던져야 하는 경우에는 null 이 들어가는 argument의 우선순위가 떨어짐을 의미하고,
                            // argument 순서가 마지막으로 배치되어야 함.
                            // 해당 함수가 모듈별로 다른 거라면 꼭 Argument가 생겨먹은게 같아야 하나요??
                            var isRetry = this.ShortManager.IsRetryInPhase(null, null, retryPegParts, executeInfo.CurRetryCount);
                            if (isRetry)
                            {
                                selPegParts = retryPegParts.ToHashSet();

                                isRetryInPhase = true;
                                executeInfo.CurRetryCount++;
                                this.ShortManager.ClearShortInfo(ShortType.InPhase);
                            }
                            else
                            {
                                // KTR retry와 remain을 분리하는 로직..?
                                // Remain과 Short을 여기서는 분리하여 따로 로직을 생성해야 할 것으로 생각됨.
                                // 위의 retry 영역에서는 short과 remain이 같은 moID인 경우에는 합치고
                                // retry 하지 않는 경우에는 분리하여 로직 작업.
                                foreach (var retryPegPart in retryPegParts)
                                {
                                    if (retryPegPart.Type == PegPartType.Short)
                                    {
                                        // ShortDemand에 Add를 하는데 무슨 의미가 있는지 모르겠음...
                                        // 추후 Short 수량에 대해서도 OperTarget을 적어달라고 하면 
                                        // 적어줄 수 있도록 별도 작업 필요함. ( PBO 공통 로직 )
                                    }
                                }

                                // KTR  확정 시점에만 Oper Target을 작성해야함.
                                foreach (var remainPegPart in remainPegParts)
                                {
                                    if (ATOption.Instance.TransferShortToNextPhase)
                                        executeInfo.PegParts.Add(remainPegPart);
                                    else
                                        PBBBackwardLogic.Instance.WriteStageInTarget(remainPegPart); // InTarget이 확정되는 시점 (이 위치에서 추후 Write하는 Logic이 추가되어야함)
                                }
                            }
                        }

                        this.ShortManager.ClearInTargetPegParts();
                    }

                    var nextRetryPegParts = this.ShortManager.CreateRetryPegPart(ShortType.NextPhase, null);
                    if (nextRetryPegParts != null && nextRetryPegParts.Count > 0)
                        nextRetryPegParts.ForEach(x => executeInfo.PegParts.Add(x));

                    this.ShortManager.ClearShortInfo();

                    PBBInterface.ModuleControl.OnEndPhase(phaseInfo);
                    ATElapsedTimeChecker.Instance.StopCustomTimer(key, true);
                }

                BackwardCommonLogic.Instance.WriteUnpeg(executeInfo);

                Logger.MonitorInfo("End Module");

                PBBInterface.ModuleControl.OnEndModule(executeInfo);
                OutputWriter.Instance.WriteShortLog();
                Agents.ForEach(x => x.Dispose());
            }
            finally
            {
            }
            
        }

        private void DoExecute(HashSet<APEPegPart> pegParts, ModelContext context)
        {
            var executeInfo = ATExecutionContext.Instance.CurrentExecutionInfo as PBBModuleExecutionInfo;

            HashSet<IAlignQueue> queues = new HashSet<IAlignQueue>();
            BackwardCommonLogic.Instance.SetAlignQueue(queues, executeInfo.Stage.Buffers);

            PBBBackwardPlannerControl control = new PBBBackwardPlannerControl();
            BackwardPlanner planner = new BackwardPlanner(queues, control);

            planner.Initialize(pegParts);

            planner.DoExecute();

            // KTR: 해당 부분은 Planner.Dispose()에서 처리가 되어야할 부분으로 보임
            planner.AlignManager.Dispose();
        }
    }
}
