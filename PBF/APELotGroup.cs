using System;
using Mozart.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public class APELotGroup : IAPELot, IFactorObject
    {
        public string LotGroupKey { get; set; }

        public bool IsReserved { get; set; }

        public string FactorObjectKey
        {
            get
            {
                return this.LotGroupKey;
            }
        }

        // 키값을 가져야할 것 같은데..?
        private List<IAPELot> _lots { get; set; }

        public double Qty { get; private set; }

        public DateTime LateLastStepTime 
        { 
            get
            {
                var lots = this.GetLotList();
                return lots.Max(x => x.LastStepTime);
            }
        }

        public DateTime LastStepTime
        {
            get
            {
                if (this.Sample is APELotGroup)
                {
                    var lots = this.GetLotList();
                    return lots.Max(x => x.LastStepTime);
                }
                else
                {
                    return this.Sample.LastStepTime;
                }
            }
            set
            {
                if (this.Sample is APELot)
                    this.Sample.LastStepTime = value;
            }
        }

        public bool HasContents
        {
            get
            {
                return this._lots.Count() > 0;
            }
        }

        public IAPELot Sample
        {
            get
            {
                return this._lots.FirstOrDefault();
            }
        }

        public APELot SampleLot
        {
            get
            {
                if (this.Sample is APELotGroup)
                    return this.Sample.Sample as APELot;
                else
                    return this.Sample as APELot;
            }
        }
        public DateTime ReleaseTime
        {
            get => throw new NotImplementedException(); set => throw new NotImplementedException();
        }

        public double CurrentQty 
        { 
            get
            {
                return this.Qty;
            }
            set
            {
                this.Qty = value;
            }
        }

        public string CurrentOperID
        {
            get => Sample.CurrentOperID;
        }

        public ATOperation CurrentOper
        {
            get => Sample.CurrentOper;
        }

        public ATOperResource CurrentArrange
        {
            get => Sample.CurrentArrange;
            set
            {
                Sample.CurrentArrange = value;
            }
        }

        #region IFactorObject

        public Dictionary<string, ATFactorValue> FactorInfos { get; set; }
        public Dictionary<string, ATFilterValue> FilterInfos { get; set; }

        public string FactorValues { get; set; }
        public string FilterValues { get; set; }

        #endregion

        public APELotGroup(string key)
        {

            this.LotGroupKey = key;

            _lots = new List<IAPELot>();

            Qty = 0;

            this.FactorInfos = new Dictionary<string, ATFactorValue>();

            this.FilterInfos = new Dictionary<string, ATFilterValue>();

            // this.ReserveManager = new ATReserveManager();

        }

        public void AddLot(IAPELot lot, ATWeightPreset preset)
        {
            this.Qty += lot.Qty;

            this._lots.AddSort(lot, new APELotInGroupComparer(preset));
        }

        public bool RemoveLot(IAPELot lot)
        {
            var result = this._lots.Remove(lot);
            
            if(result == true)
                this.Qty -= lot.Qty;

            if (this.HasContents == false)
                this.Qty = 0;

            return result;
        }

        public void SortLot(APELotInGroupComparer comparer)
        {
            this._lots.Sort(comparer);
        }

        public List<IAPELot> GetLotList()
        {
            return _lots;
        }

        public ATOperation MoveFirst(DateTime now)
        {
            this._lots.ForEach(x => x.MoveFirst(now));

            return this.Sample.CurrentOper;
        }

        public ATOperation MoveNext(DateTime now)
        {
            ATOperation oper = null;
            foreach (var x in _lots)
            {
                oper = x.MoveNext(now);
            }

            return oper;
        }

        public void Apply(Action<IAPELot, IAPELot> action)
        {
            if (action != null)
            {
                this._lots.ForEach(x => action(x.Sample, null));
            }
        }

        public string InitFilterLogs()
        {
            return this.FactorObjectKey;
        }
        public string InitFactorLogs()
        {
            return string.Format("{0},{1}", this.FactorObjectKey, _lots.Count());
        }

        public object Clone()
        {
            //throw new NotImplementedException();
            APELotGroup clone = (APELotGroup)this.MemberwiseClone();

            //clone.WeightInfo = new WeightInfo();
            clone._lots = new List<IAPELot>();

            clone._lots.AddRange(this._lots);

            return clone;
        }

        public bool CanAllocate()
        {
            if (this.Qty <= ATOption.Instance.MinimumAllocationQuantity)
                return false;

            if (this.IsReserved == true)
                return false;

            return true;
        }

        internal List<PBFResource> GetLoadableBuckets() 
        {
            // 해당 함수는 List<IBucket> 으로 할수 있도록 추후 변경 필요.
            HashSet<PBFResource> buckets = new HashSet<PBFResource>();

            foreach (var lot in this._lots)
            {
                var aLot = lot ;

                buckets.AddRange(lot.CurrentOper.GetLoadableBucket());
            }

            return buckets.ToList();
        }
    }
}
