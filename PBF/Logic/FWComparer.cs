using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    internal class APEAllocGroupComparer : IComparer<FWAllocGroup>
    {
        public static APEAllocGroupComparer Default = new APEAllocGroupComparer();

        public APEAllocGroupComparer()
        {
        }

        public int Compare(FWAllocGroup x, FWAllocGroup y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            int cmp = 0;

            cmp = x.Sequence.CompareTo(y.Sequence);

            return cmp;
        }
    }

    internal class APEBucketGroupComparer : IComparer<PBFResourceGroup>
    {
        public static APEBucketGroupComparer Default = new APEBucketGroupComparer();

        public int Compare(PBFResourceGroup x, PBFResourceGroup y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            return x.Sequence.CompareTo(y.Sequence);
        }
    }

    /// <summary>
    /// Assembly 부품 Lot의 소팅
    /// </summary>
    internal class PartLotComparer : IComparer<APELot>
    {
        private ATWeightPreset _preset;
        /// <summary>
        /// 지정된 조립 후 제품을 사용해서 새로운 부품 작업물 비교자를 생성합니다.
        /// </summary>
        /// <param name="toProduct">조립 후 제품입니다.</param>
        public PartLotComparer(ATWeightPreset preset)
        {
            _preset = preset;
        }

        /// <summary>
        /// 두 부품 작업물의 우선순위를 비교합니다.
        /// </summary>
        /// <param name="x">첫 번째 부품 작업물입니다.</param>
        /// <param name="y">두 번째 부품 작업물입니다.</param>
        /// <returns>
        /// 반환값이 음수인 경우 첫 번째 부품 작업물의 우선순위가 높습니다. 
        /// 반환값이 양수인 경우 두 번째 부품 작업물의 우선순위가 높습니다. 
        /// 반환값이 양수인 경우 두 부품 작업물의 우선순위가 동일합니다.
        /// </returns>
        public int Compare(APELot x, APELot y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            int cmp = 0;

            if (this._preset != null)
            {
                //foreach (ATWeightFactor factor in this._preset.FactorList)
                //{
                //    var method = factor.Method as CompareTargetGroup;

                //    if (method == null)
                //        continue;

                //    ATFactorValue xValue = method(x, factor, this._context);
                //    ATFactorValue yValue = method(y, factor, this._context);

                //    cmp = ATUtil.CompareOrderType(xValue, yValue, factor.OrderType);

                //    if (cmp != 0)
                //    {
                //        // 비교 결과 적기
                //        return cmp;
                //    }
                //    // 비교 결과 적기
                //}
            }

            // Lot도착시간으로 순서 소팅
            cmp = x.LastStepTime.CompareTo(y.LastStepTime);
            if (cmp != 0)
                return cmp;

            cmp = x.CurrentTarget.TargetDateTime.CompareTo(y.CurrentTarget.TargetDateTime);
            if (cmp != 0)
                return cmp;


            cmp = x.FactorObjectKey.CompareTo(y.FactorObjectKey);

            return cmp;
        }
    }

    /// <summary>
    /// Assembly 대상 Lot의 소팅
    /// </summary>
    internal class AssemblyLotComparer : IComparer<APELot>
    {
        private ATWeightPreset _preset;
        /// <summary>
        /// 조립을 수행하는 Bucket의 시작 시간을 가져옵니다.
        /// </summary>
        private DateTime _now { get; set; }

        /// <summary>
        /// 지정된 Bucket 시작 시간을 사용하여 새로운 조립 후 작업물 비교자 클래스를 생성합니다.
        /// </summary>
        /// <param name="now">Bucket 시작 시간입니다.</param>
        public AssemblyLotComparer(ATWeightPreset preset, DateTime now)
        {
            this._preset = preset;
            this._now = now;
        }

        /// <summary>
        /// 두 조립 후 작업물의 우선순위를 비교합니다.
        /// </summary>
        /// <param name="x">첫 번째 조립 후 작업물입니다.</param>
        /// <param name="y">두 번째 조립 후 작업물입니다.</param>
        /// <returns>
        /// 반환값이 음수인 경우 첫 번째 조립 후 작업물의 우선순위가 높습니다. 
        /// 반환값이 양수인 경우 두 번째 조립 후 작업물의 우선순위가 높습니다. 
        /// 반환값이 양수인 경우 두 조립 후 작업물의 우선순위가 동일합니다.
        /// </returns>
        public int Compare(APELot x, APELot y)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("AssemblyLotComparer");
            try
            {
                if (object.ReferenceEquals(x, y))
                    return 0;

                int cmp = 0;

                if (this._preset != null)
                {
                    //foreach (ATWeightFactor factor in this._preset.FactorList)
                    //{
                    //    var method = factor.Method as CompareTargetGroup;

                    //    if (method == null)
                    //        continue;

                    //    ATFactorValue xValue = method(x, factor, this._context);
                    //    ATFactorValue yValue = method(y, factor, this._context);

                    //    cmp = ATUtil.CompareOrderType(xValue, yValue, factor.OrderType);

                    //    if (cmp != 0)
                    //    {
                    //        // 비교 결과 적기
                    //        return cmp;
                    //    }
                    //    // 비교 결과 적기
                    //}
                }

                cmp = ATDemandCompare.Compare(x.CurrentTarget.SODemand, y.CurrentTarget.SODemand);
                if (cmp != 0)
                    return cmp;

                cmp = x.FactorObjectKey.CompareTo(y.FactorObjectKey);

                return cmp;
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("AssemblyLotComparer");
            }
        }
    }

    /// <summary>
    /// Lot Group 내 Lot 간 Sorting
    /// </summary>
    public class APELotInGroupComparer : IComparer<IAPELot>
    {
        private ATWeightPreset _preset;

        public APELotInGroupComparer(ATWeightPreset preset)
        {
            this._preset = preset;
        }

        public int Compare(IAPELot x, IAPELot y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            int cmp = 0;

            if (this._preset != null)
            {
                //cmp = x.LastStepTime.CompareTo(y.LastStepTime);
                //if (cmp != 0)
                //    return cmp;

                foreach (ATWeightFactor factor in this._preset.FactorList)
                {
                    var method = factor.Method as CompareLotInGroup;

                    if (method == null)
                        continue;

                    ATFactorValue xValue = method(x, factor);
                    ATFactorValue yValue = method(y, factor);

                    cmp = xValue.Value.CompareTo(yValue.Value);

                    if (cmp != 0)
                        return cmp;
                }
            }

            return x.FactorObjectKey.CompareTo(y.FactorObjectKey);
        }
    }

    internal class FWLotGroupComparer : IComparer<APELotGroup>
    {
        private ATWeightPreset _preset;
        private PBFAllocateContext _context;

        public FWLotGroupComparer(PBFAllocateContext context, ATWeightPreset preset)
        {
            this._preset = preset;
            this._context = context;
        }

        public int Compare(APELotGroup x, APELotGroup y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            int cmp = 0;

            if (this._preset != null)
            {
                foreach (ATWeightFactor factor in this._preset.FactorList)
                {
                    var method = factor.Method as CompareLotGroupOnLFS;

                    if (method == null)
                        continue;

                    ATFactorValue xValue = method(x, factor, _context);
                    ATFactorValue yValue = method(y, factor, _context);

                    cmp = xValue.Value.CompareTo(yValue.Value);

                    x.AddFactorValue(factor.Name, xValue);
                    y.AddFactorValue(factor.Name, yValue);

                    if (cmp != 0)
                        return cmp;
                }
            }

            return x.FactorObjectKey.CompareTo(y.FactorObjectKey);
        }
    }

    internal class SetupResourceComparer : IComparer<PBFResource>
    {
        public static SetupResourceComparer Default = new SetupResourceComparer();

        public int Compare(PBFResource x, PBFResource y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            int cmp = x.CurrentTime.CompareTo(y.CurrentTime);
            if (cmp != 0)
                return cmp;

            cmp = x.BucketID.CompareTo(y.BucketID);
            return cmp;
        }
    }

    internal class APEBucketComparer : IComparer<PBFResource>
    {
        private ATWeightPreset _preset;
        private PBFAllocateContext _context;
        private APELotGroup _lotGroup;

        public APEBucketComparer(PBFAllocateContext context, APELotGroup lotGroup)
        {
            this._preset = context.CompareResourceOnLFSPreset;
            this._context = context;
            this._lotGroup = lotGroup;
        }

        public int Compare(PBFResource x, PBFResource y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            int cmp = 0;

            if (this._preset != null)
            {
                foreach (ATWeightFactor factor in this._preset.FactorList)
                {
                    var method = factor.Method as CompareResourceOnLFS;

                    if (method == null)
                        continue;

                    ATFactorValue xValue = method(x, factor, this._lotGroup, _context);
                    ATFactorValue yValue = method(y, factor, this._lotGroup, _context);

                    cmp = xValue.Value.CompareTo(yValue.Value);

                    x.AddFactorValue(factor.Name, xValue);
                    y.AddFactorValue(factor.Name, yValue);

                    if (cmp != 0)
                        return cmp;
                }
            }

            if (cmp != 0)
                return cmp;

            return x.BucketID.CompareTo(y.BucketID);
        }
    }

    internal class PBFResourceOnRFSComparer : IComparer<PBFResource>
    {
        private ATWeightPreset _preset;
        private PBFAllocateContext _context;

        public PBFResourceOnRFSComparer(PBFAllocateContext context)
        {
            this._preset = context.CompareResourceOnRFSPreset;
            this._context = context;
        }

        public int Compare(PBFResource x, PBFResource y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            int cmp = 0;

            if (this._preset != null)
            {
                foreach (ATWeightFactor factor in this._preset.FactorList)
                {
                    var method = factor.Method as CompareResourceOnRFS;

                    if (method == null)
                        continue;

                    ATFactorValue xValue = method(x,  factor, _context);
                    ATFactorValue yValue = method(y,  factor, _context);

                    cmp = xValue.Value.CompareTo(yValue.Value);

                    if (cmp != 0)
                        return cmp;
                }
            }

            if (cmp != 0)
                return cmp;

            return x.BucketID.CompareTo(y.BucketID);
        }
    }

    internal class APEAddBucketComparer : IComparer<PBFResource>
    {
        private ATWeightPreset _preset;
        private PBFAllocateContext _context;
        private APELotGroup _lotGroup;
        private PBFResource _bucket;

        public APEAddBucketComparer(PBFAllocateContext context, APELotGroup lotGroup, PBFResource bucket)
        {
            this._preset = context.CompareAddResourcePreset;
            this._context = context;
            this._lotGroup = lotGroup;
            this._bucket = bucket;
        }

        public int Compare(PBFResource x, PBFResource y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            int cmp = 0;

            if (this._preset != null)
            {
                foreach (ATWeightFactor factor in this._preset.FactorList)
                {
                    var method = factor.Method as CompareAddResource;

                    if (method == null)
                        continue;

                    ATFactorValue xValue = method(_bucket, x, this._lotGroup, factor, _context);
                    ATFactorValue yValue = method(_bucket, y, this._lotGroup, factor, _context);

                    cmp = xValue.Value.CompareTo(yValue.Value);

                    x.AddFactorValue(factor.Name, xValue);
                    y.AddFactorValue(factor.Name, yValue);

                    if (cmp != 0)
                        return cmp;
                }
            }

            if (cmp != 0)
                return cmp;

            return x.BucketID.CompareTo(y.BucketID);
        }
    }

    internal class APELotGroupOnRFSComparer : IComparer<APELotGroup>
    {
        private ATWeightPreset _preset;
        private PBFAllocateContext _context;
        private PBFResource _bucket;

        public APELotGroupOnRFSComparer(PBFResource bucket, PBFAllocateContext context)
        {
            this._preset = context.CompareLotGroupOnRFSPreset;
            this._context = context;
            this._bucket = bucket;
        }

        public int Compare(APELotGroup x, APELotGroup y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            int cmp = 0;

            if (this._preset != null)
            {
                if (this._preset.SortType == SortType.WeightSorted)
                {
                    foreach (ATWeightFactor factor in this._preset.FactorList)
                    {
                        var method = factor.Method as CompareLotGroupOnRFS;

                        if (method == null)
                            continue;

                        ATFactorValue xValue = method(x, _bucket, factor, _context);
                        ATFactorValue yValue = method(y, _bucket, factor, _context);

                        cmp = xValue.Value.CompareTo(yValue.Value);

                        x.AddFactorValue(factor.Name, xValue);
                        y.AddFactorValue(factor.Name, yValue);

                        if (cmp != 0)
                            return cmp;
                    }
                }
                else if (this._preset.SortType == SortType.WeightSum)
                {
                    if (object.ReferenceEquals(x, y))
                        return 0;

                    double weightX = x.GetWeightedSum();
                    double weightY = y.GetWeightedSum();

                    cmp = weightX.CompareTo(weightY) * -1;

                    return cmp;
                }
            }

            return x.FactorObjectKey.CompareTo(y.FactorObjectKey);
        }
    }
}
