using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.ObyO.Data
{
    public class OboPhaseInfo
    {
        public HashSet<APEPegPart> PegParts { get; internal set; }

        public HashSet<APEPegPart> TargetPegParts { get; internal set; }

        public int PhaseCount { get; internal set; }

        public OboPhaseInfo(HashSet<APEPegPart> pegParts, HashSet<APEPegPart> targetPegParts, int phaseCount)
        {
            this.PegParts = pegParts;
            this.TargetPegParts = targetPegParts;
            this.PhaseCount = phaseCount;
        }
    }
}
