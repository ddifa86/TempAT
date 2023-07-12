using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DocAttribute : Attribute
    {
        //public string Name { get; set; }

        public DocAttribute()
        {
          //  this.Name = name;
        }
    }
}
