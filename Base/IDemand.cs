using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public interface IDemand
    {
        IDemandMaster Master { get; }

        DateTime DueDate { get; }

        double Quantity { get; }

        double Priority { get; }
    }
}
