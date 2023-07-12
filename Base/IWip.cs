using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    interface IWip
    {
        double Quantity { get; }

        double CurrentQty { get; }

        void Use(double quantity);
    }
}
