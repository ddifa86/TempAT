using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class APEWipQueue
    {
        public ATOperation Key { get; private set; }

        public Dictionary<string, List<APEWip>> RunWipSets { get; private set; }

        public Dictionary<string, List<APEWip>> WaitWipSets { get; private set; }

        public APEWipQueue(ATOperation key)
        {
            this.Key = key;

            APEWipQueueAgent.Instance.AddPlanWipQueue(this);
        }

        internal void Initialize()
        {
           this.RunWipSets = new Dictionary<string, List<APEWip>>();
           this.WaitWipSets = new Dictionary<string, List<APEWip>>();
        }

        internal void Dispose()
        {
           this.RunWipSets = null;
           this.WaitWipSets = null;
        }

        public void AddWipSet(string key, APEWip wip, ATWeightPreset compareWipPreset = null)
        {
            if (key == null)
                return;

            List<APEWip> lst;
            var wipSets = wip.State == LotState.Run ? this.RunWipSets : this.WaitWipSets;

            if (wipSets.TryGetValue(key, out lst) == false)
            {
                lst = new List<APEWip>();

                wipSets.Add(key, lst);
            }

            if (compareWipPreset != null)
                lst.AddSort(wip, new ATPlanWipComparer(compareWipPreset));
            else
                lst.Add(wip);
        }

        public void RemoveWipSet(APEWip wip)
        {
            var wipSets = wip.State == LotState.Run ? this.RunWipSets : this.WaitWipSets;

            foreach (var key in wip.Keys)
            {
                if (wipSets.TryGetValue(key, out var lst))
                    lst.Remove(wip);
            }
        }

        public List<APEWip> GetWipSet(string key, bool isRun)
        {
            var wipSets = isRun ? this.RunWipSets : this.WaitWipSets;

            if (wipSets.ContainsKey(key))
                return wipSets[key];

            return new List<APEWip>();
        }
    }
}
