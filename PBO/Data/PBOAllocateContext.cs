using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.ObyO.Data
{
    public class PBOAllocateContext
    {
        public List<ATOperResource> Arranges { get; internal set; }

        public APELot Lot { get; internal set; }

        public HashSet<PBOResource> Buckets { get; internal set; }

        public List<APECapacity> CapaInfos { get; internal set; }

        public PBOResource SelectedBucket { get; internal set; }

        public APERefPlan RefPlan { get; internal set; }

        public double UsagePer { get; set; }

        public DateTime CapaStartTime { get; set; }

        public DateTime CapaEndTime { get; set; }

        public PBOAllocateContext(List<ATOperResource> arranges, APELot lot, APERefPlan refPlan)
        {
            this.Arranges = arranges;
            this.Lot = lot;
            this.RefPlan = refPlan;

            this.Buckets = new HashSet<PBOResource>();
        }
    }
}
