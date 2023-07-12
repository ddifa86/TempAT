using Mozart.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class ATAssemblyInfo
    {
        internal APELot AssyLot;

        internal List<APELot> PartLots;

        internal string PartLotIds;
        /// <summary>
        /// 조립을 위해  BomDetail별 사용된 Lot 수량 정보. 
        /// </summary>
        internal DoubleDictionary<ATBomDetail, APELot, double> PartInfo;

        public ATAssemblyInfo(APELot assyLot)
        {
            this.AssyLot = assyLot;
            this.PartInfo = new DoubleDictionary<ATBomDetail, APELot, double>();
            this.PartLots = new List<APELot>();
        }

        public List<APELot> GetPartLots()
        {
            return PartLots;
        }


        public void AddPartLot(ATBomDetail detail, APELot partLot, double usedQty)
        {
            this.PartInfo.Add(detail, partLot, usedQty);
            this.PartLots.Add(partLot);

            if (PartLotIds == null)
                this.PartLotIds = partLot.LotID;
            else
                this.PartLotIds += ", " + partLot.LotID;
        }
    }
}
