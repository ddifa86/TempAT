
using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public partial class ObjectMapper
    {

        public static ATMoMaster CreateMoMaster(IComparable key, ATItemSiteBuffer itembuffer, string siteID)
        {
            var info = ObjectMapper.Create<ATMoMaster>(
                key
                , itembuffer
                , siteID
                );

            return info;
        }

        public static ATMoPlan CreateMoPlan(ATMoMaster mm, ATDemand demand, DateTime dueDate)
        {
            var info = ObjectMapper.Create<ATMoPlan>(
                mm,
                demand,
                dueDate
                );

            demand.Moplan = info;

            return info;
        }

        public static APEPegPart CreatePegPart(ATMoMaster mm, ATItemSiteBuffer itembuffer)
        {
            var info = ObjectMapper.Create<APEPegPart>(
                mm
                , itembuffer
                );            

            return info;
        }

        public static APETarget CreatePegTarget(APEPegPart pegPart, ATMoPlan plan, string targetID)
        {
            var info = ObjectMapper.Create<APETarget>(
                pegPart,
                plan,
                targetID
                );

            return info;
        }

        internal static APEWip CreatePlanWip(ATWipInfo wipInfo, LotCreateType type)
        {
            var info = ObjectMapper.Create<APEWip>(
               wipInfo
               , type
                );

            return info;
        }


        internal static ATRuleSetConfigLog CreateRuleSetConfigLog(string versionNo, string moduleKey, string targetType, string targetId, string rulesetId, int phase)
        {
            ATRuleSetConfigLog info = null;

            info = ObjectMapper.Create<ATRuleSetConfigLog>(
                    versionNo
                    , moduleKey
                    , targetType
                    , targetId
                    , rulesetId
                    , phase
                    );

            return info;
        }
    }
}
