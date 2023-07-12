using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class ATAlignQueue : IAlignQueue
    {
        public IComparable Key { get; set; }

        public ATBuffer Buffer { get; set; }

        public Dictionary<IComparable, IMergedTargetGroup> Entities { get; set; }

        public ATAlignQueue(IComparable key, ATBuffer buffer)
        {
            this.Key = key;
            this.Buffer = buffer;

            this.Entities = new Dictionary<IComparable, IMergedTargetGroup>();
        }

        public void AddEntity(IComparable key, ITargetGroup entity)
        {
            if (this.Entities.TryGetValue(key, out var peggingGroup) == false)
            {
                peggingGroup = new APEPeggingGroup(key);
                this.Entities.Add(key, peggingGroup);
            }

            peggingGroup.Merge(entity);

            // BWInterface.PeggerControl.OnAddPeggingGroup(entity, peggingGroup);
        }
    }
}
