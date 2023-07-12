using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.EngineBase
{
    public class BackwardPlanner
    {
        internal AlignQueueManager AlignManager { get; private set; }

        internal BackwardPlannerControl PlanControl { get; private set; }

        public BackwardPlanner(HashSet<IAlignQueue> queues, BackwardPlannerControl planControl)
        {
            this.AlignManager = new AlignQueueManager(queues);
            this.PlanControl = planControl;
        }

        /// <summary>
        /// BackwardPlanner를 Initialize 합니다.
        /// ITargetGroup을 AlignKey 값 별로 AlignQueue에 추가합니다.
        /// 
        /// </summary>
        /// <param name="parts"></param>
        internal virtual void Initialize(IEnumerable<ITargetGroup> parts)
        {
            foreach (var part in parts)
            {
                // KTR : 초기화 시점에 호출되는 부분인데 이름이 좀 어색함.
                PlanControl.OnSelectPart(part);

                IComparable alignKey = PlanControl.GetAlignKey(part);
                // KTR : null 넘어왔을 경우 예외처리 필요.

                IAlignQueue queue = AlignManager.GetQeueue(alignKey);

                if (queue != null)
                {
                    IComparable groupKey = PlanControl.GetMergedTargetGroupKey(part);
                    queue.AddEntity(groupKey, part);
                }
                else 
                {
                    // 예외처리
                }
            }
        }

        internal virtual void DoExecute()
        {
            while (AlignManager.HasAlignQueue)
            {
                // KTR: Queue에서 보통 가져올 때에는 Enqueue, dequeue 를 쓴단다.. Stack은 pop, push, peek
                // 먼가 섞여 있어서 그냥 GetFirstOrDefault가 어울릴 듯. 함수명은
                var queue = AlignManager.PopFirstAlignQueue();
                if (queue == null || queue.Entities.Count() == 0)
                {
                    // KTR: 예외케이스에 대한 로그를 이제 슬 작성을 해볼까??

                    continue;
                }
            
                var entities = queue.Entities.Values.ToList();

                // KTR: Naming 이슈가 좀 있네 이것도,  일단 여러개를 선택한게 아니라서 그리고 Entity가 변수명이긴한데 명확하진 않은듯
                // 변수명이랑 함수명이랑 둘다 변경하는 방안으로 가보자 => Entity 대신 TargetGroup혹은 MergedTargetGroup
                PlanControl.OnSelectedEntities(entities, queue);

                // KTR : 소팅은 이벤트가 아니기 때문에 On을 붙이는 건 어색함.
                PlanControl.OnCompareEntities(entities, queue);

                while (entities.Count > 0)
                {
                    var entity = entities.First();
                    entities.Remove(entity);

                    // KTR : OnSelectTargetGroup이 좀 더 명확하려나?
                    PlanControl.OnSelectGroup(entity);

                    DoStep(entity);
                }
            }
        }

        internal virtual void DoStep(ITargetGroup part)
        {
            try
            {
                var remainPegParts = new Stack<ITargetGroup>();

                part.SetPegPosition(PegPosition.StepOut);
                // KTR: 원석아 너무 개발자 입장에서의 이름인거 같은데?
                PlanControl.OnPushPart(part);
                remainPegParts.Push(part);

                while (remainPegParts.Count() != 0)
                {
                    // 잔여 파트 (조립 파트) 중 우선 진행이 필요한 경우, 소팅진행.
                    /* 
                         var preset = this.DefaultRuleSet.GetPreset(RulePoint.CompareAssemblyPart, CallType.Operation);
                         if (preset != null)
                         pegParts.Sort(new AssemblyPartComparer(preset));
                    */

                    var cur = remainPegParts.Pop();

                    while (cur.Step != null)
                    {
                        if (cur.PegPosition == PegPosition.StepOut)
                        {
                            PlanControl.OnStepOut(cur);

                            // OnStepOut에 넣어야할까..?
                            // currentstep == null...? => 변경 불가하도록 변경.
                            PlanControl.CreateTarget(cur, true);

                            PlanControl.ApplyYield(cur);

                            var pegParts = PartChange(cur, true);
                            if (pegParts != null && pegParts.Count > 1)
                            {
                                foreach (var pp in pegParts)
                                {
                                    pp.SetPegPosition(PegPosition.Run);
                                    
                                    PlanControl.OnPushPart(pp);
                                    remainPegParts.Push(pp);
                                }
                                break;
                            }

                            cur.SetPegPosition(PegPosition.Run);
                        }

                        if (cur.PegPosition == PegPosition.Run)
                        {
                            PlanControl.DoPegging(cur, true);

                            PlanControl.ApplyTat(cur, true);

                            cur.SetPegPosition(PegPosition.Wait);
                        }

                        if (cur.PegPosition == PegPosition.Wait)
                        {
                            PlanControl.CreateTarget(cur, false);

                            PlanControl.DoPegging(cur, false);

                            PlanControl.ApplyTat(cur, false);

                            cur.SetPegPosition(PegPosition.StepIn);
                        }

                        if (cur.PegPosition == PegPosition.StepIn)
                        {
                            // 여기서 부터 pbb, pbo 비교해서 처리 해보자.
                            // pbb의 경우, assembly면 각각을 모두 
                            // InPartChange
                            var splitPart = PartChange(cur, false);

                            if (splitPart.Count() > 1)
                            {
                                // KTR : splitPart를 소팅하는 부분을 어디에 넣을지 결정
                                // Splitpart가 여러개면 무조건 소팅을 하도록 처리 필요할 듯.
                                foreach (var pp in splitPart)
                                {
                                    PlanControl.OnStepIn(pp);
                                    pp.SetPegPosition(PegPosition.SelectBom);

                                    PlanControl.OnPushPart(pp);
                                    remainPegParts.Push(pp);
                                }
                                break;
                            }

                            PlanControl.OnStepIn(cur);
                            cur.SetPegPosition(PegPosition.SelectBom);
                        }

                        if (cur.PegPosition == PegPosition.SelectBom)
                        {
                            var splitPart = SelectBom(cur);

                            if (splitPart.Count > 1)
                            {
                                foreach (var pp in splitPart)
                                {
                                    pp.SetPegPosition(PegPosition.MovePrev);
                                    
                                    PlanControl.OnPushPart(pp);
                                    remainPegParts.Push(pp);
                                }

                                break;
                            }
                            else if(splitPart.Count == 1)
                            {
                                cur = splitPart.FirstOrDefault();
                            }

                            cur.SetPegPosition(PegPosition.MovePrev);
                        }

                        if (PlanControl.IsFinished(cur))
                        {
                            PlanControl.OnDone(cur);
                            break;
                        }

                        if (cur.PegPosition == PegPosition.MovePrev)
                        {
                            cur.Step = PlanControl.GetPrevStep(cur);
                            cur.SetPegPosition(PegPosition.StepOut);
                        }

                        if (cur.Step == null)
                        {
                            PlanControl.OnDone(cur);
                            break;
                        }

                        #region Set Align Queue 

                        IComparable alignKey = PlanControl.GetAlignKey(cur);

                        if (alignKey != null)
                        {
                            IAlignQueue queue = AlignManager.GetQeueue(alignKey);

                            if (queue != null)
                            {
                                IComparable groupKey = PlanControl.GetMergedTargetGroupKey(cur);
                                queue.AddEntity(groupKey, cur);
                                break;
                            }
                            else
                            {
                                // 먼가 예외처리가 필요해 보이는디..?
                                // PlanControl.OnDone(cur);
                            }
                        }

                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally
            {
                //remainPegParts.
            }
        }

        internal List<ITargetGroup> PartChange(ITargetGroup part, bool isOut)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("GetPartChangeInfos");
            try
            {
                List<ITargetGroup> targetGroups = new List<ITargetGroup>();
                List<object> partChangeInfos = PlanControl.GetPartChangeInfos(part, isOut);

                if (partChangeInfos == null || partChangeInfos.Count == 0)
                    return targetGroups;

                if (partChangeInfos.Count == 1)
                {
                    PlanControl.ApplyPartChangeInfo(part, partChangeInfos[0], isOut);
                }
                else
                {
                    foreach (object info in partChangeInfos)
                    {
                        ITargetGroup copied = part.Clone();
                        ITargetGroup branch = PlanControl.ApplyPartChangeInfo(copied, info, isOut);

                        targetGroups.Add(branch);
                    }
                }

                return targetGroups;
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("GetPartChangeInfos");
            }
        }
    
        internal List<ITargetGroup> SelectBom(ITargetGroup part)
        {
            List<ITargetGroup> targetGroups = new List<ITargetGroup>();
           
            
            var splitTargets = PlanControl.SplitTargetGroup(part);

            foreach (var splitTarget in splitTargets)
            {
                var targetGroup = PlanControl.SelectBom(splitTarget);

                targetGroups.AddRange(targetGroup);
            }

            return targetGroups;
        }
    }
}
