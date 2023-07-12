using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Planner
{

    public class PBFAllocationLog
    {
        
        /// <summary>
        /// AllocationKey
        /// </summary>
        public double AllocationSeq { get; set; }

        /// <summary>
        /// 할당 대상 Resource or ResourceGroup
        /// </summary>
        internal IAPEQueueManager TargetResource { get; set; }

        internal string TargetResourceID
        {
            get
            {
                return this.TargetResource.QueueName;
            }
        }

        internal APELotGroup SelectedLotGroup = null;

        /// <summary>
        /// 현재 SelectLotGroup 수
        /// </summary>
        public List<APELotGroup> CandidatedLotGroups = new List<APELotGroup>();

        /// <summary>
        /// filter된 LotGroup 수
        /// </summary>
        public List<APELotGroup> FilterLotGroups = new List<APELotGroup>();

        /// <summary>
        /// Filter 후 로드 가능한 장비 정보
        /// </summary>
        public List<PBFResource> SelectedBuckets = new List<PBFResource>();

        /// <summary>
        /// Filter에 의해 할당 불가 판단된 장비 정보
        /// </summary>
        public List<PBFResource> FilterResources = new List<PBFResource>();

        /// <summary>
        /// 최종 할당된 장비 정보
        /// </summary>
        public List<string> LoadedBuckets = new List<string>();

        /// <summary>
        /// 선택된 LotGroup 정보
        /// </summary>
        internal string AllocatedLotGroupInfo { get; set; }

        internal string LogType { get; set; }

        public PBFAllocationLog(IAPEQueueManager targetResource)
        {
            this.TargetResource = targetResource;
        }

        internal void SetAllocatedLotGroup(APELotGroup lotGroup)
        {
            var sampleLot = lotGroup.Sample as APELot;

            if (sampleLot == null)
                return;

            this.SelectedLotGroup = lotGroup;
            this.AllocatedLotGroupInfo =  lotGroup.LotGroupKey + "," + sampleLot.LotID + "," + sampleLot.CurrentItemSiteBuffer.Key + "," + sampleLot.LastStepTime;
        }
    }
}
