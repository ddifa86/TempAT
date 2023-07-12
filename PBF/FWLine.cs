using Mozart.Simulation.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Logic;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public class FWLine : APELine
    {
        public FWFactory Factory { get; private set; }

        public FWAllocator CurrentAllocator { get; private set; }

        public List<APELot> RemainLots { get; private set; }

        public DateTime StartTime { get; private set; }

        public DateTime EndTime { get; private set; }

        public Time Horizon { get; private set; }

        public PBFResource DummyBucket { get; private set; }

        public Dictionary<string, PBFResource> Buckets { get; private set; }

        internal List<FWAllocGroup> AllocGroups
        {
            get
            {
                return this.Factory.AllocGroup;
            }
        }

        internal FWLine(FWFactory solver, string name)
            :base()
        {
            this.Factory = solver;
            this.CurrentAllocator = null;
            this.RemainLots = new List<APELot>();
            this.Buckets = new Dictionary<string, PBFResource>();
        }

        internal void RunAllocate()
        {
            // Line Cycle 시작, 종료 시간 설정
            this.StartTime = Factory.NowDT;
            this.EndTime = Factory.NextStartTime;
            this.Horizon = Factory.CurrentHorizon;

            var allocGroups = this.AllocGroups.ToList();

            Factory.StartCycle();

            APEReleaseAgent.Instance.DoReleaseLot(this);

            while (allocGroups.Count > 0)
            {
                var group = allocGroups[0];
                allocGroups.RemoveAt(0);

                this.Allocation(group);
            }

            Factory.EndCycle();
        }

        internal void Allocation(FWAllocGroup group)
        {
            try
            {
                this.CurrentAllocator = new FWAllocator(this, group);

                this.CurrentAllocator.DoRun();
            }
            finally
            {
                this.CurrentAllocator = null;
            }
        }

        internal override void MoveFirst(APELot lot)
        {
            if (lot.LotType == LotCreateType.Normal)
                lot.LastStepTime = this.Factory.NowDT;

            if (lot.LastStepTime > lot.CurrentTarget.TargetDateTime)
                lot.AddVirtualLateInfo(LateCategory.Tat, LateReason.LateReleaseLot.ToString(), null, this.Factory.NowDT, this.Factory.NowDT);

            APEWipAgent.Instance.ReleasedLot(lot);

            FWInterface.LotControl.OnReleasedLot(lot);

            LotState position = LotState.Wait;

            if (lot.IsRunWip)
            {
                lot.MoveFirst(Factory.NowDT);

                FWFactoryLogic.Instance.StepIn(lot);

                // 초기 RUNTAT 반영 필요 혹은 PROCESS / TACTTIME을 고려한 OUT 시간 설정 필요
                ATDummyAgent.Instance.DoAllocate(lot);

                lot.LotState = LotState.Run;
                position = LotState.Run;
            }

            MoveNext(lot, position);
        }

        public override void MoveNext(APELot lot, LotState position = LotState.Run)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("Line_MoveNext");
            try
            {
                bool isDummyOper = MoveNext_(lot, position);

                while (isDummyOper)
                {
                    isDummyOper = MoveNext_(lot);
                }

                return;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            finally
            {
                while (this.RemainLots.Count() > 0)
                {
                    var remainLot = this.RemainLots[0];
                    this.RemainLots.RemoveAt(0);

                    if (remainLot.IsReleasedLot)
                        MoveFirst(remainLot);
                    else
                        MoveNext(remainLot, remainLot.LotState);

                }
                this.RemainLots.Clear();

                ATElapsedTimeChecker.Instance.AddElapsedTime("Line_MoveNext");
            }
        }

        /// <summary>
        /// Lot의 다음 공정을 산출하여 반환        
        /// </summary>
        /// <param name="lot"></param>
        /// <param name="position"></param>
        /// <returns> 찾은 공정이 Dummy 공정인지 여부를 반환 </returns>  
        private bool MoveNext_(APELot lot, LotState position = LotState.Run)
        {
            if (position == LotState.Run)
            {
                APEWipAgent.Instance.RemoveLot(lot);

                lot.LotState = LotState.Out;
                lot.AddPlan(lot.CurrentPlanInfo);
                lot.LastStepTime = lot.LastPlan == null ? lot.LastStepTime : lot.LastPlan.EndTime;
            }

            if (FWInterface.LotControl.IsShortLot(lot))
            {
                // 사용자판단시 더 이상 흘리는게 의미가 없는 경우. 잔여로 둠. (ex) Short
                lot.IsShort = true;
                FWFactoryLogic.Instance.OnDone(lot, LifeCycle.Remain, string.Empty);
                return false;
            }

            if (position == LotState.Run || position == LotState.Out)
            {
                FWFactoryLogic.Instance.StepOut(lot);

                FWFactoryLogic.Instance.PartChange(lot, true);

                FWFactoryLogic.Instance.ApplyYield(lot);
            }
            
            lot.ClearVirtualLateInfo();

            FWFactoryLogic.Instance.NextStep(lot);

            if (FWFactoryLogic.Instance.IsFinished(lot) == true)
            {
                FWFactoryLogic.Instance.OnDone(lot, LifeCycle.Disposal, string.Empty);
                return false;
            }

            var bomType = FWFactoryLogic.Instance.PartChange(lot, false);

            var target = FWFactoryLogic.Instance.GetOperTarget(lot);
            if (target == null)
            {
                // 비정상 종료.
                lot.IsShort = true;

                lot.AddShortInfo(LateCategory.Etc, LateReason.FailToFindTarget.ToString(), lot.Qty, null, lot.LastStepTime, lot.LastStepTime);
                FWFactoryLogic.Instance.OnDone(lot, LifeCycle.Remain, "fail to find target");

                return false;
            }

            if (bomType == BomType.Assembly)
            {
#warning 해당 부분은 전체적인 정리가 필요해 보임.
                // BW에 의존하여 조립을하고 있는 부분에 처리가 필요함.
                APEAssemblyAgent.Instance.AddRun(lot);
                return false;
            }

            lot.CurrentTarget = target;

            #region 실적 차감 (위치, 메서드 정하기)
            double overQty = lot.CurrentQty - target.RemainQty;

            if (overQty > ATOption.Instance.MinimumAllocationQuantity)
            {
                string lotID = LotHelper.GeneratLotID(lot.LotID, ATConstants.SPLIT_BATCH_PREFIX);

                APELot splitLot = LotHelper.GenerateSplitLot(lot, overQty, lotID, true);

                if (splitLot != null)
                {
                    splitLot.IsShort = true;
                    FWFactoryLogic.Instance.OnDone(splitLot, LifeCycle.Remain, "Over Target Qty");
                }
            }

            target.UsedQty += lot.CurrentQty; 
            #endregion

            if (lot.CurrentOper.IsBuffer)
            {
                FWFactoryLogic.Instance.FindBom(lot);
            }

            FWFactoryLogic.Instance.StepIn(lot);

            var isNeedtoMoveStep = AddWipQueue(lot);

            return isNeedtoMoveStep;
        }

        public bool AddWipQueue(APELot lot)
        {
            if (FWInterface.LotControl.IsDummyOperation(lot) || lot.CurrentOper.OperType == OperType.Dummy || lot.CurrentOper.IsBuffer)
            {
                ATDummyAgent.Instance.DoAllocate(lot);

                return true;
            }
            else
            {
                // Arrange 정보가 없는 경우.
                var queues = lot.CurrentOper.GetWipQueues();
                if (queues.Count() == 0 && ATOption.Instance.ApplyNoArrangeToDummy == true)
                {
                    ATDummyAgent.Instance.DoAllocate(lot);
                    return true;
                }

                if (APEWipAgent.Instance.AddLot(lot) == false)
                {
                    return false;
                }

                // Immediately 할당
                if (IsCurrentSelectBucketGroup(lot) == true)
                {
                    AddCurrentSelectBucketGroup(lot);
                }
            }

            return false;
        }

        /// <summary>
        /// 현재 Allocator에서 할당 중인 Bucket 그룹과 동일한 Group인지 여부 체크
        /// </summary>
        /// <returns></returns>
        private bool IsCurrentSelectBucketGroup(APELot lot)
        {
            if (this.CurrentAllocator != null)
            {
                var currentAllocContext = this.CurrentAllocator.CurrentContext;

                if (currentAllocContext.AllocateType == AllocateType.LotFirstSelection)
                {
                    //Move Next 후에도 할당된 BucketGroup과 동일한 경우
                    if (lot.CurrentOper.BucketGroup == currentAllocContext.SelectedBucketGroup)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void AddCurrentSelectBucketGroup(APELot lot)
        {
            var currentAllocContext = this.CurrentAllocator.CurrentContext;
            var bucketGroup = currentAllocContext.SelectedBucketGroup;

            // LotGroup 정보
            var group = bucketGroup.Queue.GetLotGroup(lot.LotGroupKey);

            // 현재 TargetGroup에존재하지 않는 경우, 바로 할당 대상으로 추가 작업 진행.
            if (currentAllocContext.SelectedLotGroupInLevel.Contains(group) == false)
                currentAllocContext.SelectedLotGroupInLevel.Add(group);
            else
            {
            }

        }

        internal void Rolling(DateTime now, DateTime nextStartTime)
        {
          
        }

    }
}
