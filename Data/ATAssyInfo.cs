using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class ATAssyInfo : IAssemblyKey
    {
        public string Key { get; set; }

        public ATBom CurrentBom { get; private set; }

        public ATItemSiteBuffer FromISB { get; set; }

        public ATOperTarget OperTarget { get; set; }

        public ATAssyInfo(ATBom bom, ATOperTarget target, ATItemSiteBuffer fromISB)
        {
            this.CurrentBom = bom;
            this.OperTarget = target;
            this.Key = bom.BomID + target.TargetID;
            this.OperTarget.AssyInfo = this;
            this.FromISB = fromISB;
        }
    }
}
