using Mozart.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class ATShortManager
    {
        /// <summary>
        /// Short 관련 정보
        /// </summary>
        public Dictionary<ShortType, List<ATShortInfo>> ShortInfos { get; private set; }

        /// <summary>
        /// 동일 AssemblyKey를 가지는 Part의 ShortInfo 별도 집계
        /// Assembly의 경우 Part들의 Short 중 Max가 실 Short 수량으로 인정.
        /// </summary>
        public DoubleDictionary<ShortType, string, Dictionary<ATItemSiteBuffer, List<ATShortInfo>>> AssyShortInfo { get; private set; }

        public APEPegPart RootPegPart { get; set; }

        public APETarget RootPegTarget { get; set; }

        public ATShortManager()
        {
            this.ShortInfos = new Dictionary<ShortType, List<ATShortInfo>>();
            this.AssyShortInfo = new DoubleDictionary<ShortType, string, Dictionary<ATItemSiteBuffer, List<ATShortInfo>>>();
        }

        public void Clear()
        {
            this.ShortInfos.Clear();
            this.AssyShortInfo.Clear();
        }
    }
}
