using Mozart.Extensions;
using Mozart.SeePlan.Pegging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public partial class APEPegPart
    {
        public string PathID { get; set; }

        public string AllocPathID { get; set; }

        public APERefPlan CurRefPlan { get; set; }

        public string RefType { get; set; }

        public Dictionary<string, ATFactorValue> FactorInfos { get; set; }

        public Dictionary<string, ATFilterValue> FilterInfos { get; set; }

        public IComparable TargetGroupKey { get; set; }

        internal ATAssyInfo RootAssyInfo { get; set; }

        public ATShortInfo CurShortInfo { get; internal set; }

        public bool IsShort
        {
            get
            {
                return CurShortInfo != null;
            }
        }

        public void AddPegTarget(APETarget pegTarget)
        {
            this.Targets.AddSort(pegTarget, new ATPegPartInGroupComparer());
        }
    }
}
