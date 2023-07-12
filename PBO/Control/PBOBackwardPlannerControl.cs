using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Logic;
using Mozart.SeePlan.Aleatorik.ObyO;
using Mozart.SeePlan.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class PBOBackwardPlannerControl : BackwardPlannerControl
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
            return ATConstants.DefaultKey;
        }

        #endregion

        #region Backward Logic
        public override void OnPushPart(ITargetGroup part)
        {
            BackwardCommonLogic.Instance.SetRetryPegPart(part);
        }

        public override List<ITargetGroup> SplitTargetGroup(ITargetGroup part)
        {
            // 아래 변수명은 PegParts로 변경되어야 하지 않을까?
            List<ITargetGroup> targetGroups = new List<ITargetGroup>();
            var operation = part.Step as ATOperation;

            if (operation.IsBuffer)
            {
                foreach (var target in part.Targets)
                {
                    // KTR : 커스텀 호출 여부차이 때문에 있는건가? 왜 있는거지...?
                    var pegPart = target.Group as APEPegPart;
                    targetGroups.Add(pegPart);
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
            return BackwardCommonLogic.Instance.SelectBom(part, OboExecutor.Current.ShortManager);
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
            PBOBackwardLogic.Instance.CreateTarget(part, isOut);
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
            // KTR 공통 로직인거 같은데?
            var pegPart = part as APEPegPart;
            var prevOper = BackwardCommonLogic.Instance.GetPrevOper(pegPart);

            return prevOper;
        }

        public override bool IsFinished(ITargetGroup part)
        {
            bool isFinished = false;
            var operation = part.Step as ATOperation;
            var pegPart = part as APEPegPart;

            bool isShort = BackwardCommonLogic.Instance.IsShortPegPart(pegPart, OboExecutor.Current.ShortManager);

            if (isShort || pegPart.IsShort)
                return true;

            if (operation.IsBuffer == false)
                return isFinished;

            var pegTarget = pegPart.SampleTarget;
             
            if (BackwardCommonLogic.Instance.IsInputBuffer(pegPart.CurrentItemSiteBuffer, pegPart.CurrentOperation))
            {
                // KTR : 여기도 통일 할 수 있을 것 같은데..?
                var target = pegTarget.CurOperTarget.DeepCopy();
                target.TargetQty = pegTarget.RemainQty;

                // KTR 해당 영역은 FW Init할 때 처리하는 부분으로 보임.  구현 위치 이동.하고 안의 로직 
                // 통일 할 수 있는지 고려.
                var factory = OboExecutor.Current.GetCurrentFactory();
                factory.AddInTarget(target);

                pegPart.CurrentOperation = null;
                pegPart.PegPosition = PegPosition.None;

                isFinished = true;
            }

            return isFinished;
        }

        public override List<object> GetPartChangeInfos(ITargetGroup part, bool isOut)
        {
            var shortManager = OboExecutor.Current.ShortManager;
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

                // Factory(F/W Engine) 와 PBO B/W Engine의 구분 작업 필요
                // B/W의 결과물을 가지고 F/W Init 시점에 Add 하도록 수행

                var factory = OboExecutor.Current.GetCurrentFactory();

                var pegTarget = part.Sample as APETarget;
                var pegPart = pegTarget.Group as APEPegPart;

                foreach (var peggedWip in peggedWips)
                {
                    factory.AddPeggedWip(peggedWip);
                    // Pegging하면서 호출하지 않고 마지막 시점에 호출해야 할 필요가 있을까?? LWS
                    PBOInterface.PegControl.OnCompleteVirtualWipPegging(pegPart, pegTarget, peggedWip, peggedWip.PegContext, isOut);
                }

                return;
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("DoPegging");
            }
        }
        #endregion
    }
}
