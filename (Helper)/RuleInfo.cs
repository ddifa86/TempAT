﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RuleInfo : Attribute
    {
        public string ProjectID { get; set; }

    }
}
