using Mozart.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class BaseContext  
    {
        protected Dictionary<object, object> _dict = new Dictionary<object, object>();

        #region Context Data
        /// <summary>
        /// The property value of a specific Key in Dispatch context. 
        /// </summary>
        /// <param name="key">Property key string.</param>
        /// <returns>Returns the property value of the key.</returns>
        public object this[object key]
        {
            get { return _dict[key]; }
            set { _dict[key] = value; }
        }
        /// <summary>
        /// The property value of a specific Key in Dispatch context.
        /// </summary>
        public ICollection<object> Keys
        {
            get { return _dict.Keys; }
        }
        /// <summary>
        /// Saves property value to a specific key.
        /// </summary>
        /// <param name="key">Property key string.</param>
        /// <param name="value">Property value.</param>
        public void Set(object key, object value)
        {
            if (key == null)
                throw new System.ArgumentNullException(string.Format(Strings.EXCEPTION_ARG_NULL, "DispatchContext.Set", "key"));

            _dict[key] = value;
        }
        /// <summary>
        /// Removes property value from a specific key.
        /// </summary>
        /// <param name="key">Property key string to remove.</param>
        public void Remove(object key)
        {
            if (key == null)
                throw new System.ArgumentNullException(string.Format(Strings.EXCEPTION_ARG_NULL, "DispatchContext.Remove", "key"));

            if (_dict.ContainsKey(key))
                _dict.Remove(key);
        }
        /// <summary>
        /// Return the property value of a sepcific key as target data type. 
        /// </summary>
        /// <typeparam name="TValue">Target property type to return.</typeparam>
        /// <param name="key">Property key string.</param>
        /// <param name="defaultValue">Default value of the property.</param>
        /// <returns>Returns the property value.</returns>
        public TValue Get<TValue>(object key, TValue defaultValue)
        {
            if (key == null)
                throw new System.ArgumentNullException(string.Format(Strings.EXCEPTION_ARG_NULL, "DispatchContext.Get", "key"));

            return _dict.GetValue<TValue>(key, defaultValue);
        }

        /// <summary>
        /// Return the property value of a specific key as target data type or create one if value does not exist.
        /// </summary>
        /// <typeparam name="TValue">Target property type to return.</typeparam>
        /// <param name="key">Property key string.</param>
        /// <param name="creator">The function to crate property value when it does not exist.</param>
        /// <returns>Returns the property value.</returns>
        public TValue GetOrAdd<TValue>(object key, Func<object, TValue> creator)
        {
            if (key == null)
                throw new System.ArgumentNullException(string.Format(Strings.EXCEPTION_ARG_NULL, "DispatchContext.GetOrAdd", "key"));
            if (creator == null)
                throw new System.ArgumentNullException(string.Format(Strings.EXCEPTION_ARG_NULL, "DispatchContext.GetOrAdd", "creator"));

            if (_dict.ContainsKey(key))
                return _dict.GetValue<TValue>(key, default(TValue));


            TValue value = creator(key);
            _dict[key] = value;
            return value;
        }

        /// <summary>
        /// Return th property value of a specific key as target data type and delete it from Context. 
        /// </summary>
        /// <typeparam name="TValue">Target property type to return.</typeparam>
        /// <param name="key">Property key string.</param>
        /// <param name="defaultValue">Default value of the property.</param>
        /// <returns>Returns the property value.</returns>
        public TValue Pop<TValue>(object key, TValue defaultValue)
        {
            if (key == null)
                throw new System.ArgumentNullException(string.Format(Strings.EXCEPTION_ARG_NULL, "DispatchContext.Pop", "key"));

            object value;
            if (_dict.TryGetValue(key, out value))
            {
                _dict.Remove(key);
                return (TValue)value;
            }
            else
            {
                return defaultValue;
            }
        }
        #endregion Context Data
    }
}
