using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class RefPlanHelper
    {
        private Dictionary<string, APERefPlan> _refPlans = new Dictionary<string, APERefPlan>();

        public void AddRefPlan(APERefPlan refPlan)
        {
            if (_refPlans.ContainsKey(refPlan.ID) == false)
                _refPlans.Add(refPlan.ID, refPlan);
        }

        public IEnumerable<APERefPlan> GetRefPlans()
        {
            return _refPlans.Values;
        }
    }
}
