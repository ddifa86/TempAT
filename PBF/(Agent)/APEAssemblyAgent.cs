using Mozart.Collections;
using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Planner.Data;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public class APEAssemblyAgent
    {
        public static APEAssemblyAgent Instance
        {
            get
            {
                return ServiceLocator.Resolve<APEAssemblyAgent>();
            }
        }

        Dictionary<ATBuffer, BufferAssemblyLogic> _logics;


        public APEAssemblyAgent()
        {
            _logics = new Dictionary<ATBuffer, BufferAssemblyLogic>();
        }

        public BufferAssemblyLogic GetAssemblyLogic(ATBuffer buffer)
        {
            if (!_logics.TryGetValue(buffer, out BufferAssemblyLogic logic))
                _logics[buffer] = logic = new BufferAssemblyLogic(buffer);

            return logic;
        }

        public bool AddPartLot(APELot partLot)
        {
            var assyBom = partLot.CurrentItemSiteBuffer.GetNextBom(BomType.Assembly);

            if (assyBom.Count() == 0)
                return false;

            var logic = APEAssemblyAgent.Instance.GetAssemblyLogic(partLot.FromBuffer);

            logic.AddPartLot(partLot, assyBom);

            AssemblyLogic.Instance.OnAddPartLot(partLot);

            return true;
        }

        public void UpdatePartLot(APELot partLot, double usedQty)
        {
            if (_logics.ContainsKey(partLot.FromBuffer) == false)
            {
                return;
            }

            var assemblyLogic = APEAssemblyAgent.Instance.GetAssemblyLogic(partLot.FromBuffer);

            assemblyLogic.UpdatePartLot(partLot, usedQty);
        }

        /// <summary>
        /// partLot을 추가하고 Assembly 시도
        /// </summary>
        /// <param name="lot"></param>
        public void AddRun(APELot lot)
        {
            if (AddPartLot(lot) == false)
                return;

            var assyBom =lot.CurrentItemSiteBuffer.GetNextBom(BomType.Assembly);

            DoAssembly(lot.FromBuffer, assyBom, lot.CurrentTarget.SODemand.ID);
        }

        /// <summary>
        /// 별도로 Assembly를 호출하여 조립 시도
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="assemblyBom"></param>
        public void DoAssembly(ATBuffer buffer, List<ATBom> assemblyBom, string soID)
        {
            if (_logics.ContainsKey(buffer) == false)
            {
                return;
            }

            var solver = FWFactory.Instance;
            var line = solver.DefaultLine;

            var logic = _logics[buffer];

            // Assembly 대상 배치
            List<APELot> targetAssyLots = new List<APELot>();

            foreach (var bom in assemblyBom)
            {
                if (logic.AssemblyLots.ContainsKey(bom) == false)
                    continue;

                // 조립 대상 필터하는 부분이 들어가야할까..?
                //if (bom != lot.CurrentBom)
                //    continue;

                targetAssyLots.AddRange(logic.AssemblyLots[bom]);
            }

            if (targetAssyLots.Count() == 0)
                return;

#warning Filter Assembly 대상..?? RuleSet 관리 여부 확인

            targetAssyLots = AssemblyLogic.Instance.FilterAssyLot(targetAssyLots, soID);

#warning Assembly 대상 소팅 RuleSet 관리 여부 확인          
            // 배치 소팅
            if (targetAssyLots.Count() != 1)
                targetAssyLots.Sort(new AssemblyLotComparer(null, solver.NowDT));

            // 조립 시도
            Dictionary<string, string> triedBoms = new Dictionary<string, string>();

            FWInterface.AssemblyControl.OnPrepareAssembly(targetAssyLots, soID);

            foreach (var targetLot in targetAssyLots)
            {
                if (targetLot.Qty <= ATOption.Instance.MinimumAllocationQuantity)
                    continue;

                // 조립 실패한 제품은 재시도하지 않음.
                if (triedBoms.ContainsKey(targetLot.CurrentBom.BomID) == true)
                    continue;

                while (true)
                {
                    APELot assyLot = logic.DoAssembly(targetLot);

                    if (assyLot != null)
                    {
                        logic.AssemblyLots[assyLot.CurrentBom].Remove(assyLot);

                        #region Assembly / Pull 추후 개선 작업 필요
                        if (assyLot.AssemblyInfo != null)
                        {
                            foreach (var detail in assyLot.AssemblyInfo.PartInfo.Keys)
                            {
                                var dic = assyLot.AssemblyInfo.PartInfo[detail];

                                foreach (var pairkey in dic)
                                {
                                    var partLot = pairkey.Key;
                                    var usedQty = pairkey.Value;

                                    if (partLot.CapaPlans != null)
                                        assyLot.CapaPlans.AddRange(partLot.CapaPlans);
                                    if (partLot.AssemblyHistory != null)
                                        assyLot.AssemblyHistory.AddRange(partLot.AssemblyHistory);

                                    // 아래 주석 내용들은 PBF에서 사용되지 않음(추후 Assembly Agent 통일을 위한 주석 처리)
                                    // assyLot.SplitInfos.AddRange(partLot.SplitInfos);
                                    // assyLot.RefPlans.AddRange(partLot.RefPlans);
                                    // assyLot.PreBuildDays = Math.Max(assyLot.PreBuildDays, partLot.PreBuildDays);
                                    // assyLot.VirtualPegWips.AddRange(partLot.VirtualPegWips);

                                    // partLot의 LifeCycle 출력
                                    OutputWriter.Instance.WriteLotHistory(partLot, usedQty, LifeCycle.Assembly.ToString(), assyLot.LastStepTime, string.Format("Assembly Lot : {0}({1})", assyLot.LotID, assyLot.Qty));

                                    // partLot의 Assembly 정보 출력.
                                    OutputWriter.Instance.WriteLotAssemblyLog(assyLot, detail, partLot, usedQty, assyLot.LastStepTime);

                                    // 사용된 partlot의 처리 작업
                                    APEAssemblyAgent.Instance.UpdatePartLot(partLot, usedQty);
                                }
                            }
                            assyLot.AssemblyHistory.Insert(0, assyLot.AssemblyInfo);
                            assyLot.AssemblyInfo = null;
                        }
                        #endregion

                        line.MoveFirst(assyLot);
                        // 할당가능한 배치로 등록하는 작업 진행
                    }
                    else
                    {
                        triedBoms.Add(targetLot.CurrentBom.BomID, targetLot.CurrentBom.BomID);
                        break;
                    }

                    if (targetLot == assyLot)
                        break;
                }
            }

            // 조립된 배치들은 등록 
            // => 등록된 배치들을 할당가능하며, 할당이 되면 그 시점에 조립부품들의 정보를 업데이트 진행.
        }
    }

    public class BufferAssemblyLogic
    {
        public ATBuffer Buffer { get; private set; }

        /// <summary>
        /// 공정 내 조립이 완료되지 않은 작업물 목록입니다.
        /// </summary>
        public Dictionary<ATBom, List<APELot>> AssemblyLots { get; private set; }

        /// <summary>
        /// Assembly Bom 별 Detail 별 Lot 수량 집계
        /// </summary>
        private DoubleDictionary<Tuple<ATBom, ATDemand>, ATBomDetail, List<APELot>> _partLotInfos;

        /// <summary>
        /// Lot별 자신이 part로 포함된 List 정보 
        /// </summary>
        private Dictionary<APELot, List<List<APELot>>> _addBatchListInfos;

        public BufferAssemblyLogic(ATBuffer buffer)
        {
            this.Buffer = buffer;

            this.AssemblyLots = new Dictionary<ATBom, List<APELot>>();

            this._partLotInfos = new DoubleDictionary<Tuple<ATBom, ATDemand>, ATBomDetail, List<APELot>>();

            this._addBatchListInfos = new Dictionary<APELot, List<List<APELot>>>();

            this.Initialize();
        }

        public DoubleDictionary<Tuple<ATBom, ATDemand>, ATBomDetail, List<APELot>> GetPartLotInfos()
        {
            return this._partLotInfos;
        }

        /// <summary>
        /// 공정 내 조립 OperTarget으로부터 조립 후 작업물을 생성합니다.
        /// </summary>
        public void Initialize()
        {
            var executeInfo = ATExecutionContext.Instance.CurrentExecutionInfo as PBFModuleExecutionInfo;

            List<ATOperTarget> targets;
            if (executeInfo.AssemblyTargets.TryGetValue(this.Buffer, out targets) == false)
                return;

            foreach (var target in targets)
            {
                var lotsize = target.RemainQty;

                var lotQty = target.RemainQty;

                while (lotQty > 0)
                {
                    // Lot 사이즈 조절
                    var batchqty = Math.Min(lotsize, target.RemainQty);

#warning FEAction 포인트 LotID
                    string lotID = LotHelper.GeneratLotID(ATConstants.ASSY_BATCH_PREFIX, target.TargetID);
                    var lot = ObjectMapper.CreateLot(lotID, batchqty, target.Oper, target, null, LotCreateType.Assembly);

                    if (this.AssemblyLots.TryGetValue(target.CurrentBom, out List<APELot> lst) == false)
                        this.AssemblyLots.Add(target.CurrentBom, lst = new List<APELot>());

                    lst.Add(lot);

                    lot.CurrentRoute = target.CurrentBomRoute;

                    lotQty -= lot.Qty;
                }
            }
        }

        internal void AddPartLot(APELot lot, List<ATBom> assemblyBom)
        {
            foreach (var bom in assemblyBom)
            {
                Tuple<ATBom, ATDemand> key = new Tuple<ATBom, ATDemand>(bom, lot.CurrentTarget.SODemand);
                if (_partLotInfos.ContainsKey(key) == false)
                {
                    foreach (var detail in bom.BomDetails)
                    {
                        _partLotInfos.Add(key, detail, new List<APELot>());
                    }
                }

                var bomdetail = bom.BomDetails.Where(x => x.FromItemSiteBuffer.Key == lot.CurrentItemSiteBuffer.Key).FirstOrDefault();

                if (bomdetail == null)
                {
                    continue;
                }

                List<APELot> list;
                if (_partLotInfos.TryGetValue(key, bomdetail, out list) == false)
                {
                    continue;
                }

#warning PartLotCompare 추가 작업
                var index = list.BinarySearch(lot, new PartLotComparer(null));

                if (index < 0)
                    index = ~index;

                list.Add(lot);

                // 하나의 배치가 여러개의 제품으로 조합이 되는 경우.
                // 특정 제품에 조합된 이후, 모든 Assembly Part List 에서 제거해주기 위한 정보 등록.
                List<List<APELot>> infos;
                if (_addBatchListInfos.TryGetValue(lot, out infos) == false)
                {
                    _addBatchListInfos.Add(lot, infos = new List<List<APELot>>());
                }

                infos.Add(list);
            }
        }

        internal void UpdatePartLot(APELot partLot, double usedQty)
        {
            partLot.Qty -= usedQty;

            if (partLot.Qty <= ATOption.Instance.MinimumAllocationQuantity)
            {
                List<List<APELot>> lst;
                if (this._addBatchListInfos.TryGetValue(partLot, out lst))
                {
                    lst.ForEach(x => x.Remove(partLot));
                    this._addBatchListInfos.Remove(partLot);
                }
                else
                {
                    // 이상한 경우.
                }

                var assyBom = partLot.CurrentItemSiteBuffer.GetNextBom(BomType.Assembly);
                foreach (var bom in assyBom)
                {
                    Tuple<ATBom, ATDemand> key = new Tuple<ATBom, ATDemand>(bom, partLot.CurrentTarget.SODemand);
                    if (_partLotInfos.TryGetValue(key, out var details))
                    {
                        bool isDel = true;
                        foreach (var detail in details)
                        {
                            if (detail.Value.Count != 0)
                            {
                                isDel = false;
                                break;
                            }
                        }

                        if (isDel)
                            _partLotInfos.Remove(key);
                    }

                }
            }
        }

        public List<APELot> GetRemainAssemblyLots()
        {
            return _addBatchListInfos.Keys.ToList();
        }

        public APELot DoAssembly(APELot assembledLot)
        {
            ATBom targetBom = assembledLot.CurrentBom;
            //string key = targetBom.BomID + assembledLot.CurrentTarget.SODemand.ID;
            Tuple<ATBom, ATDemand> key = new Tuple<ATBom, ATDemand>(targetBom, assembledLot.CurrentTarget.SODemand);

            // Part 정보가 없는 경우.
            Dictionary<ATBomDetail, List<APELot>> partDic;
            if (this._partLotInfos.TryGetValue(key, out partDic) == false)
                return null;

            Dictionary<ATBomDetail, List<APELot>> availablePartLot = new Dictionary<ATBomDetail, List<APELot>>();

            DateTime now = FWFactory.Instance.NowDT;

            DateTime latestTime = ATUtil.DateMinValue;

            double canAssemblyQty = double.MaxValue;

            foreach (var pairkey in partDic)
            {
                var detail = pairkey.Key;
                var partlst = pairkey.Value;

                availablePartLot.Add(detail, new List<APELot>());
                double partQty = 0;

                foreach (var partlot in partlst)
                {
                    if (partQty >= assembledLot.Qty)
                        break;

                    // CurrentQty => 파트로 일부 사용되고 남은 잔여 수량
                    if (partlot.CurrentQty <= ATOption.Instance.MinimumAllocationQuantity)
                        continue; // 예외처리..?

                    if (AssemblyLogic.Instance.CanAssemblyPartLot(partlot, assembledLot, now, availablePartLot, pairkey) == false)
                        continue;

                    partQty += partlot.CurrentQty.ConvertValue(detail.FromQty, detail.ToQty, PlanType.Forward);

                    if (partlot.LastStepTime > latestTime)
                        latestTime = partlot.LastStepTime;

                    availablePartLot[detail].Add(partlot);
                }

                // 조립 가능 수량 산출
                // 파트 중 가장 작은 값으로 산정
                if (canAssemblyQty > partQty)
                    canAssemblyQty = partQty;

                if (canAssemblyQty <= ATOption.Instance.MinimumAllocationQuantity)
                    break;
            }

            if (canAssemblyQty <= ATOption.Instance.MinimumAllocationQuantity)
                return null;

            if (assembledLot.Qty < canAssemblyQty)
                canAssemblyQty = assembledLot.Qty;

            // 조립 가능한 수량이 배치 수량보다 적은 경우 Split을 해서라도 조립을 진행할지 판단
            if (canAssemblyQty < assembledLot.Qty && AssemblyLogic.Instance.CanAssembleSmallBatch(assembledLot, canAssemblyQty, now) == false)
                return null;

            var sbatch = assembledLot;

            if (canAssemblyQty < assembledLot.Qty && (assembledLot.Qty - canAssemblyQty) > ATOption.Instance.MinimumAllocationQuantity)
            {
                // Assy 번호 재발번해서 진행
                string lotID = LotHelper.GeneratLotID(ATConstants.ASSY_BATCH_PREFIX, assembledLot.CurrentTarget.TargetID);
                sbatch = ObjectMapper.CreateLot(lotID, canAssemblyQty, assembledLot.CurrentTarget.Oper, assembledLot.CurrentTarget, null, LotCreateType.Assembly);

                assembledLot.Qty -= canAssemblyQty;
                assembledLot.CurrentQty -= canAssemblyQty;
            }

            sbatch.OnCreateLot(sbatch, assembledLot, LotCreateType.Assembly);

            sbatch.LastStepTime = latestTime;

            ATAssemblyInfo assyInfo = new ATAssemblyInfo(sbatch);

            sbatch.AssemblyInfo = assyInfo;

            foreach (var part in availablePartLot)
            {
                var detail = part.Key;
                var partlots = part.Value;

                var realQty = canAssemblyQty.ConvertValue(detail.FromQty, detail.ToQty, PlanType.Backward);

                foreach (var partlot in partlots)
                {
                    var usedQty = Math.Min(partlot.Qty, realQty);

                    partlot.CurrentQty -= usedQty;
                    realQty -= usedQty;

                    // 조립에 사용된 정보 등록
                    assyInfo.AddPartLot(detail, partlot, usedQty);
                    if (realQty <= ATOption.Instance.MinimumAllocationQuantity)
                        break;
                }
            }

            return sbatch;
        }

        // 조립된 배치가 실질적으로 진행 후 파트의 정보를 업데이트 진행

    }

    internal class AssemblyInfo
    {
        internal APELot AssyLot;

        internal BufferAssemblyLogic Logic;
        /// <summary>
        /// 조립을 위해  BomDetail별 사용된 Lot 수량 정보. 
        /// </summary>
        internal DoubleDictionary<ATBomDetail, APELot, double> PartInfo;

        public AssemblyInfo(BufferAssemblyLogic logic, APELot assyLot)
        {
            this.AssyLot = assyLot;
            this.Logic = logic;
            this.PartInfo = new DoubleDictionary<ATBomDetail, APELot, double>();
        }

    }

}
