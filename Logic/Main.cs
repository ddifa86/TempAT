using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Outputs;
using Mozart.SeePlan.Aleatorik.Persists;
using Mozart.SeePlan.Aleatorik.Inputs;

using Mozart.Task.Execution;
using Mozart.Extensions;
using Mozart.Collections;
using Mozart.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
 

namespace Mozart.SeePlan.Aleatorik.Logic
{
    [FeatureBind()]
    public partial class Main
    {
        public void RUN0(ModelContext context, ref bool handled)
        {
            var executor = new ModuleExecutor();
            executor.RunInternal(context);

            //var modules = context.GetOrderedExecutionModules().Select(m => m.Name).ToArray();

            //modules = modules.Where(x => x == "CustomPegged").ToArray();

            //ModelRunner.Execute(context, modules);
        }

        public void ON_INITIALIZE0(ModelContext context, ref bool handled)
        {
            //ElapsedTimeChecker.Instance.StartCustomTimer("ON_INITIALIZE0", useTimer: true);
            //try
            //{
            //    foreach (var stage in ATExecutionContext.Instance.Stages)
            //    {
            //        // Stage에 정보를 설정할 일이 있을까..?
            //        ATDataGenModule.Instance.Initialize(stage);

            //        ATDataGenModule.Instance.GenerateABomNet(stage);

            //        ATDataGenModule.Instance.InitItemSiteBufferInfo(stage);

            //        ATDataGenModule.Instance.Done(stage);
            //    }
            //}
            //finally
            //{
            //    ElapsedTimeChecker.Instance.StopCustomTimer("ON_INITIALIZE0", "ON_INITIALIZE0", false, true);
            //}
        }


 

        public void PROGRESS_REPORT0(ModelContext context, string stage, ref bool handled)
        {
            //switch (stage)
            //{
            //    case ATConstants.DATA_LOADING_START:
            //        ATElapsedTimeChecker.Instance.ResetTimer(ATConstants.PERSIST_IN);
            //        break;
            //    case ATConstants.DATA_LOADING_END:
            //        ATElapsedTimeChecker.Instance.AddElapsedTime(ATConstants.PERSIST_IN);
            //        break;
            //    case ATConstants.PEGGING_MODULE_START:
            //        ATElapsedTimeChecker.Instance.ResetTimer(ATConstants.PEG_MODULE);
            //        break;
            //    case ATConstants.PEGGING_MODULE_END:
            //        ATElapsedTimeChecker.Instance.AddElapsedTime(ATConstants.PEG_MODULE);
            //        break;
            //    case ATConstants.PLANNING_MODULE_START:
            //        ATElapsedTimeChecker.Instance.ResetTimer(ATConstants.PLAN_MODULE);
            //        break;
            //    case ATConstants.PLANNING_MODULE_END:
            //        ATElapsedTimeChecker.Instance.AddElapsedTime(ATConstants.PLAN_MODULE);
             //       OutputWriter.Instance.WriteExecutionTimeLog(context);
            //        break;
            //    case ATConstants.PEGGER_MODULE_START:
            //        ATElapsedTimeChecker.Instance.AddElapsedTime(ATConstants.PEGGER_MODULE);
            //        break;
                 
            //    default:
            //        break;
            //}
        }

        public void ON_DONE0(ModelContext context, ref bool handled)
        {
        }

        public void SHUTDOWN0(ModelTask task, ref bool handled)
        {
        }

        public void END_SETUP0(ModelTask task, ref bool handled)
        {
        }
    }
}