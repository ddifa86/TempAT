using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik
{
    public class LotGroupKeyMethod
    {
        /// <summary>
        /// 현재 ItemSiteBuffer와 타겟의 타겟 주차가 동일한 재공들을 재공 그룹으로 구성
        /// </summary>
        /// <param name="lot"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(LotGroupKey))]
        public string ItemSiteBufferWeek(IAPELot lot)
        {
            var target = (lot as APELot).CurrentTarget;
            var key = target.CurrentItemBufferKey + "@" + target.TargetWeek;

            return key;
        }

        /// <summary>
        /// 현재 ItemSiteBuffer가 동일한 재공들을 재공 그룹으로 구성
        /// </summary>
        /// <param name="lot"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(LotGroupKey))]
        public string ItemSiteBufferID(IAPELot lot)
        {
            var aLot = (lot as APELot);

            var key = aLot.CurrentItemSiteBuffer.Key;

            return key;
        }

        /// <summary>
        /// 동일 버퍼의 재공들을 하나의 재공 그룹으로 구성
        /// </summary>
        /// <param name="lot"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(LotGroupKey))]
        public string BufferID(IAPELot lot)
        {
            string res = "";
            if (lot.CurrentOper.IsBuffer)
                res = lot.CurrentOper.OperID;
            else
                res = lot.CurrentOper.CurrentBuffer.BufferID;


            return res;
        }

        /// <summary>
        /// 재공과 재공 그룹을 1:1로 구성
        /// </summary>
        /// <param name="lot"></param>
        /// <returns></returns>
        [RuleFactor(RulePoint = typeof(LotGroupKey))]
        public string LotID(IAPELot lot)
        {
            return (lot as APELot).LotID;
        }
    }
}
