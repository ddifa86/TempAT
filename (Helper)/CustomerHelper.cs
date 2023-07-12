using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class CustomerHelper
    {
        private Dictionary<string, ATCustomer> _customers = new Dictionary<string, ATCustomer>();

        public ATCustomer GetCustomer(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            if (_customers.TryGetValue(key, out var value))
                return value;
            else
                return null;
        }

        public bool AddCustomer(ATCustomer obj)
        {
            if (_customers.ContainsKey(obj.CustomerID) == false)
            {
                _customers.Add(obj.CustomerID, obj);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
