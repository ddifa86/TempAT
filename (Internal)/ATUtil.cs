using Mozart.DataActions;
using Mozart.SeePlan.DataModel;
using Mozart.Simulation.Engine;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik
{
    public static partial class ATUtil
    {
        public static string VerisonNo
        {
            get
            {
                return ModelContext.Current.VersionNo;
            }
        }



        public static bool BoolYN(string yn, bool defaultValue)
        {
            if (yn.IsEmptyID())
                return defaultValue;

            if (yn.Trim().ToUpper().Equals("Y"))
                return true;
            else
                return false;
        }

        public static string CreateKey(params object[] args)
        {
            string key = string.Empty;

            foreach (var k in args)
            {
                if (string.IsNullOrEmpty(key))
                    key = k.ToString();
                else
                    key += "@" + k.ToString();
            }

            return key;
        }

        /// <summary>
        /// 주어진 값과 시간 간격 단위를 시간 간격 개체로 변환합니다.
        /// </summary>
        /// <param name="value">값 입니다.</param>
        /// <param name="uom">시간 간격 단위입니다.</param>
        /// <returns>변환된 시간 간격 개체입니다.</returns>
        public static Time UomToTime(this double value, UomType uom)
        {
            var sec = value.UomToSecond(uom);
            return Time.FromSeconds(sec);
        }

        /// <summary>
        /// 주어진 값과 시간 간격 단위를 초 단위 값으로 변환합니다.
        /// </summary>
        /// <param name="value">값 입니다.</param>
        /// <param name="uom">시간 간격 단위입니다.</param>
        /// <returns>변환된 초 단위 값입니다.</returns>
        public static double UomToSecond(this double value, UomType uom)
        {
            switch (uom)
            {
                //case UomType.MONTH:
                //    return (7 * 24 * 60 * 60 * 30) * value; // 30이아니라 현재 월 정보를 기준으로 판단이 필요함.

                case UomType.Week:
                    return (7 * 24 * 60 * 60) * value;

                case UomType.Day:
                    return (24 * 60 * 60) * value;

                case UomType.Shift:
                    return (ShopCalendar.ShiftHours * 60 * 60) * value;

                case UomType.Hour:
                    return (60 * 60) * value;

                case UomType.Minute:
                    return 60 * value;
            }

            return value;
        }

        public static double UomToDay(this double value, UomType uom)
        {
            switch (uom)
            {
                // * 24 * 60 * 60
                case UomType.Week:
                    return (7) * value;

                case UomType.Day:
                    return value;

                case UomType.Shift:
                    return (ShopCalendar.ShiftHours / 24) * value;

                case UomType.Hour:
                    return 1 / 24 * value;

                case UomType.Minute:
                    return 1 / 24 / 60 * value;

                case UomType.Second:
                    return 1 / 24 / 60 / 60 * value;
            }

            return value;
        }

        /// <summary>
        /// 주어진 시간 간격을 해당 시간 간격 단위의 값으로 변환합니다.
        /// </summary>
        /// <param name="value">시간 간격입니다.</param>
        /// <param name="uom">시간 간격 단위입니다.</param>
        /// <returns>변환된 시간 간격 단위의 값입니다.</returns>
        public static double TimeToUom(this Time value, UomType uom)
        {
            switch (uom)
            {
                case UomType.Week:
                    return value.TotalDays / 7;

                case UomType.Day:
                    return value.TotalDays;

                case UomType.Shift:
                    return value.TotalHours / ShopCalendar.ShiftHours;

                case UomType.Hour:
                    return value.TotalHours;

                case UomType.Minute:
                    return value.TotalMinutes;
            }

            return value.TotalSeconds;
        }

        public static double SecondToUom(this double value, UomType uom)
        {
            if (value <= ATOption.Instance.MinimumAllocationQuantity)
                return 0;

            switch (uom)
            {
                case UomType.Day:
                    return value / ATConstants.Day;
                case UomType.Hour:
                    return value / ATConstants.Hour;
                case UomType.Minute:
                    return value / ATConstants.Minute;
                default:
                    break;
            }

            return value;
        }

        /// <summary>
        /// 수량 변환을 위한 메서드
        /// </summary>
        /// <param name="value">변환하고자 하는 값</param>
        /// <param name="detail">BOM_DETAIL 정보</param>
        /// <param name="type">Module Type </param>
        /// <param name="toQty">Bin Case 경우 누적 Ratio</param>
        /// <returns></returns>
        public static double ConvertValue(this double value, double fromQty, double toQty, PlanType type)
        {
            if (type == PlanType.Backward)
                return value * (fromQty / toQty);
            else
                return value * (toQty / fromQty);
        }


        public static Type GetValueType(string valueType)
        {
            var vType = valueType.ToUpper();

            if (vType == "STRING")
                return typeof(string);
            else if (vType == "INT")
                return typeof(int);
            else if (vType == "DOUBLE" || vType == "PERCENT" || vType == "NUMBER" || vType == "DECIMAL")
                return typeof(double);
            else if (vType == "DATETIME")
                return typeof(DateTime);
            else if (vType == "BOOL")
                return typeof(bool);
            else if (
                    vType == "WEEK" ||
                    vType == "DAY" ||
                    vType == "SHIFT" ||
                    vType == "HOUR" ||
                    vType == "MINUTE" ||
                    vType == "SEC"
                )
                return typeof(Time);
            else
                return typeof(string);
        }

        public static object ChangeType(object value, Type conversionType)
        {
            return Convert.ChangeType(value, conversionType);
        }

        public static bool IsVaildChangeType(object value, Type conversionType)
        {
            try
            {
                Convert.ChangeType(value, conversionType);

                return true;
            }
            catch
            {
                return false;
            }

        }

        public static double ConvertDouble(object value, double defaultValue)
        {
            try
            {
                return Convert.ToDouble(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static Type GetType(DataType dataType)
        {
            Type type = typeof(string);

            switch (dataType)
            {
                case DataType.String:
                    type = typeof(string);
                    break;
                case DataType.Int:
                    type = typeof(int);
                    break;

                case DataType.Double:             
                    type = typeof(double);
                    break;

                case DataType.DateTime:
                    type = typeof(DateTime);
                    break;
            }

            return type;
        }

        public static T DataCopy<T>(object from, T to)
        {
            foreach (var item in from.GetType().GetProperties())
            {
                if (item.PropertyType.Name != "EntityState")
                {
                    var value = item.GetValue(from);

                    to.GetType().GetProperty(item.Name)?.SetValue(to, value);
                }
            }

            return to;
        }

        public static void SaveLocalFileStorage<T>(string dataDirectory, string fileName, Mozart.Data.Entity.EntityTable<T> table) where T : class, new()
        {
            if (table == null)
                return;

            DataTable schema = ToDataTable(table);
            schema.TableName = fileName;

            LocalFileStorage db = new LocalFileStorage(dataDirectory);
            db.Save(schema);
        }

        public static DataTable ToDataTable<T>(Mozart.Data.Entity.EntityTable<T> table) where T : class, new()
        {
            var schema = CreateSchema(typeof(T));

            if (schema == null)
                return null;

            foreach(T item in table.Rows)
            {
                AddDataRow(schema, item);
            }
            return schema;
        }

        private static DataTable CreateSchema(Type type)
        {
            DataTable schema = new DataTable();

            System.Reflection.PropertyInfo[] props = type.GetProperties();
            foreach(System.Reflection.PropertyInfo prp in props)
            {
                if (prp.Name == "ObjectState")
                    continue;

                schema.Columns.Add(prp.Name, prp.PropertyType);
            }

            return schema;
        }

        private static void AddDataRow<T>(DataTable table, T item) where T : class, new()
        {
            var row = CopyDataRow(table, item);

            if (row != null)
                table.Rows.Add(row);
        }

        private static DataRow CopyDataRow<T>(DataTable table, T item) where T : class, new()
        {
            if (table == null)
                return null;

            var row = table.NewRow();
            Type type = typeof(T);
            System.Reflection.PropertyInfo[] props = type.GetProperties();
            foreach(System.Reflection.PropertyInfo prp in props)
            {
                if (table.Columns.Contains(prp.Name) == false)
                    continue;

                row[prp.Name] = prp.GetValue(item, null);
            }

            return row;
        }

        public static string TrimData(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            return value.Replace(" ", "");
        }
        
    }
}
