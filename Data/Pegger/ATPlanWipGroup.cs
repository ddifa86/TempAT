using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class ATKitInfo
    {
        public string Keys { get; private set; }

        public List<APEWip> Items { get; private set; }

        public ATKitInfo(List<string> keys, HashSet<APEWip> wips) 
        {
            foreach (var key in keys)
            {
                if (string.IsNullOrEmpty(this.Keys))
                    this.Keys = key;
                else
                    this.Keys += "/" + key;
            }

            this.Items = wips.ToList();
        }
    }
}
