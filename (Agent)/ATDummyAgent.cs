using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.ObyO;
using Mozart.SeePlan.Aleatorik.ObyO.Data;
using Mozart.SeePlan.Aleatorik.Planner;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class ATDummyAgent : IAgent
    {
        public IBucket DummyBucket;

        private IDummyBucket _dummyBucketStrategy { get; set; }

        public static ATDummyAgent Instance
        {
            get
            {
                return ServiceLocator.Resolve<ATDummyAgent>();
            }
        }

        public void Initialize()
        {
            // 이 부분은 추후 수정 필요
            if (ATExecutionContext.Instance.CurrentExecutionInfo is PBOModuleExecutionInfo)
            {
                DummyBucket = new PBOResource();
                SetDummyBucketType(new PBODummyBucket());
            }
            else
            {
                DummyBucket = new PBFResource();
                SetDummyBucketType(new FWDummyBucket());
            }
        }

        public void SetDummyBucketType(IDummyBucket type)
        {
            this._dummyBucketStrategy = type;
        }

        public void Dispose()
        {
            this.DummyBucket = null;
        }

        public void DoAllocate(APELot lot)
        {
            this._dummyBucketStrategy.DoAllocate(this.DummyBucket, lot);
        }
    }

    public interface IDummyBucket
    {
        void DoAllocate(IBucket bucket, APELot lot);
    }

    public class FWDummyBucket : IDummyBucket
    {
        public void DoAllocate(IBucket bucket, APELot lot)
        {
            var dummyBucket = bucket as PBFResource;
            dummyBucket.CurrentTime = lot.LastStepTime;

            double runTat = FWInterface.LotControl.GetTat(lot, true);
            double waitTat = FWInterface.LotControl.GetTat(lot, false);
            double totalTat = runTat + waitTat;

            var planInfo = dummyBucket.DummyAllocated(lot, totalTat);

            lot.CurrentQty -= lot.CurrentQty;

            FWInterface.AllocatorControl.OnBucketAllocated(dummyBucket, null, lot, planInfo, null);
        }
    }

    public class PBODummyBucket : IDummyBucket
    {
        public void DoAllocate(IBucket bucket, APELot lot)
        {
            var dummyBucket = bucket as PBOResource;

            double tat = PBOInterface.PlanControl.GetTat(lot);

            var info = dummyBucket.DummyAllocated(lot, tat);

            PBOInterface.PlanControl.OnBucketAllocated(dummyBucket, lot, info, null);
        }
    }
}
