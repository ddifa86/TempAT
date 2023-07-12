using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public interface IDemandMaster
    {
        IComparable Key { get; }

        IReadOnlyCollection<IDemand> Demands { get; }
    }
}
