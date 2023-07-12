using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Mozart.SeePlan.Aleatorik.Data
{
    public class ATSelectBomInfo
    {
        public ATBom Bom { get; private set; }

        public APEPegPart PegPart { get; private set; }

        public APETarget Target { get; private set; }

        public double Qty { get; internal set; }

        public dynamic Property { get; internal set; }

        public SelectType SelectType { get; internal set; }

        public APESelectBomContext Context { get; set; }

        public ATSelectBomInfo(APETarget target, ATBom bom, SelectType type, APESelectBomContext context)
        {
            this.Bom = bom;
            this.Target = target;
            this.PegPart = target.Group as APEPegPart;
            this.SelectType = type;
            this.Context = context;
        }
    }
}
