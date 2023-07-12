using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public interface IAlignQueue
    {
        IComparable Key { get; }

        Dictionary<IComparable, IMergedTargetGroup> Entities { get; }

        public void AddEntity(IComparable key, ITargetGroup entity);
    }
}
