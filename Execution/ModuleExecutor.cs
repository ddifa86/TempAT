using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Mozart.Reflection;
using Mozart.Task.Execution;
using Mozart.Task.Execution.Framework;
using Mozart.SeePlan.Cbsim;
using Mozart.Simulation.Engine;
using Mozart.SeePlan.Aleatorik.Data;
using System.Reflection;
using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Inputs;

namespace Mozart.SeePlan.Aleatorik
{
    
    [ProvideExecutor(FEProvider.Aleatorik, Priority = 6)]
    public class ModuleExecutor : CombinedModuleBase
    {
        private Dictionary<string, Type> moduleTypes;

        protected override void Initialize(Task.Execution.ModelContext context)
        {
            
            base.Initialize(context);

            this.moduleTypes = new Dictionary<string, Type>();

            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            foreach (var type in assembly.GetTypes())
            {
                if (typeof(ExecutionModule).IsSameOrParentOf(type))
                {
                    var sampleModule = Activator.CreateInstance(type) as ExecutionModule; 

                    this.RegisterExecutionModuleType(sampleModule.Name, type);

                    continue;
                }
            }

            RegisterTypes();
        }

        private void RegisterTypes()
        {
            //MyObject Class 에 생성되는  "[FEBaseClassAttribute(Root = "BCP", IsTypeBinding = true, Mandatory = true, Description = null)]" 
            //부분이 BCP프로젝트/Site프로젝트에 따라 상이함(일부 Type 누락발생 )
            //다음과 같은 내용을 추가하여 MyObject에 노출된 클래스들이 정상적으로 
            //사용되도록 하였음.
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            foreach (var type in assembly.GetTypes())
            {
                if (type.BaseType != null)
                {
                    if (!type.BaseType.FullName.StartsWith("Mozart.SeePlan"))
                        continue;

                    var attr = type.GetCustomAttributes<FEBaseClassAttribute>().ToArray();
                    if (attr == null)
                        continue;

                    foreach (var it in attr)
                    {
                        if (it.Root == "PKG" && it.IsTypeBinding)
                        {
                            if (TypeRegistry.Resolve(type.BaseType) == null)
                            {
                                Mozart.Task.Execution.TypeRegistry.Register(type.BaseType, type, null);
                                break;
                            }
                        }
                    }
                }
            }

            //삭제논의sh
            //Mozart.Task.Execution.TypeRegistry.Register(typeof(Mozart.SeePlan.Cbsim.CbsLoadInfo), typeof(BCPLoadInfo), null);            
            //Mozart.Task.Execution.TypeRegistry.Register(typeof(Mozart.SeePlan.Cbsim.CbsBucket), typeof(BCPBucket), null);
//#if !BUILD_PKG
//            if (Mozart.Task.Execution.TypeRegistry.Resolve(typeof(Mozart.SeePlan.Cbsim.CbsBatch)) == null)
//            {
//                Mozart.Task.Execution.TypeRegistry.Register(typeof(Mozart.SeePlan.Cbsim.CbsBatch), typeof(PkgBatch), null);
//                Mozart.Task.Execution.TypeRegistry.Register(typeof(Mozart.SeePlan.Cbsim.CbsStep), typeof(PkgStep), null);
//                Mozart.Task.Execution.TypeRegistry.Register(typeof(Mozart.SeePlan.Cbsim.CbsPlan), typeof(PkgPlan), null);
//                Mozart.Task.Execution.TypeRegistry.Register(typeof(Mozart.SeePlan.Cbsim.CbsResource), typeof(PkgResource), null);
//            }
//#endif
        }
        private void AfterInputPersist()
        {
            #region error
            ErrorHelper.WriteError();
            ErrorHelper.ClearErrLogs();
            #endregion


            foreach (var entity in AleatorikInputMart.Instance.SCENARIO_OPTION_CONFIG.Rows)
            {
                if (entity.MODULE_ID == ModuleType.Global.ToString())
                    continue;

                OutputWriter.Instance.WriteExecutionOptionConfigLog(entity);

                var scenario = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;

                if (entity.SCENARIO_ID != scenario)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                    entity, ErrKey.ScenarioOptionConfig, "", string.Format(" Target Scenario ID : {0}", scenario));
                    continue;
                }

                var info = ATExecutionContext.Instance.GetExecutionInfo(entity.MODULE_ID);
                if (info == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                    entity, ErrKey.ScenarioOptionConfig, "EXECUTION_PLAN@MODULE_KEY", ErrDesc.NotFoundReferredData("MODULE_KEY", "EXECUTION_PLAN"));

                    continue;
                }

