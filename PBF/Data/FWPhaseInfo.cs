using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Planner.Data
{
    public class FWPhaseInfo
    {
        public List<APELot> Lots { get; internal set; }

        public List<APELot> TargetLotInPhase { get; internal set; }

        public int PhaseCount { get; internal set; }

        public FWPhaseInfo(List<APELot> lots, List<APELot> targetlot, int phaseCount)
        {
            this.Lots = lots;
            this.TargetLotInPhase = targetlot;
            this.PhaseCount = phaseCount;
        }
    }
}
