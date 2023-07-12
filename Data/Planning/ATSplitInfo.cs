using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class ATSplitInfo
    {
        public APELot OrgLot { get; private set; }
        public ATOperTarget CurrentTarget { get; private set; }

        public ATBom CurrentBom { get; private set; }

        public ATBomDetail CurrentBomDetail { get; private set; }

        public APEWip OrgWip { get; private set; }

        public ATItemSiteBuffer CurrentItemSiteBuffer { get; private set; }

        public double Qty { get; private set; }
        
        public ATSplitInfo(APELot lot, double qty)
        {
            this.OrgLot = lot;
            this.CurrentTarget = lot.CurrentTarget;
            this.CurrentBom = lot.CurrentBom;
            this.CurrentBomDetail = lot.CurrentBomDetail;
            this.OrgWip = lot.Wip;
            this.CurrentItemSiteBuffer = lot.CurrentItemSiteBuffer;
            this.Qty = qty;
        }
    }
}
