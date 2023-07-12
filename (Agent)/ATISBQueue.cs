using Mozart.Collections;
using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    /// <summary>
    /// Class의 목적
    /// 
    /// </summary>
    public class ATISBQueue
    {
        public ATItemSiteBuffer Key;

        public ATItemSiteBuffer ItemSiteBuffer => Key;
        /// <summary>
        /// 현 ITEM STIE BUFFER의 Wip 수량
        /// </summary>
        public HashSet<APEWip> WipInfos;

        /// <summary>
        /// 현 ISB의 Wip 수량
        /// </summary>
        public double WipQty;

        // 현 ITEM SITE BUFFER의 누적 Wip 수량

        public double RealCumWipQty;

        public double VirtualCumWipQty;

        public double CumWipQty
        {
            get
            {
                return RealCumWipQty - VirtualCumWipQty;
            }
        }

        // 구간별 Wip 집계
        // 조립 전 구간까지 윕 집계 로직.
        // public SortedDictionary<string, double> CumWipOnRagne;

        public Dictionary<string, APEWip> CumWipQueue;

        /// <summary>
        /// Assembly 의 경우에 Bom별 Cum 수량 정보 저장
        /// </summary>
        public Dictionary<ATBom, double> AssemblyCumWipQty;

        public ATISBQueue(ATItemSiteBuffer itemsite)
        {
            this.Key = itemsite;

            ATISBQueueAgent.Instance.AddPlanWipQueue(this);
        }

        internal void Initialize()
        {
            this.WipInfos = new HashSet<APEWip>();
            this.CumWipQueue = new Dictionary<string, APEWip>();
            this.AssemblyCumWipQty = new Dictionary<ATBom, double>();

            this.WipQty = 0;
            this.RealCumWipQty = 0;
            this.VirtualCumWipQty = 0;

            foreach (var bom in ItemSiteBuffer.PrevBoms.Keys)
            {
                if (bom.BomType == BomType.Assembly)
                {
                    this.AssemblyCumWipQty.Add(bom, 0);
                }
            }
        }

        internal void Dispose()
        {
            this.WipInfos = null;
            this.CumWipQueue = null;
            this.AssemblyCumWipQty = null;

            this.WipQty = 0;
            this.VirtualCumWipQty = 0;
            this.RealCumWipQty = 0;
        }

        /// <summary>
        /// Wip 정보 등록
        /// </summary>
        /// <param name="wip"></param>
        public bool AddPlanWip(APEWip wip)
        {
            if (WipInfos.Add(wip))
            {
                WipQty += wip.RemainQty;
                RealCumWipQty += wip.RemainQty;

                CumWipQueue.Add(wip.WipInfo.LotID, wip);
                wip.ReferenceWip = wip;
                OutputWriter.Instance.WriteItemSiteBufferWipLog(wip, "Add", wip.ItemSiteBuffer);

                return true;
            }
            else
            {
                // ??
                if (CumWipQueue.TryGetValue(wip.WipInfo.LotID, out var value))
                {
                    WipQty += wip.RemainQty;
                    RealCumWipQty += wip.RemainQty;
                }
                return false;
            }
        }

        public void RollbackPlanWip(APEWip wip, double pegWipQty)
        {
            this.VirtualCumWipQty -= pegWipQty;

            if (CumWipQueue.ContainsKey(wip.WipInfo.LotID))
            {
                foreach (var itemSiteBuffer in wip.CumItemSiteBuffes)
                {
                    if (itemSiteBuffer.WipQueue.CumWipQueue.TryGetValue(wip.WipInfo.LotID, out var value))
                        itemSiteBuffer.WipQueue.VirtualCumWipQty -= pegWipQty.ConvertValue(value.FromQty, value.ToQty, PlanType.Forward);
                }
            }
        }

        /// <summary>
        /// Pegging 시점 호출 작업 필요.
        /// </summary>
        /// <param name="wip"></param>
        /// <param name="pegWipQty"></param>
        public void PeggedPlanWip(APEWip wip, double pegWipQty)
        {
            if (wip.IsAllPeggedWip)
                this.WipInfos.Remove(wip);

            this.WipQty -= pegWipQty;
            if (this.WipQty <= ATOption.Instance.MinimumAllocationQuantity)
                this.WipQty = 0;

            this.RealCumWipQty -= pegWipQty;
            this.VirtualCumWipQty -= pegWipQty;
            
            OutputWriter.Instance.WriteItemSiteBufferWipLog(wip, "Peg", wip.ItemSiteBuffer, pegWipQty);

            if (CumWipQueue.ContainsKey(wip.WipInfo.LotID))
            {
                if (wip.IsAllPeggedWip)
                {
                    CumWipQueue.Remove(wip.WipInfo.LotID);
                    foreach (var itemSiteBuffer in wip.CumItemSiteBuffes)
                    {
                        itemSiteBuffer.WipQueue.CumWipQueue.Remove(wip.WipInfo.LotID);
                    }
                }
                else
                {
                    foreach (var itemSiteBuffer in wip.CumItemSiteBuffes)
                    {
                        if (itemSiteBuffer.WipQueue.CumWipQueue.TryGetValue(wip.WipInfo.LotID, out var value))
                        {
                            itemSiteBuffer.WipQueue.RealCumWipQty -= pegWipQty.ConvertValue(value.FromQty, value.ToQty, PlanType.Forward);
                            itemSiteBuffer.WipQueue.VirtualCumWipQty -= pegWipQty.ConvertValue(value.FromQty, value.ToQty, PlanType.Forward);
                        }
                    }
                }
            }
        }

        public double GetCumWipQty(ATBomDetail detail, DateTime dt, bool isDetail = true)
        {
            if (isDetail == false)
                return CumWipQty.ConvertValue(detail.FromQty, detail.ToQty, PlanType.Forward);

            double cumWipQty = 0;
            var newDt = dt.SplitDate();

            foreach (var item in CumWipQueue)
            {
                if (item.Value.AvailableTime <= newDt)
                    cumWipQty += item.Value.CumWipQty;
            }

            return cumWipQty.ConvertValue(detail.FromQty, detail.ToQty, PlanType.Forward);
        }

        public double GetCumWipQty()
        {
            return this.CumWipQty;
        }

        public double GetCurrentWipQty(DateTime dt)
        {
            double wipQty = 0;
            foreach(var wip in this.WipInfos)
            {
                if (wip.AvailableTime > dt)
                    continue;

                wipQty += wip.RemainQty;
            }

            return wipQty;
        }

        public double GetCumWipQty(DateTime dt, bool isDetail = true)
        {
            if (isDetail == false)
                return CumWipQty;

            double cumWipQty = 0;
            var newDt = dt.SplitDate();
            
            foreach (var item in CumWipQueue)
            {
                if (item.Value.AvailableTime <= newDt)
                    cumWipQty += item.Value.CumWipQty;
            }

            return cumWipQty;
        } 

        public void UpdateCumWipOnRange(bool isFirst, APEWip wip, ATBom bom, ATBomDetail detail)
        {
            #region Bom을 고려한 수량 변화
            double toQty = detail.ToQty;
            if (bom.BomType == BomType.SplitCo || bom.BomType == BomType.SplitBy)
            {
                toQty = 0;
                foreach (var item in bom.BomDetails)
                {
                    if (item == detail)
                    {
                        toQty += item.ToQty;
                    }
                    else 
                    {
                        foreach (var so in detail.ToItemSiteBuffer.SoItemSiteBuffers)
                        {
                            if (item.ToItemSiteBuffer.SoItemSiteBuffers.Contains(so))
                            {
                                toQty += item.ToQty;
                                break;
                            }
                        }
                    }
                }
            }

            wip.FromQty *= detail.FromQty;
            wip.ToQty *= toQty;
            #endregion

            #region Yield를 고려한 수량 변화
            if (isFirst && wip.WipInfo.LotType == WipType.Wip)
            {
                wip.ToQty *= wip.Oper.CumYield;
            }
            else
            {
                var route = detail.Bom.BomRoutes.First().Route;
                wip.ToQty *= route.CumYield;
            }
            #endregion

            if (CumWipQueue.TryGetValue(wip.WipInfo.LotID, out var value) == false)
            {
                RealCumWipQty += wip.CumWipQty;
                CumWipQueue.Add(wip.WipInfo.LotID, wip);
                OutputWriter.Instance.WriteItemSiteBufferWipLog(wip, "Add", detail.ToItemSiteBuffer);
            }
        }
    }
}
