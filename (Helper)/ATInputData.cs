using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class ATInputData
    {
        //input datas
        static readonly Lazy<BomHelper> bomHelper = new Lazy<BomHelper>(() => ServiceLocator.Resolve<BomHelper>());
        static readonly Lazy<ATPropertyHelper> propertyHelper = new Lazy<ATPropertyHelper>(() => ServiceLocator.Resolve<ATPropertyHelper>());
        static readonly Lazy<CalendarHelper> calendarHelper = new Lazy<CalendarHelper>(() => ServiceLocator.Resolve<CalendarHelper>());
        static readonly Lazy<AllocationGroupHelper> allocationGroupHelper = new Lazy<AllocationGroupHelper>(() => ServiceLocator.Resolve<AllocationGroupHelper>());
        static readonly Lazy<CustomerHelper> customerHelper = new Lazy<CustomerHelper>(() => ServiceLocator.Resolve<CustomerHelper>());
        static readonly Lazy<DemandHelper> demandHelper = new Lazy<DemandHelper>(() => ServiceLocator.Resolve<DemandHelper>());
        static readonly Lazy<RefPlanHelper> refPlanHelper = new Lazy<RefPlanHelper>(() => ServiceLocator.Resolve<RefPlanHelper>());
        static readonly Lazy<ItemSiteBufferHelper> itemSiteBufferHelper = new Lazy<ItemSiteBufferHelper>(() => ServiceLocator.Resolve<ItemSiteBufferHelper>());
        static readonly Lazy<ResourceHelper> resourceHelper = new Lazy<ResourceHelper>(() => ServiceLocator.Resolve<ResourceHelper>());
        static readonly Lazy<SetupHelper> setupHelper = new Lazy<SetupHelper>(() => ServiceLocator.Resolve<SetupHelper>());
        static readonly Lazy<WipHelper> wipHelper = new Lazy<WipHelper>(() => ServiceLocator.Resolve<WipHelper>());

        #region helpers
        // ATPersistHelper
        // RuleBuilder (?)
        #endregion

        #region objects
        // LotHelper
        // OperTargetHelper
        // RuleFactor(?)
        // RuleFactorAttribute(?)
        #endregion

        //input datas 
        public static BomHelper Boms => bomHelper.Value;
        public static ATPropertyHelper Properties => propertyHelper.Value;
        public static CalendarHelper Calendars => calendarHelper.Value;
        public static AllocationGroupHelper AllocationGroups => allocationGroupHelper.Value;
        public static CustomerHelper Customers => customerHelper.Value;
        public static DemandHelper Demands => demandHelper.Value;
        public static RefPlanHelper RefPlans => refPlanHelper.Value;
        public static ItemSiteBufferHelper ItemSiteBuffers => itemSiteBufferHelper.Value;
        public static ResourceHelper Resources => resourceHelper.Value;
        public static SetupHelper Setups => setupHelper.Value;
        public static WipHelper Wips => wipHelper.Value;

    }
}
