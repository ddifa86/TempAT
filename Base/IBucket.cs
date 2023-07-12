using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public interface IBucket
    {
        string BucketID { get; }

        ATResource Target { get; }

        CapacityType CapacityType { get; }

        List<APEPlanInfo> Plans { get; }

        void AddPlan(APEPlanInfo info, IAPELot lot);
        // List<APEPlanInfo> UsedPlanInfos { get; }

        //DateTime CurrentTime { get;  }

        //DateTime StartTime { get; }

        //DateTime EndTime { get; }

        //double Capacity { get; }

        //double UsedQty { get; }

        //double RemainQty 
        //{
        //    get;
        //}
    }
}
