
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik
{
    [FeatureBind()]
    public class CompareWipMethod
    {
        /// <summary>
        /// 사용 가능 시점 (Available Time)이 빠른 재공 우선
        /// </summary>
        /// <param name="wip"></param>
        /// <param name="factor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareWip))]
        public ATFactorValue EarlierAvailTimeWipFirst(APEWip wip, ATWeightFactor factor, APEPegContext context)
        {
            UomType uomtype = UomType.Second;

            if (factor.Criteria != null && factor.Criteria.Count() > 0)
            {
                var uom = factor.Criteria.FirstOrDefault();

                if (uom != null)
                    uomtype = ATUtil.StringToEnum<UomType>(uom.ToString(), UomType.Second);
            }

            DateTime adjustTime = wip.AvailableTime;

            switch (uomtype)
            {
                case UomType.Day:
                    {
                        adjustTime = wip.AvailableTime.StartTimeOfDay();
                        break;
                    }
                case UomType.Hour:
                    {
                        double hour = wip.AvailableTime.Hour;
                        adjustTime = wip.AvailableTime.Date.AddHours(hour);
                        break;
                    }
                case UomType.Minute:
                    {
                        double hour = wip.AvailableTime.Hour;
                        double minute = wip.AvailableTime.Minute;
                        adjustTime = wip.AvailableTime.Date.AddHours(hour).AddMinutes(minute);
                        break;
                    }
                case UomType.Second:
                    {
                        adjustTime = wip.AvailableTime;
                        break;
                    }
                case UomType.Shift:
                    {
                        int idx = wip.AvailableTime.ClassifyShift();
                        adjustTime = wip.AvailableTime.GetShiftStartTime(idx);
                        break;
                    }
            }

            double result = Math.Round(adjustTime.ToOADate(), 4);

            return new ATFactorValue(result, string.Format("Wip AvailableTime : {0}", wip.AvailableTime.ToString("yyyy-MM-dd HH:mm:ss")));
        }

        /// <summary>
        /// 제품 등급이 높은 재공 우선
        /// </summary>
        /// <param name="planWip"></param>
        /// <param name="factor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareWip))]
        public ATFactorValue HigherGradeWipFirst(APEWip planWip, ATWeightFactor factor, APEPegContext context)
        {
            double grade = planWip.Item.Grade;

            return new ATFactorValue(grade, planWip.Item.Grade.ToString());
        }

        /// <summary>
        /// 수량이 큰 재공 우선
        /// </summary>
        /// <param name="planWip"></param>
        /// <param name="factor"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(CompareWip))]
        public ATFactorValue LargetQtyWipFirst(APEWip planWip, ATWeightFactor factor, APEPegContext context)
        {
            return new ATFactorValue(planWip.RemainQty * -1, planWip.Qty.ToString());
        }
    }
}
