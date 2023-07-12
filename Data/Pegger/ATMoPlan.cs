using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Pegging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    //[Mozart.Task.Execution.FEBaseClassAttribute(Root = "BCP", Category = "BCP", IsTypeBinding = true, Mandatory = true, Description = null)]
    public class ATMoPlan : MoPlan
    {
        public ATMoMaster Mo { get; private set; }
        public ATDemand Demand { get; private set; }

        public ATDemand SODemand
        {
            get
            {
                return this.Demand.SoDemand;
            }
        }

        public string ID
        {
            get
            {
                return this.Demand.ID;
            }
        }

      

        public ATMoPlan(ATMoMaster mm, ATDemand demand, DateTime dueDate)
            : base( demand.Qty, dueDate)
        {
            this.Demand = demand;
            this.Mo = mm;

            this.Priority = demand.Priority;
        }

    }
}
