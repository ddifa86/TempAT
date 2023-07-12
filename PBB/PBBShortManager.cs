using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.ObyO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class PBBShortManager : APEShortManager
    {
        public PBBShortManager(ICollection<APEPegPart> pegParts) : base(pegParts)
        {
        }

        // KTR : public new는 머야? 해당 문법을 사용하는 배경은 한번 확인 해봐야 할 듯
        public new bool IsRetryInPhase(Dictionary<IComparable, APETargetGroup> targetGroups, ITargetGroup pegPart, List<APEPegPart> pegParts, int retryCount)
        {
            bool isRetry = PBBBackwardLogic.Instance.IsRetryInPhase(pegParts, this, retryCount);

            if (ATOption.Instance.TransferShortToNextPhase && isRetry == false)
                ConvertToNextPhase();

            return isRetry;
        }
    }
}
