using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public static partial class ATUtil
    {

        public static string GetEnumProperty<T>() where T : struct
        {
            Array values = System.Enum.GetValues(typeof(T));

            string property = string.Empty;

            foreach (int value in values)
            {
                var name = Enum.GetName(typeof(T), value);

                if (name == "None")
                    continue;

                if (string.IsNullOrEmpty(property))
                    property = name;

                else
                    property += "," + name;
            }

            return property;
        }
        /// <summary>
        /// 지정된 문자열을 해당하는 열거형 개체로 변환합니다. 변환에 실패한 경우 오류 내용을 ERROR_HISTORY에 기록합니다.
        /// </summary>
        /// <typeparam name="T">열거형 형식입니다.</typeparam>
        /// <param name="value">변환할 문자열입니다.</param>
        /// <param name="defaultValue">변환에 실패했을 때 반환할 기본 열거형 개체입니다.</param>
        /// <param name="dataItemName">문자열이 포함된 인풋 테이블의 이름입니다.</param>
        /// <param name="propertyName">문자열이 포함된 속성의 이름입니다.</param>
        /// <returns>문자열에 해당하는 열거형 개체입니다.</returns>
        public static T StringToEnum<T>(this string value, T defaultValue)
            where T : struct
        {
            if (value.IsEmptyID())
            {
                return defaultValue;
            }

            if (Enum.TryParse<T>(value, true, out T result))
                return result;

            return defaultValue;
        }

        /// <summary>
        /// 지정된 문자열을 요일 개체로 변환합니다.
        /// </summary>
        /// <param name="value">변환할 문자열입니다.</param>
        /// <param name="result">변환된 요일입니다.</param>
        /// <param name="dataItemName">문자열이 포함된 인풋 테이블의 이름입니다.</param>
        /// <param name="propertyName">문자열이 포함된 속성의 이름입니다.</param>
        /// <returns>변환에 성공한 경우 <see langword="true"/>이고, 그 외의 경우 <see langword="false"/>입니다.</returns>
        public static bool StringToDayOfWeek(string value, out DayOfWeek result)
        {
            value = value.ToUpper();

            if (double.TryParse(value, out var doub))
            {
                result = default;
                return false;
            }

            if (Enum.TryParse<DayOfWeek>(value, true, out result))
                return true;

            else if (!value.EndsWith("day", StringComparison.CurrentCultureIgnoreCase) && Enum.TryParse<DayOfWeek>(value + "day", true, out result))
                return true;

            else if (value.Contains("TUE"))
            {
                result = DayOfWeek.Tuesday;
                return true;
            }

            else if (value.Contains("WED"))
            {
                result = DayOfWeek.Wednesday;
                return true;
            }

            else if (value.Contains("THU"))
            {
                result = DayOfWeek.Thursday;
                return true;
            }

            else if (value.Contains("SAT"))
            {
                result = DayOfWeek.Saturday;
                return true;
            }

            else if (int.TryParse(value, out int num))
            {
                result = (DayOfWeek)(num % 7);
                return true;
            }

            return false;
        }
    }
}
