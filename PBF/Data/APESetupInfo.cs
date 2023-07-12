using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Planner;
using Mozart.Simulation.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public class FWSetupInfo
    {
        public PBFResource Bucket { get; internal set; }

        public Time SetupTime { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public PBFResource SetupResource { get; set; }

        public ATSetupDetail OrgSetupInfo { get; set; }

        public double NonWorkCapa { get; internal set; }

        public dynamic Property { get; internal set; }

        public bool HasSetupResource
        {
            get
            {
                return this.SetupResource != null;
            }
        }

        public FWSetupInfo(PBFResource bucket, Time setupTime, ATSetupDetail setupInfo) 
        {
            this.Bucket = bucket;
            this.SetupTime = setupTime;
            this.StartTime = bucket.CurrentTime;
            this.EndTime = bucket.CurrentTime + (TimeSpan)setupTime;
            this.SetupResource = GetSetupResource(setupInfo);
            this.OrgSetupInfo = setupInfo;
            this.Property = new DynamicDictionary();
        }

        public PBFResource GetSetupResource(ATSetupDetail info)
        {
            if (info != null && info.HasSetupResource)
            {
                List<PBFResource> setupResources = info.SetupResources.Select(x => x.Bucket as PBFResource).ToList();
                setupResources.Sort(SetupResourceComparer.Default);

                return setupResources.First();
            }

            return null;
        }

        public FWSetupInfo DeepCopy()
        {
            FWSetupInfo setupInfo = (FWSetupInfo)this.MemberwiseClone();

            return setupInfo;
        }

    }
}
