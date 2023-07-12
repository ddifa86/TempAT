using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.ObyO
{
    public partial class OboFactory
    {
        /// <summary>
        /// B/W 시 등록된 Pegged Wip 정보.
        /// </summary>
        private List<APEWip> _peggedWips = new List<APEWip>();

        /// <summary>
        /// B/W 시 등록된 투입 타겟 정보
        /// </summary>
        private List<ATOperTarget> _inTargets = new List<ATOperTarget>();

        /// <summary>
        /// Factory 투입 대상 Lot 정보
        /// </summary>
        internal Stack<APELot> ReleasedLots;

        int LotSequence = 0;

        /// <summary>
        /// Factory 내 할당 관련 처리.
        /// </summary>
        internal OboAllocator Allocator { get; set; }

        /// <summary>
        /// Factory 내 조립 
        /// </summary>
        internal OboAssyManager AssyManager;

        internal HashSet<string> ShortLots;

        public OboFactory()
        {
            _peggedWips = new List<APEWip>();
            _inTargets = new List<ATOperTarget>();

            Allocator = new OboAllocator(this);
            AssyManager = new OboAssyManager(this);
            ShortLots = new HashSet<string>();
        }

        public void AddPeggedWip(APEWip wip)
        {
            _peggedWips.Add(wip);
        }

        public void AddInTarget(ATOperTarget target)
        {
            _inTargets.Add(target);
        }

        public void Init()
        {

        }

        public void Done()
        {
            var remainLots = this.AssyManager.GetRemainPartLot();
            // 아래 반복문은 partLots를 돌면서 ShortLog를 등록하는 부분으로 수정 필요
            foreach (var lot in remainLots)
            {
                lot.ShortCategory = "ASSEMBLY";
                lot.ReasonName = "Component Part Short";
                
                DoShort(lot);
            }
            
            var partLotInfos = this.AssyManager.GetPartLotInfos();
            foreach (var key in partLotInfos.Keys)
            {
                var partLots = partLotInfos[key];
                var demand = key.OperTarget.SODemand;
                double max = key.OperTarget.RemainQty;

                foreach (var partLot in partLots)
                {
                    var lots = partLot.Value;
                    if (lots.Count <= 0)
                        continue;

                    double qty = partLot.Value.Sum(x => x.Qty);
                    double needQty = max.ConvertValue(partLot.Key.FromQty, partLot.Key.ToQty, PlanType.Backward);
                    double shortQty = needQty - qty;

                    if (shortQty >= ATOption.Instance.MinimumAllocationQuantity)
                    {
                        LateReason reason;
                        if (partLot.Key.FromItemSiteBuffer.IsMaterialItemSiteBuffer)
                            reason = LateReason.HAWAMaterialShort;
                        else
                            reason = LateReason.FERTMaterialShort;
                        
                        demand.LateInfoManager.AddShortInfo(partLot.Key.FromItemSiteBuffer, partLot.Key.Bom, null, null, LateCategory.Material, reason.ToString(), shortQty, null, key.OperTarget.TargetDateTime, key.OperTarget.TargetDateTime, key.OperTarget.CurRefPlan?.ID);
                    }
                }
            }

            var alignQueue = ReleaseAgent.Instance.GetAlignQueue();
            foreach (var queue in alignQueue)
            {
                var lots = queue.Value.GetLots();

                foreach (var lot in lots)
                {
                    lot.ShortCategory = "FW Short"; 
                    lot.ReasonName = "Remnant by Multiple Batch Size";
                    DoShort(lot);

                    lot.AddShortInfo(LateCategory.Capacity, LateReason.MultipleBatchSizeShort.ToString(), lot.Qty, null, lot.LastStepTime, lot.LastStepTime);
                }
            }

            this.AssyManager.ClearPartLots();
            ReleaseAgent.Instance.Clear();
            this.ShortLots.Clear();
        }

        public void DoExecute()
        {
            CreateReleaseLot();

            ReleasedLots = ReleaseAgent.Instance.GetLots();

            while (ReleasedLots.Count() != 0)
            {
                bool isShortLot = false;
                var lot = ReleasedLots.Pop();
                foreach (var key in lot.OrgLotKeys)
                {
                    if (this.ShortLots.Contains(key))
                    {
                        isShortLot = true;
                        break;
                    }
                }

                if (isShortLot)
                {
                    lot.ShortCategory = "FW Short";
                    lot.ReasonName = "Already Short Lot";
                    lot.IsShort = true;

                    OnDone(lot, LifeCycle.Remain, lot.ReasonName);

                    lot.AddShortInfo(LateCategory.Etc, lot.ReasonName, lot.Qty, null, lot.LastStepTime, lot.LastStepTime);
                }
                else
                {
                    if (lot.IsReleasedLot)
                        MoveFirst(lot);
                    else
                        MoveNext(lot, lot.LotState);
                }

                if (ReleasedLots.Count() == 0)
                    ReleasedLots = ReleaseAgent.Instance.GetLots();
            }

        }

        private void CreateReleaseLot()
        {
            Stack<APELot> lots = new Stack<APELot>();
            List<APELot> tempLots = new List<APELot>();
            double preDays = 0;

            foreach (var wip in this._peggedWips)
            {
                string preFix = wip.CreationType == LotCreateType.SplitByBom ? string.Empty : ATConstants.WIP_BATCH_PREFIX;
                string lotID = LotHelper.GeneratLotID(preFix, wip.WipInfo.LotID);

                var lot = CreateLot(lotID, wip.MapTarget, wip.Qty, preDays, wip.CreationType, wip);
                OutputWriter.Instance.WriteLotHistory(lot, lot.Qty, LotCreateType.Creation.ToString(), lot.LastStepTime, lot.LotID);

                tempLots.Push(lot);
            }

            foreach (ATOperTarget target in this._inTargets)
            {
                double lotSize = target.CurrentItemSiteBuffer.GetInputBatchSize();

                double lotQty = target.RemainQty;

                while (lotQty > 0)
                {
                    double batchQty = Math.Min(lotSize, lotQty);

                    string lotID = LotHelper.GeneratLotID(ATConstants.TARGET_BATCH_PREFIX, target.TargetID);
                    var lot = CreateLot(lotID, target, batchQty, preDays, LotCreateType.Normal, null);
                    OutputWriter.Instance.WriteLotHistory(lot, lot.Qty, LotCreateType.Creation.ToString(), lot.LastStepTime, lot.LotID);

                    lotQty -= lot.Qty;

                    if (lot.CurrentItemSiteBuffer.IsInputItemSiteBuffer(lot, lot.CurrentTarget) == false)
                    {
                        DoShort(lot);

                        if (lot.CurrentItemSiteBuffer.IsMaterialItemSiteBuffer == false)
                            lot.AddShortInfo(LateCategory.Material, LateReason.UnReleasedFERTMaterialShort.ToString(), lot.Qty, null, lot.LastStepTime, lot.LastStepTime);

                        continue;
                    }

                    tempLots.Push(lot);
                }
            }

            this._inTargets.Clear();
            this._peggedWips.Clear();

            var defaultRuleSet = ATRuleAgent.Instance.CurrentRuleSet;
            var preset = defaultRuleSet.GetRule(RulePoint.CompareLot, CallType.Init);
            if (preset != null)
                tempLots.Sort(new ATReleasLotComparer(preset));
            else
                tempLots.Reverse();

            foreach (var lot in tempLots)
            {
                lot.OrgLotKeys.Add(lot.LotID);
                lots.Push(lot);
            }
            
            ReleaseAgent.Instance.SetDefaultStack(lots);
        }

        // 해당 부분 별도 lot helper로 처리 필요.
        
        /// <summary>
        /// 투입 Target을 이용한 Lot
        /// </summary>
        /// <param name="target"></param>
        /// <param name="preBuildDays"></param>
        /// <param name="lotID"></param>
        /// <returns></returns>
        public APELot CreateLot(string lotID, ATOperTarget target, double qty, double preBuildDays, LotCreateType type, APEWip wip)
        {
            var lot = ObjectMapper.CreateLot(lotID, qty, target.Oper, target, wip, type);

            if (lot.IsWipLot)
            {
                lot.LastStepTime = ATUtil.MinTime(wip.AvailableTime, target.MaxLateTargetDateTime);
                lot.VirtualPegWips.Add(wip);
            }
            else
            {
                lot.LastStepTime = target.MinEarlyTargetDateTime.StartTimeOfDayT();
            }
            
            lot.PreBuildDays = preBuildDays;

            if (lot.InitOperTarget.PegTarget?.PegPart?.CurRefPlan != null)
                lot.RefPlans.Add(lot.InitOperTarget.PegTarget.PegPart.CurRefPlan);

            return lot;
        }
        
        /// <summary>
        /// 선행 정보를 변경하여 생성되는 재투입 Lot
        /// </summary>
        /// <param name="orgLot"></param>
        /// <param name="retryQty"></param>
        /// <param name="preBuildDays"></param>
        /// <param name="lotID"></param>
        /// <returns></returns>
        internal List<APELot> CreateRetryLot(APELot orgLot, double retryQty)
        {
            List<APELot> result = new List<APELot>();

            List<APELot> retryLots = new List<APELot>();
            retryLots.Add(orgLot);

            if (orgLot.AssemblyHistory.Count() > 0)
            {
                retryLots.Clear();
                
                //PartLot이 Assembly Lot이면 Range 대상에서 제외
                foreach (var history in orgLot.AssemblyHistory)
                {
                    foreach (var partLot in history.PartLots)
                    {
                        if (partLot.LotType == LotCreateType.Assembly)
                            continue;

                        retryLots.Add(partLot);
                    }
                }
            }

            foreach (APELot part in retryLots)
            {
                // Option 정보가 필요함.
                var preBuildDays = part.PreBuildDays + 1; 

                // SplitLot의 수량을 투입 기준시점의 수량으로 전환하여 Lot 생성
                var ratio = part.InitOperTarget.GetBCumChangeRatio(orgLot.CurrentTarget);
                var qty = retryQty * ratio;

                string lotID = part.LotID + "_R" + LotSequence++;

                var lot = CreateLot(lotID, part.InitOperTarget, qty, preBuildDays, LotCreateType.Retrying, part.Wip);

                result.Add(lot);
            }

            // Mbs??
            return result;
        }
    }
}
