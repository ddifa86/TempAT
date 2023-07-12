using Mozart.SeePlan.Aleatorik.Data;
using Mozart.Simulation.Engine;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    [FEComponent(FECategory.PlanByForward, FEControl.Bucket, Root = FEProvider.Aleatorik)]
    public class FWBucketControl : IModelController
    {
        #region IModelController 멤버
        Type IModelController.ControllerType
        {
            get { return typeof(FWBucketControl); }
        }
        #endregion


        [FEAction]
        public virtual void OnCreateBucket(PBFResource bucket)
        {
        }

        [FEAction]
        public virtual void OnStartCycle( PBFResource bucket, DateTime now, DateTime nextTime)
        {
           
        }

        [FEAction]
        public virtual void OnEndCycle(PBFResource bucket, DateTime now, DateTime nextTime)
        {

        }

        [FEAction]
        public virtual bool IsNeedSetup(PBFResource bucket, IAPELot lot, ATSetupDetail setupInfo, PBFAllocateContext context)
        {
            if (setupInfo != null)
                return true;

            return false;
        }

        [FEAction]
        public virtual Time GetSetupTime(PBFResource bucket, IAPELot lot, ATSetupDetail setupInfo, PBFAllocateContext context)
        {
            if (setupInfo != null)
                return setupInfo.SetupTime;

            return Time.FromMinutes(0);
        }

    }
}