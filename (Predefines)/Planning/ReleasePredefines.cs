using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class ReleasePredefines
    {
        public static ReleasePredefines Instance
        {
            get
            {
                return ServiceLocator.Resolve<ReleasePredefines>();
            }
        }
    }
}
