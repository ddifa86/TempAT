using Mozart.Data.Entity;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public static class ATPersistHelper
    {
        public delegate bool ConfigDelegate(ATInputErrorConfig config, object entity);

        public static bool HasErrorData = false;

        public static StringBuilder ErrorStr = new StringBuilder();

        // INPUT_ERROR_CONFIG 관련 메서드
        //public static Dictionary<string, List<ATInputErrorConfig>> InputErrorConfig = new Dictionary<string, List<ATInputErrorConfig>>();

        //internal static HashedSet<string> GetTableInfo(string key)
        //{
        //    HashedSet<string> columns;
        //    if (TableInfo.TryGetValue(key, out columns))
        //        return columns;

        //    return null;
        //}

        //public static bool ValidateData(object entity, ATInputErrorConfig config)
        //{
        //    ConfigDelegate ConfigMethod = null;
        //    Type type = entity.GetType();

        //    ErrorReasonCode reason = config.ReasonCode; 

        //    switch (reason)
        //    {
        //        case ErrorReasonCode.OutOfRange:
        //            ConfigMethod = OutOfRange;
        //            break;
        //        case ErrorReasonCode.NotFoundReferredData:
        //            ConfigMethod = MissingData;
        //            break;
        //        case ErrorReasonCode.Null:
        //            ConfigMethod = NullData;
        //            break;
        //        case ErrorReasonCode.MismatchReservedWord:
        //            ConfigMethod = NonReservedWord;
        //            break;
        //        case ErrorReasonCode.DataTypeMisMatch:
        //            ConfigMethod = DataTypeMismatch;
        //            break;
        //        default:
        //            break;
        //    }

        //    if (ConfigMethod == null)
        //        return true;

        //    if (!ConfigMethod(config, entity))
        //    {
        //        OutputWriter.Instance.WriteErrorLog(config, entity, reason);

        //        switch (config.LogLevel)
        //        {
        //            case ErrorSeverity.Info:
        //                if (reason == ErrorReasonCode.OutOfRange)
        //                    type.GetProperty(config.ColumnName).SetValue(entity, 0);
        //                return true;

        //            case ErrorSeverity.Warning:
        //                return false;

        //            case ErrorSeverity.Error:
        //                HasErrorData = true;
        //                return false;
        //            default:
        //                break;
        //        }
        //        return false;
        //    }

        //    return true;
        //}

        //public static bool OutOfRange(ATInputErrorConfig config, object entity)
        //{
        //    Type ty = entity.GetType();
        //    PropertyInfo propertyInfo = ty.GetProperty(config.ColumnName);
        //    var value = propertyInfo.GetValue(entity);

        //    double target;

        //    // OutOfRange에 double로 converting 불가능한 값이 넘어온 경우??
        //    if (!double.TryParse(value.ToString(), out target))
        //    {
        //        return false;
        //    }

        //    List<LogicalOperationType> operatorList = new List<LogicalOperationType>();
        //    List<bool> valueList = new List<bool>();
        //    var parameter = config.Parameter.Replace(" ", "").Split(',');

        //    for (int i = 0; i < parameter.Length; i++)
        //    {
        //        if (i % 2 == 0)
        //        {
        //            var param = parameter[i].Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);

        //            // 연산자를 이상하게 입력한 경우
        //            if (param.Count() != 2)
        //                return false;

        //            //사전 정의되지 않은 비교연산자를 입력한 경우
        //            ComparisonOperatorType oper = ATUtil.StringToEnum<ComparisonOperatorType>(param[0], ComparisonOperatorType.None);
        //            if (oper == ComparisonOperatorType.None)
        //                return false;

        //            // [] 사이에 Converting 되지 않는 값을 입력한 경우
        //            if (!double.TryParse(param[1], out double compare))
        //                return false;

        //            valueList.Add(CalcOperator(oper, compare, target));
        //        }
        //        else
        //        {
        //            LogicalOperationType oper = ATUtil.StringToEnum<LogicalOperationType>(parameter[i], LogicalOperationType.None);
        //            if (oper == LogicalOperationType.None)
        //                return false;

        //            operatorList.Add(oper);
        //        }
        //    }

        //    while (valueList.Count >= 2)
        //    {
        //        var result = CalcOperator(valueList[0], valueList[1], operatorList[0]);
        //        valueList.RemoveRange(0, 2);
        //        operatorList.RemoveRange(0, 1);
        //        valueList.Insert(0, result);
        //    }

        //    return valueList.FirstOrDefault();
        //}

        //public static bool MissingData(ATInputErrorConfig config, object entity)
        //{
        //    Type ty = entity.GetType();
        //    PropertyInfo propertyInfo = ty.GetProperty(config.ColumnName);
        //    var value = propertyInfo.GetValue(entity);

        //    if (value != null)
        //    {
        //        var key = config.RefTableName + "@" + config.RefColumnName;

        //        if (!DataDictionary[key].ContainsKey(value))
        //            return false;
        //    }

        //    return true;
        //}

        //public static bool NullData(ATInputErrorConfig config, object entity)
        //{
        //    Type ty = entity.GetType();
        //    PropertyInfo propertyInfo = ty.GetProperty(config.ColumnName);
        //    var value = propertyInfo.GetValue(entity);

        //    if (string.IsNullOrEmpty(value.ToString()))
        //        return false;

        //    return true;
        //}

        //public static bool NonReservedWord(ATInputErrorConfig config, object entity)
        //{
        //    var param = config.Parameter.Replace(" ", "").Split(',');

        //    string orgKeyColumn = param[0];
        //    string refKeyColumn = param.Length == 2 ? param[1] : null;

        //    Type ty = entity.GetType();

        //    PropertyInfo orgColInfo = ty.GetProperty(config.ColumnName);
        //    PropertyInfo orgKeyInfo = ty.GetProperty(orgKeyColumn);

        //    var orgValue = orgColInfo.GetValue(entity);
        //    var orgKeyValue = orgKeyInfo.GetValue(entity);

        //    if (orgKeyValue == null)
        //        return false;

        //    if (config.TableName == config.RefTableName)
        //    {
        //        PropertyInfo refColInfo = ty.GetProperty(config.RefColumnName);
        //        var refValue = refColInfo.GetValue(entity).ToString();

        //        if (string.IsNullOrEmpty(refValue))
        //            return false;

        //        var keyValue = config.TableName + "@" + orgKeyColumn + "@" + orgKeyValue;

        //        var reservedWords = refValue.Replace(" ", "").Split(',');

        //        if (!ReservedDictionary.ContainsKey(keyValue))
        //            ReservedDictionary.Add(keyValue, reservedWords);

        //        if (!reservedWords.Contains(orgValue))
        //            return false;
        //        else
        //            return true;
        //    }
        //    else
        //    {
        //        var keyValue = config.RefTableName + "@" + refKeyColumn + "@" + orgKeyValue;

        //        if (ReservedDictionary.TryGetValue(keyValue, out string[] dicValue))
        //        {
        //            if (dicValue.Contains(orgValue))
        //                return true;
        //            else
        //                return false;
        //        }
        //        else
        //            return false;
        //    }
        //}

        //public static bool DataTypeMismatch(ATInputErrorConfig config, object entity)
        //{
        //    Type ty = entity.GetType();
            
        //    PropertyInfo valuePropertyInfo = ty.GetProperty(config.ColumnName);
        //    var value = valuePropertyInfo.GetValue(entity);

        //    PropertyInfo typePropertyInfo = ty.GetProperty(config.RefColumnName);
        //    var type = typePropertyInfo.GetValue(entity).ToString();

        //    try
        //    {
        //        if (value == null)
        //            return true;

        //        var result = ATUtil.ChangeType(value, ATUtil.GetValueType(type));
        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}

        //public static bool CalcOperator(ComparisonOperatorType oper, double compareValue, double targetValue)
        //{
        //    switch (oper)
        //    {
        //        case ComparisonOperatorType.IsGreaterThan:
        //            return compareValue < targetValue;
        //        case ComparisonOperatorType.IsGreaterThanOrEqualTo:
        //            return compareValue <= targetValue;
        //        case ComparisonOperatorType.IsLessThan:
        //            return compareValue > targetValue;
        //        case ComparisonOperatorType.IsLessThanOrEqualTo:
        //            return compareValue >= targetValue;
        //        case ComparisonOperatorType.Equals:
        //            return compareValue == targetValue;
        //        case ComparisonOperatorType.DoesNotEqual:
        //            return compareValue != targetValue;
        //        default:
        //            return false;
        //    }
        //}

        //public static bool CalcOperator(bool compare1, bool compare2, LogicalOperationType oper)
        //{

        //    switch (oper)
        //    {
        //        case LogicalOperationType.And:
        //            return compare1 && compare2;
        //        case LogicalOperationType.Or:
        //            return compare1 || compare2;
        //        default:
        //            return false;
        //    }
        //}

        //public static void AddDictionary(object entity)
        //{
        //    //return;

        //    Type entityType = entity.GetType();
        //    foreach (var item in entityType.GetProperties())
        //    {
        //        if (item.PropertyType.Name != "EntityState")
        //        {
        //            var key = entityType.Name + "@" + item.Name;
        //            var value = item.GetValue(entity);

        //            if (value != null)
        //            {
        //                if (DataDictionary.TryGetValue(key, out Dictionary<object, object> valueDic))
        //                {
        //                    if (!valueDic.ContainsKey(value))
        //                    {
        //                        DataDictionary[key].Add(value, value);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //public static List<ATInputErrorConfig> GetInputErrorConfig(string tableName)
        //{
        //    List<ATInputErrorConfig> retInputErrorConfig = new List<ATInputErrorConfig>();

        //    if (InputErrorConfig.ContainsKey(tableName))
        //        retInputErrorConfig = InputErrorConfig[tableName];

        //    return retInputErrorConfig;
        //}

        public static void SetDefaultPropertyValue(dynamic item, string category)
        {
            var props = ATInputData.Properties.GetPropertyValueByCategory(category);
            if (props != null)
            {
                foreach (var prop in props.Where(x => x.DefaultValue != null))
                {
                    item.Property[prop.PropertyID] = prop.DefaultValue;
                }
            }
        }

        public static void SetErrorStr(string str)
        {
            if (ErrorStr.Length == 0)
            {
                ErrorStr.Append("====================== CHECK ERROR LOG TABLE ===================== : \r\n");
            }
            ErrorStr.Append(str);
            ErrorStr.Append("\r\n");
        }

        //#region Old
        //public static string GetReasonDetail(ATInputErrorConfig config, object entity, ErrorReasonCode reason)
        //{
        //    Type type = entity.GetType();
        //    string reasonDetail = null;

        //    if (ATPersistHelper.TableInfo.TryGetValue(config.TableName, out HashedSet<string> columns))
        //    {
        //        foreach (var column in columns)
        //        {
        //            reasonDetail += string.Format("{0} : {1}, ", column, type.GetProperty(column).GetValue(entity));
        //        }
        //        reasonDetail = "(Key : " + reasonDetail.Remove(reasonDetail.Length - 2, 2) + ")";
        //    }

        //    switch (reason)
        //    {
        //        case ErrorReasonCode.OutOfRange:
        //            reasonDetail += string.Format("(Out of Range Column & Value : {0}, {1}), (Parameter : {2})", config.ColumnName, type.GetProperty(config.ColumnName).GetValue(entity), config.Parameter);
        //            break;
        //        case ErrorReasonCode.NotFoundReferredData:
        //            reasonDetail += string.Format("(Missing Data : {0}), (Ref Table & Column : {1}, {2})", type.GetProperty(config.ColumnName).GetValue(entity), config.RefTableName, config.RefColumnName);
        //            break;
        //        case ErrorReasonCode.Null:
        //            break;
        //        case ErrorReasonCode.MismatchReservedWord:
        //            string keyColumn = config.Parameter.Replace(" ", "").Split(',').FirstOrDefault();
        //            string keyValue = type.GetProperty(keyColumn).GetValue(entity).ToString();
        //            string key = config.RefTableName + "@" + keyColumn + "@" + keyValue;
        //            string reservedWord = null;

        //            if (ATPersistHelper.ReservedDictionary.TryGetValue(key, out string[] reservedWords))
        //                reservedWord = string.Join(", ", reservedWords);

        //            reasonDetail += string.Format("(Non Reserved Value : {0} = {1}), (Reserved Words : {2})", config.ColumnName, type.GetProperty(config.ColumnName).GetValue(entity), reservedWord);
        //            break;
        //        case ErrorReasonCode.DataTypeMisMatch:
        //            reasonDetail += string.Format("(Data Type : {0}), (Data Type Mismatch Value : {1})", type.GetProperty(config.RefColumnName).GetValue(entity), type.GetProperty(config.ColumnName).GetValue(entity));
        //            break;
        //        default:
        //            break;
        //    }

        //    return reasonDetail;
        //}
        //#endregion
    }
}
