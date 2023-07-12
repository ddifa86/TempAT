using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.ObyO.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.ObyO
{
    internal class OboCapaInfoComparer : IComparer<APECapacity>
    {
        private ATWeightPreset _preset;
        private PBOAllocateContext _context;

        public OboCapaInfoComparer(ATWeightPreset preset, PBOAllocateContext context)
        {
            this._preset = preset;
            this._context = context;
        }

        public int Compare(APECapacity x, APECapacity y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            int cmp = 0;

            if (this._preset != null)
            {
                foreach (ATWeightFactor factor in this._preset.FactorList)
                {
                    var method = factor.Method as CompareCapacity;

                    if (method == null)
                        continue;

                    ATFactorValue xValue = method(x, factor, _context);
                    ATFactorValue yValue = method(y, factor, _context);

                    cmp = xValue.Value.CompareTo(yValue.Value);

                    if (cmp != 0)
                        return cmp;
                }
            }

            cmp = x.StartTime.CompareTo(y.StartTime);
            if (cmp != 0)
                return cmp;

            cmp = x.Bucket.BucketID.CompareTo(y.Bucket.BucketID);

            return cmp;
        }
    }


    internal class OboBucketComparer : IComparer<PBOResource>
    {
        public APETarget Target;
        public DateTime Maxlatedate;

        public OboBucketComparer(APETarget target)
        {
            this.Target = target;

            double maxdays = (target.MoPlan as ATMoPlan).Demand.MaxLateDays;

            this.Maxlatedate = target.TargetDateTime.AddDays(maxdays);
        }

        public int Compare(PBOResource x, PBOResource y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;


            //  // Max 지연 여부를 감안했을 때에도 할당 가능 여부 체크
            //  // Short 내는 조건
            //  string date = string.Empty;
            //  //double xcapa = Math.Min( x.SumCapacity(this.Maxlatedate, ref date), 1000000000);
            //  //double ycapa = Math.Min( y.SumCapacity(this.Maxlatedate, ref date), 1000000000);

            ////  bool x_usable = Target.RemainQty <= xcapa; // 할당가능하면 true
            // // bool y_usable = Target.RemainQty <= ycapa;

            //  int cmp = x_usable.CompareTo(y_usable) * -1;
            //  if (cmp != 0)
            //      return cmp;

            //  // rtf 달성 날짜까지 Capa가 큰 곳으로 우선 진행
            //  cmp = xcapa.CompareTo(ycapa) * -1;
            //  if (cmp != 0)
            //      return cmp;

            //  // capa 날짜가 빠른 것 우선
            //  cmp = x.AvailableDate.CompareTo(y.AvailableDate);
            //  if (cmp != 0)
            //      return cmp;

            //  // 달성율
            //  cmp = x.FirstTarget.UsedRatio.CompareTo(y.FirstTarget.UsedRatio);
            //  if (cmp != 0)
            //      return cmp;

            //  return x.Resource.ResourceID.CompareTo(y.Resource.ResourceID);
            return 0;


        }
    }
}
