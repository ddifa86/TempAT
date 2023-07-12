using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class APESelectBomContext
    {
        // Allocation 대상 PegPart
        public ATBom Bom { get; set; }

        public APEPegPart PegPart { get; set; }

        public APETarget Target { get; set; }

        public ATDemand Demand
        {
            get
            {
                return Target.MoPlan.SODemand;
            }
        }

        // KTR : Context는 BaseContext 를 상속받아서 하는 방향으로 통일.
        public dynamic Property { get; internal set; }

        public APESelectBomContext(APEPegPart pegPart)
        {
            this.Property = new DynamicDictionary();
            this.PegPart = pegPart;
            this.Target = pegPart.SampleTarget;
        }
    }
}
