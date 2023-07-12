//using Mozart.SeePlan.Aleatorik.Data;
//using Mozart.Task.Execution;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Mozart.SeePlan.Aleatorik.Pegger
//{
//    public class BWLine
//    {
//        public BWLine()
//        { 
//        }

//        public bool IsShortPegPart(APEPegPart pegPart)
//        {
//            var pegTarget = pegPart.SampleTarget;

//            if (pegPart.CurrentStep.IsBuffer)
//            {
//                if (pegTarget.MaxLateTargetDateTime < ATOption.Instance.PlanStartTime)
//                {
//                    foreach (var target in pegPart.Targets)
//                    {
//                        var pt = target as APETarget;
//                        var so = pt.Demand;
//                        double cumTat = so.ItemSiteBuffer.MinCumTat - pt.PegPart.CurrentItemSiteBuffer.MinCumTat;
//                        double remain = (so.DueDateTime - ATOption.Instance.PlanStartTime).TotalSeconds;
//                        string detail = string.Format("Buffer : {0}, Remain : {1}, CumTat : {2}", pegPart.CurrentStep.OperID, remain, cumTat);

//                        string reason = LateReason.BWBomPathShort.ToString();

//                        if (cumTat >= (so.DueDateTime - ATOption.Instance.PlanStartTime).TotalSeconds)
//                            reason = LateReason.LackOfWip.ToString();

//                        pt.AddShortInfo(LateCategory.Tat, reason, pegTarget.RemainQty, detail, pt.TargetDateTime, pt.TargetDateTime);
//                    }

//                    pegPart.Status = Status.Short;
//                }
//            }

//            if (BWInterface.PeggerControl.IsShortPegPart(pegPart) == true)
//            {
//                pegPart.CurrentStep = null;
//                return true;
//            }

//            return false;
//        }

//        public void MoveFirst(APEPegPart pegPart)
//        {
//            pegPart.CurrentStep = PeggerLogic.Instance.GetPrevPeggingStep(pegPart);

//            if (IsShortPegPart(pegPart) == true)
//                return;

//            if (pegPart.CurrentStep == null || pegPart.CurrentStep.OperType != OperType.Buffer)
//                return;

//            AlignBufferAgent.Instance.AddAlignPegPart(pegPart);
//        }

//        /// <summary>
//        /// PrevStep을 찾아서 NextStep으로 Move하는 메서드 
//        /// </summary>
//        /// <param name="pegPart"></param>
//        public void MoveNext(APEPegPart pegPart)
//        {
//            while (pegPart != null)
//            {
//                var prevStep = PeggerLogic.Instance.GetPrevPeggingStep(pegPart);

//                if (prevStep == null)
//                {
//                    if (BackwardCommonLogic.Instance.IsInputBuffer(pegPart.CurrentItemSiteBuffer, pegPart.CurrentStep))
//                    {
//                        AlignBufferAgent.Instance.AddInTargetPegPart(pegPart);
//                    }
//                    else
//                    {
//                        foreach (var pt in pegPart.Targets)
//                        {
//                            pt.AddShortInfo(LateCategory.Tat, LateReason.NoBwBomPathShort.ToString(), pt.RemainQty, null, pt.TargetDateTime, pt.TargetDateTime);
//                        }
//                    }
                    
//                    break;
//                }

//                pegPart.CurrentStep = prevStep;

//                if (IsShortPegPart(pegPart) == true)
//                    break;

//                if (pegPart.CurrentStep.OperType == OperType.Buffer)
//                {
//                    AlignBufferAgent.Instance.AddAlignPegPart(pegPart);
//                    break;
//                }
//                else
//                {
//                    pegPart = MainFlow(pegPart, pegPart.CurrentStep);
//                }
//            }
//        }

//        /// <summary>
//        /// B/W 로직을 전체적으로 진행하는 메서드
//        /// </summary>
//        /// <param name="pegPart"></param>
//        /// <param name="isBuffer"></param>
//        /// <returns></returns>
//        public APEPegPart MainFlow(APEPegPart pegPart, ATOperation oper) //DoStep
//        {
//            if (StepOut(pegPart, oper) == false)
//                return null;

