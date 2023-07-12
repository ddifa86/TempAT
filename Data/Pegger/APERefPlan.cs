using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class APERefPlan
    {
        public string ID { get; private set; }

        public string Key { get; set; }

        public ATItemSiteBuffer ItemSiteBuffer { get; private set; }

        public DateTime DueDate { get; private set; }

        /// <summary>
        /// 참조 계획 수량
        /// </summary>
        public double Qty { get; internal set; }

        public double VirtualUsedQty { get; set; }
        
        public double UsedQty { get; set; }

        public double RemainQty
        {
            get 
            {
                return Qty - VirtualUsedQty - UsedQty;
            }
        }

        public ATBom Bom { get; set; }

        public ATRoute Route { get; set; }

        public ATOperation Operation { get; set; }

        public ATResource Resource { get; set; }

        public ATStage Stage { get; set; }

        public string SoID { get; set; }

        public string Type { get; set; }

        public dynamic Property { get; set; }

        public ATOperTarget RefTarget { get; set; }

        public APERefPlan OrgRefPlan { get; set; }

        public APERefPlan(string id, ATItemSiteBuffer itemsitebuffer, DateTime duedate, double qty, string type, ATStage stage, string soID)
        {
            this.ID = id;
            this.ItemSiteBuffer = itemsitebuffer;
            this.DueDate = duedate;
            this.Qty = qty;
            this.Type = type;
            this.Stage = stage;
            this.SoID = soID;
            this.Property = new DynamicDictionary();
            this.OrgRefPlan = this;
        }

        public APERefPlan ShallowCopy(double qty)
        {
            var copy = (APERefPlan)this.MemberwiseClone();

            copy.Qty = qty;
            copy.UsedQty = 0;
            copy.RefTarget = null;
            copy.VirtualUsedQty = 0;

            return copy;
        }

    }
}
