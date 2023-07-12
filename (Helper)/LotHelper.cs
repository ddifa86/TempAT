
using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
 
    // 여기 있을 놈이 아닌 듯...
    static public partial class LotHelper
    {
        //...
        private static int _internalSequence = 0;

        private static int _binnedWipCreationSequence = 1;

        internal static void InitGenerateIdx()
        {
            _internalSequence = 0;
            _binnedWipCreationSequence = 1;
        }

        public static string GeneratLotID(string prefix, string id)
        {
            var lotID = prefix + "_" + id + _internalSequence++;

            return lotID;
        }

        public static int GetBinnedWipCreationSequence()
        {
            return _binnedWipCreationSequence++;
        }


        public static APELot GenerateSplitLot(APELot orglot, double qty, string lotID, bool copyHistory)
        {
            if (qty <= 0 || qty >= orglot.Qty || (orglot.CurrentQty > 0 && qty > orglot.CurrentQty))
                return null;

            var splitLot = orglot.Clone() as APELot;

            splitLot.OrgLot = orglot;
            splitLot.LotID = lotID;
            splitLot.Qty = qty;
            splitLot.CurrentQty = qty;
            splitLot.CurrentTarget = orglot.CurrentTarget;

            orglot.Qty -= qty;
            if (orglot.CurrentQty > 0)
                orglot.CurrentQty -= qty;

            if (copyHistory == false)
            {
                splitLot.Plans = new List<APEPlanInfo>();
                splitLot.CapaPlans = new List<APEPlanInfo>();
                splitLot.VirtualPegWips = new List<APEWip>();
                splitLot.SplitInfos = new List<ATSplitInfo>();
                splitLot.AssemblyHistory = new List<ATAssemblyInfo>();
                splitLot.RefPlans = new HashSet<APERefPlan>();
                splitLot.VirtualLateInfos = new Dictionary<string, ATLateInfo>();

                splitLot.OnCreateLot(splitLot, orglot, LotCreateType.SplitByCapacity);
            }

            return splitLot;
        }

        public static APELot GenerateSplitLot(APELot orglot, double qty)
        {
            if (qty <= 0 || (orglot.CurrentQty > 0 && qty > orglot.CurrentQty))
                return null;

            var orgQty = orglot.Qty;

            var splitLot = orglot.Clone() as APELot;

            splitLot.OrgLot = orglot;
            splitLot.LotID = LotHelper.GeneratLotID(orglot.LotID, ATConstants.SPLIT_BATCH_PREFIX);
            splitLot.Qty = qty;
            splitLot.CurrentQty = qty;
            splitLot.CurrentTarget = orglot.CurrentTarget;

            splitLot.Plans = new List<APEPlanInfo>(orglot.Plans);
            splitLot.CapaPlans = new List<APEPlanInfo>(orglot.CapaPlans);
            splitLot.VirtualPegWips = new List<APEWip>(orglot.VirtualPegWips);
            splitLot.SplitInfos = new List<ATSplitInfo>(orglot.SplitInfos);
            splitLot.AssemblyHistory = new List<ATAssemblyInfo>(orglot.AssemblyHistory);
            splitLot.RefPlans = new HashSet<APERefPlan>(orglot.RefPlans);
            splitLot.OrgLotKeys = new HashSet<string>(orglot.OrgLotKeys);
            
            orglot.Qty -= qty;
            if (orglot.CurrentQty > 0)
                orglot.CurrentQty -= qty;

            OutputWriter.Instance.WriteLotHistory(orglot, orgQty, LotCreateType.SplitByCapacity.ToString(), orglot.LastStepTime, string.Format("Split Lots : {0}({1})", splitLot.LotID, splitLot.Qty));

            return splitLot;
        }

    }
}
