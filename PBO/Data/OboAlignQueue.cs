using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.ObyO.Data
{
    public class OboAlignQueue
    {
        public ATOperation Operation;
        private List<APELot> _lots;


        public OboAlignQueue(ATOperation oper)
        {
            this.Operation = oper;
            this._lots = new List<APELot>();
        }

        public List<APELot> GetLots()
        {
            return _lots;
        }

        public void RemoveLot(APELot lot)
        {
            _lots.Remove(lot);
        }

        public void AddAlignQueue(APELot lot)
        {
            _lots.AddSort(lot, new QueueLotComparer());
        }

        #region Compare
        private class QueueComparer : IComparer<ATOperation>
        {
            public QueueComparer()
            {
            }

            public int Compare(ATOperation x, ATOperation y)
            {
                if (object.ReferenceEquals(x, y))
                    return 0;

                int cmp = 0;

                cmp = x.CurrentBuffer.Sequence.CompareTo(y.CurrentBuffer.Sequence) * -1;
                if (cmp != 0)
                    return cmp;

                cmp = x.RouteID.CompareTo(y.RouteID);
                if (cmp != 0)
                    return cmp;

                cmp = x.Sequence.CompareTo(y.Sequence) * -1;
                return cmp;
            }
        }

        private class QueueLotComparer : IComparer<APELot>
        {
            public QueueLotComparer()
            {
            }

            public int Compare(APELot x, APELot y)
            {
                if (object.ReferenceEquals(x, y))
                    return 0;

                int cmp = 0;

                cmp = x.LastStepTime.CompareTo(y.LastStepTime);
                if (cmp != 0)
                    return cmp;

                cmp = x.CurrentTarget.TargetDate.CompareTo(y.CurrentTarget.TargetDate);
                if (cmp != 0)
                    return cmp;
                
                cmp = x.Qty.CompareTo(y.Qty) * -1;
                if (cmp != 0)
                    return cmp;

                cmp = x.LotID.CompareTo(y.LotID);
                return cmp;
            }
        }
        #endregion
    }
}
