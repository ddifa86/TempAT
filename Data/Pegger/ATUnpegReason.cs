using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class ATUnpegReason
    {
        public UnpegCategory Category { get; set; }

        public string Reason { get; set; }

        public string ReasonDetail { get; set; }

        public ATUnpegReason( )
        {
        }

        public ATUnpegReason(UnpegCategory category, string reason, string reasonDetail)
        {
            this.Category = category;
            this.Reason = reason;
            this.ReasonDetail = reasonDetail;
        }
    }
}
