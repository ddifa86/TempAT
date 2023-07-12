using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public partial class PBFAllocateContext : BaseContext
    {
        internal ATWeightPreset FilterResourceOnRFSPreset;
        internal ATWeightPreset CompareResourceOnRFSPreset;
        internal ATWeightPreset FilterLotGroupOnRFSPreset;
        internal ATWeightPreset CompareLotGroupOnRFSPreset;
    }
}
