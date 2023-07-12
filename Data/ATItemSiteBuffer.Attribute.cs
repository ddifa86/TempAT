using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Outputs;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public partial class ATItemSiteBuffer
    {
        #region Property

//        static public ATItemSiteBuffer Dummy = new ATItemSiteBuffer

        public BomPathType IsUsablePath { get; set; }

        // 투입부터 해당 ItemSite까지 가장 빨리 올 수 있는 누적 Tat 정보
        public double MinCumTat { get; set; }

        public double MaxCumTat { get; set; }

        public double LateCumTat { get; set; }

        public double MaxCumYield { get; set; }

        /// <summary>
        /// PlanWip Queue
        /// 현 ItemSiteBuffer의 Wip 과 F/W 누적 Wip 정보를 집계.
        /// </summary>
        public ATISBQueue WipQueue { get; set; }

        /// <summary>
        /// Buffer별 참조가능한 Resource 정보들
        /// </summary>
        public Dictionary<ATBuffer, HashSet<ATResource>> PrevResources { get; set; } 

        /// <summary>
        /// 현 ItemSiteBuffer로와 연결될 수 있는 Buffer별 Bom 정보들
        /// </summary>
        public Dictionary<ATBuffer, HashSet<ATBom>> PrevBomsAll { get; set; } 

        /// <summary>
        /// 현 ItemSiteBuffer로 연결될 수 있는 이전 Buffer들별 ItemSiteBuffer 정보
        /// </summary>
        public Dictionary<ATBuffer, HashSet<ATItemSiteBuffer>> PrevItemSiteBuffers { get; set; }

        public Dictionary<ATBuffer, HashSet<ATItemSiteBuffer>> NextItemSiteBuffers { get; set; }

        /// <summary>
        /// 기여하는 SOItemSiteBuffer
        /// </summary>
        public HashSet<ATItemSiteBuffer> SoItemSiteBuffers { get; set; }

        /// <summary>
        /// 이전 Buffer까지 존재가능한 Assembly ItemSiteBuffer 정보들.
        /// 현재 ItemSiteBuffer로가 조립가능한 ItemSiteBuffer로이면 현 ItemSiteBuffer로도 포함
        /// </summary>
        public HashSet<ATItemSiteBuffer> PrevAssyItemSiteBuffers { get; set; }

        public bool IsAssemblyProduct = false;

        public bool IsMaterialItemSiteBuffer
        {
            get
            {
                return this.Item.ItemType == ItemType.Material;
            }
        }
        #endregion

        #region  하이닉스 전용
        // 
        /// <summary>
        /// soItemSite별 누적 Wip 수량
        /// </summary>
        public Dictionary<string, double> SumWipQtysBySoItemSites = new Dictionary<string, double>();
              
        public SortedDictionary<DateTime, APEBufferPSISummary> PSISummarys = new SortedDictionary<DateTime, APEBufferPSISummary>();

        #endregion

        public double GetMinCumTAT(DateTime? dt = null)
        {
            if (dt == null)
                return MaxCumTat;


            return MaxCumTat;
        }

        public bool IsNoCarryover { get; internal set; }

        public bool IsInfiniteMaterial { get; internal set; }

        public double InputLotSize { get; internal set; }
                
        public List<ATCalendarDetail> InfiniteMaterialInfo { get; internal set; }

        public Dictionary<string, List<APERefPlan>> RefPlans { get; internal set; } 

        #region WipQueue 관리
        internal void AddPlanWip(APEWip wip)
        {
            bool isFirst = this.WipQueue.AddPlanWip(wip);

            if (isFirst)
                UpdateNextBomCumWip(this, wip, wip);
        }

        internal void PeggedPlanWip(APEWip wip, double peggedQty)
        {
            this.WipQueue.PeggedPlanWip(wip, peggedQty);
        }

        private void UpdateNextBomCumWip(ATItemSiteBuffer itemsitebuffer, APEWip wip, APEWip orgWip)
        {
            bool isFirst = wip == orgWip;
            if (itemsitebuffer.NextBoms.Count == 0 && itemsitebuffer.OrgItemSiteBuffers.Count > 0)
            {
                foreach (var isb in itemsitebuffer.OrgItemSiteBuffers)
                {
                    UpdateNextBomCumWip(isb, wip, orgWip);
                }
            }

            foreach (var pair in itemsitebuffer.NextBoms)
            {
                var bom = pair.Key;
                var details = pair.Value;

                if (bom.BomType == BomType.Assembly)
                    continue;

                if (bom.BomType == BomType.SplitBy || bom.BomType == BomType.SplitCo)
                    details = new List<ATBomDetail> { bom.MainBomDetail };

                foreach (var detail in details)
                {
                    var cloneWip = wip.ShallowCopy();
                    cloneWip.ReferenceWip = orgWip;

                    var toItemSite = detail.ToItemSiteBuffer;
                    if (detail.Bom.BomRoutes.Count == 0)
                        continue;

                    double sumTat;
                    if (isFirst && wip.WipInfo.LotType == WipType.Wip)
                    {
                        var oper = wip.Oper;
                        sumTat = oper.CumRunTat + oper.CumWaitTat;
                    }
                    else
                    {
                        var route = detail.Bom.BomRoutes.First().Route;
                        sumTat = route.SumRunTat + route.SumWaitTat;
                    }
                    
                    cloneWip.AvailableTime = cloneWip.AvailableTime.AddSeconds(sumTat);

                    if (orgWip.CumItemSiteBuffes.Add(toItemSite) == false)
                        continue;

                    toItemSite.WipQueue.UpdateCumWipOnRange(isFirst, cloneWip, bom, detail);

                    // partchage 전환 비율이 반영된 wip 수량 전달
                    UpdateNextBomCumWip(toItemSite, cloneWip, orgWip);
                }
            }
        }
        #endregion

        #region PSI 관리

        /// <summary>
        /// 초기 재공을 집계
        /// </summary>
        /// <param name="info"></param>
        public void AddBufferPSISummaryPlanWip(APEWip info)
        {
            if (info.Oper.IsBuffer == false)
                return;

            DateTime inDate = info.AvailableTime.StartTimeOfDayT();

            double boh = 0;
            double inQty = 0;

            if (inDate <= ATOption.Instance.PlanStartTime)
                boh = info.RemainQty;
            else
                inQty = info.RemainQty;

            AddBufferPSISummary(inDate, boh, inQty, 0);
        }

        /// <summary>
        /// Obo 전용 PSI Summary 집계 로직
        /// </summary>
        /// <param name="info"></param>
        public void AddBufferPSISummary(APEPlanInfo info)
        {
            var operation = info.LotInfo.Operation;
            if (operation.IsBuffer == false)
                return;

            double qty = info.AllocationInfo.AllocLotQty;

            double inQty = qty;
            double outQty = qty;

            // 초기 Wip의 경우에는 inQty는 0으로 처리.
            // Pegging되는 Wip들의 경우 모두 중복 집계될 수 있으므로 0으로 처리.
            if (info.LotInfo.IsFirstOperation && string.IsNullOrEmpty(info.LotInfo.WipType) == false)
                inQty = 0;

            // In수량 집계
            DateTime inDate = info.ArrivalTime.StartTimeOfDayT();

            AddBufferPSISummary(inDate, 0, inQty, 0);

            // out 수량 집계.
            DateTime outDate = info.EndTime.StartTimeOfDayT();
            if (outDate < ATOption.Instance.PlanStartTime)
                outDate = ATOption.Instance.PlanStartTime;

            if (outDate >= ATOption.Instance.PlanStartTime && outDate < ATOption.Instance.PlanEndTime)
                AddBufferPSISummary(outDate, 0, 0, outQty);
        }

        /// <summary>
        /// PBF PSI Summary 집계 로직
        /// </summary>
        /// <param name="info"></param>
        public void AddPSISummary(APEPlanInfo info)
        {
            var operation = info.LotInfo.Operation;
            var route = operation.Route as ATRoute;

            bool isIn = info.LotInfo.Operation.IsBuffer;
            bool isOut = info.LotInfo.Operation == route.LastOper;

            if (isIn)
            {
                double inQty = info.AllocationInfo.AllocLotQty;

                if (info.LotInfo.IsFirstOperation && string.IsNullOrEmpty(info.LotInfo.WipType) == false) // BOH 중복 집계 차단
                    inQty = 0;

                DateTime inDate = info.ArrivalTime.StartTimeOfDayT();

                AddBufferPSISummary(inDate, 0, inQty, 0);

                if ((operation as ATBuffer).Stage.BufferRoute.LastOper == operation)
                    isOut = true;
            }
            
            if (isOut)
            {
                double outQty = info.AllocationInfo.AllocLotQty;

                DateTime outDate = info.EndTime.StartTimeOfDayT();

                if (outDate < ATOption.Instance.PlanStartTime)
                    outDate = ATOption.Instance.PlanStartTime;

                if (outDate < ATOption.Instance.PlanStartTime || outDate >= ATOption.Instance.PlanEndTime)
                    return;

                if (info.LotInfo.Bom.BomType == BomType.Assembly)
                {
                    foreach (var detail in info.LotInfo.Bom.BomDetails)
                    {
                        double newQty = outQty.ConvertValue(detail.FromQty, detail.ToQty, PlanType.Backward); // * detail.FromQty / detail.ToQty;
                        detail.FromItemSiteBuffer.AddBufferPSISummary(outDate, 0, 0, newQty);
                    }
                }
                else
                {
                    AddBufferPSISummary(outDate, 0, 0, outQty);
                }
            }
        }

        public void AddBufferPSISummary(DateTime date, double boh, double inQty, double outQty)
        {
            var key = date;
            APEBufferPSISummary sum;
            if (PSISummarys.TryGetValue(key, out sum) == false)
            {
                sum = new APEBufferPSISummary(this, date);

                PSISummarys.Add(key, sum);
            }

            sum.BOH += boh;
            sum.In += inQty;
            sum.Out += outQty;                    
        }
        #endregion

        internal void SetNextAttributeInfos()
        {
            foreach (var pair in this.NextBoms)
            {
                var bom = pair.Key;
                RegistNextItemSiteBuffers(bom);
            }

            foreach (var pair in this.PrevBoms)
            {
                var bom = pair.Key;
                var details = pair.Value;

                bom.NextItemSiteBuffers.AddRange(this.NextItemSiteBuffers.ToDictionary(x => x.Key, x => x.Value));
            }
        }

        internal void SetPrevAttributeInfos(ATBuffer firstBuffer)
        {
            // 투입 공정의 경우 MinCumTat = 0 으로 전환.
            if (this.BufferID == firstBuffer.BufferID || this.Item.ItemType == ItemType.Material)
            {
                this.MinCumTat = 0;
                this.MaxCumTat = 0;
                this.LateCumTat = 0;
                this.MaxCumYield = 1;
            }

            foreach (var pair in this.PrevBoms)
            {
                var bom = pair.Key;

                //Bom의 가용성 여부 체크
                if (this.BufferID != firstBuffer.BufferID)
                    bom.SetUsableBom();

                RegistPrevBomResource(bom);

                RegistPrevBom(bom);

                RegistPrevItemSiteBuffers(bom);

                RegistPrevAssyItemSiteBuffers(bom);

                SetCumInfo(bom);
            }

            // 현 ItemStieBuffer가 투입으로 부터 전개될 수 있는지에 대한 가부 체크
            SetUsablePath(firstBuffer);

            #region NextBom 정보 설정
            foreach (var pair in this.NextBoms)
            {
                var bom = pair.Key;
                var details = pair.Value;

                // BomDetail별 가용성 체크
                details.ForEach(x => x.IsUsableDetail = this.IsUsablePath);

                bom.PrevResources.AddRange(this.PrevResources.ToDictionary(x => x.Key, x => x.Value)); //.DeepClone()
                bom.PrevBoms.AddRange(this.PrevBomsAll.ToDictionary(x => x.Key, x => x.Value)); //.DeepClone()
                bom.PrevItemSiteBuffers.AddRange(this.PrevItemSiteBuffers.ToDictionary(x => x.Key, x => x.Value)); //.DeepClone()
                bom.PrevAssyItemSiteBuffers.AddRange(this.PrevAssyItemSiteBuffers);
                bom.MaxCumYield = Math.Min(bom.MaxCumYield, this.MaxCumYield * bom.GetMaxYield());
            }
            #endregion
        }

        /// <summary>
        /// 현재 ItemSiteBuffer의 PrevBom 중에 투입까지 진행가능한 Bom 정보 존재 여부 설정
        /// </summary>
        /// <param name="firstBuffer"></param>
        private void SetUsablePath(ATBuffer firstBuffer)
        {
            if (this.BufferID == firstBuffer.BufferID || this.Item.ItemType == ItemType.Material)
            {
                this.IsUsablePath = BomPathType.Y;
                return;
            }

            bool hasNPath = false;
            bool hasDPath = false;
            bool hasYPath = false;
            foreach (var bom in this.PrevBoms.Keys)
            {
                if (bom.IsUsableBom == BomPathType.N)
                    hasNPath = true;
                if (bom.IsUsableBom == BomPathType.D)
                    hasDPath = true;
                if (bom.IsUsableBom == BomPathType.Y)
                    hasYPath = true;
            }

            if (hasDPath)
                this.IsUsablePath = BomPathType.D;
            else if (hasNPath)
                this.IsUsablePath = hasYPath ? BomPathType.D : BomPathType.N;
            else if (hasYPath)
                this.IsUsablePath = BomPathType.Y;
            else
                this.IsUsablePath = BomPathType.N;
        }


        private void SetCumInfo(ATBom bom)
        {
            // Tat 집계
            // CUM TAT 산출 => MIN 값을 취하여 최소시간으로 진행가능한 TAT 정보 저장.
            if (bom.IsUsableBom != BomPathType.N)
            {
                this.MinCumTat = Math.Min(this.MinCumTat, bom.GetFromItemSiteCumTat(DateTime.MinValue) + bom.MinTat);
                this.MaxCumTat = Math.Min(this.MaxCumTat, bom.GetFromItemSiteMaxCumTat() + bom.MaxTat);
                this.LateCumTat = Math.Max(this.LateCumTat, bom.GetFromItemSiteLateCumTat() + bom.MaxTat);
                this.MaxCumYield = Math.Max(this.MaxCumYield, bom.MaxCumYield);
            }
        }

        

        /// <summary>
        /// PrevBomResources 정보 등록
        /// </summary>
        /// <param name="bom"></param>
        private void RegistPrevBomResource(ATBom bom)
        {
            // Bom의 FromItemSiteBuffer 기준으로 등록
            foreach (var detail in bom.BomDetails)
            {
                detail.ToItemSiteBuffer.PrevResources.AddRange(bom.PrevResources.ToDictionary(x => x.Key, x => x.Value));
            }
        }

        /// <summary>
        /// PrevBomsByBuffer 정보 등록
        /// </summary>
        /// <param name="bom"></param>
        private void RegistPrevBom(ATBom bom)
        {
            HashSet<ATBom> boms;
            foreach (var info in bom.PrevBoms)
            {
                if (this.PrevBomsAll.TryGetValue(info.Key, out boms) == false)
                    this.PrevBomsAll.Add(info.Key, boms = new HashSet<ATBom>());

                boms.AddRange(info.Value);
            }

            if (this.PrevBomsAll.TryGetValue(this.Buffer, out boms) == false)
                this.PrevBomsAll.Add(this.Buffer, boms = new HashSet<ATBom>());

            boms.Add(bom);
        }


        /// <summary>
        /// 이전 Buffer의 조립 ItemSiteBuffer 정보들 등록.
        /// </summary>
        /// <param name="bom"></param>
        private void RegistPrevAssyItemSiteBuffers( ATBom bom)
        {
            PrevAssyItemSiteBuffers.AddRange(bom.PrevAssyItemSiteBuffers);
        }


        #region ItemSite별 초기 데이터 구성 작업

        private void RegistPrevItemSiteBuffers(ATBom bom)
        {
            foreach (var info in bom.PrevItemSiteBuffers)
            {
                HashSet<ATItemSiteBuffer> prevIsb;
                if (this.PrevItemSiteBuffers.TryGetValue(info.Key, out prevIsb) == false)
                {
                    prevIsb = new HashSet<ATItemSiteBuffer>();
                    this.PrevItemSiteBuffers.Add(info.Key, prevIsb);
                }

                prevIsb.AddRange(info.Value);
            }
        }

        private void RegistNextItemSiteBuffers(ATBom bom)
        {
            foreach (var info in bom.NextItemSiteBuffers)
            {
                HashSet<ATItemSiteBuffer> nextIsb;
                if (this.NextItemSiteBuffers.TryGetValue(info.Key, out nextIsb) == false)
                {
                    nextIsb = new HashSet<ATItemSiteBuffer>();
                    this.NextItemSiteBuffers.Add(info.Key, nextIsb);
                }

                nextIsb.AddRange(info.Value);
            }
        }

        public bool IsInputItemSiteBuffer(APELot lot, ATOperTarget target)
        {
            if (lot.IsWipLot)
                return true;

            if (this.IsMaterialItemSiteBuffer == false)
            {
                if (ATOption.Instance.BlockProductSupply == true)
                    return false;
                else
                    return true;
            }

            if (ATOption.Instance.ApplyInfiniteMaterial == false)
                return false;

            if (this.IsInfiniteMaterial)
                return true;

            return false;
        }

        public double GetInputBatchSize()
        {
            double inputLotSize = this.InputLotSize;
            if (InputLotSize <= ATOption.Instance.MinimumAllocationQuantity)
                inputLotSize = double.MaxValue;

            return inputLotSize;
        }
        #endregion
    }
}
