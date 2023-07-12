using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Logic;
using Mozart.SeePlan.Aleatorik.ObyO;
using Mozart.SeePlan.Pegging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public partial class APETarget : ILateTracker, ITarget
    {
        public string TargetID { get; private set; }

        public double ShipEarlyTime { get; internal set; }

        public double RefTime { get; set; }

        public DateTime TargetDateTime
        {
            get { return this.DueDate; }
            set { this.DueDate = value; }
        }

        public DateTime MaxLateTargetDateTime
        {
            get
            {
                double maxdays = (this.MoPlan as ATMoPlan).Demand.MaxLateDays;
                return TargetDateTime.AddDays(maxdays);
            }
        }

        public ATDemand Demand
        {
            get
            {
                return this.MoPlan.Demand;
            }
        }

        public ATDemand SoDemand
        {
            get
            {
                return this.MoPlan.SODemand;
            }
        }

        public ATMoPlan MoPlan { get; set; }

        public double Qty { get; set; }


        virtual public double RemainQty
        {
            get { return this.Qty; }
            set { this.Qty = value; }
        }

        public string TargetDate { get { return ATUtil.ToDate(this.DueDate); } }

        public string TargetWeek { get { return ATUtil.ToWeek(this.DueDate); } }

        public string TargetMonth { get { return ATUtil.ToMonth(this.DueDate); } }

        public ATItemSiteBuffer ItemSiteBuffer
        {
            get
            {
                return this.PegPart.CurrentItemSiteBuffer;
            }
        }

        public ATItem Item
        {
            get
            {
                return this.PegPart.CurrentItemSiteBuffer.Item;
            }
        }

        public APEPegPart PegPart
        {
            get
            {
                return this.Group as APEPegPart;
            }
        }

        public ITargetGroup Group { get; set; }

        public DateTime DueDate { get; set; }

        public double BCumTat { get; internal set; }

        public double BCumChangeRatio { get; internal set; }

        public ATOperTarget CurOperTarget { get; set; }

        public Dictionary<string, ATLateInfo> VirtualLateInfos { get; set; }

        private Dictionary<string, ATOperTarget> _operTargets { get; set; }

        public APETarget(APEPegPart pegPart, ATMoPlan moPlan, string targetID)
        {
            #region PegTarget 상속 부분 처리
            if (pegPart == null)
                throw new System.ArgumentNullException(string.Format(Strings.EXCEPTION_ARG_NULL, "PegTarget.PegTarget", "pegPart"));

            this.Group = pegPart;
            this.MoPlan = moPlan;
            this.Qty = moPlan.Qty;
            DateTime calcDateTime = moPlan.DueDate.AddHours(FactoryConfiguration.Current.StartTime.Hours).AddSeconds(86399);
            this.DueDate = calcDateTime;
            //this.DueDate = moPlan.DueDate;
            #endregion

            this.TargetID = targetID;
            
            _operTargets = new Dictionary<string, ATOperTarget>();
            CurOperTarget = null;

            BCumChangeRatio = 1;
            VirtualLateInfos = new Dictionary<string, ATLateInfo>();
        }

        public void AddOperTarget(ATOperTarget target)
        {
            //OBO와 BW 모듈에서 해당 부분을 동기화 시킬 필요가 있음.
            //추가로 TargetAgent를 만들고 Key를 생성하는 RulePoint의 생성 필요함.
            //해당 Key로 Target을 저장하는 로직이 필요하며, RulePoint 미정의시에는 따로 저장하지 않음.
            //해당  RulePoint는 FW에서도 사용해야하므로 넘어가는 Argument에 대한 고민이 필요함.

            // _operTargets 에 정보를 담아야할까..?

            // Target을 관리하는 TargetAgent가 필요함..
            // OBO, BW 에서 PegTarget 객체가 동일한 공정을 지나가는 경우가존재할까..?
            //// obo retry할때는 새로 만들고, 

            ATOperation oper = target.Oper;
            bool isOut = target.IsOut == "Y";

            target.Tat = oper.GetTat(this.DueDate, isOut);

            if (isOut) // Run
            {
                double yield = oper.GetYield(this.DueDate);
                yield = PBOInterface.PegControl.GetYield(this, yield);
                
                target.Yield = yield;
                target.TotalTat = target.Tat;
            }
            else // Wait
            {
                target.Yield = this.CurOperTarget.Yield;
                // RunTat, WaitTat 정보 더하기.
                target.TotalTat = this.CurOperTarget.Tat + target.Tat;
            }

            this.BCumTat += target.Tat;
            target.BCumTat = this.BCumTat;

            // 연결 작업.
            target.Next = this.CurOperTarget;
            if (this.CurOperTarget != null)
                this.CurOperTarget.Prev = target;

            if (RefPlanLogic.Instance.IsRefPlan(this, target))
                target.CurRefPlan = this.CurOperTarget.CurRefPlan;

            this.CurOperTarget = target;
        }


        public ATOperTarget GetOperTarget(ATOperation oper, bool isOut)
        {
            string bOut = isOut ? "Y" : "N";

            string key = ATUtil.CreateKey(oper.RouteID, oper.OperID, bOut);

            if (this._operTargets.ContainsKey(key) == true)
                return _operTargets[key];

            return null;
        }

        public APETarget Clone(ITargetGroup newPegPart)
        {
            var clone = (APETarget)this.MemberwiseClone();
            clone.Group = newPegPart;
            clone._operTargets = new Dictionary<string, ATOperTarget>(this._operTargets);

            return clone;
        }

        public void Dispose()
        {
            this._operTargets = null;
            this.Group = null;
        }
        public override string ToString()
        {
            return ATUtil.CreateKey(this.TargetID, this.RemainQty);
        }

    }
}
