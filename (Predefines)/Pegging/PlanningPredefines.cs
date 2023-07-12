using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class PlanningPredefines
    {
        internal static PlanningPredefines Instance
        {
            get
            {
                return ServiceLocator.Resolve<PlanningPredefines>();
            }
        }

        public string GET_BATCH_ID_DEF(string id, int eternalNo, bool isWip, ref bool handled, string prevReturnValue)
        {
            string prefix = isWip ? "W" : "Z";
            string batchID = string.Format("{0}{1}{2:0#####}", prefix, id, eternalNo);

            return batchID;
        }
    }
}
