using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Planner;
using Mozart.Simulation.Engine;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public partial class AllocatorLogic
    {
        public static AllocatorLogic Instance
        {
            get
            {
                return ServiceLocator.Resolve<AllocatorLogic>();
            }
        }

        /// <summary>
        /// LotGroup 내 소팅
        /// </summary>
        public void SortedLotInGroup(APELotGroup lotgroup)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("Allocate_SortedLotInGroup");
            try
            {
                lotgroup.SortLot(new APELotInGroupComparer(FWFactory.Instance.DefaultLotInGroupPreset));
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("Allocate_SortedLotInGroup");
            }
        }

        public APELotGroup GetLoadableLots(PBFResource bucket, APELotGroup lotgroup, PBFAllocateContext context)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("Allocate_GetLoadableLots");
            try
            {
                return FWInterface.AllocatorControl.GetLoadableLots(bucket, lotgroup, context);
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("Allocate_GetLoadableLots");
            }
        }

        internal FWSetupInfo GetSetupInfo(PBFResource bucket, APELotGroup lotGroup, PBFAllocateContext context)
        {
            var orgSetupInfo = bucket.SetupManager.GetSetupInfo(lotGroup);

            if (FWInterface.BucketControl.IsNeedSetup(bucket, lotGroup, orgSetupInfo, context) == true)
            {
                var setupTime = FWInterface.BucketControl.GetSetupTime(bucket, lotGroup, orgSetupInfo, context);

                FWSetupInfo setupInfo = new FWSetupInfo(bucket, setupTime, orgSetupInfo);

                return setupInfo;
            }

            return null;
        }

        public bool DoSetup(PBFResource bucket, APELotGroup lotGroup, FWSetupInfo setupInfo, PBFAllocateContext context)
        {
            bool isNeedReserve = setupInfo.EndTime >= bucket.EndTime;

            DateTime setupStartTime = setupInfo.StartTime;
            DateTime setupEndTime = isNeedReserve ? bucket.EndTime : setupInfo.EndTime;

            var setupTime = (setupEndTime - setupStartTime).TotalSeconds - setupInfo.NonWorkCapa;

            // Setup의 종료 시간이 다음날인 경우 Setup 정보를 예약
            if (isNeedReserve)
            {
                if (FWInterface.AllocatorControl.OnReservationLot(lotGroup.SampleLot, lotGroup, bucket, context, PlanInfoType.Setup))
                    return false;
            }

            #region CapacityManager
            var capa = bucket.CurrentCapaInfo as ATTimeCapacity;
            capa.UseTo(setupEndTime);

            AllocationInfo allocationInfo = new AllocationInfo(PlanInfoType.Setup, setupStartTime, setupEndTime, setupEndTime, bucket, bucket, context.AllocationLog.AllocationSeq, context.Level, 0, setupTime, 0, 0, null, context.AllocationLog);

            APEPlanInfo planInfo = new APEPlanInfo(null, allocationInfo);
            bucket.AddPlan(planInfo, null);

            capa.SetupTime += setupTime;

            if (setupInfo.HasSetupResource)
            {
                var setupCapa = setupInfo.SetupResource.CurrentCapaInfo as ATTimeCapacity;
                setupCapa.UseTo(setupEndTime);

                AllocationInfo srAllocationInfo = new AllocationInfo(PlanInfoType.Setup, setupStartTime, setupEndTime, setupEndTime, setupInfo.SetupResource, bucket, context.AllocationLog.AllocationSeq, context.Level, 0, setupTime, 1, 1, null, context.AllocationLog);
                APEPlanInfo srPlanInfo = new APEPlanInfo(null, srAllocationInfo);
                setupInfo.SetupResource.AddPlan(srPlanInfo, null);

                setupCapa.UsedCapa += setupTime;
            }
            #endregion

            FWInterface.AllocatorControl.OnSetup(bucket, lotGroup, setupInfo, context);

            if (isNeedReserve)
            {
                var sampleLot = lotGroup.SampleLot;
                
                sampleLot.CurrentPlanInfo.AllocationLog = context.AllocationLog;
                sampleLot.CurrentPlanInfo.Level = context.Level;

                FWSetupInfo reserveSetupInfo = setupInfo.DeepCopy();
                reserveSetupInfo.StartTime = bucket.EndTime;
                reserveSetupInfo.SetupTime = setupInfo.EndTime - bucket.EndTime;

                // Setup 잔여 시간이 남지 않은 경우 Lot만 예약
                if (reserveSetupInfo.SetupTime == 0)
                    reserveSetupInfo = null;

                bucket.ReserveManager.ReserveLot(sampleLot, false, context.AddArrInfos, reserveSetupInfo);

                return false;
            }

            return true;
        }

        public bool CheckForAvailableBeforeAllocation(PBFResource bucket, APELotGroup lotgroup,  PBFAllocateContext context)
        {
            return FWInterface.AllocatorControl.CheckForAvailableBeforeAllocation(bucket, lotgroup, context);
        }
    }
}
