using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class ATShortInfo
    {
        /// <summary>
        /// ?? 필요할까..
        /// </summary>
        public APEPegPart PegPart { get; private set; }


        /// <summary>
        /// Short이 발생한 공정 정보
        /// </summary>
        public ATOperTarget OperTarget { get; private set; }

        /// <summary>
        /// 필요할까..
        /// </summary>
        public ATItemSiteBuffer ItemSiteBuffer { get; private set; }

        // Short 수량
        public double Qty { get; private set; }


        public double BCumChangeRatio
        {
            get
            {
                return this.OperTarget.BCumChangeRatio;
            }
        }

        public string Category { get; set; }

        public string Reason { get; set; }
 
        /// <summary>
        /// 
        /// </summary>
        public APELot ShortLot;

        // 필요시 ShortBom과 ShortResource도 Dictionary로 관리....
        public ATBom Bom;
        public ATResource Resource;
        public ShortType ShortType;

        /*
         * BW , FW 무관하게 ShortInfo를 등록하는 함수를 만들어야 할 것으로 보임.
         */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pegPart"> Short을 관리하는 Key. Assembly의 경우 해당 정보를 이용하여 </param>
        /// <param name="operTarget"> Short 난 시점의 SnapShot 정보 </param>
        /// <param name="qty"></param>
        /// <param name="category"></param>
        /// <param name="reason"></param>
        public ATShortInfo(APEPegPart pegPart, ATOperTarget operTarget, double qty, string category, string reason, ShortType shortType = ShortType.InPhase)
        {
            this.OperTarget = operTarget;
            this.PegPart = pegPart;

            this.Qty = qty;
            this.Category = category;
            this.Reason = reason;

            this.ItemSiteBuffer = pegPart.CurrentItemSiteBuffer;
            this.Bom = ATBom.NULL;
            this.ShortType = shortType;
        }

        /// <summary>
        /// F/W 시점의 Short 정보 등록.
        /// </summary>
        /// <param name="lot"></param>
        /// <param name="target"></param>
        /// <param name="qty"></param>
        /// <param name="category"></param>
        /// <param name="reason"></param>
        public ATShortInfo(APELot lot, ATOperTarget target, double qty, ShortType shortType = ShortType.InPhase)
        {
            this.ShortLot = lot;
            this.OperTarget = target;
            this.PegPart = target.PegTarget.PegPart;

            this.Qty = qty;
            this.Category = lot.ShortCategory;
            this.Reason = lot.ReasonName;

            this.ItemSiteBuffer = lot.CurrentItemSiteBuffer;

            this.Bom = lot.CurrentBom;

            var lastCapaPlan = lot.CapaPlans.LastOrDefault();
            if (lastCapaPlan != null)
                Resource = lastCapaPlan.Bucket?.Target;

            this.ShortType = shortType;
        }
    }
}
