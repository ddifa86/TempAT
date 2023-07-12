using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mozart.SeePlan.Aleatorik
{
    public partial class PBOModuleExecutionInfo : ModuleExecutionInfo
    {
        #region 필수 정보
        public List<ATDemand> Demands { get; internal set; }

        #endregion

        #region 결과 정보

        /// <summary>
        /// Pegging 된 정보
        /// </summary>
        public List<APEWip> PeggedWips { get; internal set; }

        /// <summary>
        /// 투입 Target 정보
        /// </summary>
        public List<ATOperTarget> StageInTargets { get; internal set; }

        /// <summary>
        /// Unpeg된 Wip 정보
        /// </summary>
        public List<APEWip> UnpeggedWips { get; internal set; }

        /// <summary>
        /// StageOut된 Wip의 정보
        /// </summary>
        public List<APEWip> StageOutWips { get; internal set; }


        public Dictionary<string, ATDemand> ShortDemands { get; internal set; }

        public Dictionary<ATBuffer, List<ATOperTarget>> StageInAssemblyTargets { get; internal set; }

        public Dictionary<IComparable, List<ATOperTarget>> OperTargets { get; internal set; }

        #endregion

        internal double AllocationKey { get; set; }

        internal PBOModuleExecutionInfo(string key, ATStage stage, int sequence, string refKey)
            : base(key, ModuleType.PBO, stage, sequence, refKey)
        {
            this.Demands = new List<ATDemand>();
            this.StageInAssemblyTargets = new Dictionary<ATBuffer, List<ATOperTarget>>();
            this.PeggedWips = new List<APEWip>();
            this.UnpeggedWips = new List<APEWip>();
            this.StageInTargets = new List<ATOperTarget>();
            this.StageOutWips = new List<APEWip>();
            this.ShortDemands = new Dictionary<string, ATDemand>();
            this.OperTargets = new Dictionary<IComparable, List<ATOperTarget>>();
        }

        internal override void OnStarted()
        {
            base.OnStarted();
        }

        internal override void OnEnded()
        {
            base.OnEnded();
        }

        internal override void OnPrepareInput()
        {
           
        }

        internal override void OnPrepareOutput()
        {
        }

        public void AddShortDemand(ATDemand demand, double shortQty)
        {
            ATDemand shortDemand;
            if (this.ShortDemands.TryGetValue(demand.ID, out shortDemand) == false)
            {
                shortDemand = demand.Clone();
                shortDemand.Qty = 0;

                this.ShortDemands.Add(demand.ID, shortDemand);
            }

            shortDemand.Qty += shortQty;
        }

        internal void AddOutStockWip(APELot lot)
        {
            var wipInfo = new ATWipInfo(lot.LotID, lot.Qty, WipType.Inventory, LotState.Wait, lot.CurrentItemSiteBuffer.Site, lot.CurrentItemSiteBuffer.Item, lot.CurrentRoute, lot.CurrentOper, null, lot.LastStepTime, lot.LastStepTime, this.Stage);

            wipInfo.ItemSiteBuffer = lot.CurrentItemSiteBuffer;


            //ATItemSiteBuffer itemBuffer = ItemSiteBufferHelper.GetItemSite(wip.ItemSiteBuffer.SiteID, wip.ItemSiteBuffer.ItemID, bufferID);

            APEWip wip = new APEWip(wipInfo, LotCreateType.StageOut);
            this.StageOutWips.Add(wip);
        }
    }
}
