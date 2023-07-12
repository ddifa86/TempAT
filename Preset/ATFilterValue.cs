using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class ATFilterValue 
    {
        //
        // 요약:
        //     WeightFactor의 가중치가 반영된 점수를 가져옵니다. 만약 Factor 속성이 설정되지 않은 상태인 경우 0을 반환합니다.
        public bool Value { get; }
        //
        // 요약:
        //     점수를 얻게된 사유를 가져오거나 설정합니다.
        public string Description { get; set; }

        public object Tag { get; set; }

        public ATFilterValue(bool value, string desc)
        {
            this.Value = value;
            this.Description = desc;
        }
    }
}
