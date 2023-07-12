using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class ATBinPegInfo
    {
        public APEWip BinnedWip { get; internal set; }

        public string PeggingGroupKey { get; internal set; }

        public ATItem FromItem { get; internal set; }

        public ATSite FromSite { get; internal set; }

        public ATBom Bom
        {
            get
            {
                return BomDetail.Bom;
            }
        }

        public ATBomDetail BomDetail { get; private set; }

        public int CreationSequence { get; internal set; }

        public double FromPortion { get; internal set; }

        public double ToPortion { get; internal set; }

        public double TargetQty { get; private set; }

        public double PrevInTargetQty { get; private set; }

        public int Level { get; internal set; }

        public double PeggedQty { get; set; }


        public string BinnedWipID
        {
            get
            {
                return this.BinnedWip.WipInfo.LotID;
            }
        }

        public ATBinPegInfo(APEWip binnedWip, string peggingGroupKey, ATItem fromItem, ATSite fromSite, ATBomDetail detail,
            double fromPortion, double toPortion, double targetQty, double prevInTargetQty)
        {
            this.BinnedWip = binnedWip;
            this.PeggingGroupKey = peggingGroupKey;
            this.FromItem = fromItem;
            this.FromSite = fromSite;
            this.BomDetail = detail;
            this.CreationSequence = LotHelper.GetBinnedWipCreationSequence();
            this.FromPortion = fromPortion;
            this.ToPortion = toPortion;
            this.TargetQty = targetQty;
            this.PrevInTargetQty = TargetQty / fromPortion;
        }

        public ATBinPegInfo ShallowCopy()
        {
            var newInfo = (ATBinPegInfo)this.MemberwiseClone();
            return newInfo;
        }
    }
}
