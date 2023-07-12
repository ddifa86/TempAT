using Mozart.SeePlan.Pegging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    //[Mozart.Task.Execution.FEBaseClassAttribute(Root = "BCP", Category = "BCP", IsTypeBinding = true, Mandatory = true, Description = null)]
    public class ATMoMaster : MoMaster
    {
        /// <summary>
        /// MoMaster의 고유 식별자를 가져옵니다.
        /// </summary>
        public virtual IComparable Key { get; internal set; }

        /// <summary>
        /// 대표 ItemBuffer 정보
        /// </summary>
        public ATItemSiteBuffer ItemSiteBuffer { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="itemBuffer"></param>
        public string SiteID { get; set; }

         
        
        public ATMoMaster(IComparable key, ATItemSiteBuffer itemBuffer, string siteID)
        {
            this.Key = key;
            this.ItemSiteBuffer = itemBuffer;
            this.SiteID = siteID;
        }
    }
}
