using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Planner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class SetupHelper
    {
        private Dictionary<string, ATSetupInfo> _setupInfos = new Dictionary<string, ATSetupInfo>();
        private HashSet<string> _setupProperties = new HashSet<string>() { ATReservedCode.ITEM_ID, ATReservedCode.BOM_ID, ATReservedCode.ROUTING_ID, ATReservedCode.OPER_ID };
        private char[] _separator = { '|', '&' };

        public string GetUsedSeparator(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            return string.Join("", value.Where(x => _separator.Contains(x)));
        }

        public bool IsReservedSetupProperty(string property)
        {
            if (_setupProperties.Contains(property))
                return true;
            else
                return false;
        }

        public List<string> GetSetupValues(string condition)
        {
            var values = condition.Split(_separator).ToList();
            return values;
        }

        public HashSet<string> GetSetupProperties(string setupType)
        {
            HashSet<string> values = new HashSet<string>();

            if (string.IsNullOrEmpty(setupType))
                return values;

            var types = setupType.Split(_separator);
            types.ForEach(x => values.Add(x));

            return values;
        }

        public void AddSetupInfo(ATSetupDetail obj, HashSet<string> setupProperties)
        {
            ATSetupInfo info;
            if (_setupInfos.TryGetValue(obj.SetupID, out info) == false)
            {
                info = new ATSetupInfo(obj.SetupID);
                _setupInfos.Add(obj.SetupID, info);
            }
            
            info.SetupInfos.AddSort(obj, ATSetupInfoComparer.Default);
            info.Properties.AddRange(setupProperties);
        }

        public ATSetupInfo GetSetupInfos(string setupID)
        {
            if (string.IsNullOrEmpty(setupID))
                return null;

            ATSetupInfo info;
            if (_setupInfos.TryGetValue(setupID, out info))
                return info;

            return null;
        }
    }

    

    internal class ATSetupInfoComparer : IComparer<ATSetupDetail>
    {
        public static ATSetupInfoComparer Default = new ATSetupInfoComparer();

        public int Compare(ATSetupDetail x, ATSetupDetail y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            int cmp = x.Priority.CompareTo(y.Priority);
            return cmp;
        }
    }
}
