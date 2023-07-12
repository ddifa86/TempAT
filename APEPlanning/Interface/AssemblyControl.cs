using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class AssemblyInterface
    {
        public virtual double AdjustAssemblyQty(APELot lot, double canAssemblyQty )
        {
            return canAssemblyQty;
        }

        public virtual void OnArriveInAssemblyStep(APELot lot, ATOperTarget target)
        {
            return;
        }

        public virtual void OnCompleteAssembled(APELot lot, ATAssemblyInfo assyInfo)
        {
            return;
        }
    }
}
