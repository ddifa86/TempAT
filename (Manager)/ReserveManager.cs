using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Planner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Manager
{
    /// <summary>
    /// 설비의 예약을 등록하고 관리하는 Manager입니다.
    /// </summary>
    public class ReserveManager
    {
        public PBFResource Bucket { get; private set; }

        public List<ReserveInfo> ReserveInfos { get; set; }

        public bool IsReserved
        {
            get
            {
                return this.ReserveInfos.Count > 0;
            }
        }

        public ReserveManager(PBFResource bucket)
        {
            this.Bucket = bucket;
            this.ReserveInfos = new List<ReserveInfo>();
        }

        /// <summary>
        /// 작업물을 예약합니다.
        /// </summary>
        /// <param name="lot"> 예약 대상 작업물입니다. </param>
        /// <param name="addFirst"> 우선 예약 여부입니다. </param>
        /// <param name="context"></param>
        public void ReserveLot(IAPELot lot, bool addFirst, List<ATOperResource> addArrInfos, FWSetupInfo setupInfo)
        {
            string key = lot.LotGroupKey;

            if (lot is APELot)
                key = (lot as APELot).LotID;

            foreach (var reserveInfo in ReserveInfos) // 이미 예약된 Lot은 재예약 하지 않도록 처리 (없앨수 있으면 없애야함)
            {
                if (reserveInfo.ReservedLot.LotGroupKey == key)
                    return;
            }

            APEWipAgent.Instance.RemoveLot(lot);
            lot.LotGroupKey = key;

            ReserveInfo info = new ReserveInfo(lot, addArrInfos, setupInfo);

            if (addFirst)
                ReserveInfos.Insert(0, info);
            else
                ReserveInfos.Add(info);

            if (info.AddArrInfos != null)
            {
                foreach (var addArr in info.AddArrInfos)
                {
                    var addBucket = addArr.Bucket as PBFResource;
                    addBucket.ReserveManager.ReserveInfos.Add(info);
                }
            }

            lot.IsReserved = true;
        }

        /// <summary>
        /// 예약 정보중 첫번째 예약 정보를 반환합니다.
        /// </summary>
        /// <returns> 첫번째 예약 정보입니다. </returns>
        internal ReserveInfo RemoveReserveInfo()
        {
            var info = this.ReserveInfos.FirstOrDefault();

            if (info == null)
                return null;

            this.ReserveInfos.Remove(info);

            info.ReservedLot.IsReserved = false;

            if (info.AddArrInfos != null)
            {
                foreach (var addArr in info.AddArrInfos)
                {
                    var addBucket = addArr.Bucket as PBFResource;
                    addBucket.ReserveManager.ReserveInfos.Remove(info);
                }
            }

            return info;
        }

        /// <summary>
        /// 현재 예약되어 있는 모든 예약정보를 반환합니다.
        /// </summary>
        /// <returns> 현재 예약되어 있는 모든 예약정보입니다. </returns>
        public List<ReserveInfo> GetReserveInfos()
        {
            return this.ReserveInfos;
        }
    }
}
