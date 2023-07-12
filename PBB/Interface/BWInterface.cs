using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Pegger
{
    public static class PBBInterface
    {
        static readonly Lazy<PBBInitControl> PBBInitControl = new Lazy<PBBInitControl>(() => ServiceLocator.Resolve<PBBInitControl>());
        static readonly Lazy<PBBModuleControl> PBBModuleControl = new Lazy<PBBModuleControl>(() => ServiceLocator.Resolve<PBBModuleControl>());
        static readonly Lazy<PBBPeggerControl> PBBPeggerControl = new Lazy<PBBPeggerControl>(() => ServiceLocator.Resolve<PBBPeggerControl>());

        public static PBBInitControl InitControl => PBBInitControl.Value;

        public static PBBModuleControl ModuleControl => PBBModuleControl.Value;

        public static PBBPeggerControl PegControl => PBBPeggerControl.Value;
    }
}
