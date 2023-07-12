using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.ObyO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class PBOShortManager : APEShortManager
    {
        public PBOShortManager(ICollection<APEPegPart> pegParts) : base(pegParts)
        {
        }

        public new bool IsRetryInPhase(Dictionary<IComparable, APETargetGroup> targetGroups, ITargetGroup pegPart, List<APEPegPart> pegParts, int retryCount)
        {
            bool isRetry = OboLogic.Instance.IsRetryInPhase(targetGroups, pegPart, this, retryCount);

            if (ATOption.Instance.TransferShortToNextPhase && isRetry == false)
                ConvertToNextPhase();

            return isRetry;
        }
    }
}
