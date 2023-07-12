using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class ATWeightPreset : WeightPreset
    {
        public ATRule Rule { get; private set; }

        public string RuleID { get; private set; }

        public RulePoint RulePoint { get; private set; }

        public SortType SortType { get; private set; }
        
        public string RulePointID
        {
            get
            {
                return this.RulePoint.ToString();
            }
        }

        public SortedSet<ATWeightFactor> FactorList { get; private set; }

        public ATWeightPreset(ATRule rule, string id, RulePoint rulePoint, SortType sortType)
        {
            this.Rule = rule;
            this.RuleID = id;
            this.RulePoint = rulePoint;
            this.SortType = sortType;
            this.FactorList = new SortedSet<ATWeightFactor>(ATWeightFactorComparer.Default);
        }

        #region Interal
        private class ATWeightFactorComparer : IComparer<ATWeightFactor>
        {
            public static ATWeightFactorComparer Default = new ATWeightFactorComparer();

            public int Compare(ATWeightFactor x, ATWeightFactor y)
            {
                var cmp = x.Sequence.CompareTo(y.Sequence);
                if (cmp == 0)
                    cmp = x.Name.CompareTo(y.Name);

                return cmp;
            }
        }
        #endregion
    }
}