                if (entity.OPTION_ID == ATReservedCode.DEFAULT_RULE_SET)
                {
                    if (entity.OPTION_VALUE.IsNullOrEmpty())
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Critical, ErrorReasonCode.IncompatibleRule,
                            entity, ErrKey.ScenarioOptionConfig, "RULESET_MASTER@RULESET_ID",
                            "The “DefaultRuleSet” option of the " + entity.MODULE_ID + " module has invalid value.");
                        continue;
                    }

                    var ruleSet = ATRuleAgent.Instance.GetRuleSet(entity.OPTION_VALUE);
                    if (ruleSet == null)
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Critical, ErrorReasonCode.IncompatibleRule,
                        entity, ErrKey.ScenarioOptionConfig, "RULESET_MASTER@RULESET_ID",
                        "The “DefaultRuleSet” option of the " + entity.MODULE_ID + " module has invalid value.");
                        continue;
                    }
                }

                var option = info.GetOption(entity.PHASE_NO);
                option[entity.OPTION_ID] = entity.OPTION_VALUE;
            }

            foreach (var entity in ATInputData.Wips.GetPersistUnpegWip())
            {
                OutputWriter.Instance.WriteUnpegInfo(entity.Key, entity.Value);
            }
        }
        protected override void Execute(Task.Execution.ModelContext context)
        {
            ATElapsedTimeChecker.Instance.StopCustomTimer("DataLoad",true);

            var pkgContext = ATExecutionContext.Instance;
            var executionInfos = pkgContext.ExecutionInfos;

            AfterInputPersist();

            if (executionInfos.Count == 0)
            {
                OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Critical, ErrorReasonCode.IncompatibleConfig, null, string.Empty,
                    "EXECUTION_PLAN@SCENARIO_ID", "The “PlanScenario” argument of the Experiment has invalid value");
            }

            if (ATPersistHelper.HasErrorData)
            {
                Logger.MonitorInfo(ATPersistHelper.ErrorStr.ToString());
                return;
            }

            ATElapsedTimeChecker.Instance.StartCustomTimer("BomNetwork_Generate");

            ATDataGenModule.Instance.Initialize();

            ATElapsedTimeChecker.Instance.StopCustomTimer("BomNetwork_Generate", true);

            //ATElapsedTimeChecker.Instance.StartCustomTimer("Engine_Total");

            for (int i = 0; i < executionInfos.Count; i++)
            {
                var info = pkgContext.GetExecutionInfo(i, true);
                var moduleType = info.ModuleType.ToString();

                pkgContext.CurrentExecutionStep = ATExecutionContext.ExecutionStep.MODULE_INIT;

                ExecutionModule module = this.CreateExecutionModule(moduleType);
                if (module == null)
                {
                    //BCPWriter.WriteErrorHistory_EnumParseFail(BCPConstants.ErrorCategoryFailToGenerateExecutionInfo, typeof(ModuleType), BCPConstants.TBL_EXECUTION_CONFIG, BCPConstants.COL_EXE_MODULE_ID, moduleType);
                    break;
                }

                string key = string.Format("{0}_Total", info.Key);

                ATElapsedTimeChecker.Instance.StartCustomTimer(key);

                info.OnStarted();

                //control.OnBeginModule(module, info);

                pkgContext.CurrentExecutionStep = ATExecutionContext.ExecutionStep.MODULE_RUN;

                module.Execute(context);

                info.OnEnded();

                ATElapsedTimeChecker.Instance.StopCustomTimer(key, true);

                if (context.HasErrors)
                    return;
            }

            pkgContext.CurrentExecutionStep = ATExecutionContext.ExecutionStep.DONE;

            ATElapsedTimeChecker.Instance.StopCustomTimer("Engine_Total", true);


            #region 시간 출력

            var elapesed = ATElapsedTimeChecker.Instance.ElapsedTimeByType;

            foreach (KeyValuePair<string, TimeInfo> entry in elapesed)
            {
                OutputWriter.Instance.WriteElapsedTimeLog(entry.Key, entry.Value);
            }

            //ATElapsedTimeChecker.Instance.PrintElapsedTimes();

            ATElapsedTimeChecker.Instance.CleartElapsedTimes();

            #endregion
           
        }


        private void RegisterExecutionModuleType(string moduleKey, Type moduleType)
        {
            if (this.moduleTypes == null)
                this.moduleTypes = new Dictionary<string, Type>();

            this.moduleTypes.Add(moduleKey, moduleType);
        }

        private ExecutionModule CreateExecutionModule(string moduleKey)
        {
            Type moduleType;
            if (!this.moduleTypes.TryGetValue(moduleKey, out moduleType))
                return null;

            var module = Activator.CreateInstance(moduleType) as ExecutionModule;
            return module;
        }

        internal void RunInternal(ModelContext context)
        {
            this.Initialize(context);
            this.Execute(context);
        }
    }
}
