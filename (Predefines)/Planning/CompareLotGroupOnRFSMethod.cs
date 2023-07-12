using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public class CompareLotGroupOnRFSMethod
    {
        /// <summary>
        /// 지연된 재공 그룹 중 가장 많이 늦은 재공 그룹 우선
        /// </summary>
        /// <param name="lotGroup"></param>
        /// <param name="bucket"></param>
        /// <param name="factor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareLotGroupOnRFS))]
        public ATFactorValue MoreDelayedLotGrpFirst(APELotGroup lotGroup, PBFResource bucket, ATWeightFactor factor, PBFAllocateContext context)
        {
            double rawValue = 0d;
            var sample = lotGroup.SampleLot;

            var availableTime = context.NowDt;
            var targetDate = sample.CurrentTarget.TargetDateTime;
            var maxLatenessDays = sample.CurrentTarget.SODemand.MaxLateDays;
            var lateTargetDate = targetDate.AddDays(maxLatenessDays);

            if (availableTime >= targetDate && availableTime < lateTargetDate)
                rawValue = (lateTargetDate - availableTime).TotalDays / maxLatenessDays;

            var log = string.Join(";", availableTime.ToDisplayString(), targetDate.ToDisplayString(), lateTargetDate.ToDisplayString(), Math.Round(rawValue, 2));

            return new ATFactorValue(factor, rawValue, log);
        }

        /// <summary>
        /// 선행 진행된 재공 그룹 중 여유 시간이 가장 적은 재공 그룹 우선
        /// </summary>
        /// <param name="lotGroup"></param>
        /// <param name="bucket"></param>
        /// <param name="factor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareLotGroupOnRFS))]
        public ATFactorValue LessPrecededLotGrpFirst(APELotGroup lotGroup, PBFResource bucket, ATWeightFactor factor, PBFAllocateContext context)
        {
            double rawValue = 0d;
            var sample = lotGroup.SampleLot;

            var availableTime = context.NowDt;
            var targetDate = sample.CurrentTarget.TargetDateTime;
            var maxEarlinessDays = sample.CurrentTarget.SODemand.MaxEarlyDays;
            var earlyTargetDate = targetDate.AddDays(-maxEarlinessDays);

            if (availableTime >= targetDate)
                rawValue = 1d;
            else if (availableTime >= earlyTargetDate && availableTime < targetDate)
                rawValue = 1 - ((targetDate - availableTime).TotalDays / maxEarlinessDays);

            var log = string.Join(";", availableTime.ToDisplayString(), targetDate.ToDisplayString(), earlyTargetDate.ToDisplayString(), Math.Round(rawValue, 2));

            return new ATFactorValue(factor, rawValue, log);
        }

        /// <summary>
        /// 셋업 시간이 가장 짧은 재공 그룹 우선
        /// </summary>
        /// <param name="lotGroup"></param>
        /// <param name="bucket"></param>
        /// <param name="factor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareLotGroupOnRFS))]
        public ATFactorValue ShorterSetupLotGrpFirst(APELotGroup lotGroup, PBFResource bucket, ATWeightFactor factor, PBFAllocateContext context)
        {
            double rawValue = 1d;
            double setupHrs = 0d;

            FWSetupInfo setupInfo;
            if (context.SetupInfoDict.TryGetValue(lotGroup, out setupInfo))
            {
                rawValue = 1 - (setupInfo.SetupTime.TotalHours / context.MaxSetupHrs);
                setupHrs = setupInfo.SetupTime.TotalHours;
            }

            var log = string.Join(";", setupHrs, context.MaxSetupHrs, Math.Round(rawValue, 2));

            return new ATFactorValue(factor, rawValue, log);
        }

        /// <summary>
        /// 타겟의 SO Priority가 높은 (값이 작은) 재공 그룹 우선
        /// </summary>
        /// <param name="lotGroup"></param>
        /// <param name="bucket"></param>
        /// <param name="factor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareLotGroupOnRFS))]
        public ATFactorValue HigherSoPriorityLotGrpFirst(APELotGroup lotGroup, PBFResource bucket, ATWeightFactor factor, PBFAllocateContext context)
        {
            double maxPriority = ATInputData.Demands.MaxPriority;
            double priority = lotGroup.SampleLot.SoDemand.Priority;

            double rawValue = 1 - (priority / maxPriority);

            var log = string.Join(";", priority, maxPriority, Math.Round(rawValue, 2));

            return new ATFactorValue(factor, rawValue, log);
        }

        /// <summary>
        /// 타겟 일자가 빠른 재공 그룹 우선
        /// </summary>
        /// <param name="lotGroup"></param>
        /// <param name="bucket"></param>
        /// <param name="factor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareLotGroupOnRFS))]
        public ATFactorValue CloserTargetDateLotGrpFirst(APELotGroup lotGroup, PBFResource bucket, ATWeightFactor factor, PBFAllocateContext context)
        {
            var sample = (lotGroup.Sample as APELot);

            var targetDate = double.Parse(sample.CurrentTarget.TargetDate);

            return new ATFactorValue(targetDate, string.Format("TargetDate : {0})", targetDate));
        }

        /// <summary>
        /// 타겟 주차가 빠른 재공 그룹 우선
        /// </summary>
        /// <param name="lotGroup"></param>
        /// <param name="bucket"></param>
        /// <param name="factor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareLotGroupOnRFS))]
        public ATFactorValue CloserTargetWeekLotGrpFirst(APELotGroup lotGroup, PBFResource bucket, ATWeightFactor factor, PBFAllocateContext context)
        {
            var sample = (lotGroup.Sample as APELot);

            var targetWeek = double.Parse(sample.CurrentTarget.TargetWeek);

            return new ATFactorValue(targetWeek, string.Format("TargetWeek : {0})", targetWeek));
        }

        /// <summary>
        /// 먼저 도착한 재공 그룹 우선
        /// </summary>
        /// <param name="lotGroup"></param>
        /// <param name="bucket"></param>
        /// <param name="factor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareLotGroupOnRFS))]
        public ATFactorValue EarlierAvailTimeLotGrpFirst(APELotGroup lotGroup, PBFResource bucket, ATWeightFactor factor, PBFAllocateContext context)
        {
            var arrivalTime = lotGroup.LastStepTime.ToOADate();
            var bucketTime = bucket.CurrentTime.ToOADate();

            var biggerTime = arrivalTime > bucketTime ? arrivalTime : bucketTime;

            return new ATFactorValue(biggerTime, string.Format("ArrivalTime : {0})", biggerTime));
        }
    }
}
