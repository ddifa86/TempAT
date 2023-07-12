using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.ObyO.Data;
using Mozart.SeePlan.Aleatorik.Planner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public interface IConstraintManager
    {
    }

    internal static class ConstraintManagerExtend
    {
        internal static List<ATConstraintDetail> GetConstraintDetails(this IConstraintManager obj, string applyDate)
        {
            List<ATConstraintDetail> details = new List<ATConstraintDetail>();

            if (obj is APELot)
            {
                var lot = obj as APELot;

                if (lot.Item != null && lot.Item.HasConstraint)
                    details.AddRange(lot.Item.GetConstraintDetails(applyDate));

                if (lot.CurrentItemSiteBuffer.Site != null && lot.CurrentItemSiteBuffer.Site.HasConstraint)
                    details.AddRange(lot.CurrentItemSiteBuffer.Site.GetConstraintDetails(applyDate));

                if (lot.CurrentItemSiteBuffer.Buffer != null && lot.CurrentItemSiteBuffer.Buffer.HasConstraint)
                    details.AddRange(lot.CurrentItemSiteBuffer.Buffer.GetConstraintDetails(applyDate));

                if (lot.CurrentItemSiteBuffer != null && lot.CurrentItemSiteBuffer.HasConstraint)
                    details.AddRange(lot.CurrentItemSiteBuffer.GetConstraintDetails(applyDate));

                if (lot.CurrentTarget.SODemand != null && lot.CurrentTarget.SODemand.HasConstraint)
                    details.AddRange(lot.CurrentTarget.SODemand.GetConstraintDetails(applyDate));

                if (lot.CurrentTarget.SODemand.Customer != null && lot.CurrentTarget.SODemand.Customer.HasConstraint)
                    details.AddRange(lot.CurrentTarget.SODemand.Customer.GetConstraintDetails(applyDate));

                if (lot.CurrentBom != null && lot.CurrentBom.HasConstraint)
                    details.AddRange(lot.CurrentBom.GetConstraintDetails(applyDate));

                if (lot.CurrentOper != null && lot.CurrentOper.HasConstraint)
                    details.AddRange(lot.CurrentOper.GetConstraintDetails(applyDate));

                if (lot.CurrentArrange != null &&lot.CurrentArrange.HasConstraint)
                    details.AddRange(lot.CurrentArrange.GetConstraintDetails(applyDate));

                if (lot.Wip != null && lot.Wip.WipInfo.HasConstraint)
                    details.AddRange(lot.Wip.WipInfo.GetConstraintDetails(applyDate));
            }
            else if (obj is IBucket)
            {
                var bucket = obj as IBucket;
                details.AddRange(bucket.Target.GetConstraintDetails(applyDate));
            }

            details = details.OrderBy(x => x.RemainQty).ToList();
            return details;
        }
    }
}
