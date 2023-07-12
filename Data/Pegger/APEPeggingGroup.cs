using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Pegger;
using Mozart.SeePlan.DataModel;
using Mozart.SeePlan.Pegging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class APEPeggingGroup : IMergedTargetGroup
    {
        public IComparable Key { get; set; }

        public List<ITargetGroup> Items { get; }

        public List<ITarget> Targets { get; set; }

        public ITarget Sample
        {
            get
            {
                return this.Targets.FirstOrDefault();
            }
        }

        public Step Step 
        {
            get
            { 
                return this.Items.FirstOrDefault().Step;
            }
            set
            {
                // ?
            }
        }

        public PegPosition PegPosition { get; set; }

        public APEPeggingGroup(IComparable key)
        {
            this.Key = key;
            this.Items = new List<ITargetGroup>();
            this.Targets = new List<ITarget>();
        }

        public virtual ITargetGroup Clone()
        {
            var clone = (APEPeggingGroup)this.MemberwiseClone();

            var copied = this.CopyTargets(clone);
            clone.Targets = new List<ITarget>(copied.ToList());

            return clone;
        }

        public IEnumerable<APETarget> CopyTargets(APEPeggingGroup peggingGroup)
        {
            foreach (APETarget pt in this.Targets)
            {
                yield return pt.Clone(peggingGroup);
            }
        }

        public void Merge(ITargetGroup targetGroup)
        {
            this.Items.Add(targetGroup);
            this.AddTargets(targetGroup.Targets);
        }

        public void AddTargets(IEnumerable<ITarget> targets)
        {
            if (Targets != null)
                Targets.AddRange(targets);
        }

        public void RemoveTarget(ITarget target)
        {
            if (target != null)
                this.Targets.Remove(target);
        }

        public void Apply(Action<ITargetGroup, ITargetGroup> action)
        {
            if (action != null)
                this.Items.ForEach(x => action(x, null));
        }

        public void SetPegPosition(PegPosition position)
        {
            this.PegPosition = position;
            this.Apply((x, _) => { x.PegPosition = position; });
        }
    }
}
