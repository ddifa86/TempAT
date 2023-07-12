using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Planner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Manager
{
    /// <summary>
    /// 예약 정보입니다.
    /// </summary>
    public class ReserveInfo
    {
        public IAPELot ReservedLot { get; private set; }

        public FWSetupInfo SetupInfo { get; set; }

        public List<ATOperResource> AddArrInfos { get; set; }

        public ReserveInfo(IAPELot lot, List<ATOperResource> addArrInfos, FWSetupInfo setupInfo)
        {
            this.ReservedLot = lot;
            this.AddArrInfos = addArrInfos;
            this.SetupInfo = setupInfo;
        }
    }
}
