using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public interface IConstraint
    {
        List<ATConstraintInfo> ConstraintInfos { get; set; }

        List<ATConstraint> Constraints { get; set; }

        bool HasConstraint { get; set; }
    }

    internal static class ConstraintPropertyExtend
    {
        internal static List<ATConstraintDetail> GetConstraintDetails(this IConstraint obj, string applyDate)
        {
            List<ATConstraintDetail> details = new List<ATConstraintDetail>();
            foreach (var constraint in obj.Constraints)
            {
                if (constraint.ConstraintDetails.TryGetValue(applyDate, out var detail))
                    details.Add(detail);
            }

            return details;
        }
    }
}
