using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Logic;
using Mozart.SeePlan.Aleatorik.ObyO.Data;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.ObyO
{
    public class ReleaseAgent : IAgent
    {
        public static ReleaseAgent Instance
        {
            get { return ServiceLocator.Resolve<ReleaseAgent>(); }
        }

        private static Stack<APELot> _defaultStack;
        private SortedDictionary<ATOperation, OboAlignQueue> _alignQueue;

        public void Dispose()
        {
            _defaultStack.Clear();
            _alignQueue.Clear();
        }

        public void Clear()
        {
            _defaultStack.Clear();
            _alignQueue.Clear();
        }

        public void Initialize()
        {
            _defaultStack = new Stack<APELot>();
            _alignQueue = new SortedDictionary<ATOperation, OboAlignQueue>(new ATOperationComparer());
        }

        public SortedDictionary<ATOperation, OboAlignQueue> GetAlignQueue()
        {
            return _alignQueue;
        }

        public void SetDefaultStack(Stack<APELot> lots)
        {
            _defaultStack = lots;
        }

        public void PushDefaultStack(APELot lot)
        {
            _defaultStack.Push(lot);
        }

        public Stack<APELot> GetLots()
        {
            Stack<APELot> lots = new Stack<APELot>();

            if (_defaultStack.Count > 0)
            {
                return _defaultStack;
            }

            foreach (var queue in _alignQueue)
            {
                APELot mbsLot = MBSLogic.Instance.GetMbsLot(queue.Value);

                if (mbsLot != null)
                {
                    lots.Push(mbsLot);
                    break;
                }
            }

            return lots;
        }

        public void AddAlignQueue(ATOperation oper, APELot lot)
        {
            OboAlignQueue queue;
            if (_alignQueue.TryGetValue(oper, out queue) == false)
            {
                queue = new OboAlignQueue(oper);
                _alignQueue.Add(oper, queue);
            }

            queue.AddAlignQueue(lot);

            lot.OrgLotKeys.Clear();
            lot.OrgLotKeys.Add(lot.LotID + lot.CurrentOperID);
        }

        internal class ATOperationComparer : IComparer<ATOperation>
        {

            public ATOperationComparer()
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

                cmp = x.Sequence.CompareTo(y.Sequence) * -1;
                if (cmp != 0)
                    return cmp;

                cmp = x.RouteID.CompareTo(y.RouteID);

                return cmp;
            }
        }
    }
}
