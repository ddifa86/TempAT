using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public partial class APEWip
    {
        public APEWip RealWip { get; set; }

        public APEPegContext PegContext { get; set; }

        internal APETarget PegTarget { get; set; }

        internal APETargetGroup PegTargetGroup { get; set; }
    }
}
