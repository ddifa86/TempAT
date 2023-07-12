using Mozart.Data.Entity;
using Mozart.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Mozart.SeePlan.Aleatorik.Data
{
    // SOP Product 기준으로 BOM 구성 정보를 가짐.
    // RootItemBufferKey와 Buffer 정보를 넣으면 해당 Buffer에서 존재할 수 있는 ItemBuffer 정보를 반환

    public class ABomNetwork
    {
        /// <summary>
        /// Salse 되는 시점의 ItemBuffer => BCP의 SOP
        /// </summary>
        public ATItemSiteBuffer RootItemSiteBuffer { get; private set; }

        /// <summary>
        /// 현재 ItemBuffer 정보
        /// </summary>
        public ATItemSiteBuffer ItemSiteBuffer { get; private set; }

        /// <summary>
        /// 해당 노드의 대표 BomType
        /// </summary>
        public BomType BomType;

        /// <summary>
        /// 
        /// </summary>
        public bool IsUsablePath { get; internal set; }

        public bool HasDemands { get; internal set; }

        public string RootKey
        {
            get
            {
                return RootItemSiteBuffer.Key;
            }
        }

        public string SiteID
        {
            get
            {
                return this.ItemSiteBuffer.SiteID;
            }
        }

        public string BufferID
        {
            get
            {
                return   this.ItemSiteBuffer.BufferID;
            }
        }

        public string ItemID
        {
            get
            {
                return this.ItemSiteBuffer.ItemID;
            }
        }

        public ABomNetwork Parent;

        public List<ABomNetwork> Child;

        /// <summary>
        /// 해당 BufferBom과 관련있는 Bom / BomDetail 정보 저장
        /// </summary>
        internal Dictionary<ATBom, List<ATBomDetail>> NextBoms;

        internal List<ATBomDetail> NextBomDetails;

        // 중복 전개 정보 리스트
        internal List<ATBomDetail> DuplicatePrevBomDetails;

    

        internal double WipSumQty
        {
            get; set;
        }

        /// <summary>
        /// 
        /// </summary>
        public string Key
        {
            get
            {
                return ItemSiteBuffer.Key;
            }
        }

        public override string ToString()
        {
            return string.Format("{0}",  this.ItemSiteBuffer.Key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootitembufferKey"></param>
        /// <param name="bomdetail"></param>
        /// <param name="fromitembuffer">bomdetail의 from에 해당하는 정보</param>
        public ABomNetwork(ABomNetwork parent, ATItemSiteBuffer itembuffer)
        {
            this.RootItemSiteBuffer = parent == null ? itembuffer : parent.RootItemSiteBuffer;

            this.Parent = parent;

            this.ItemSiteBuffer = itembuffer;
            this.BomType = BomType.None;

            // Assembly가 존재하는 Node 여부 체크
            var boms = this.ItemSiteBuffer.GetPrevBom(BomType.Assembly); // BomType.None;
            if (boms.Count() > 0)
                this.BomType = BomType.Assembly;

            this.Child = new List<ABomNetwork>();
            this.IsUsablePath = false;

            // 해당 정보는 ItemSiteBuffer의 NextBom 정보가 가지고 있음.. 굳이 따로 집계할 필요가 있을가..?
            this.NextBoms = new Dictionary<ATBom, List<ATBomDetail>>();
            this.NextBomDetails = new List<ATBomDetail>();
            this.DuplicatePrevBomDetails = new List<ATBomDetail>();

            // ItemSite이 생산될 수 있는 SoItem 정보
            if (this.ItemSiteBuffer.SumWipQtysBySoItemSites.ContainsKey(this.RootItemSiteBuffer.Key) == false)
            {
                this.ItemSiteBuffer.SumWipQtysBySoItemSites.Add(this.RootItemSiteBuffer.Key, 0);
                this.ItemSiteBuffer.SoItemSiteBuffers.Add(this.RootItemSiteBuffer);
            }
        }


        internal void AddNextBomDetail(ATBomDetail detail)
        {
            List<ATBomDetail> lst;
            if (this.NextBoms.TryGetValue(detail.Bom, out lst) == false)
            {
                
                if( detail.Bom.SoItemSites.ContainsKey(this.RootItemSiteBuffer) == false)
                    detail.Bom.SoItemSites.Add(this.RootItemSiteBuffer, this.RootItemSiteBuffer);             

                this.NextBoms.Add(detail.Bom, lst = new List<ATBomDetail>());
            }
            
            lst.Add(detail);

            NextBomDetails.Add(detail);
        }
    }
}
