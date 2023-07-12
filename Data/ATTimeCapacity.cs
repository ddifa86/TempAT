using Mozart.SeePlan.Aleatorik.Planner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class ATTimeCapacity : APECapacity
    {
        #region value
        public DateTime CurrentTime { get; set; }

        public override double Remain
        {
            get
            {
                double remain = (this.EndTime - this.CurrentTime).TotalSeconds;
                if (remain < 1)
                {
                    this.CurrentTime = this.EndTime;
                    remain = 0;
                }

                return remain;
            }
        }

        #endregion

        public ATTimeCapacity(ATCapacityInfo info, IBucket bucket)
            :base(info, bucket)
        {
            this.CurrentTime = this.StartTime;
        }

        #region method

        public void UseTo(DateTime toTime)
        {
            var bucket = this.Bucket as PBFResource;
            if (toTime < this.CurrentTime)
                return;

            if (bucket.IsInfinite)
                return;

            // Idle 집계.
            // IdleCapaCity 산출 작업 후 장비에 등록

            // NonWorkingTime 과 겹쳤을 때, 처리.
            toTime = bucket.GetNextWorkingTime(toTime, this.EndTime);

            this.CurrentTime = toTime;
        }

        #endregion
    }
}
