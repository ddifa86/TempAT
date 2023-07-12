using Mozart.SeePlan.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class ATFactorValue : WeightValue
    {
        public object Tag { get; set; }

        public string Unit { get; set; }

        public ATFactorValue(double value, string description = null, string unit = null)
            : base(value, description)
        {
            this.Unit = unit;
        }

        /// <summary>
        /// 주어진 WeightFactor와 가중치가 반영되지 않은 점수 및 사유를 사용하여 새로운 WeightValue를 생성합니다.
        /// </summary>
        /// <param name="factor">점수 계산 대상 WeightFactor입니다.</param>
        /// <param name="rawValue">가중치가 반영되지 않은 점수입니다.</param>
        /// <param name="description">점수를 얻게된 사유입니다.</param>
        public ATFactorValue(WeightFactor factor, double rawValue, string description = null)
            : base(factor, rawValue, description)
        {
        }
    }
}