//             PeggerLogic.Instance.WriteTarget(pegPart, true);

//            // ApplyYield            
//            PeggerLogic.Instance.ApplyYield(pegPart);

//            if (oper.IsBuffer == false)
//            { 
//                var infos = PeggerLogic.Instance.GetPartChangeInfo(pegPart, true);
                
//                if (infos != null && infos.Count > 0) // OutPartChange (Normal or Split) 하나밖에 없음..
//                    pegPart = PeggerLogic.Instance.ApplyPartChange(pegPart, infos.First());
//            }

//            PeggerLogic.Instance.PegWip(pegPart, true);

//            PeggerLogic.Instance.ShiftTat(pegPart, true);

//            PeggerLogic.Instance.WriteTarget(pegPart, false);

//            PeggerLogic.Instance.PegWip(pegPart, false);

//            PeggerLogic.Instance.ShiftTat(pegPart, false);

//            if (oper.IsBuffer) 
//            {
//                // => IsPeggingGroup ?
//                var splitPegParts = PeggerLogic.Instance.SplitPeggingGroup(pegPart);

//                foreach (APEPegPart pp in splitPegParts)
//                {
//                    // ObO 와 로직에 대한 싱크를 맞출 필요가 있음.
//                    // SelectBomInfo의 형태로 구현하도록 변경하기.
//                    StepIn(pp, oper);
//                    var customPegParts = BWInterface.PeggerControl.CustomSplitPegPart(pp);
//                    List<APEPegPart> pegParts = new List<APEPegPart>();

//                    foreach (var customPegPart in customPegParts)
//                    {
//                        var selectPegParts = PeggerLogic.Instance.SelectBoms(customPegPart);
//                        pegParts.AddRange(selectPegParts);
//                    }

//                    if (pegParts == null || pegParts.Count == 0)
//                        return null;

//                    foreach (var newPegPart in pegParts)
//                    {
//                        MoveNext(newPegPart);
//                    }
//                }

//                return null;
//            }
//            else
//            {
//                var infos = PeggerLogic.Instance.GetPartChangeInfo(pegPart, false);

//                if (infos == null || infos.Count <= 0) // InPartChange (Assembly)
//                {
//                    StepIn(pegPart, oper);

//                    MoveNext(pegPart);
//                    return null;
//                }
//                else
//                {
//                    // 단일인 경우에는 로스가 심함..
//                    foreach (var info in infos)
//                    {
//                        var newPegPart = PeggerLogic.Instance.ApplyPartChange(pegPart.Clone(), info);

//                        StepIn(newPegPart, oper);

//                        MoveNext(newPegPart);
//                    }
//                    return null;
//                }
//            }
//        }

//        private bool StepOut(APEPegPart pegPart, ATOperation oper)
//        {
//            var step = pegPart.CurrentStep as ATOperation;

//            if (step.IsBuffer)
//            {
//                if (pegPart is APEPeggingGroup)
//                {
//                    var merged = pegPart as APEPeggingGroup;
//                    foreach (var pp in merged.Items)
//                    {
//                        pp.NextBom = pp.CurrentBom;
//                        pp.NextRoute = pp.CurrentRoute;
//                        pp.NextBomDetail = pp.CurrentBomDetail;

//                        pp.CurrentBom = ATBom.NULL;
//                        pp.CurrentRoute = null;
//                        pp.CurrentBomDetail = null;
//                    }
//                }
//                // ShippingLogic.Instance.ApplyShippingDate(pegPart);
//            }

//            BWInterface.PeggerControl.OnStepOut(pegPart, oper);

//            return true;
//        }

//        private void StepIn(APEPegPart pegPart, ATOperation oper)
//        {
//            BWInterface.PeggerControl.OnStepIn(pegPart, oper);
//        }
//    }
//}
