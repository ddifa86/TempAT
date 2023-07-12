using Mozart.Collections;
using Mozart.Extensions;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik.ObyO
{
    /// <summary>
    /// Factory 내 발생되는 조립에 대한 처리 작업.
    /// </summary>
    public class OboAssyManager
    {
        OboFactory _factory;
        // 조립 대상 키, FromItemSiteBuffer, List<Lot>
        DoubleDictionary<ATAssyInfo, ATBomDetail, HashSet<APELot>> _assyPartInfos;

        Dictionary<APELot, HashSet<APELot>> _partLots;


        public OboAssyManager(OboFactory factory)
        {
            _factory = factory;
            _assyPartInfos = new DoubleDictionary<ATAssyInfo, ATBomDetail, HashSet<APELot>>();
            _partLots = new Dictionary<APELot, HashSet<APELot>>();
        }

        public DoubleDictionary<ATAssyInfo, ATBomDetail, HashSet<APELot>> GetPartLotInfos()
        {
            return this._assyPartInfos;
        }

        public List<APELot> GetRemainPartLot()
        {
            return _partLots.Keys.Where(x => x.Qty >= ATOption.Instance.MinimumAllocationQuantity).ToList();
           
        }

        public void ClearPartLots()
        {
            this._partLots.Clear();
        }

        public bool AddPartLot(ATAssyInfo assyInfo, APELot partLot)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("Assembly_AddPartLot");
            try
            {
                HashSet<APELot> list;
                if (_assyPartInfos.ContainsKey(assyInfo) == false)
                {
                    foreach (var detail in assyInfo.CurrentBom.BomDetails)
                    {
                        list = new HashSet<APELot>();
                        _assyPartInfos.Add(assyInfo, detail, list);
                    }
                }

                list = _assyPartInfos[assyInfo, partLot.CurrentBomDetail];

                list.Add(partLot);
                _partLots.Add(partLot, list);

                partLot.OrgLotKeys.Clear();
                partLot.OrgLotKeys.Add(partLot.LotID + partLot.CurrentOperID);

                return true;
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("Assembly_AddPartLot");
            }
        }

        public void UpdatePartLot(APELot partLot, double usedQty)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("Assembly_UpdatePartLot");
            try
            {
                partLot.Qty -= usedQty;

                if (partLot.Qty <= ATOption.Instance.MinimumAllocationQuantity)
                {
                    var list = this._partLots[partLot];
                    list.Remove(partLot);
                }
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("Assembly_UpdatePartLot");
            }
        }

        /// <summary>
        /// partLot을 추가하고 Assembly 시도
        /// </summary>
        /// <param name="lot"></param>
        public List<APELot> AddRun(APELot lot, ATAssyInfo assyInfo)
        {
            List<APELot> assyLots = new List<APELot>();
            
            if (AddPartLot(assyInfo, lot) == false)
                return null;

            PBOInterface.PlanControl.OnArriveInAssemblyStep(lot, assyInfo);

            while (true)
            {
                var assyLot = DoAssembly(assyInfo);

                if (assyLot != null)
                {
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

                                // partLot이 WipLot이고 쪼개진 경우이면, Wip 정보 수정 작업 진행...?
                                // PartLot의 진행이력 생성.
                                
                                assyLot.PreBuildDays = Math.Max(assyLot.PreBuildDays, partLot.PreBuildDays); // 선행 가능 정보 병합.
                                assyLot.CapaPlans.AddRange(partLot.CapaPlans); // 장비 진행 이력 병합.
                                assyLot.SplitInfos.AddRange(partLot.SplitInfos); // BinnedWip 정보 병합
                                assyLot.VirtualPegWips.AddRange(partLot.VirtualPegWips); // Wip 정보 병합 (PartLot이 WipLot이 아닐때도 Arrange 해줘야함...)
                                assyLot.AssemblyHistory.AddRange(partLot.AssemblyHistory); // Assembly 정보 병합
                                assyLot.RefPlans.AddRange(partLot.RefPlans); // 확정계획 정보 병합
                                assyLot.OrgLotKeys.AddRange(partLot.OrgLotKeys);

                                // partLot의 LifeCycle 출력
                                OutputWriter.Instance.WriteLotHistory(partLot, usedQty, LifeCycle.Assembly.ToString(), assyLot.LastStepTime, string.Format("Assembly Lot : {0}({1})", assyLot.LotID, assyLot.Qty));

                                // partLot의 Assembly 정보 출력.
                                OutputWriter.Instance.WriteLotAssemblyLog(assyLot, detail, partLot, usedQty, assyLot.LastStepTime);

                                // 사용된 partassyLot의 처리 작업
                                UpdatePartLot(partLot, usedQty);
                            }
                        }
                        // 나중에 들어간 assembly 정보가 앞에 들어가기.
                        assyLot.AssemblyHistory.Insert(0, assyLot.AssemblyInfo);
                        assyLot.AssemblyInfo = null;

                        PBOInterface.PlanControl.OnCompleteAssembled(assyLot, assyLot.AssemblyHistory.First());
                    }
                    #endregion
                    assyLots.Add(assyLot);
                }
                else
                {
                    break;
                }
            }

            return assyLots;
        }

        /// <summary>
        /// 별도로 Assembly를 호출하여 조립 시도
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="assemblyBom"></param>
        public APELot DoAssembly(ATAssyInfo assyKey)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("Assembly_DoAssembly");
            try
            {
                if (_assyPartInfos.ContainsKey(assyKey) == false)
                {
                    return null;
                }

                var partDic = _assyPartInfos[assyKey];

                Dictionary<ATBomDetail, List<APELot>> availablePartLot = new Dictionary<ATBomDetail, List<APELot>>();
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
                        // CurrentQty => 파트로 일부 사용되고 남은 잔여 수량
                        double convertQty = partlot.CurrentQty.ConvertValue(detail.FromQty, detail.ToQty, PlanType.Forward);
                        if (convertQty < ATOption.Instance.MinimumAllocationQuantity)
                            continue;

                        // 조립 가능 수량 계산.
                        partQty += convertQty;

                        if (partlot.LastStepTime > latestTime)
                            latestTime = partlot.LastStepTime;

                        availablePartLot[detail].Add(partlot);

                        break;
                    }

                    // 조립 가능 수량 산출
                    // 파트 중 가장 작은 값으로 산정
                    canAssemblyQty = Math.Min(canAssemblyQty, partQty);
                }


                if (canAssemblyQty <= ATOption.Instance.MinimumAllocationQuantity)
                    return null;

                //if( 최소단위 배치 수량보다 큰지 작은지 판단 후 조립..?)
                // 조립 가능한 수량이 배치 수량보다 적은 경우 Split을 해서라도 조립을 진행할지 판단
                //if (canAssemblyQty < assembledLot.Qty 
                //    && AssemblyLogic.Instance.CanAssembleSmallBatch(assembledLot, canAssemblyQty, now) == false)
                //    return null;

                var assyTarget = assyKey.OperTarget;
                string lotID = LotHelper.GeneratLotID(ATConstants.ASSY_BATCH_PREFIX, assyTarget.TargetID); 
                var sbatch = _factory.CreateLot(lotID, assyTarget, canAssemblyQty, 0, LotCreateType.Assembly, null);
                
                canAssemblyQty = PBOInterface.PlanControl.AdjustAssemblyQty(sbatch, canAssemblyQty);
                sbatch.Qty = sbatch.CurrentQty = canAssemblyQty;

                sbatch.LastStepTime = latestTime;

                ATAssemblyInfo assyInfo = new ATAssemblyInfo(sbatch);
                sbatch.AssemblyInfo = assyInfo;

                foreach (var part in availablePartLot)
                {
                    var detail = part.Key;
                    var partlots = part.Value;

                    // 실사용 수량.
                    var realQty = canAssemblyQty.ConvertValue(detail.FromQty, detail.ToQty, PlanType.Backward); //; ReverseValueType(detail.Value, detail.ValueType);

                    //List<APELot> unUsablePartLots = new List<APELot>();
                    foreach (var partlot in partlots)
                    {
                        var usedQty = Math.Min(partlot.Qty, realQty);

                        partlot.CurrentQty -= usedQty;
                        realQty -= usedQty;

                        // 조립에 사용된 정보 등록
                        assyInfo.AddPartLot(detail, partlot, usedQty);

                        //if (partlot.CurrentQty < ATOption.Instance.MinimumAllocationQuantity)
                        //    unUsablePartLots.Add(partlot);

                        if (realQty <= ATOption.Instance.MinimumAllocationQuantity)
                            break;
                    }

                    //foreach (var partLot in unUsablePartLots)
                    //{
                    //    partlots.Remove(partLot);
                    //}
                }

                return sbatch;
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("Assembly_DoAssembly");
            }
        }
    }

    public class OBOBufferAssemblyLogic
    {
        public ATBuffer Buffer { get; private set; }

        /// <summary>
        /// 공정 내 조립이 완료되지 않은 작업물 목록입니다.
        /// </summary>
        public Dictionary<ATBom, List<APELot>> AssemblyLots { get; private set; }

        /// <summary>
        /// Assembly Bom 별 Detail 별 Lot 수량 집계
        /// </summary>
        private DoubleDictionary<ATBom, ATBomDetail, List<APELot>> _partLotInfos;

        /// <summary>
        /// Lot별 자신이 part로 포함된 List 정보 
        /// </summary>
        private Dictionary<APELot, List<List<APELot>>> _addBatchListInfos;


        private List<ATOperTarget> _assemblyTargets;
        public OBOBufferAssemblyLogic(ATBuffer buffer, List<ATOperTarget> assemblyTargets)
        {
            this.Buffer = buffer;

            this.AssemblyLots = new Dictionary<ATBom, List<APELot>>();

            this._partLotInfos = new DoubleDictionary<ATBom, ATBomDetail, List<APELot>>();

            this._addBatchListInfos = new Dictionary<APELot, List<List<APELot>>>();

            this._assemblyTargets = assemblyTargets;

        }

        /// <summary>
        /// 공정 내 조립 OperTarget으로부터 조립 후 작업물을 생성합니다.
        /// </summary>
        public void Initialize(ATOperTarget target)
        {

            var lotsize = target.RemainQty;

            var lotQty = target.RemainQty;

            while (lotQty > 0)
            {
                // Lot 사이즈 조절
                var batchqty = Math.Min(lotsize, target.RemainQty);

#warning FEAction 포인트 LotID
                string lotID = LotHelper.GeneratLotID(ATConstants.ASSY_BATCH_PREFIX, target.TargetID); //BatchControl.Instance.GetBatchID(batch.TargetID, ++LotIndex, false);
                var lot = ObjectMapper.CreateLot(lotID, batchqty, target.Oper, target, null, LotCreateType.Assembly);

                if (this.AssemblyLots.TryGetValue( target.CurrentBom, out List<APELot> lst) == false )
                {
                    this.AssemblyLots.Add( target.CurrentBom, lst = new List<APELot>() );
                }

                lst.Add(lot);

                lot.CurrentRoute = target.CurrentBomRoute;

                lotQty -= lot.Qty;
            }
        }

        internal void AddPartLot(APELot lot, List<ATBom> assemblyBom)
        {
            foreach (var bom in assemblyBom)
            {
                if (_partLotInfos.ContainsKey(bom) == false)
                {
                    foreach (var detail in bom.BomDetails)
                    {
                        _partLotInfos.Add(bom, detail, new List<APELot>());
                    }
                }

                var bomdetail = bom.BomDetails.Where(x => x.FromItemSiteBuffer.Key == lot.CurrentItemSiteBuffer.Key).FirstOrDefault();

                if (bomdetail == null)
                {
                    continue;
                }

                List<APELot> list;
                if (_partLotInfos.TryGetValue(bom, bomdetail, out list) == false)
                {
                    continue;
                }

#warning PartLotCompare 추가 작업
                //var index = list.BinarySearch(lot, new PartLotComparer(null));

                //if (index < 0)
                //    index = ~index;

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
                if (this._addBatchListInfos.TryGetValue(partLot, out lst) == true)
                {
                    lst.ForEach(x => x.Remove(partLot));
                }
                else
                {
                    // 이상한 경우.
                }
            }


        }

        public APELot DoAssembly(APELot assembledLot)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("BufferAssemblyLogic_DoAssembly");
            try
            {
                ATBom targetBom = assembledLot.CurrentBom;

                // Part 정보가 없는 경우.
                if (this._partLotInfos.ContainsKey(targetBom) == false)
                {
                    return null;
                }

                Dictionary<ATBomDetail, List<APELot>> partDic = this._partLotInfos[targetBom];
                Dictionary<ATBomDetail, List<APELot>> availablePartLot = new Dictionary<ATBomDetail, List<APELot>>();
                HashSet<APELot> lots = new HashSet<APELot>();
                foreach (var item in _partLotInfos.Values)
                {
                    foreach (var item2 in item.Values)
                    {
                        lots.AddRange(item2);
                    }
                }
                DateTime now = ATOption.Instance.PlanStartTime;// APESolver.Instance.NowDT;

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
                        //??
                        if (partQty >= assembledLot.Qty)
                            break;

                        // CurrentQty => 파트로 일부 사용되고 남은 잔여 수량
                        if (partlot.CurrentQty < ATOption.Instance.MinimumAllocationQuantity)
                        {
                            // 예외처리..?
                            continue;
                        }

                        // laststeptime..
                        //if (AssemblyLogic.Instance.FilterPartLot(partlot, assembledLot, now) == true)
                        if (partlot.CurrentTarget.SODemand.ID != assembledLot.CurrentTarget.SODemand.ID)
                        {
                            continue;
                        }

                        partQty += partlot.CurrentQty.ConvertValue(detail.FromQty, detail.ToQty, PlanType.Forward);

                        if (partlot.LastStepTime > latestTime)
                            latestTime = partlot.LastStepTime;

                        
                        availablePartLot[detail].Add(partlot);

                        break;
                    }

                    // 조립 가능 수량 산출
                    // 파트 중 가장 작은 값으로 산정
                    if (canAssemblyQty > partQty)
                        canAssemblyQty = partQty;

                }

                if (canAssemblyQty <= ATOption.Instance.MinimumAllocationQuantity)
                    return null;

                //// 승진형 이부부만 주석처리해주면 될 것같아요 그냥
                //if (assembledLot.Qty < canAssemblyQty)
                //    canAssemblyQty = assembledLot.Qty;

                // 조립 가능한 수량이 배치 수량보다 적은 경우 Split을 해서라도 조립을 진행할지 판단
               // if (canAssemblyQty < assembledLot.Qty 
                   // &&  AssemblyLogic.Instance.CanAssembleSmallBatch(assembledLot, canAssemblyQty, now) == false)
               //      return null;

                var sbatch = assembledLot;

                 
                // Assy 번호 재발번해서 진행.
                //string lotID = LotHelper.GeneratLotID(ATConstants.ASSY_BATCH_PREFIX, assembledLot.CurrentTarget.TargetID);
                //sbatch = PegSolver.GenerateSplitLot(assembledLot, canAssemblyQty, lotID);
                var assyTarget = assembledLot.CurrentTarget;
                string lotID = LotHelper.GeneratLotID(ATConstants.ASSY_BATCH_PREFIX, assyTarget.TargetID); //BatchControl.Instance.GetBatchID(batch.TargetID, ++LotIndex, false);
                sbatch = ObjectMapper.CreateLot(lotID, canAssemblyQty, assyTarget.Oper, assyTarget, null, LotCreateType.Assembly);
               

                sbatch.LastStepTime = latestTime;

                ATAssemblyInfo assyInfo = new ATAssemblyInfo( sbatch);

                sbatch.AssemblyInfo = assyInfo;

                foreach (var part in availablePartLot)
                {
                    var detail = part.Key;
                    var partlots = part.Value;

                    var realQty = canAssemblyQty.ConvertValue(detail.FromQty, detail.ToQty, PlanType.Backward); //; ReverseValueType(detail.Value, detail.ValueType);

                    List<APELot> unUsablePartLots = new List<APELot>();
                    foreach (var partlot in partlots)
                    {
                        var usedQty = Math.Min(partlot.Qty, realQty);

                        partlot.CurrentQty -= usedQty;
                        realQty -= usedQty;

                        // 조립에 사용된 정보 등록
                        assyInfo.AddPartLot(detail, partlot, usedQty);

                        if (partlot.CurrentQty < ATOption.Instance.MinimumAllocationQuantity)
                            unUsablePartLots.Add(partlot);

                        if (realQty <= ATOption.Instance.MinimumAllocationQuantity)
                            break;
                    }

                    foreach (var partLot in unUsablePartLots)
                    {
                        partlots.Remove(partLot);
                    }
                }

                return sbatch;
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("BufferAssemblyLogic_DoAssembly");
            }

            // 조립된 배치가 실질적으로 진행 후 파트의 정보를 업데이트 진행
        }

        public List<APELot> GetRemainAssemblyLots()
        {
            return _addBatchListInfos.Keys.ToList();
        }

        public DoubleDictionary<ATBom, ATBomDetail, List<APELot>> GetRemainPartLots()
        {
            return _partLotInfos;
        }

    } 

}
