using LinqToDB.Common;
using Mozart.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class ATPropertyHelper
    {
        private Dictionary<string, List<ATProperty>> _propertyByCategory = new Dictionary<string, List<ATProperty>>();
        private Dictionary<string, ATProperty> _properties = new Dictionary<string, ATProperty>();

        public ATPropertyHelper()
        {
        }

        public bool AddProperty(ATProperty property)
        {
            if (_properties.ContainsKey(property.PropertyID))
            {
                if (property.PropertyID.Contains("#"))
                    _properties[property.PropertyID] = property;
                else
                    return false;
            }
            else
            {
                _properties.Add(property.PropertyID, property);
            }

            if (_propertyByCategory.TryGetValue(property.Category, out var value))
            {
                if (property.PropertyID.Contains("#"))
                {
                    var reservedProp = value.Where(x => x.PropertyID == property.PropertyID).FirstOrDefault();
                    if (reservedProp != null)
                    {
                        value.Remove(reservedProp);
                    }
                }

                value.Add(property);
            }
            else
            {
                _propertyByCategory.Add(property.Category, new List<ATProperty>() { property });
            }

            return true;
        }

        public List<ATProperty> GetPropertyValueByCategory(string category)
        {
            if (_propertyByCategory.TryGetValue(category, out var properties))
                return properties;
            else
                return null;
        }

        public ATProperty GetPropertyByID(string id)
        {
            if (_properties.TryGetValue(id, out var property))
                return property;
            else
                return null;
        }

        public object GetPropertyValue(string category, string propertyID, string value)
        {
            if (_propertyByCategory.TryGetValue(category, out var properties))
            {
                var props = properties.Where(x => x.PropertyID == propertyID);
                if (props == null || props.Count() <= 0)
                {
                    // 정보가 누락된 경우에는 Null
                    return null;
                }
                else
                {
                    ATProperty propertyInfo = props.FirstOrDefault();

                    if (value == null || string.IsNullOrEmpty(value))
                        return propertyInfo.DefaultValue;

                    var result = Converter.ChangeType(value, propertyInfo.Type);
                    
                    return result;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
