using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class ConstraintHelper
    {
        private static Dictionary<string, ATConstraintInfo> _constraintInfos = new Dictionary<string, ATConstraintInfo>();
        private static Dictionary<string, List<ATConstraintInfo>> _constraintInfoByCondition = new Dictionary<string, List<ATConstraintInfo>>();

        public static bool AddConstraintInfo(ATConstraintInfo obj)
        {
            string key = obj.ConstraintID;

            if (_constraintInfos.ContainsKey(key))
                return false;
            else
                _constraintInfos.Add(key, obj);
            
            string propKey = obj.PropertyID + "@" + obj.Condition;
            List<ATConstraintInfo> constInfos;
            if (_constraintInfoByCondition.TryGetValue(propKey, out constInfos) == false)
            {
                constInfos = new List<ATConstraintInfo>();
                _constraintInfoByCondition.Add(propKey, constInfos);
            }

            constInfos.Add(obj);

            return true;
        }

        public static List<ATConstraintInfo> GetConstraintsInfos()
        {
            return _constraintInfos.Values.ToList();
        }

        public static ATConstraintInfo GetConstraintInfo(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            ATConstraintInfo info;
            if (_constraintInfos.TryGetValue(key, out info))
                return info;

            return null;
        }

        public static List<ATConstraintInfo> GetConstraintInfoByCondition(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            List<ATConstraintInfo> infos;
            if (_constraintInfoByCondition.TryGetValue(key, out infos))
                return infos;

            return null;
        }
    }
}
