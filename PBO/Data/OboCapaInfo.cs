using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik.ObyO.Data
{
    public class OboCapaInfo : IComparable<OboCapaInfo>
    {
        public PBOResource Bucket;

        public ATCapacityInfo OrgCapaInfo { get; }

        public List<APEPlanInfo> UsedPlanInfos { get; internal set; }

        public string BucketID
        {
            get
            {
                return Bucket.Target.ResourceID;
            }
        }

        public DateTime StartTime
        {
            get
            {
                return OrgCapaInfo.StartTime;
            }
        }
        public DateTime EndTime
        {
            get
            {
                return OrgCapaInfo.EndTime;
            }
        }


        /*
        * 실적 집계 및 잔여 수량, 할당 수량 부분은 다른 클래스에서 처리가 필요함.
        */

        public OboCapaInfo Next { get; internal set; }

        public OboCapaInfo Prev { get; internal set; }

        public double Capacity { get; private set; }

        public double UsedQty { get;  set; }

        public dynamic Property { get; set; }

        /// <summary>
        /// 가상할당 수량 ?
        /// </summary>
        public double VirtualUsedQty { get;  set; }


        // 잔여값이 아니네??
        public double Remain
        {
            get
            {
                return Capacity - UsedQty - VirtualUsedQty;
            }
        }


        public double UsedRatio
        {
            get
            {
                if (Capacity <= 0)
                    return 0;

                return Math.Round((UsedQty / Capacity) * 100, 4);
            }
        }

        private ATOperResource _currentArrange;
        public ATOperResource CurrentArrange
        {
            get
            {
                return _currentArrange;
            }
            internal set
            {
                _currentArrange = value;
            }
        }

        public OboCapaInfo(PBOResource bucket, ATCapacityInfo capaInfo)
        {
            this.OrgCapaInfo = capaInfo;
            this.Bucket = bucket;
            this.Capacity = OrgCapaInfo.Capacity;
            this.UsedPlanInfos = new List<APEPlanInfo>();
            this.Property = new DynamicDictionary();
        }
        internal void UpdateAct(APEPlanInfo planInfo, double qty, bool isCommit)
        {
            this.VirtualUsedQty -= qty;

            if (this.VirtualUsedQty <= ATOption.Instance.MinimumAllocationQuantity)
                this.VirtualUsedQty = 0;

            if (isCommit)
            {
                this.UsedQty += qty;
                UsedPlanInfos.Add(planInfo);
                // 태룡수석님께 확인하기 -> 무한일수도 있는데 Capa를 지우는게 맞는지??

                //if (this.Capacity - this.UsedQty <= ATOption.Instance.MinimumAllocationQuantity)
                //{
                //    this.Bucket.CapaInfos.Remove(this);

                //}
            }
        }

        /// <summary>
        /// 해당 날짜를 할당 가능한지 여부 체크.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool IsJITDateTime(DateTime time)
        {
            if (this.StartTime <= time && this.EndTime > time)
                return true;

            return false;
        }


        public int CompareTo(OboCapaInfo other)
        {
            if (IsJITDateTime(other.StartTime))
                return 0;

            if (this.StartTime > other.StartTime)
                return 1;

            return -1;
        }


    }
}
