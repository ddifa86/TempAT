using Mozart.Collections;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Inputs;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class DemandHelper
    {
        public double MaxPriority { get; set; }

        public Dictionary<string, ATDemand> _demandsBySO = new Dictionary<string, ATDemand>();
        public Dictionary<string, HashSet<ATDemand>> _demandsByItem = new Dictionary<string, HashSet<ATDemand>>();
        internal HashSet<DEMAND> _rowDemand = new HashSet<DEMAND>();

        public ATDemand GetDemandByID(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            if (_demandsBySO.TryGetValue(value, out var result))
                return result;
            else
                return null;
        }

        public HashSet<ATDemand> GetDemandsByItem(string value, bool isAll)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            if (_demandsByItem.TryGetValue(value, out var so))
            {
                if (isAll)
                    return so;
                else
                    return new HashSet<ATDemand>() { so.FirstOrDefault() };
            }
            else
                return null;
        }

        public IEnumerable<ATDemand> GetDemands()
        {
            return _demandsBySO.Values;
        }

        public bool AddDemand(ATDemand obj)
        {
            bool isSave = true;

            if (_demandsBySO.TryGetValue(obj.ID, out var value))
                isSave = false;
            else
                _demandsBySO.Add(obj.ID, obj);

            HashSet<ATDemand> demands;
            if (_demandsByItem.TryGetValue(obj.ItemID, out demands) == false)
            {
                demands = new HashSet<ATDemand>();
                _demandsByItem.Add(obj.ItemID, demands);
            }

            demands.Add(obj);

            this.MaxPriority = Math.Max(this.MaxPriority, obj.Priority);

            return isSave;
        }

        public void AddRowDemand(DEMAND entity)
        {
            _rowDemand.Add(entity);
        }

    }
}
