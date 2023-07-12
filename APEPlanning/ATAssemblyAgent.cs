using Mozart.Collections;
using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class ATAssemblyAgent : IAgent
    {
        public static ATAssemblyAgent Instance
        {
            get { return ServiceLocator.Resolve<ATAssemblyAgent>(); }
        }

        DoubleDictionary<IAssemblyKey, ATBomDetail, HashSet<APELot>> _assyPartInfos;
        Dictionary<APELot, HashSet<APELot>> _partLots;


        AssemblyInterface AssyControl;

        #region Agent
        public void Initialize()
        {
            _assyPartInfos = new DoubleDictionary<IAssemblyKey, ATBomDetail, HashSet<APELot>>();
            _partLots = new Dictionary<APELot, HashSet<APELot>>();

            var current = ATExecutionContext.Instance.CurrentExecutionInfo.ModuleType;
            if (current == ModuleType.PBO)
            {
                //AssyControl = ObyO.OboInterfaces.PlanControl;
                //
                AssyControl = new AssemblyInterface();
            }
            else if (current == ModuleType.PBF)
            {
                //AssyControl = Planner.FWInterface.AssemblyControl;
                AssyControl = new AssemblyInterface();
            }
        }

        public void Dispose()
        {
            ClearPartLots();
            this._assyPartInfos = null;
            this._partLots = null;
            this.AssyControl = null;
        }
        #endregion

        public List<APELot> GetRemainPartLot()
        {
            return _partLots.Keys.ToList();
        }

        public void ClearPartLots()
        {
            this._assyPartInfos.Clear();
            this._partLots.Clear();
        }


        public bool AddPartLot(APELot lot, IAssemblyKey key)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("Assembly_AddPartLot");
            try
            {
                var target = lot.CurrentTarget;
                var bom = key.CurrentBom;


                HashSet<APELot> list;
                if (_assyPartInfos.ContainsKey(key) == false)
                {
                    foreach (var detail in bom.BomDetails)
                    {
                        list = new HashSet<APELot>();
                        _assyPartInfos.Add(key, detail, list);
                    }
                }

                list = _assyPartInfos[key, lot.CurrentBomDetail];

                list.Add(lot);
                _partLots.Add(lot, list);

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

                    // 수량 집계.
                }
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("Assembly_UpdatePartLot");
            }
        }

        public List<APELot> AddRun(APELot lot, IAssemblyKey key)
        {
            List<APELot> assyLots = new List<APELot>();

            ATOperTarget target = lot.CurrentTarget;

            if (AddPartLot(lot, key) == false)
            {
                return null;
            }

            AssyControl.OnArriveInAssemblyStep(lot, target);

            while (true)
            {
                var assyLot = DoAssembly(target);

                if (assyLot != null)
                {
                    AssyControl.OnCompleteAssembled(assyLot, assyLot.AssemblyInfo);

                    #region PBO의 경우
                    if (assyLot.AssemblyInfo != null)
                    {
                        foreach (var detail in assyLot.AssemblyInfo.PartInfo.Keys)
                        {
                            var dic = assyLot.AssemblyInfo.PartInfo[detail];

                            foreach (var pairkey in dic)
                            {
                                var partLot = pairkey.Key;
                                var usedQty = pairkey.Value;

                                if (true)
                                {
                                    // PBO의 경우
                                    // PartLot의 이력정보들을 모두 Merge 하는 작업 수행 ?

                                    assyLot.PreBuildDays = Math.Max(assyLot.PreBuildDays, partLot.PreBuildDays);

                                    // 장비 진행 이력 병합.
                                    assyLot.CapaPlans.AddRange(partLot.CapaPlans);

                                    // BinnedWip 정보 병합
                                    assyLot.SplitInfos.AddRange(partLot.SplitInfos);

                                    // Wip 정보 병합 (PartLot이 WipLot이 아닐때도 Arrange 해줘야함...)

                                    assyLot.VirtualPegWips.AddRange(partLot.VirtualPegWips);

                                    assyLot.AssemblyHistory.AddRange(partLot.AssemblyHistory);

                                    assyLot.RefPlans.AddRange(partLot.RefPlans);
                                }
                                else
                                {
                                    // PBF의 경우
                                    lot.Plans.AddRange(partLot.Plans);
                                    lot.CapaPlans.AddRange(partLot.CapaPlans);

                                    // PartLot들이 Split되어 Assembly에 기여하더라도 최초에 한번 기록하면 문제가 없을 것으로 보임
                                    partLot.Plans.Clear();
                                    partLot.CapaPlans.Clear();
                                }


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
                    }
                    #endregion

                    //#region PBF의 경우
                    //if (lot.AssemblyInfo != null)
                    //{
                    //    foreach (var detail in lot.AssemblyInfo.PartInfo.Keys)
                    //    {
                    //        var dic = lot.AssemblyInfo.PartInfo[detail];

                    //        foreach (var pairkey in dic)
                    //        {
                    //            var partLot = pairkey.Key;
                    //            var usedQty = pairkey.Value;

                    //            lot.Plans.AddRange(partLot.Plans);
                    //            lot.CapaPlans.AddRange(partLot.CapaPlans);

                    //            // PartLot들이 Split되어 Assembly에 기여하더라도 최초에 한번 기록하면 문제가 없을 것으로 보임
                    //            partLot.Plans.Clear();
                    //            partLot.CapaPlans.Clear();

                    //            // partLot의 LifeCycle 출력
                    //            OutputWriter.Instance.WriteLotHistory(partLot, usedQty, LifeCycle.Assembly.ToString(), lot.LastStepTime, string.Format("Assembly Lot : {0}({1})", lot.LotID, lot.Qty));

                    //            // partLot의 Assembly 정보 출력.
                    //            OutputWriter.Instance.WriteLotAssemblyLog(lot, detail, partLot, usedQty, lot.LastStepTime);

                    //            // 사용된 partlot의 처리 작업
                    //            UpdatePartLot(partLot, usedQty);
                    //        }
                    //    }

                    //    lot.AssemblyInfo = null;
                    //}
                    //#endregion

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
        public APELot DoAssembly(ATOperTarget target)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("Assembly_DoAssembly");
            try
            {
                if (_assyPartInfos.ContainsKey(target) == false)
                {
                    return null;
                }

                var partDic = _assyPartInfos[target];

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

                    canAssemblyQty = Math.Min(canAssemblyQty, partQty);
                }

                if (canAssemblyQty <= ATOption.Instance.MinimumAllocationQuantity)
                    return null;

                //if( 최소단위 배치 수량보다 큰지 작은지 판단 후 조립..?)
                // 조립 가능한 수량이 배치 수량보다 적은 경우 Split을 해서라도 조립을 진행할지 판단
                //if (canAssemblyQty < assembledLot.Qty 
                //    && AssemblyLogic.Instance.CanAssembleSmallBatch(assembledLot, canAssemblyQty, now) == false)
                //    return null;

                string lotID = LotHelper.GeneratLotID(ATConstants.ASSY_BATCH_PREFIX, target.TargetID);
                var sbatch = ObjectMapper.CreateLot(lotID, canAssemblyQty, target.Oper, target, null, LotCreateType.Assembly);

                canAssemblyQty = AssyControl.AdjustAssemblyQty(sbatch, canAssemblyQty);

                if (canAssemblyQty <= ATOption.Instance.MinimumAllocationQuantity)
                    return null;

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
    
}
