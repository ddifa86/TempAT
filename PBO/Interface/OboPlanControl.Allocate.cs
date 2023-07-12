using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.ObyO.Data;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.ObyO
{
    public partial class PBOPlanControl : IModelController
    {
        //[FEAction]
        //public virtual void DoAllocate(APELot lot)
        //{
        //    throw new InvalidOperationException();
        //}

        //[FEAction(DependentTo = "DoAllocate", DependentType = FEDependencyType.Inclusive)]
        [FEAction]
        public virtual double GetTat(APELot lot)
        {
            return lot.CurrentTarget.TotalTat;
        }

        [FEAction]
        public virtual void OnPrepareVirtualAllocate(APELot lot, PBOAllocateContext context)
        {
            
        }

        [FEAction]
        public virtual bool CheckForAvailableBeforeAllocation(APELot lot, APECapacity capacity, PBOAllocateContext context)
        {
            return true;
        }

        [FEAction]
        public virtual double GetConstraintUsagePer(APELot lot, PBOResource bucket, ATConstraint constraint)
        {
            return 1;
        }

        [FEAction]
        public virtual void OnBucketAllocated(PBOResource bucket, IAPELot lot, APEPlanInfo planInfo, PBOAllocateContext context)
        {

        }

        //[FEAction(DependentTo = "DoAllocate", DependentType = FEDependencyType.Inclusive)]
        [FEAction]
        public virtual void OnCompleteVirtualAllocated(APELot lot, APEPlanInfo planInfo)
        {
        }

        [FEAction]
        public virtual void OnCompleteAllocated(APELot lot, APEPlanInfo planInfo)
        {

        }
         
    }
}
