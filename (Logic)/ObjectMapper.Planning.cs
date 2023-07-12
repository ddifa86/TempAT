using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Planner;

namespace Mozart.SeePlan.Aleatorik
{
    public partial class ObjectMapper
    {
        internal static FWAllocGroup CreateAllocGroup(ATAllocationGroup group)
        {
            var info = ObjectMapper.Create<FWAllocGroup>(
               group
                );

            return info;
        }

        internal static PBFResourceGroup CreateBucketGroup(ATResourceGroup group)
        {
            var info = ObjectMapper.Create<PBFResourceGroup>(
               group
                );

            return info;
        }

        internal static PBFResource CreateBucket(ATResource resource)
        {
            var info = ObjectMapper.Create<PBFResource>(
               resource
                );

            return info;
        }

        internal static APELot CreateLot(string lotID, double qty, ATOperation oper, ATOperTarget target, APEWip wip, LotCreateType createtype)
        {
            var info = ObjectMapper.Create<APELot>(
               lotID,
               qty,
               oper,
               target,
               wip,
               createtype
                );

            if (info.IsWipLot)
            {
                // info.LastStepTime = ATUtil.MinTime(wip.AvailableTime, target.MaxLateTargetDateTime);
                info.LastStepTime = wip.AvailableTime;
            }
            else
            {
                info.LastStepTime = target.MinEarlyTargetDateTime.StartTimeOfDayT();
            }

            return info;
        }


    }
}
