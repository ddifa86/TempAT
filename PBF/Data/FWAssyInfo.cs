using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Planner.Data
{
    public class FWAssyInfo : IAssemblyKey
    {
        public string Key
        {
            get
            {
                return this.CurrentBom.BomID + this.Demand.ID;
            }
        }

        public ATBom CurrentBom { get; private set; }

        public ATDemand Demand { get; private set; }

        public FWAssyInfo(ATBom currentBom, ATDemand demand)
        {
            this.CurrentBom = currentBom;
            this.Demand = demand;
        }
    }
}
