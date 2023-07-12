using Mozart.SeePlan.Aleatorik.ObyO;
using Mozart.SeePlan.Aleatorik.ObyO.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public partial class ATOperation
    {
        internal Dictionary<ATOperResource, PBOResource> OboArrangeInfos = new Dictionary<ATOperResource, PBOResource>();
        public double MBSValue;

        public bool IsMBSOper
        {
            get
            {
                if (ATOption.Instance.DiscardMultipleBatchSize == false && MBSValue != 0)
                    return true;
                else
                    return false;
            }
        }

        public PBOResource GetLoadableBucket(ATOperResource arrange)
        {
            if (this.OboArrangeInfos.TryGetValue(arrange, out var bucket))
                return bucket;

            return null;
        }
    }
}
