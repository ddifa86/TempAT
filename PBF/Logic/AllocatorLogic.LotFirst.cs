using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public partial class AllocatorLogic
    {
       

        /// <summary>
        /// LotGroup First Selected 시점에 BucketGroup 내의 Queue의 LotGroup 을 Filter
        /// </summary>
        /// <param name="lotGroup"></param>
        /// <param name="bucket"></param>
        /// <param name="context"></param>
        public bool FilterLotGroupInLevel(APELotGroup lotGroup, PBFAllocateContext context)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("FilterLotGroupInLevel");
            try
            {
                if (lotGroup.Sample.LastStepTime >= context.NextDt)
                {
                    lotGroup.AddFilterValue("IsLoadableTime", new ATFilterValue(true, string.Format("{0}", lotGroup.Sample.LastStepTime)));
                    return true;
                }

                if (context.FilterLotGroupInLevelPreset == null)
                    return false;

                foreach (var factor in context.FilterLotGroupInLevelPreset.FactorList)
                {
                    FilterLotGroupInLevel method = factor.Method as FilterLotGroupInLevel;

                    ATFilterValue value = method(lotGroup, factor, context);

                    if (value.Value == true)
                    {
                        lotGroup.AddFilterValue(factor.Name, value);
                        lotGroup.GetLotList().ForEach(x => (x as APELot).AddVirtualLateInfo(LateCategory.Constraint, factor.Name, value.Description, context.NowDt, context.NextDt));

                        return true;
                    }
                }

                return false;
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("FilterLotGroupInLevel");
            }
        }

        /// <summary>
        /// LotGroup First Selected 시점에 BucketGroup 내의 Queue의 LotGroup 을 Filter
        /// </summary>
        /// <param name="lotGroup"></param>
        /// <param name="bucket"></param>
        /// <param name="context"></param>
        public bool FilterLotGroupOnLFS(APELotGroup lotGroup, PBFAllocateContext context)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("FilterLotGroupOnLFS");
            try
            {
                if (lotGroup.Sample == null)
                {
                    // 다음 공정으로 들어갈 때 Lot의 장비그룹과 lotGroupKey가 동일한 경우, 해당 케이스가 발생될 수 있음.
                    return false;
                }

                if (lotGroup.Sample.LastStepTime >= context.NextDt)
                {
                    lotGroup.AddFilterValue("IsLoadableTime", new ATFilterValue(true, string.Format("{0}", lotGroup.Sample.LastStepTime)));

                    return true;
                }

                if (context.FilterLotGroupOnLFSPreset == null)
                    return false;

                foreach (var factor in context.FilterLotGroupOnLFSPreset.FactorList)
                {
                    FilterLotGroupOnLFS method = factor.Method as FilterLotGroupOnLFS;

                    ATFilterValue value = method(lotGroup, factor, context);

                    if (value.Value == true)
                    {
                        lotGroup.AddFilterValue(factor.Name, value);
                        lotGroup.GetLotList().ForEach(x => (x as APELot).AddVirtualLateInfo(LateCategory.Constraint, factor.Name, value.Description, context.NowDt, context.NextDt));

                        return true;
                    }
                }

                return false;
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("FilterLotGroupOnLFS");
            }
        }

        public List<PBFResource> GetLoadableBuckets(PBFResourceGroup bucketGroup, APELotGroup lotGroup, PBFAllocateContext context)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("Allocate_GetLoadableBuckets");
            try
            {
                var loadalbeBuckets = lotGroup.GetLoadableBuckets();
                  
                loadalbeBuckets.ForEach(x => x.InitFactorValue());

                var availableBucket = new List<PBFResource>();

                foreach (var bucket in loadalbeBuckets)
                {
                    if (bucket.CanOperation() == false || bucket.ReserveManager.IsReserved)
                    {
                        bucket.AddFilterValue("HasRemainCapa", new ATFilterValue(true,""));

                        string reason = null;
                        string desc = null;
                        if (bucket.CurrentCapaInfo.Capacity == 0)
                        {
                            reason = LateReason.NoResourceCapacity.ToString();
                        }
                        else
                        {
                            reason = LateReason.LackOfResourceCapacity.ToString();
                            desc = string.Format("Calendar ID : {0}", bucket.CurrentCapaInfo.CapaInfo.CalAttr.CalendarID);
                        }
                        
                        lotGroup.GetLotList().ForEach(x => (x as APELot).AddVirtualLateInfo(LateCategory.Capacity, reason, desc, context.NowDt, context.NextDt, new HashSet<IBucket>() { bucket }));

                        context.AllocationLog.FilterResources.Add(bucket);

                        continue;
                    }

                    // 추후 캘린더를 고려하여 로드가능여부 기본 판단 로직 추가 필요.
                    if (AllocatorLogic.Instance.FilterResource(bucket, lotGroup, context) == true)
                    {
                        context.AllocationLog.FilterResources.Add(bucket);
                            
                        continue;
                    }

                    availableBucket.Add(bucket);
                }

                return availableBucket;
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("Allocate_GetLoadableBuckets");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool FilterResource(PBFResource bucket, APELotGroup lotGroup, PBFAllocateContext context)
        {
            ATElapsedTimeChecker.Instance.ResetTimer("FilterResourceOnLFS");
            try
            {
                if (context.FilterResourceOnLFSPreset == null)
                    return false;

                foreach (var factor in context.FilterResourceOnLFSPreset.FactorList)
                {
                    FilterResourceOnLFS method = factor.Method as FilterResourceOnLFS;

                    ATFilterValue value = method(bucket, factor, lotGroup, context);

                    if (value.Value == true)
                    {
                        bucket.AddFilterValue(factor.Name, value);
                        lotGroup.GetLotList().ForEach(x => (x as APELot).AddVirtualLateInfo(LateCategory.Constraint, factor.Name, value.Description, context.NowDt, context.NextDt));

                        return true;
                    }
                }

                return false;
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("FilterResourceOnLFS");
            }
        }


    }
}
