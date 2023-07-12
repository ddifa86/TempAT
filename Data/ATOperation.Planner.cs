using Mozart.SeePlan.Aleatorik.ObyO.Data;
using Mozart.SeePlan.Aleatorik.Planner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public partial class ATOperation
    {
        #region Planning 정보

        // 별도로 객체만들어서 관리가 필요함.. APE용 ArrangeManager를 만들고 해당 부분은 삭제 처리 필요.

        public PBFResourceGroup BucketGroup { get; set; }

        // 결국에 통합되어야함
        public Dictionary<PBFResource, ATOperResource> ArrangeInfos = new Dictionary<PBFResource, ATOperResource>();

        public List<PBFResource> GetLoadableBucket()
        {
            return this.ArrangeInfos.Keys.ToList();
        }

        public ATOperResource GetArrange(PBFResource bucket)
        {
            ATOperResource arr;
            if (this.ArrangeInfos.TryGetValue(bucket, out arr) == false)
                return null;

            return arr;
        }

        internal List<IAPEQueueManager> GetWipQueues()
        {
            List<IAPEQueueManager> lst = new List<IAPEQueueManager>();

            var buckets = this.ArrangeInfos.Keys;

            if (this.BucketGroup == null || buckets.Count() == 0)
                return lst;

            lst.AddRange(buckets);
            lst.Add(this.BucketGroup);

            return lst;
        }


        #endregion

    }
}
