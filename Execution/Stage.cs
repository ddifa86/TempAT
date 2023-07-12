using Mozart.SeePlan.Aleatorik.Data;
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    //[Mozart.Task.Execution.FEBaseClassAttribute(Root = "PKG", Category = "PKG", IsTypeBinding = true, Mandatory = true, Description = null)]
    public partial class Stage
    {
        #region Property
        public string StageID { get; private set; }

        public string Description { get; private set; }

        #endregion

        #region Stage Managing Data

        /// <summary>
        /// Stage 내부 Buffer 구성 정보
        /// Key : BufferID
        /// </summary>
        public Dictionary<string, ATBuffer> Buffers { get; private set; }


        /// <summary>
        /// Stage 내 Buffer간 Route
        /// </summary>
        public ATRoute BufferRoute { get; private set; }

        /// <summary>
        /// Stage 내 존재하는 Resource 정보
        /// </summary>
        public List<ATResource> Resources { get; private set; }

        /// <summary>
        /// Stage 내 Buffer별 할당 그룹 정보
        /// 
        /// </summary>
        public SortedDictionary<ATAllocationGroup, ATAllocationGroup> AllocationGroups { get; private set; }

        /// <summary>
        /// Stage 내부 활용 Demand 정보
        /// Key : DemandID
        /// </summary>
        /// 
        public Dictionary<string, ATDemand> Demands { get; private set; }
             
        #endregion

        public Stage(string stageID, string description)
        {
            this.StageID = stageID;
            this.Description = description;
            this.Buffers = new Dictionary<string, ATBuffer>();
            this.AllocationGroups = new SortedDictionary<ATAllocationGroup, ATAllocationGroup>(AllocationGroupComparer.Default);
            this.Demands = new Dictionary<string, ATDemand>();
            this.BufferRoute = new ATRoute(this.StageID, DateTime.MinValue, DateTime.MaxValue);
            this.Resources = new List<ATResource>();
        }

        public void AddAllocationGroup(ATAllocationGroup allocgroup)
        {
            if (this.AllocationGroups.ContainsKey(allocgroup) == true)
                return;

            this.AllocationGroups.Add(allocgroup, allocgroup);
        }

      

        #region Inner Class
        internal class AllocationGroupComparer : IComparer<ATAllocationGroup>
        {
            public static AllocationGroupComparer Default = new AllocationGroupComparer();

            public int Compare(ATAllocationGroup x, ATAllocationGroup y)
            {
                if (object.ReferenceEquals(x, y))
                    return 0;

                var cmp = x.Sequence.CompareTo(y.Sequence);
                if (cmp == 0)
                    cmp = x.AllocationGroupID.CompareTo(y.AllocationGroupID);

                return cmp;
            }
        }

       
        #endregion

    }
}
