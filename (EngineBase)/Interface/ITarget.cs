using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public interface ITarget
    {
        ITargetGroup Group { get; }

        DateTime DueDate { get; }

        double Qty { get; set; }

        // double Priority { get; } 필요성 확인 후 추가
    }
}
