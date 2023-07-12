using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Planner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Logic
{
    public partial class MBSLogic
    {
        internal double GetAllocableQty(APELotGroup lotGroup, PBFResource bucket, DateTime loadableTime, PBFAllocateContext context)
        {
            double allocableQty = 0;

            foreach (var lot in lotGroup.GetLotList())
            {
                if (lot.LastStepTime >= bucket.EndTime)
                    break;

                allocableQty += lot.CurrentQty;
            }

            double mbsValue = lotGroup.CurrentOper.MBSValue;
            APELot sampleLot = lotGroup.SampleLot;

            if (bucket.IsTimeCapa)
                loadableTime = ATUtil.MaxTime(loadableTime, sampleLot.LastStepTime);

            ATOperResource arrange = sampleLot.CurrentOper.GetArrange(bucket);
            if (arrange == null)
                return allocableQty;

            sampleLot.CurrentArrange = arrange;

            # region UsagePer, Utilization, Constraint 계산
            double usagePer = arrange.GetUsagePer(context.NowDt);
            double utilization = bucket.Target.GetUtilization(context.NowDt);

            if (bucket.IsTimeCapa)
                loadableTime = ATUtil.MaxTime(loadableTime, bucket.CurrentTime);

            double constraintQty = double.MaxValue;
            if (bucket.CurrentConstraintDetails.Count > 0)
                constraintQty = bucket.CurrentConstraintDetails.Min(x => x.RemainQty);

            var lotConstraints = sampleLot.GetConstraintDetails(ATUtil.ToDate(context.NowDt));
            if (lotConstraints.Count > 0)
                constraintQty = Math.Min(constraintQty, lotConstraints.Min(x => x.RemainQty));

            foreach (PBFResource addBucket in context.AddBuckets)
            {
                if (addBucket.CurrentConstraintDetails.Count > 0)
                    constraintQty = Math.Min(constraintQty, addBucket.CurrentConstraintDetails.Min(x => x.RemainQty));

                if (addBucket.IsTimeCapa == false)
                    continue;

                var addArr = arrange.GetAddArrangeInfo(addBucket.Target.ResGroupID, addBucket.BucketID);
                if (addArr == null)
                    continue;

                usagePer = Math.Max(usagePer, addArr.UsagePer);
                loadableTime = ATUtil.MaxTime(loadableTime, addBucket.CurrentTime);
            }

            allocableQty = Math.Min(allocableQty, constraintQty);
            #endregion

            #region Capa를 고려한 할당 수량 계산
            double mainCapacity = double.MaxValue;
            if (bucket.IsInfinite == false)
            {
                var from = loadableTime;
                var to = bucket.EndTime;

                if (bucket.IsTimeCapa)
                {
                    var nextSection = bucket.GetNextNonWorkingPeriod(loadableTime, bucket.EndTime, LotSplitOption.Split, true, true);
                    if (nextSection != APENonWorkingPeriod.NULL)
                        to = loadableTime >= nextSection.Start ? loadableTime : nextSection.Start;
                }

                mainCapacity = bucket.CalculateCapacity(from, to);
                allocableQty = context.CurrentAllocator.GetAllocableLotQty(mainCapacity, allocableQty, usagePer, utilization, bucket.CapacityType);
            }

            if (bucket.IsTimeCapa == false)
            {
                foreach (var addArr in context.AddArrInfos)
                {
                    var addBucket = addArr.Bucket as PBFResource;
                    if (addBucket.IsInfinite == true)
                        continue;

                    double addUsagePer = addArr.GetUsagePer(context.NowDt);
                    double addCapacity = addBucket.CurrentCapaInfo.Remain;

                    double addUtilRate;
                    if (context.AddUtilizationRate.TryGetValue(addBucket, out addUtilRate) == false)
                    {
                        addUtilRate = addBucket.Target.GetUtilization(context.NowDt);
                        context.AddUtilizationRate.Add(addBucket, addUtilRate);
                    }

                    allocableQty = context.CurrentAllocator.GetAllocableLotQty(addCapacity, allocableQty, addUsagePer, addUtilRate, addBucket.CapacityType);
                }
            }
            #endregion

            double gap = allocableQty.Ceiling() - allocableQty;
            if (gap < ATOption.Instance.MinimumAllocationQuantity)
                allocableQty = allocableQty.Ceiling();

            allocableQty = Math.Truncate(allocableQty / mbsValue) * mbsValue;

            return allocableQty;
        }
    }
}
