using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public static class FWInterface
    {
        static readonly Lazy<FWModuleControl> FWModuleControl = new Lazy<FWModuleControl>(() => ServiceLocator.Resolve<FWModuleControl>());
        static readonly Lazy<FWInitControl> FWInitControl = new Lazy<FWInitControl>(() => ServiceLocator.Resolve<FWInitControl>());
        static readonly Lazy<FWFactoryControl> FWLotControl = new Lazy<FWFactoryControl>(() => ServiceLocator.Resolve<FWFactoryControl>());
        static readonly Lazy<FWBucketControl> FWBucketControl = new Lazy<FWBucketControl>(() => ServiceLocator.Resolve<FWBucketControl>());
        static readonly Lazy<FWAllocatorControl> FWAllocatorControl = new Lazy<FWAllocatorControl>(() => ServiceLocator.Resolve<FWAllocatorControl>());
        static readonly Lazy<FWAssemblyControl> FWAssemblyControl = new Lazy<FWAssemblyControl>(() => ServiceLocator.Resolve<FWAssemblyControl>());


        public static FWInitControl InitControl => FWInitControl.Value;
        public static FWModuleControl ModuleControl => FWModuleControl.Value;

        public static FWFactoryControl LotControl => FWLotControl.Value;
        public static FWBucketControl BucketControl => FWBucketControl.Value;

        public static FWAllocatorControl AllocatorControl => FWAllocatorControl.Value;

        public static FWAssemblyControl AssemblyControl => FWAssemblyControl.Value;

    }
}
