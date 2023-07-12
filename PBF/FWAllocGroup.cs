using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public class FWAllocGroup
    {
        public ATAllocationGroup Target { get; private set; }

        public AllocateType AllocateType { get; private set; }

        public int Sequence;

        /// <summary>
        /// 
        /// </summary>
        public List<PBFResourceGroup> BucketGroups { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public List<PBFResource> Buckets { get; private set; }

        public FWAllocGroup(ATAllocationGroup target)
        {
            this.Target = target;
            this.Sequence = target.Sequence;
            this.AllocateType = target.AllocateType;

            this.BucketGroups = new List<PBFResourceGroup>();
            this.Buckets = new List<PBFResource>();
        }

        public override string ToString()
        {
            return string.Format("{0} / {1}", this.Target.AllocationGroupID, this.Sequence);
        }

    }
}
