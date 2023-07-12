using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.ObyO
{
     
    public static class PBOInterface
    {
        static readonly Lazy<PBOModuleControl> PBOModuleControl = new Lazy<PBOModuleControl>(() => ServiceLocator.Resolve<PBOModuleControl>());
        static readonly Lazy<PBOPegControl> PBOPegControl = new Lazy<PBOPegControl>(() => ServiceLocator.Resolve<PBOPegControl>());
        static readonly Lazy<PBOPlanControl> PBOPlanControl = new Lazy<PBOPlanControl>(() => ServiceLocator.Resolve<PBOPlanControl>());

        static readonly Lazy<PBOInitControl> PBOInitControl = new Lazy<PBOInitControl>(() => ServiceLocator.Resolve<PBOInitControl>());
 
        public static PBOInitControl InitControl => PBOInitControl.Value;

        public static PBOModuleControl ModuleControl => PBOModuleControl.Value;

        public static PBOPegControl PegControl => PBOPegControl.Value;

        public static PBOPlanControl PlanControl => PBOPlanControl.Value;
    }
     
}
