using Mozart.SeePlan.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class ATWeightFactor : WeightFactor
    {
        public ATFactor FactorInfo { get; private set; }
 

        public Delegate Method
        {
            get
            {
                // Deligate 함수 호출 부분 변경.
                return this.FactorInfo.Method;
            }
        }

        public double Weight
        {
            get
            {
                return this.Factor;
            }
        }

        public ATWeightFactor(ATFactor factorInfo, double weightFactor, float sequence, string parameter)
            : base(factorInfo.FactorID, weightFactor, sequence, SeePlan.DataModel.FactorType.NONE, OrderType.ASC)
        {
            this.FactorInfo = factorInfo;

            if (string.IsNullOrEmpty(parameter) == false)
            {
                string[] parameters = parameter.Split(',');
                this.Criteria = parameters;
            }
        }
    }
}
