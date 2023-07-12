using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB.Common;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik
{
    public class ATProperty
    {
        public string Category { get; private set; }

        public string PropertyID { get; private set; }

        public Type Type { get; private set; }

        public string Description { get; private set; }

        public object DefaultValue { get; private set; }

        public bool IsSetupProperty { get; internal set; }

        public string ReservedWord { get; internal set; }

        public string DefaultValueStr { get; internal set; }

        public ATProperty(string category, string proertyID, DataType type, string desc, string defaultvalue, string reservedWord = "")
        {
            this.Category = category;
            this.PropertyID = proertyID;
            this.Type = ATUtil.GetType(type);
            this.Description = desc;
            this.DefaultValue = Converter.ChangeType(defaultvalue, this.Type);
            this.DefaultValueStr = defaultvalue;
            this.ReservedWord = reservedWord;
        }
    }
}
