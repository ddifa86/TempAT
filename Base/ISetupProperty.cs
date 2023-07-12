using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public interface ISetupProperty
    {
        Dictionary<string, string> SetupProperty { get; }
    }

    internal static class SetupPropertyExtend
    {
        internal static string GetSetupProperty(this ISetupProperty obj, string key)
        {
            if (obj.SetupProperty.TryGetValue(key, out string value))
                return value;

            return null;
        }

        internal static void SetSetupProperty(this ISetupProperty obj, string key, string value)
        {
            if (obj.SetupProperty.ContainsKey(key) == false)
                obj.SetupProperty.Add(key, value);
            else
                obj.SetupProperty[key] = value;
        }
    }
}
