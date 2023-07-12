using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class AlignQueueManager
    {
        private SortedDictionary<IComparable, IAlignQueue> _alignQueues { get; set; }

        internal bool HasAlignQueue
        {
            get
            {
                return this._alignQueues != null && this._alignQueues.Count > 0;
            }
        }

        public AlignQueueManager(HashSet<IAlignQueue> queues)
        {
            _alignQueues = new SortedDictionary<IComparable, IAlignQueue>();

            Initialze(queues);
        }

        private void Initialze(HashSet<IAlignQueue> queues)
        {
            foreach (var item in queues)
            {
                this._alignQueues.Add(item.Key, item);
            }
        }

        public IAlignQueue GetQeueue(IComparable key)
        {
            IAlignQueue queue;
            if (_alignQueues.TryGetValue(key, out queue) == false)
            {
                // factorying 기법을 사용해볼까..?
                // IAlingQueue = objectmapper.CreateAlignQueue()..
                return null;
            }

            return queue;
        }

        public IAlignQueue PopFirstAlignQueue()
        {
            var first = this._alignQueues.FirstOrDefault();

            // KTR : 예외처리
            this._alignQueues.Remove(first.Key);

            return first.Value;
        }

        public void Dispose()
        {
            // KTR : dictionary 내 AlignQ에 대해서는 메모리 해제를 따로 안해줘도 될까?
            _alignQueues.Clear();
            _alignQueues = null;
        }
    }
}
