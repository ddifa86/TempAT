
using Mozart.SeePlan.Aleatorik.Planner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Mozart.SeePlan.Aleatorik.Data
{
    public partial class APELot : IAPELot, IDisposable
    {
        public void OnCreateLot(APELot lot, APELot orgLot, LotCreateType type)
        {
            FWInterface.LotControl.OnCreateLot(lot, orgLot, type);

            if (type == LotCreateType.SplitByCapacity)
                OutputWriter.Instance.WriteLotHistory(orgLot, orgLot.Qty, type.ToString(), orgLot.LastStepTime, string.Format("Split Lots : {0}({1})", lot.LotID, lot.Qty));
            else if (type == LotCreateType.Creation)
                OutputWriter.Instance.WriteLotHistory(lot, lot.Qty, type.ToString(), lot.LastStepTime, lot.LotID);
        }
    }
}
