using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Planner;
using Mozart.Simulation.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Manager
{
    /// <summary>
    /// 설비의 Setup을 등록하고 관리하는 Manager입니다.
    /// </summary>
    public class SetupManager
    {
        public PBFResource Bucket { get; private set; }

        public Dictionary<string, string> LastSetupPropertyValues { get; set; }

        public SetupManager(PBFResource bucket)
        {
            this.Bucket = bucket;
            this.LastSetupPropertyValues = new Dictionary<string, string>();
        }

        public ATSetupDetail GetSetupInfo(IAPELot lot)
        {
            if (this.Bucket.Target.IsSetupResource == false)
                return null;

            if (this.Bucket.LastCapaPlan == null)
                return null;

            APELot sampleLot = lot is APELotGroup ? (lot as APELotGroup).SampleLot : lot as APELot;

            var info = GetSetupInfo(sampleLot);

            return info;
        }

        internal ATSetupDetail GetSetupInfo(APELot lot)
        {
            var setupInfo = this.Bucket.Target.SetupInfo;
            var properties = setupInfo.Properties;

            Dictionary<string, string> lastKeyInfo = new Dictionary<string, string>();
            Dictionary<string, string> currentKeyInfo = new Dictionary<string, string>();

            foreach (var prop in properties)
            {
                string currentValue = lot.GetPropertyValue(prop);
                currentKeyInfo.Add(prop, currentValue);

                string lastValue = this.Bucket.LastAllocatedLot.GetPropertyValue(prop);
                lastKeyInfo.Add(prop, lastValue);
            }

            foreach (var detail in setupInfo.SetupInfos)
            {
                if (detail.IsNeedSetup(lastKeyInfo, currentKeyInfo))
                    return detail;
            }

            return null;
        }

        /// <summary>
        /// 설비의 Off Time을 고려하여 Setup 시간을 조정합니다.
        /// </summary>
        /// <param name="loadableTime"> 작업물 할당이 가능한 시간입니다. </param>
        /// <param name="setupInfo"> 현재 예정중인 Setup 정보입니다. </param>
        /// <returns> 작업물 할당이 가능한 시간입니다. </returns>
        public void AdjustSetupTime(DateTime loadableTime, FWSetupInfo setupInfo, AllocateType allocateType)
        {
            if (allocateType != AllocateType.Reserve)
            {
                // IDLE을 고려한 Setup 시간 조정
                var setupEndTime = this.Bucket.CurrentTime + (TimeSpan)setupInfo.SetupTime;

                if (setupEndTime < loadableTime)
                    setupEndTime = loadableTime;

                var setupStartTime = setupEndTime - (TimeSpan)setupInfo.SetupTime;

                setupInfo.StartTime = setupStartTime;
                setupInfo.EndTime = setupEndTime;

                // SetupResource의 가용 시간을 고려한 Setup 시간 조정
                if (setupInfo.HasSetupResource)
                {
                    var setupResource = setupInfo.SetupResource;

                    if (setupInfo.StartTime < setupResource.CurrentTime)
                    {
                        double diff = (setupResource.CurrentTime - setupInfo.StartTime).TotalSeconds;
                        setupInfo.StartTime = setupInfo.StartTime.AddSeconds(diff);
                        setupInfo.EndTime = setupInfo.EndTime.AddSeconds(diff);
                    }
                }

                PBFResource setupBucket = setupInfo.HasSetupResource ? setupInfo.SetupResource : this.Bucket;

                DateTime endTime = setupInfo.EndTime >= this.Bucket.EndTime ? this.Bucket.EndTime : setupInfo.EndTime;

                // Setup Policy를 고려한 시간 조정
                if (ATOption.Instance.SetupPolicy == SetupPolicy.Push.ToString())
                {
                    var nonWorkingPeriods = setupBucket.GetNonWorkingPeriods(setupInfo.StartTime, endTime);
                    var lastPeriod = nonWorkingPeriods.LastOrDefault();
                    if (lastPeriod != null)
                    {
                        double diff = (lastPeriod.End - setupInfo.StartTime).TotalSeconds;
                        setupInfo.StartTime = setupInfo.StartTime.AddSeconds(diff);
                        setupInfo.EndTime = setupInfo.EndTime.AddSeconds(diff);
                    }
                }
                else if (ATOption.Instance.SetupPolicy == SetupPolicy.Extend.ToString())
                {
                    var sumNonWorkingTime = setupBucket.GetCumNonWorkingTime(setupInfo.StartTime, endTime);

                    setupInfo.EndTime = setupInfo.EndTime.Add(sumNonWorkingTime);
                    setupInfo.NonWorkCapa = sumNonWorkingTime.TotalSeconds;
                }
            }
        }
    }
}
