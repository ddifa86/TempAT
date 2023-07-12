using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public partial class ATOperTarget
    {

/* 'Mozart.SeePlan.Aleatorik (net472)' 프로젝트에서 병합되지 않은 변경 내용
이전:
        public ObyO.Data.ATAssyInfo AssyInfo = null;
이후:
        public ATAssyInfo AssyInfo = null;
*/
        public Data.ATAssyInfo AssyInfo = null;

        public double GetBCumChangeRatio(ATOperTarget to)
        {
            return this.BCumChangeRatio / to.BCumChangeRatio;
        }
    }
}
