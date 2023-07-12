using Mozart.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public partial class ATBom 
    {
        #region Property
        /// <summary>
        /// BomPath 를 통해 Input ~ Output 까지 연결 여부
        /// </summary>
        public BomPathType IsUsableBom = BomPathType.N;

        /// <summary>
        /// Bom에서 사용가능한 Resource 정보 
        /// </summary>
        public HashSet<ATResource> Resource = new HashSet<ATResource>();

        /// <summary>
        /// Buffer별 참조가능한 Resource 정보
        /// </summary>
        public Dictionary<ATBuffer, HashSet<ATResource>> PrevResources = new Dictionary<ATBuffer, HashSet<ATResource>>();

        /// <summary>
        /// Bom과 연결가능한 Buffer별 Bom 정보들.
        /// </summary>
        public Dictionary<ATBuffer, HashSet<ATBom>> PrevBoms = new Dictionary<ATBuffer, HashSet<ATBom>>();

        /// <summary>
        /// Bom과 연결된 이전 Buffer별 ItemSiteBuffer 정보들
        /// </summary>
        public Dictionary<ATBuffer, HashSet<ATItemSiteBuffer>> PrevItemSiteBuffers = new Dictionary<ATBuffer, HashSet<ATItemSiteBuffer>>();

        public Dictionary<ATBuffer, HashSet<ATItemSiteBuffer>> NextItemSiteBuffers = new Dictionary<ATBuffer, HashSet<ATItemSiteBuffer>>();

        /// <summary>
        /// Bom을 통해 생산가능한 SoItems
        /// </summary>
        public Dictionary<ATItemSiteBuffer, ATItemSiteBuffer> SoItemSites;

        #endregion

        /// <summary>
        /// routing이 
        /// </summary>
        public double MinTat = Int32.MaxValue;

        public double MaxTat = 0;

        public double MaxCumYield = 1;

        /// <summary>
        /// ??
        /// </summary>
        public HashSet<ATItemSiteBuffer> PrevAssyItemSiteBuffers = new HashSet<ATItemSiteBuffer>();

        private string _all_resids;
        public string All_ResIDs
        {
            get
            {
                if (string.IsNullOrEmpty(_all_resids) == false)
                    return _all_resids;

                string ids = string.Empty;

                foreach (var pair in PrevResources)
                {
                    if (pair.Value.Count() == 0)
                        continue;
                    ids += pair.Key.BufferID + " : ";
                    foreach (var res in pair.Value)
                    {
                        ids += res.ResourceID + ",";
                    }

                    ids += " /";
                }

                _all_resids = ids;

                if (string.IsNullOrEmpty(_all_resids))
                    _all_resids = "-";

                return _all_resids;
            }
        }

        string _nextIsbs;
        public string NextIsbs
        {
            get
            {
                if (string.IsNullOrEmpty(_nextIsbs) == false)
                    return _nextIsbs;

                string isbs = string.Empty;
                foreach (var pair in NextItemSiteBuffers)
                {
                    if (pair.Value.Count() == 0)
                        continue;

                    isbs += pair.Key.BufferID + " :";
                    foreach (var isb in pair.Value)
                    {
                        isbs += isb.Key + ",";
                    }

                    isbs += "/";
                }
                _nextIsbs = isbs;

                if (string.IsNullOrEmpty(_nextIsbs))
                    _nextIsbs = "-";

                return _nextIsbs;
            }
        }
        string _prevIsbs;
        public string PrevIsbs
        {
            get
            {
                if (string.IsNullOrEmpty(_prevIsbs) == false)
                    return _prevIsbs;

                string isbs = string.Empty;
                foreach (var pair in PrevItemSiteBuffers)
                {
                    if (pair.Value.Count() == 0)
                        continue;

                    isbs += pair.Key.BufferID + " :";
                    foreach (var isb in pair.Value)
                    {
                        isbs += isb.Key + ",";
                    }

                    isbs += "/";
                }
                _prevIsbs = isbs;

                if (string.IsNullOrEmpty(_prevIsbs))
                    _prevIsbs = "-";

                return _prevIsbs;
            }
        }

        internal void AddBomRoute(ATBomRouting bomRoute)
        {
            this.BomRoutes.AddSort(bomRoute, ATBomRouting.ATBomRoutingComparer.Default);
            this.MinTat = Math.Min(this.MinTat, bomRoute.Route.SumTat);
            this.MaxTat = Math.Max(this.MaxTat, bomRoute.Route.SumTat);
        }

        internal double GetFromItemSiteMaxCumTat()
        {
            double result = 0;

            if (this.BomType == BomType.Assembly)
            {
                foreach (var detail in this.BomDetails)
                {
                    result = Math.Max(result, detail.FromItemSiteBuffer.MaxCumTat);
                }
            }
            else
            {
                var detail = this.BomDetails.FirstOrDefault();
                if (detail != null)
                {
                    result = detail.FromItemSiteBuffer.MaxCumTat;
                }
            }

            return result;
        }
        internal double GetFromItemSiteLateCumTat()
        {
            double result = 0;

            if (this.BomType == BomType.Assembly)
            {
                foreach (var detail in this.BomDetails)
                {
                    result = Math.Max(result, detail.FromItemSiteBuffer.LateCumTat);
                }
            }
            else
            {
                var detail = this.BomDetails.FirstOrDefault();
                if (detail != null)
                {
                    result = detail.FromItemSiteBuffer.LateCumTat;
                }
            }

            return result;
        }
        internal double GetFromItemSiteCumTat(DateTime dt)
        {
            double result =0;

            if (this.BomType == BomType.Assembly)
            {
                // part 들 중 가장 긴 cumtat 값을 취함.
                foreach (var detail in this.BomDetails)
                {
                    result = Math.Max(result, detail.FromItemSiteBuffer.GetMinCumTAT(dt));
                }
            }
            else
            {
                var detail = this.BomDetails.FirstOrDefault();
                if (detail != null)
                {
                    result = detail.FromItemSiteBuffer.GetMinCumTAT(dt);
                }
            }
            

            return result;
        }
 
        // Bompath 구성 시점에 Bom이 가용한 Path인지 판단
        internal void SetUsableBom()
        {
            List<BomPathType> pathInfos = new List<BomPathType>();
            if (this.BomType == BomType.Assembly)
            {
                // Assembly의 경우, 모든 Detail이 True여야 함.
                // 단, 외부에서 가져오는 제품의 경우에는 제외 => 조건정보가 필요한 상태임.
                // 
                bool hasNPath = false;
                bool hasDPath = false;

                foreach (var detail in this.BomDetails)
                {
                    if (detail.IsUsableDetail == BomPathType.N)
                        hasNPath = true;

                    if (detail.IsUsableDetail == BomPathType.D)
                        hasDPath = true;
                }

                if (hasNPath)
                    this.IsUsableBom = BomPathType.N;
                else if (hasDPath)
                    this.IsUsableBom = BomPathType.D;
                else
                    this.IsUsableBom = BomPathType.Y;

            }
            else
            {
                bool hasNPath = false;
                bool hasDPath = false;
                bool hasYPath = false;

                foreach (var detail in this.BomDetails)
                {
                    if (detail.IsUsableDetail == BomPathType.N)
                        hasNPath = true;

                    if (detail.IsUsableDetail == BomPathType.D)
                        hasDPath = true;

                    if (detail.IsUsableDetail == BomPathType.Y)
                        hasYPath = true;
                }
                
                // Split 혹은 Normal의 경우에는 BomDetail 중 하나라도 True이면 가용한 Bom으로 처리
                //var usableDetail = this.BomDetails.Where(x => x.IsUsableDetail != BomPathType.N);

                if (hasDPath)
                    this.IsUsableBom = BomPathType.D;
                else if (hasNPath)
                    this.IsUsableBom = hasYPath ? BomPathType.D : BomPathType.N;
                else
                    this.IsUsableBom = BomPathType.Y;
            }
        }


        internal void AddResource(ATResource resource)
        {

        }

        public HashSet<ATResource> GetPrevBomResources(ATBuffer buffer)
        {
            HashSet<ATResource> lst = new HashSet<ATResource>();

            foreach (var detail in this.BomDetails)
            {
                var from = detail.FromItemSiteBuffer;

                if (from.PrevResources.ContainsKey(buffer) == true)
                {
                    var datas = from.PrevResources[buffer];
                    lst.AddRange(datas);
                }
            }

            return lst;
        }

        internal double GetMaxYield()
        {
            double yield = 0;

            foreach (var bomroute in this.BomRoutes)
            {
                var route = bomroute.Route;

                yield = Math.Max(yield, route.CumYield);
            }

            return yield;
        }

        internal HashSet<ATResource> GetPrevResources(ATBuffer buffer)
        {
            HashSet<ATResource> lst;
            if (this.PrevResources.TryGetValue(buffer, out lst) == false)
            {
                lst = new HashSet<ATResource>();
                this.PrevResources.Add(buffer, lst);
            }

            return lst;
        }
    }
}
