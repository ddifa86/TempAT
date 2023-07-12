using Mozart.SeePlan.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    [Mozart.Task.Execution.FEBaseClassAttribute(Root = "PKG", Category = "PKG", IsTypeBinding = true, Mandatory = true, Description = null)]
    public class ATRoute : Route
    {
        public ATBom Bom { get; internal set; }

        public DateTime EffStartTime;

        public DateTime EffEndTime;


        public double SumRunTat { get; internal set; }

        public double SumWaitTat { get; internal set; }

        public double SumTat
        {
            get
            {
                return SumRunTat + SumWaitTat;
            }
        }

        public double CumYield { get; internal set; }

        public bool IsIncrementalYieldRoute { get; set; }

        /*
         * 관련 Resource 정보 ,Hynix 전용
         */

        public string ResourceIDs { get; internal set; }

        public StepCollection Opers
        {
            get { return this.RootActivity.Steps; }
        }

        public ATRoute()
           : base()
        {
            this.RootActivity = new Part(this, "dummy");
        }

        public ATRoute(string processID, DateTime eff_start_time, DateTime eff_end_time)
            : base(processID)
        {
            this.RootActivity = new Part(this, "dummy");

            this.EffStartTime = eff_start_time;
            this.EffEndTime = eff_end_time;

            this.SumRunTat = 0;
            this.SumWaitTat = 0;

            this.CumYield = 1;
            this.IsIncrementalYieldRoute = false;
        }

        public ATOperation FirstOper
        {
            get
            {
                if (this.Opers == null || this.Opers.Count == 0)
                    return null;

                return this.Opers.First() as ATOperation;
            }
        }

        public ATOperation LastOper
        {
            get
            {
                if (this.Opers.Count == 0 || this.Opers == null)
                    return null;

                return this.Opers.Last() as ATOperation;
            }
        }

        public void AddOper(ATOperation oper)
        {
            /// Route 내의 Oper들의 총 Tat
            this.SumRunTat += oper.RunTat;
            this.SumWaitTat += oper.WaitTat;

            // Route 내 누적 수율
            this.CumYield *= oper.Yield;

            this.Opers.Add(oper);
        }

        public void LinkOpers()
        {
            ATOperation prev = null;
            bool isFirstResOper = false;
            foreach (ATOperation step in this.Opers)
            {
                step.CumYield *= step.Yield;
                step.CumRunTat += step.RunTat;
                step.CumWaitTat += step.WaitTat;

                if (prev != null)
                {
                    Transition trans = new Transition(prev, step);

                    var cumPrev = prev;
                    while (cumPrev != null)
                    {
                        cumPrev.CumYield *= step.Yield;
                        cumPrev.CumRunTat += step.RunTat;
                        cumPrev.CumWaitTat += step.WaitTat;

                        cumPrev = cumPrev.GetDefaultPrevStep() as ATOperation;
                    }
                }

                if (isFirstResOper == false && step.OperType == OperType.Operation)
                {
                    step.IsFirstResOper = true;
                    isFirstResOper = true;
                }

                prev = step;
            }
        }

        public void SetFirstResOperation()
        {
            foreach (ATOperation oper in this.Opers)
            {
                if (oper.OperType == OperType.Operation)
                {
                    oper.IsFirstResOper = true;
                    break;
                }
            }
        }

        public ATOperation FindOper(string id)
        {
            var step = this.Opers.Where(x => x.StepID == id).FirstOrDefault();
            if (step == null)
                return null;

            return step as ATOperation;
        }

        #region ToString

        public override string ToString()
        {
            return base.ToString();
        }

        #endregion //ToString

       
    }
}
