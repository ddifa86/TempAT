using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class ATFactor
    {
        /// <summary>
        /// 팩터 Key
        /// </summary>
        public string FactorID { get; private set; }

        /// <summary>
        /// 팩터 반영될 Rule Point
        /// </summary>
        public RulePoint RulePoint { get; private set; }

        public string RulePointID
        {
            get
            {
                return this.RulePoint.ToString();
            }
        }

        /// <summary>
        /// 팩터 설명
        /// </summary>
        public string Description { get; private set; }

        // 
        public string Expression { get; private set; }

        public FactorType FactorType { get; private set; }

        //
        public Delegate Method { get; internal set; }

        public ATFactor(string factorID, RulePoint RulePoint, string desc, FactorType factorType, string expression)
        {
            this.FactorID = factorID;
            this.RulePoint = RulePoint;
            this.Description = desc;
            this.FactorType = factorType;
            this.Expression = expression;
        }
        
    }
}
