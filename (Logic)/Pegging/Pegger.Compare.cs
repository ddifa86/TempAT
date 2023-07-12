using Mozart.RuleFlow;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Pegging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    internal class APEPeggingGroupComparer : IComparer<IMergedTargetGroup>
    {
        private ATWeightPreset _preset;

        public APEPeggingGroupComparer(ATWeightPreset preset)
        {
            this._preset = preset;
        }

        public int Compare(IMergedTargetGroup x, IMergedTargetGroup y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            int cmp = 0;

            if (this._preset != null)
            {
                foreach (ATWeightFactor factor in this._preset.FactorList)
                {
                    var method = factor.Method as ComparePeggingGroup;

                    if (method == null)
                        continue;

                    ATFactorValue xValue = method(x as APEPeggingGroup, factor);
                    ATFactorValue yValue = method(y as APEPeggingGroup, factor);

                    cmp = xValue.Value.CompareTo(yValue.Value);

                    if (cmp != 0)
                        return cmp;
                }
            }

            cmp = x.Key.CompareTo(y.Key);

            return cmp;
        }
    }

    internal class APETargetGroupComparer : IComparer<APETargetGroup>
    {
        private ATWeightPreset _preset;
        private APEPegContext _context;

        public APETargetGroupComparer(APEPegContext context)
        {
            this._preset = context.CompareTargetGroupPreset;
            this._context = context;
        }

        public APETargetGroupComparer(ATWeightPreset preset)
        {
            this._preset = preset;
        }

        public int Compare(APETargetGroup x, APETargetGroup y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            int cmp = 0;

            if (this._preset != null)
            {
                foreach (ATWeightFactor factor in this._preset.FactorList)
                {
                    var method = factor.Method as CompareTargetGroup;

                    if (method == null)
                        continue;

                    ATFactorValue xValue = method(x, factor, this._context);
                    ATFactorValue yValue = method(y, factor, this._context);

                    cmp = xValue.Value.CompareTo(yValue.Value);

                    if (cmp != 0)
                        return cmp;
                }
            }

            cmp = x.Key.CompareTo(y.Key);

            return cmp;
        }
    }

    internal class ATTargetComparer : IComparer<ITarget>
    {
        private ATWeightPreset _preset;
        private APEPegContext _context;

        public ATTargetComparer(APEPegContext context)
        {
            this._context = context;
            this._preset = context.CompareTargetInGroupPreset;
        }

        public ATTargetComparer(ATWeightPreset preset)
        {
            this._preset = preset;
        }

        public int Compare(ITarget x, ITarget y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            int cmp = 0;

            if (this._preset != null)
            {
                foreach (ATWeightFactor factor in this._preset.FactorList)
                {
                    var method = factor.Method as CompareTargetInGroup;

                    if (method == null)
                        continue;

                    ATFactorValue xValue = method(x as APETarget, factor, this._context);
                    ATFactorValue yValue = method(y as APETarget, factor, this._context);

                    cmp = xValue.Value.CompareTo(yValue.Value);

                    if (cmp != 0)
                        return cmp;
                }
            }

            // 최종까지 동일한 경우 Key를 이용하여 소팅
            var targetX = x as APETarget;
            var targetY = y as APETarget;

            cmp = targetX.TargetID.CompareTo(targetY.TargetID);

            // 결과 적기
            return cmp;
        }

    }

    internal class ATReleasLotComparer : IComparer<APELot>
    {
        private ATWeightPreset _preset;

        public ATReleasLotComparer(ATWeightPreset preset)
        {
            _preset = preset;
        }

        public int Compare(APELot x, APELot y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            int cmp = 0;

            if (this._preset != null)
            {
                foreach (ATWeightFactor factor in this._preset.FactorList)
                {
                    var method = factor.Method as CompareLot;

                    if (method == null)
                        continue;

                    ATFactorValue xValue = method(x, factor);
                    ATFactorValue yValue = method(y, factor);

                    cmp = xValue.Value.CompareTo(yValue.Value) * -1;

                    if (cmp != 0)
                        return cmp;
                }
            }

            cmp = x.LotID.CompareTo(y.LotID);

            return cmp;
        }
    }

    internal class ATPegPartInGroupComparer : IComparer<ITarget>
    {
        public ATPegPartInGroupComparer()
        {

        }

        public int Compare(ITarget x, ITarget y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            var targetX = x as APETarget;
            var targetY = y as APETarget;

            int cmp = 0;
            cmp = targetX.TargetDateTime.CompareTo(targetY.TargetDateTime);
            if (cmp != 0)
                return cmp;

            cmp = targetX.SoDemand.ID.CompareTo(targetY.SoDemand.ID);
            return cmp;
        }
    }

    internal class ATPlanWipComparer : IComparer<APEWip>
    {
        private APEPegContext _context;
        private ATWeightPreset _preset;

        public ATPlanWipComparer(ATWeightPreset preset)
        {
            this._context = null;
            this._preset = preset;
        }

        public ATPlanWipComparer(APEPegContext context)
        {
            this._context = context;
            this._preset = context.CompareWipPreset;
        }

        public int Compare(APEWip x, APEWip y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            int cmp = 0;

            if (this._preset != null)
            {
                foreach (ATWeightFactor factor in this._preset.FactorList)
                {
                    var method = factor.Method as CompareWip;

                    if (method == null)
                        continue;

                    ATFactorValue xValue = method(x, factor, _context);
                    ATFactorValue yValue = method(y, factor, _context);

                    cmp = xValue.Value.CompareTo(yValue.Value);

                    if (cmp != 0)
                        return cmp;
                }
            }

            cmp = x.WipInfo.LotID.CompareTo(y.WipInfo.LotID);

            return cmp;
        }
    }

    internal class ATKitInfoComparer : IComparer<ATKitInfo>
    {
        public static ATKitInfoComparer Default = new ATKitInfoComparer();

        public int Compare(ATKitInfo x, ATKitInfo y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            return 0;
        }
    }

    public class ATRefPlanComparer : IComparer<APERefPlan>
    {
        private ATWeightPreset _preset;
        private APEPegPart _pegPart;

        public ATRefPlanComparer(ATWeightPreset preset, APEPegPart pegPart)
        {
            this._preset = preset;
            this._pegPart = pegPart;
        }
        public int Compare(APERefPlan x, APERefPlan y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            int cmp = 0;
            if (this._preset != null)
            {
                foreach (ATWeightFactor factor in this._preset.FactorList)
                {
                    var method = factor.Method as CompareRefProdPlan;

                    if (method == null)
                        continue;

                    ATFactorValue xValue = method(x, factor, _pegPart);
                    ATFactorValue yValue = method(y, factor, _pegPart);
                    
                    cmp = xValue.Value.CompareTo(yValue.Value);

                    if (cmp != 0)
                        return cmp;

                }
            }

            return x.ID.CompareTo(y.ID) * -1;
        }
    }

    public class ATBomComparer : IComparer<ATBom>
    {
        private ATWeightPreset _preset;
        private APEPegPart _pegPart;
        private APESelectBomContext _context;

        public ATBomComparer(ATWeightPreset preset, APEPegPart pegPart, APESelectBomContext context)
        {
            this._preset = preset;
            this._pegPart = pegPart;
            this._context = context;
        }
        public int Compare(ATBom x, ATBom y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            int cmp = 0;

            if (this._preset != null)
            {
                foreach (ATWeightFactor factor in this._preset.FactorList)
                {
                    var method = factor.Method as CompareBom;

                    if (method == null)
                        continue;

                    ATFactorValue xValue = method(x, factor, this._pegPart, _context);
                    ATFactorValue yValue = method(y, factor, this._pegPart, _context);

                    x.AddFactorValue(factor.Name, xValue);
                    y.AddFactorValue(factor.Name, yValue);

                    cmp = xValue.Value.CompareTo(yValue.Value);

                    if (cmp != 0)
                        return cmp;
                }
            }

            cmp = x.Priority.CompareTo(y.Priority);
            if (cmp != 0)
                return cmp;

            cmp = x.BomID.CompareTo(y.BomID);

            return cmp;
        }
    }

    internal class ATBomPriorityComparer : IComparer<ATBom>
    {
        public ATBomPriorityComparer()
        {
        }

        public int Compare(ATBom x, ATBom y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            int cmp = 0;

            cmp = x.Priority.CompareTo(y.Priority);
            if (cmp != 0)
                return cmp;

            cmp = x.BomID.CompareTo(y.BomID);
            return cmp;
        }
    }

    internal class AssemblyPartComparer : IComparer<APEPegPart>
    {
        private ATWeightPreset _preset;
        public AssemblyPartComparer(ATWeightPreset preset)
        {
            this._preset = preset;
        }

        public int Compare(APEPegPart x, APEPegPart y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            int cmp = 0;
            if (this._preset != null)
            {
                foreach (ATWeightFactor factor in this._preset.FactorList)
                {
                    var method = factor.Method as CompareAssemblyPart;

                    if (method == null)
                        continue;

                    ATFactorValue xValue = method(x, factor);
                    ATFactorValue yValue = method(y, factor);

                    cmp = xValue.Value.CompareTo(yValue.Value);

                    if (cmp != 0)
                        return cmp;

                }
            }

            var xSoDemand = x.SampleTarget.SoDemand;
            var ySoDemand = y.SampleTarget.SoDemand;

            return xSoDemand.ID.CompareTo(ySoDemand.ID) * -1;
        }
    }

    internal class ATCalendarDetailComparer : IComparer<ATCalendarDetail>
    {
        private DateTime _now;
        public ATCalendarDetailComparer(DateTime now)
        {
            _now = now;
        }

        public int Compare(ATCalendarDetail x, ATCalendarDetail y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            int cmp = 0;

            // 가용한 패턴
            bool xIsEffective = x.IsEffectiveTime(_now);
            bool yIsEffective = y.IsEffectiveTime(_now);

            cmp = xIsEffective.CompareTo(yIsEffective) * -1;

            if (cmp != 0)
                return cmp;

            // 패턴 우선순위
            double xPriority = x.Priority;
            double yPriority = y.Priority;

            cmp = xPriority.CompareTo(yPriority);
            if (cmp != 0)
                return cmp;

            // 여기서부터는 예외처리
            cmp = x.PatternSeq.CompareTo(y.PatternSeq);
            if (cmp != 0)
                return cmp;

            cmp = x.CalendarID.CompareTo(y.CalendarID);

            return cmp;

        }
    }

    public static class ATDemandCompare
    {
        public static int Compare(ATDemand xSoDemand, ATDemand ySoDemand)
        {
            int cmp = 0;

            // 1. MaxLateDueDate 우선순위
            cmp = xSoDemand.DueDateTime.AddDays(xSoDemand.MaxLateDays).CompareTo(ySoDemand.DueDateTime.AddDays(ySoDemand.MaxLateDays));
            if (cmp != 0)
                return cmp;

            // 2. ItemGrade 우선순위
            cmp = xSoDemand.ItemSiteBuffer.Item.Grade.CompareTo(ySoDemand.ItemSiteBuffer.Item.Grade);
            if (cmp != 0)
                return cmp;

            // 3. SoPriority 우선순위
            cmp = xSoDemand.Priority.CompareTo(ySoDemand.Priority);
            if (cmp != 0)
                return cmp;

            // 4. DueDate 우선순위
            cmp = xSoDemand.DueDateTime.CompareTo(ySoDemand.DueDateTime);
            if (cmp != 0)
                return cmp;

            // 5. SoItem의 최소 누적 TAT 우선 순위
            cmp = xSoDemand.ItemSiteBuffer.GetMinCumTAT(xSoDemand.MaxLateDueDateTime).CompareTo(ySoDemand.ItemSiteBuffer.GetMinCumTAT(ySoDemand.MaxLateDueDateTime)) * -1;
            if (cmp != 0)
                return cmp;

            return cmp;
        }
    }
}
