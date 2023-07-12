using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Inputs;

namespace Mozart.SeePlan.Aleatorik
{
    public class WipHelper
    {
        private static Dictionary<string, ATWipInfo> _wipInfos = new Dictionary<string, ATWipInfo>();
        private static Dictionary<string, APEWip> _planWips = new Dictionary<string, APEWip>();
        private static HashSet<WIP> _unpegWips = new HashSet<WIP>();
        private static Dictionary<WIP, ATUnpegReason> _persistUnpegWips = new Dictionary<WIP, ATUnpegReason>();

        public IEnumerable<ATWipInfo> GetWipInfos()
        {
            return _wipInfos.Values;
        }

        public HashSet<WIP> GetUnpegWips()
        {
            return _unpegWips;
        }

        public ATWipInfo GetWipInfo(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            if (_wipInfos.TryGetValue(key, out var value))
                return value;
            else
                return null;
        }

        public bool AddWipInfo(ATWipInfo obj)
        {
            if (_wipInfos.ContainsKey(obj.LotID) == false)
            {
                _wipInfos.Add(obj.LotID, obj);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void AddUnpegWips(WIP wip)
        {
            _unpegWips.Add(wip);
        }

        public IEnumerable<APEWip> GetPlanWips()
        {
            return _planWips.Values;
        }

        public bool AddPlanWip(APEWip obj)
        {
            if (_planWips.ContainsKey(obj.WipInfo.LotID) == false)
            {
                _planWips.Add(obj.WipInfo.LotID, obj);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool AddPersistUnpegWip(WIP wip, ATUnpegReason reason)
        {
            if (_persistUnpegWips.ContainsKey(wip) == false)
            {
                _persistUnpegWips.Add(wip, reason);
                return true;
            }

            return false;
        }

        public Dictionary<WIP, ATUnpegReason> GetPersistUnpegWip()
        {
            return _persistUnpegWips;
        }
    }
}
