using Mozart.SeePlan.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public interface IFactorObject
    {

        string FactorObjectKey { get; }

        Dictionary<string, ATFactorValue> FactorInfos { get; set; }

        Dictionary<string, ATFilterValue> FilterInfos { get; set; }

        string FactorValues { set; get; }

        string FilterValues { set; get; }

        string InitFilterLogs();

        string InitFactorLogs();
    }

    internal static class FactorObjectExtend
    {
        static internal void InitFactorValue(this IFactorObject obj)
        {
            obj.FactorInfos.Clear();
            obj.FilterInfos.Clear();
            obj.FilterValues = obj.InitFilterLogs();
            obj.FactorValues = obj.InitFactorLogs();
        }

        static internal bool AddFactorValue(this IFactorObject obj, string factorName, ATFactorValue value)
        {
            if (obj.FactorInfos.ContainsKey(factorName) == false)
            {
                obj.FactorInfos.Add(factorName,  value);

                string strValue = string.IsNullOrEmpty(value.Unit) == false ? "(" + value.Unit + ")" : string.Empty;
                obj.FactorValues += string.Format("^{0};{1};{2}", factorName, value.Value + strValue, value.Description);

                return true;
            }

            return false;
        }

        static internal bool AddFilterValue(this IFactorObject obj,  string factorName, ATFilterValue value)
        {
            if (obj.FilterInfos.ContainsKey(factorName) == false)
            {
                obj.FilterInfos.Add(factorName, (ATFilterValue)value);
                obj.FilterValues += string.Format("^{0};{1};{2}", factorName, value.Value, value.Description);

                return true;
            }

            return false;
        }

        static internal string GetFactorLog(this List<IFactorObject> lst)
        {
            var factorLog = new StringBuilder();

            foreach (var obj in lst)
            {
                string curLog = obj.FactorValues;

                if (factorLog.Length != 0)
                    factorLog.AppendLine();

                factorLog.Append(curLog);
            }

            return factorLog.ToString();
        }

        static internal string GetFilterLog(this List<IFactorObject> lst)
        {
            var filterLog = new StringBuilder();

            foreach (var obj in lst)
            {
                string curLog = obj.FilterValues;

                if (filterLog.Length != 0)
                    filterLog.AppendLine();
                
                filterLog.Append(curLog);
            }

            return filterLog.ToString();
        }

        static internal double GetWeightedSum(this IFactorObject obj)
        {
            return obj.FactorInfos.Sum(x => x.Value.Value);
        }
    }
}
