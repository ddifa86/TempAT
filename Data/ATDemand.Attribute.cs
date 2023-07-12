using Mozart.SeePlan.Aleatorik.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public partial class ATDemand
    {
        #region Shipment 지정
        /// <summary>
        /// 출하 날짜
        /// </summary>
        internal DateTime ShipDate { get; set; }

        /// <summary>
        /// 출하 요일
        /// </summary>
        internal DayOfWeek ShipDaysOfWeek;

        /// <summary>
        /// 출하 요일 지정 여부
        /// </summary>
        internal bool HasShipDaysOfWeek { get; set; }

        #endregion

        //#region 참조 Demand 설정

        //internal Dictionary<ATItemSiteBuffer, List<ATRefPlan>> RefPlans = new Dictionary<ATItemSiteBuffer, List<ATRefPlan>>();

        //#endregion
    }
}
