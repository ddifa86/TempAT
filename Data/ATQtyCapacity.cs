using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class ATQtyCapacity : APECapacity
    {
        #region value

        public ATOperResource CurrentArrange { get; set; }

        public override double Remain
        {
            get
            {
                return Capacity - UsedCapa - VirtualUsedQty;
            }
        }

        #endregion

        public ATQtyCapacity(ATCapacityInfo info, IBucket bucket)
            :base(info, bucket)
        {
        }
        #region method

        #endregion
    }
}
