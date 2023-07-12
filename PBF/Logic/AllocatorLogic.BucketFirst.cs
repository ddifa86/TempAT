using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public partial class AllocatorLogic
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool FilterResourceOnRFS(PBFResource bucket, PBFAllocateContext context)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("FilterResourceOnRFS");
            try
            {
                if (bucket.Target.ResCategory != ResourceCategory.Resource)
                    return true;

                bool canOperation = bucket.CanOperation();
                if (canOperation == false)
                    return true;

                if (context.FilterResourceOnRFSPreset == null)
                    return false;

                foreach (var factor in context.FilterResourceOnRFSPreset.FactorList)
                {
                    FilterResourceOnRFS method = factor.Method as FilterResourceOnRFS;

                    ATFilterValue value = method(bucket, factor, context);

                    if (value.Value == true)
                    {
                        bucket.AddFilterValue(factor.Name, value);
                        // bucket Queue에 대기중인 Lots에 대해 Short 등록?
                        return true;
                    }
                }

                return false;
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("FilterResourceOnRFS");
            }
        }

        /// <summary>
        /// Bucket First Selected 시점에 대상이 되는 Buckets 들 우선순위 소팅
        /// </summary>
        /// <param name="buckets"></param>
        /// <param name="context"></param>
        public void SortResourceOnRFS(List<PBFResource> buckets, PBFAllocateContext context)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("CompareResourceOnRFS");
            try
            {
                buckets.Sort(new PBFResourceOnRFSComparer(context));
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("CompareResourceOnRFS");
            }

        }

        /// <summary>
        /// Bucket First Selected 시점에 Bucket 내의 Queue의 LotGroup 을 Filter
        /// </summary>
        /// <param name="lotGroup"></param>
        /// <param name="bucket"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool FilterLotGroupOnRFS(APELotGroup lotGroup, PBFResource bucket, PBFAllocateContext context)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("FilterLotGroupOnRFS");
            try
            {
                if (lotGroup.Qty <= ATOption.Instance.MinimumAllocationQuantity)
                {
                    //다음 공정으로 보내기 전체적으로 다
                    //lotgroup.Lots.ForEach(x => this.Line.MoveNext(x));
                    return true;

                }

                if (lotGroup.LastStepTime >= context.NextDt)
                {
                    lotGroup.AddFilterValue("IsLoadableTime", new ATFilterValue(true, string.Format("{0}", lotGroup.Sample.LastStepTime)));

                    return true;
                }

                if (context.FilterLotGroupOnRFSPreset == null)
                    return false;

                foreach (var factor in context.FilterLotGroupOnRFSPreset.FactorList)
                {
                    FilterLotGroupOnRFS method = factor.Method as FilterLotGroupOnRFS;

                    ATFilterValue value = method(lotGroup, bucket, factor, context);

                    if (value.Value == true)
                    {
                        lotGroup.AddFilterValue(factor.Name, value);
                        lotGroup.GetLotList().ForEach(x => (x as APELot).AddVirtualLateInfo(LateCategory.Constraint, factor.Name, value.Description, context.NowDt, context.NextDt));

                        return true;
                    }
                }

                // Arrange 추가 확인
                return false;
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("FilterLotGroupOnRFS");
            }
        }

        public void SortLotGroupOnRFS(List<APELotGroup> lotGroups, PBFResource bucket, PBFAllocateContext context)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("CompareLotGroupOnRFS");
            try
            {
                foreach (var lotGroup in lotGroups)
                {
                    var setupInfo = AllocatorLogic.Instance.GetSetupInfo(bucket, lotGroup, context);
                    if (setupInfo != null)
                    {
                        context.SetupInfoDict.Add(lotGroup, setupInfo);
                        context.MaxSetupHrs = Math.Max(context.MaxSetupHrs, setupInfo.SetupTime.TotalHours);
                    }
                }

                var preset = context.CompareLotGroupOnRFSPreset;
                if (preset != null && preset.SortType == SortType.WeightSum)
                {
                    foreach (var lotGroup in lotGroups)
                    {
                        lotGroup.InitFactorValue();

                        foreach (ATWeightFactor factor in preset.FactorList)
                        {
                            var method = factor.Method as CompareLotGroupOnRFS;

                            if (method == null)
                                continue;

                            ATFactorValue value = method(lotGroup, bucket, factor, context);

                            lotGroup.AddFactorValue(factor.Name, value);
                        }
                    }
                }

                lotGroups.Sort(new APELotGroupOnRFSComparer(bucket, context));
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("CompareLotGroupOnRFS");
            }
        }
    }
}
