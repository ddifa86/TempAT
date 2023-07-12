//using Mozart.SeePlan.Aleatorik.Data;
//using Mozart.SeePlan.Aleatorik.Logic;
//using Mozart.Task.Execution;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Mozart.Extensions;


//namespace Mozart.SeePlan.Aleatorik.ObyO
//{
//    public partial class OboLogic
//    {
//        public List<APEPegPart> OnPartChange(APEPegPart pegPart, bool isOut)
//        {
//            ATElapsedTimeChecker.Instance.ResetTimer("GetPartChangeInfos");
//            try
//            {
//                List<ATBomDetail> partchangeInfos = OboInterfaces.PegControl.GetPartChangeInfos(pegPart, isOut);

//                if (partchangeInfos == null || partchangeInfos.Count == 0)
//                    return new List<APEPegPart>();

//                if (partchangeInfos.Count == 1)
//                {
//                    OboInterfaces.PegControl.ApplyPartChangeInfo(pegPart, partchangeInfos[0], isOut);                    

//                    return new List<APEPegPart>();
//                }

//                List<APEPegPart> branchList = new List<APEPegPart>();
//                foreach (object info in partchangeInfos)
//                {
//                    APEPegPart copied = pegPart.Clone();
                    
//                    APEPegPart branch = OboInterfaces.PegControl.ApplyPartChangeInfo(copied, info, isOut);

//                    if (pegPart.CurrentBom.BomType == BomType.Assembly)
//                    {
//                        ATAssyInfo assyInfo = new ATAssyInfo(pegPart.CurrentBom, pegPart.SampleTarget.CurOperTarget, branch.CurrentItemSiteBuffer);

//                        // PegPart의 RootAssyInfo 등록 필요. => Short 난 경우 Merge를 위한 작업.
//                        if (copied.RootAssyInfo == null)
//                            copied.RootAssyInfo = assyInfo;
//                    }

//                    branchList.Add(branch);
//                }

//                return branchList;
//            }
//            finally
//            {
//                ATElapsedTimeChecker.Instance.AddElapsedTime("GetPartChangeInfos");
//            }
//        }

//        public void ApplyYield(APEPegPart pegPart)
//        {
//            ATElapsedTimeChecker.Instance.ResetTimer("ApplyYield");
//            try
//            {
//                ATOperation oper = pegPart.CurrentStep as ATOperation;
//                APETarget pegTarget = pegPart.Sample as APETarget;

//                double yield = pegTarget.CurOperTarget.Yield;

//                var oldQty = pegTarget.RemainQty;
//                var newQty = oldQty / yield;

//                pegTarget.RemainQty = newQty;

//                // 전환 수량 정보 업데이트
//                pegTarget.BCumChangeRatio *= ATUtil.ConvertValue(1, 1, yield, PlanType.Backward);
//            }
//            finally
//            {
//                ATElapsedTimeChecker.Instance.AddElapsedTime("ApplyYield");
//            }
//        }

//        

//        public void ApplyTat(APEPegPart pegPart, bool isOut)
//        {
//            /// control 내용?
//            APETarget pegTarget = pegPart.Sample as APETarget;
//            ATOperation oper = pegPart.CurrentStep as ATOperation;
//            double tat = oper.GetTat(pegTarget.TargetDateTime, isOut);             

//            TimeSpan val = TimeSpan.FromSeconds(tat);
//            pegTarget.TargetDateTime = pegTarget.TargetDateTime - val;
//        }

//        public ATOperation GetPrevOperation(APEPegPart pegPart)
//        {
//            var step = pegPart.CurrentStep;

//            // BufferStep의 경우
//            if (step.IsBuffer)
//            {
//                if (BackwardCommonLogic.Instance.IsInputBuffer(pegPart.CurrentItemSiteBuffer, pegPart.CurrentStep) == true)
//                    return null;

//                if (pegPart.IsShort)
//                    return null;

//                if (pegPart.CurrentRoute == null)
//                    return null;

//                step = pegPart.CurrentRoute.Opers.Last as ATOperation;
//                return step;
//            }
//            else
//            {
//                // Route의 경우, 첫공정의 이전 공정은 BomDetail의 FromBuffer
//                step = pegPart.CurrentStep.GetDefaultPrevStep() as ATOperation;

//                if (step == null)
//                    step = pegPart.CurrentBomDetail.FromBuffer;
//            }

//            return step;
//        }
//    }
//}
