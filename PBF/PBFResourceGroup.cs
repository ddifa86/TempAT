using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public class PBFResourceGroup : IAPEQueueManager
    {
        public ATResourceGroup Target { get; private set; }

        public List<PBFResource> Buckets { get; private set; }

        public FWWipQueue Queue { get; private set; }

        public string BucketGroupID
        {
            get
            {
                return this.Target.GroupID;
            }
        }

        public string QueueName
        {
            get
            {
                return this.Target.GroupID;
            }
        }

        public int Sequence
        {
            get
            {
                return this.Target.Sequence;
            }
        }

        public bool IsReSorting
        {
            get
            {
                return this.Target.IsReSorting;
            }
        }

        public PBFResourceGroup(ATResourceGroup group)
        {
            this.Target = group;

            this.Buckets = new List<PBFResource>();

            this.Queue = new FWWipQueue();

            APEWipAgent.Instance.RegistQueue(this);
        }

        public override string ToString()
        {
            return string.Format("{0} / {1}", this.Target.GroupID, this.Sequence);
        }
    }
}
