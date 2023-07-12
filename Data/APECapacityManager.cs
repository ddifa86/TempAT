using Mozart.SeePlan.Aleatorik.Planner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class APECapacityManager
    {
        private List<APECapacity> _orgCapaInfos { get; }

        private List<APECapacity> _capaInfos { get; }

        public IBucket Bucket { get; }

        public bool HasCapaInfos { get; set; }

        #region 생성자
        public APECapacityManager(IBucket bucket, CapacityType capaType)
        { 
            this.Bucket = bucket;
            this._capaInfos = new List<APECapacity>();
            this._orgCapaInfos = new List<APECapacity>();
            this.HasCapaInfos = false;

            if (bucket.Target.CapaInfos != null && bucket.Target.CapaInfos.Count > 0)
            {
                APECapacity prev = null;
                foreach (ATCapacityInfo tmp in bucket.Target.CapaInfos)
                {
                    ATCapacityInfo info = tmp;
                    if (capaType != bucket.CapacityType)
                    {
                        info = tmp.ShallowCopy(info.Capacity);
                        if (bucket.Target.OffTimeInfoManager.DailyNonWorkingInfos.TryGetValue(tmp.CalAttr.ApplyDate, out var nonWorkingTimes))
                        {
                            foreach (var nonWorkingTime in nonWorkingTimes)
                            {
                                double time = (nonWorkingTime.EndTime - nonWorkingTime.StartTime).TotalSeconds;
                                if (time <= ATOption.Instance.MinimumAllocationQuantity)
                                    time = 0;
                                info.Capacity -= time;
                            }
                        }
                    }

                    APECapacity capaInfo;
                    if (capaType == CapacityType.Time)
                        capaInfo = new ATTimeCapacity(info, bucket);
                    else
                        capaInfo = new ATQtyCapacity(info, bucket);

                    if (prev != null)
                    {
                        prev.Next = capaInfo;
                        capaInfo.Prev = prev;
                    }

                    prev = capaInfo;

                    this._orgCapaInfos.Add(capaInfo);
                    this._capaInfos.Add(capaInfo);
                }

                this.HasCapaInfos = true;
            }
        }

        #endregion

        #region method

        public List<APECapacity> GetOrgCapaInfos()
        {
            return _orgCapaInfos;
        }
        
        //fw
        public APECapacity GetCapacity(DateTime now, DateTime nextStartTime, CapacityType capaType)
        {
            APECapacity capaInfo = this._capaInfos.Where(x => x.StartTime <= now && x.EndTime > now).FirstOrDefault();

            // 가용 가능한 Capa가 없는 경우
            if (capaInfo == null)
            {
                double capacity = 0;
                var factoryNow = now.SplitDate();

                ATCapacityInfo info = new ATCapacityInfo(this.Bucket.Target, capacity, factoryNow, factoryNow.AddDays(1));

                if (capaType == CapacityType.Time)
                {
                    capacity = (nextStartTime - ShopCalendar.StartTimeOfDayT(now)).TotalSeconds; // 의미 없는 Row
                    capaInfo = new ATTimeCapacity(info, this.Bucket);
                }
                else
                {
                    capaInfo = new APECapacity(info, this.Bucket);
                }

                // 바로 return 가능
            }
           
            // 첫날인 경우.
            var isFirstDate = FWFactory.Instance.StartTime == now;

            double ratio = 1.0;

            #region 첫날인 경우 소요된 시간만큼을 감안한 Capa Ratio 산출
            if (isFirstDate)
            {
                // Horizon 내 전체 시간
                var totalsec = (capaInfo.EndTime - capaInfo.StartTime).TotalSeconds;

                // 현재 롤링 내 전체 시간
                var partSec = (capaInfo.EndTime - now).TotalSeconds;

                ratio = partSec / totalsec;

                capaInfo.Capacity = capaInfo.Capacity * ratio;
            }
            #endregion

            return capaInfo;
        }

        //obo
        public List<APECapacity> GetLoadableCapaInfos(DateTime start, DateTime end)
        {
            List<APECapacity> capaList = new List<APECapacity>();
            
            DateTime planStart = ATOption.Instance.PlanStartTime;
            DateTime planEnd = planStart.AddDays(ATOption.Instance.FrozenHorizon);

            bool applyFrozen = ATOption.Instance.FrozenHorizon != 0;

            var capaInfos = this._capaInfos;

            if (ATOption.Instance.ApplyOverCapacity)
                capaInfos = this._orgCapaInfos;

            var curCapaInfo = capaInfos.FirstOrDefault();

            while (curCapaInfo != null)
            {
                if (end < curCapaInfo.StartTime)
                    break;

                if (start >= curCapaInfo.EndTime)
                {
                    curCapaInfo = curCapaInfo.Next;
                    continue;
                }

                if (applyFrozen)
                {
                    if (curCapaInfo.StartTime < planEnd)
                    {
                        curCapaInfo = curCapaInfo.Next;
                        continue;
                    }
                }

                capaList.Add(curCapaInfo);
                curCapaInfo = curCapaInfo.Next;
            }

            return capaList;
        }
        
        public void RemoveCapaInfo(APECapacity info)
        {
            _capaInfos.Remove(info);
        }
        #endregion
    }
}
