
using Mozart.SeePlan.Aleatorik.Inputs;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik
{
    public class AllocationGroupHelper
    {
        private Dictionary<string, ATAllocationGroup> _allocationGroups = new Dictionary<string, ATAllocationGroup>();

        public ATAllocationGroup GetAllocationGroup(string allocationGroupID)
        {
            if (string.IsNullOrEmpty(allocationGroupID))
                return null;

            if (_allocationGroups.TryGetValue(allocationGroupID, out var value))
                return value;
            else
                return null;
        }

        public bool AddAlloctionGroup(ATAllocationGroup obj)
        {
            if (_allocationGroups.ContainsKey(obj.AllocationGroupID))
                return false;

            _allocationGroups.Add(obj.AllocationGroupID, obj);
            return true;
        }
    }
}
