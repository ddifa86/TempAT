using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Pegger;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class PBBBackwardLogic
    {
        public static PBBBackwardLogic Instance
        {
            get
            {
                return ServiceLocator.Resolve<PBBBackwardLogic>();
            }

        }

        /// <summary>
        /// 어느 시점에 아래 메서드를 호출할 것인지? LWS
        /// </summary>
        /// <param name="wip"></param>
        /// <param name="isCommit"></param>
        public void CommitPeggedWip(APEWip wip, bool isCommit)
        {
            var peggedWip = BackwardCommonLogic.Instance.CommitPeggedWip(wip.Qty, wip, isCommit);

            if (peggedWip != null)
            {
                var executeInfo = ATExecutionContext.Instance.CurrentExecutionInfo as PBBModuleExecutionInfo;
                executeInfo.AddPegWips(peggedWip);
            }
        }

        public bool IsRetryInPhase(List<APEPegPart> pegParts, APEShortManager shortManager, int retryCount)
        {
            bool isRetry = false;

            if (retryCount < ATOption.Instance.RetryCount)
                isRetry = true;

            isRetry = PBBInterface.ModuleControl.IsRetryInPhase(pegParts, isRetry, shortManager, retryCount);

            return isRetry;
        }
        public void CreateTarget(ITargetGroup part, bool isOut)
        {
            part.Apply((x, _) =>
            {
                var pegPart = x as APEPegPart;

                var target = BackwardCommonLogic.Instance.CreateTarget(pegPart, isOut);

                var executionInfo = ATExecutionContext.Instance.CurrentExecutionInfo as PBBModuleExecutionInfo;

                BackwardCommonLogic.Instance.AddOperTarget(executionInfo, target);

                // KTR 확정되지 않은 Target을 바로 Output 하고 있는 것 같은데, 이슈가 있어 보임.
                // 기타 확정되지 않은 결과를 적고 있는 요소가 있는지 확인 필요.
                OutputWriter.Instance.WriteTargetPlan(pegPart, target, isOut, target.TargetQty);

                // Assembly Tartet 여부 확인.
                if (BackwardCommonLogic.Instance.IsAssemblyTarget(target, pegPart.CurrentOperation, isOut))
                {
                    var buffer = target.CurrentBom.FromBuffer;

                    List<ATOperTarget> lst;
                    if (executionInfo.StageInAssemblyTargets.TryGetValue(buffer, out lst) == false)
                    {
                        lst = new List<ATOperTarget>();

                        executionInfo.StageInAssemblyTargets.Add(buffer, lst);
                    }

                    var copyTarget = target.DeepCopy();
                    lst.Add(target.DeepCopy());

                    BackwardCommonLogic.Instance.AddOperTarget(executionInfo, copyTarget); // 왜 필요한지 모르겠음...
                }
            });
        }

        public void ComparePeggingGroup(List<IMergedTargetGroup> groups, ATBuffer buffer)
        {
            var ruleSet = ATRuleAgent.Instance.GetRuleSet(buffer);
            var preset = ruleSet.GetRule(RulePoint.ComparePeggingGroup, CallType.Operation);
            groups.Sort(new APEPeggingGroupComparer(preset));
        }

        public void WriteStageInTarget(APEPegPart pegPart)
        {
            var executionInfo = ATExecutionContext.Instance.CurrentExecutionInfo as PBBModuleExecutionInfo;

            foreach (APETarget pegTarget in pegPart.Targets)
            {
                if (pegTarget.Qty <= ATOption.Instance.MinimumAllocationQuantity)
                    continue;

                ATOperTarget target = pegTarget.CurOperTarget.DeepCopy();
                target.TargetQty = pegTarget.Qty;

                OutputWriter.Instance.WriteInTargetPlan(target, pegTarget.PegPart);

                executionInfo.StageInTargets.Add(target);

                BackwardCommonLogic.Instance.AddOperTarget(executionInfo, target);
            }

        }

        public void CalculateKitInfo()
        {
        }

        public void SelectKit()
        {
        }
    }
}
