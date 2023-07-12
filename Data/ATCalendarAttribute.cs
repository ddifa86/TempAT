using Mozart.SeePlan.Aleatorik.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class ATCalendarAttribute
    {

        public ATCalendarDetail Detail { get; private set; }

        public string CalendarID
        {
            get
            {
                return this.Detail.Calendar.CalendarID;
            }
        }

        public string CalendarType
        {
            get
            {
                return this.Detail.Calendar.CalendarType;
            }
        }

        public string PatternSeq
        {
            get { return this.Detail.PatternSeq; }
        }


        public string ApplyDate { get; set; }


        public string Attribute { get; internal set; }

        internal Type Type;

        private dynamic _value;

        public dynamic Value
        {
            get
            {
                return ATUtil.ChangeType(this._value, this.Type);
            }
            set
            {
                this._value = value;
            }
        }

        public bool IsVaild
        {
            get
            {
                return ATUtil.IsVaildChangeType(this._value, this.Type);
            }
        }

        public double Priority
        {
            get 
            {
                return this.Detail.Priority;
            }
        }


        public DateTime EffectiveStartTime { get; set; } 

        public DateTime EffectiveEndTime { get; set; } //get { return this.Detail.EffectiveEndTime; }

        public ATCalendarAttribute(ATCalendarDetail detail, string attribute, dynamic value, DataType type, string internalPatternSeq)
        {
            this.Detail = detail;

            this.Attribute = attribute;

            this._value = value;

            this.Type = ATUtil.GetType(type);

            this.ApplyDate = internalPatternSeq;

        }

        public ATCalendarAttribute ShallowCopy()
        {
            return (ATCalendarAttribute)this.MemberwiseClone();
        }
 
    }
}
