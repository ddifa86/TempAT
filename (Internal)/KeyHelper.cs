using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public static class KeyHelper
    {
        // KeyHelper.cs
        public static string CreateIndexKey<T>(Expression<Func<T, dynamic>> keySelector)
        {

            List<string> splitKey = NameOfPropertys(keySelector);
            return string.Join(",", splitKey.ToArray());

        }

        public static List<string> NameOfPropertys<T>(Expression<Func<T, dynamic>> e)
        {
            List<string> names = new List<string>();

            if (e == null)
                return null;

            if (e.Body is NewExpression)
            {

                var newExpression = e.Body as NewExpression;
                foreach (MemberInfo member in newExpression.Members)
                    names.Add(member.Name);
            }

            else if (e.Body is MemberExpression)
            {
                var memberExpression = e.Body as MemberExpression;
                names.Add(memberExpression.Member.Name);

            }

            else if (e.Body is UnaryExpression)
            {
                var unaryExpression = e.Body as UnaryExpression;
                var memberExpression = unaryExpression.Operand as MemberExpression;
                names.Add(memberExpression.Member.Name);
            }

            return names;
        }

    }
}
