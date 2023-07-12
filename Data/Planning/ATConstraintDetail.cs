using Mozart.SeePlan.Aleatorik.Outputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class ATConstraintDetail
    {
        private double _usedQty;
        private double _virtualUsedQty;

        public ATConstraint Constraint { get; set; }

        public double Qty { get; internal set; }
        
        public double RemainQty
        {
            get
            {
                return Qty - UsedQty - VirtualUsedQty;
            }
        }

        public double UsedQty 
        {
            get
            {
                if (this.Constraint.Policy == ConstraintPolicy.None)
                    return _usedQty;
                else
                    return this.Constraint.UsedQty;
            }
            set 
            {
                if (this.Constraint.Policy == ConstraintPolicy.None)
                    _usedQty = value;
                else
                    this.Constraint.UsedQty = value;
            }
        }

        public double VirtualUsedQty
        {
            get
            {
                if (this.Constraint.Policy == ConstraintPolicy.None)
                    return _virtualUsedQty;
                else
                    return this.Constraint.VirtualUsedQty;
            }
            set
            {
                if (this.Constraint.Policy == ConstraintPolicy.None)
                    _virtualUsedQty = value;
                else
                    this.Constraint.VirtualUsedQty = value;
            }
        }
        public double UsedRatio
        {
            get
            {
                if (Qty <= 0)
                    return 0;

                return Math.Round((UsedQty / Qty) * 100, 4);
            }
        }

        public bool IsWriteDetail { get; set; }

        public ATCalendarAttribute Attribute { get; set; }

        public ATConstraintDetail(ATConstraint constraint, ATCalendarAttribute attribute)
        {
            this.Constraint = constraint;
            this.Attribute = attribute;
            this.Qty = attribute.Value;
        }

        internal void AddVirtualUsedQty(double qty)
        {
            this.VirtualUsedQty += qty;
        }

        internal void UpdateVirtualAct(double qty, bool isCommit)
        {
            this.VirtualUsedQty -= qty;

            if (this.VirtualUsedQty <= ATOption.Instance.MinimumAllocationQuantity)
                this.VirtualUsedQty = 0;

            if (isCommit)
                this.UsedQty += qty;
        }
    }
}
