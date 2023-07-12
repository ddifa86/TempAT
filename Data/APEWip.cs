
using System;
using System.Collections.Generic;
using System.Linq;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public partial class APEWip
    {
        #region Base
        public ATWipInfo WipInfo { get; private set; }

        public APEWip ReferenceWip { get; set; }

        public HashSet<ATItemSiteBuffer> CumItemSiteBuffes;

        public double FromQty { get; set; }

        public double ToQty { get; set; }

        public double CumWipQty
        {
            get
            {
                return ReferenceWip.RemainQty.ConvertValue(FromQty, ToQty, PlanType.Forward);
            }
        }

        public LotCreateType CreationType { get; private set; }

        public double Qty { get; private set; }

        public double Act { get; set; }

        public double VirtualAct { get; internal set; }
        public double RemainQty
        {
            get
            {
                return Qty - VirtualAct - Act ;
            }
        }
        
        public bool IsAllPeggedWip
        {
            get 
            {
                return RemainQty <= ATOption.Instance.MinimumAllocationQuantity && VirtualAct <= ATOption.Instance.MinimumAllocationQuantity;
            }
        }
        
        public LotState State { get; set; }

        public DateTime AvailableTime { get; set; }

        public ATItem Item { get; set; }

        public ATRoute Route { get; set; }

        public ATOperation Oper { get; set; }    
        
        public int MapCount { get; set; }

        public ATBuffer Buffer
        {
            get
            {
                if (this.Oper.IsBuffer)
                    return this.Oper as ATBuffer;

                else
                {
                    ATBuffer buffer;
                    if ( this.Route.Bom.BomType == BomType.Assembly)
                    {
                        // ToBuffer와 동일하다고 판단하여 ToBuffer로 설정
                        buffer = this.Route.Bom.BomDetails.First().ToBuffer;
                    }
                    else
                    {
                        // 그외는 FromBuffer로 설정
                        buffer = this.Route.Bom.BomDetails.First().FromBuffer;
                    }

                    return buffer;
                }
            }
        }

        public string ItemID
        {
            get
            {
                return this.Item.ItemID;
            }
        }
 

        public string OperID
        {
            get
            {
                return this.Oper.StepID;
            }
        }

        public string RouteID
        {
            get
            {
                if (this.Route == null)
                    return string.Empty;

                return this.Route.RouteID;
            }
        }

        public ATItemSiteBuffer ItemSiteBuffer;

        public dynamic Property { get; internal set; }

        /// <summary>
        /// 해당 Wip이 Pegging된 MaptTarget
        /// </summary>
        public ATOperTarget MapTarget;

        public ATDemand StageOutSalesOrder;
        #endregion

        #region BinPegInfo

        // Split의 경우에만 해당.
        public APETarget SourceTarget;

        public string SourceTargetID
        {
            get
            {
                return this.SourceTarget != null ? this.SourceTarget.TargetID : string.Empty;
            }
        }

        public List<string> Keys { get; set; }

        #endregion

        public APEWip(ATWipInfo wipInfo, LotCreateType type)
        {
            this.WipInfo = wipInfo;
            this.CreationType = type;

            this.Qty = wipInfo.UnitQty;
            this.State = wipInfo.LotState;
            this.AvailableTime = wipInfo.AvailableTime;
            this.Oper = wipInfo.Oper;
            this.Item = wipInfo.Item;
            this.Route = wipInfo.Route;

            this.Property = new DynamicDictionary();
            this.ItemSiteBuffer = wipInfo.ItemSiteBuffer;
            this.Keys = new List<string>();
            this.FromQty = 1;
            this.ToQty = 1;
            this.CumItemSiteBuffes = new HashSet<ATItemSiteBuffer>();
        }

        public APEWip ShallowCopy()
        {
            var newWip = (APEWip)this.MemberwiseClone();
            return newWip;
        }

        public APEWip ShallowCopy(ATOperTarget target, double qty)
        {
            var newWip = (APEWip)this.MemberwiseClone();
            newWip.MapTarget = target;
            newWip.Qty = qty;
            newWip.Act = 0;

            return newWip;
        }
    }

    public class ATStageOutPlanWipComparer : IComparer<APEWip>
    {
        public ATOperTarget Target;

        public ATStageOutPlanWipComparer(ATOperTarget target)
        {
            this.Target = target;
        }

        public int Compare(APEWip x, APEWip y)
        {
            if (object.ReferenceEquals(x, y))
                return 0;

            if (x.StageOutSalesOrder != y.StageOutSalesOrder)
            {
                if (x.StageOutSalesOrder == Target.SODemand)
                    return -1;

                if (y.StageOutSalesOrder == Target.SODemand)
                    return 1;
            }

            var cmp = x.AvailableTime.CompareTo(y.AvailableTime);
            if (cmp != 0)
                return cmp;

            cmp = x.WipInfo.LotID.CompareTo(y.WipInfo.LotID);
            return cmp;
        }
    }
}
