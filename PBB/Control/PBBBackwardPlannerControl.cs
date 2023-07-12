using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Logic;
using Mozart.SeePlan.Aleatorik.Pegger;
using Mozart.SeePlan.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class PBBBackwardPlannerControl : BackwardPlannerControl
    {
        #region AlignQueue
        public override void OnSelectPart(ITargetGroup part)
        {
            BackwardCommonLogic.Instance.SetLastOper(part as APEPegPart);
        }

        public override IComparable GetAlignKey(ITargetGroup part)
        {
            return BackwardCommonLogic.Instance.GetAlignKey(part);
        }

        public override IComparable GetMergedTargetGroupKey(ITargetGroup part)
        {
            var operation = part.Step as ATOperation;

            if (operation.IsBuffer == false)
                return null;

            var pegPart = part as APEPegPart;
            ATRuleSet ruleSet = ATRuleAgent.Instance.GetRuleSet(pegPart.CurrentOperation);

            var buffer = pegPart.CurrentOperation as ATBuffer;
            IComparable key = null;
            var factors = ruleSet.GetRule(RulePoint.PeggingGroupKey, CallType.Operation);
            if (factors == null || factors.FactorList.Count <= 0)
            {
                // KTR : 이런건 Predefine Factor로 해야하지 않을까?
                key = buffer.BufferID + pegPart.SampleTarget.SoDemand.DueDateTime.WeekOfYear();
            }
            else
            {
                foreach (var factor in factors.FactorList)
                {
                    var method = factor.Method as PeggingGroupKey;
                    if (key == null)
                        key = method(pegPart);
                    else
                        key += "@" + method(pegPart);
                }
            }

            return key;
        }

        public override void OnCompareEntities(List<IMergedTargetGroup> entities, IAlignQueue queue)
        {
            var buffer = (queue as ATAlignQueue).Buffer;

            PBBBackwardLogic.Instance.ComparePeggingGroup(entities, buffer);
        }
        #endregion

        #region Backward Logic
        public override void OnPushPart(ITargetGroup part)
        {
            BackwardCommonLogic.Instance.SetRetryPegPart(part);
        }

        public override void OnSelectGroup(ITargetGroup part)
        {
            var targetGroup = part as APEPeggingGroup;

            PBBInterface.PegControl.OnSelectedPeggingGroup(targetGroup);
        }

        public override List<ITargetGroup> SplitTargetGroup(ITargetGroup part)
        {
            List<ITargetGroup> targetGroups = new List<ITargetGroup>();
            var operation = part.Step as ATOperation;

            if (operation.IsBuffer)
            {
                // KTR : PeggingGroup인 경우에는 PegPart를 나누는 작업을 해야지?

                foreach (var target in part.Targets)
                {
                    var pegPart = target.Group as APEPegPart;
                    var splitPegParts = PBBInterface.PegControl.CustomSplitPegPart(pegPart);

                    targetGroups.AddRange(splitPegParts);
                }
            }
            else
            {
                targetGroups.Add(part);
            }

            return targetGroups;
        }

        public override List<ITargetGroup> SelectBom(ITargetGroup part)
        {
            return BackwardCommonLogic.Instance.SelectBom(part, BWExecutor.Current.ShortManager);
        }

        public override void OnStepOut(ITargetGroup part)
        {
            BackwardCommonLogic.Instance.OnStepOut(part);
        }

        public override void OnStepIn(ITargetGroup part)
        {
            BackwardCommonLogic.Instance.PegControl.OnStepIn(part as APEPegPart);
        }

        public override void CreateTarget(ITargetGroup part, bool isOut)
        {
            PBBBackwardLogic.Instance.CreateTarget(part, isOut);
        }

        public override void ApplyYield(ITargetGroup part)
        {
            BackwardCommonLogic.Instance.ApplyYield(part);
        }

        public override void ApplyTat(ITargetGroup part, bool isOut)
        {
            BackwardCommonLogic.Instance.ApplyTat(part, isOut);
        }

        public override Step GetPrevStep(ITargetGroup part)
        {
            var pegPart = part as APEPegPart;
            var prevOper = BackwardCommonLogic.Instance.GetPrevOper(pegPart);

            return prevOper;
        }

        public override bool IsFinished(ITargetGroup part)
        {
            bool isFinished = false;
            var operation = part.Step as ATOperation;
            var pegPart = part as APEPegPart;

            bool isShort = BackwardCommonLogic.Instance.IsShortPegPart(pegPart, BWExecutor.Current.ShortManager);

            if (isShort || pegPart.IsShort)
                return true;

            if (operation.IsBuffer == false)
                return isFinished;

            if (BackwardCommonLogic.Instance.IsInputBuffer(pegPart.CurrentItemSiteBuffer, pegPart.CurrentOperation))
            {
                BWExecutor.Current.ShortManager.AddInTargetPegPart(pegPart);
                pegPart.CurrentOperation = null;

                isFinished = true;
            }

            return isFinished;
        }

        public override List<object> GetPartChangeInfos(ITargetGroup part, bool isOut)
        {
            var shortManager = BWExecutor.Current.ShortManager;
            return BackwardCommonLogic.Instance.GetPartChangeInfos(part, isOut, shortManager);
        }

        public override ITargetGroup ApplyPartChangeInfo(ITargetGroup part, object partChangeInfo, bool isOut)
        {
            return BackwardCommonLogic.Instance.ApplyPartChangeInfo(part, partChangeInfo, isOut);
        }
        #endregion

        #region Pegging
        public override void DoPegging(ITargetGroup part, bool isOut)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("DoPegging");
            try
            {
                var peggedWips = BackwardCommonLogic.Instance.DoPegging(part, isOut);

                // 임시 위치 LWS
                foreach (var peggedWip in peggedWips)
                {
                    PBBBackwardLogic.Instance.CommitPeggedWip(peggedWip, true);
                }
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("DoPegging");
            }
        }
        #endregion
    }
}
