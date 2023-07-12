using Mozart.Data.Entity;
using Mozart.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.Extensions;
using Mozart.Collections;

namespace Mozart.SeePlan.Aleatorik.Data
{
    [Mozart.Task.Execution.FEBaseClassAttribute(Root = "PKG", Category = "PKG", IsTypeBinding = true, Mandatory = true, Description = null)]
    public partial class ATBom : IFactorObject, IPropertyObject, IConstraint
    {
        public static readonly ATBom NULL = new ATBom("NULL", BomType.None, 999, DateTime.MinValue, DateTime.MaxValue);

        public string FactorObjectKey => this.BomID;

        public string BomID { get; private set; }

        public BomType BomType { get; internal set; }

        public double Priority { get; private set; }

        public DateTime EffStartTime { get; private set; }

        public DateTime EffEndtime { get; private set; }

        public string FromSiteID { get; private set; }

        public string ToSiteID { get; private set; }

        public List<ATBomDetail> BomDetails { get; private set; }

        public ATBomDetail MainBomDetail { get; internal set; }

        public List<ATBomRouting> BomRoutes { get; private set; }

        /// <summary>
        /// Bom과 연관된 FromBuffer 정보
        /// </summary>
        private ATBuffer _fromBuffer;

        public ATBuffer FromBuffer
        {
            get
            {
                if (_fromBuffer == null)
                {
                    _fromBuffer = BomDetails.First().FromBuffer;
                }

                return _fromBuffer;
            }
        }

        /// <summary>
        /// Bom과 연관된 ToBuffer 정보
        /// </summary>
        private ATBuffer _toBuffer;

        public ATBuffer ToBuffer
        {
            get
            {
                if (_toBuffer == null)
                {
                    _toBuffer = BomDetails.First().ToBuffer;
                }

                return _toBuffer;
            }
        }

        public Dictionary<string, ATFactorValue> FactorInfos { get; set; }

        public Dictionary<string, ATFilterValue> FilterInfos { get; set; }

        public string FactorValues { get; set; }

        public string FilterValues { get; set; }

        public bool IsInvalidBom { get; set; }

        private Dictionary<Tuple<ATItemSiteBuffer, ATItemSiteBuffer>, List<ATBomDetail>> _lowerBomDetail { get; set; }

        private Dictionary<Tuple<ATItemSiteBuffer, ATItemSiteBuffer>, List<ATBomDetail>> _upperBomDetail { get; set; }

        private Dictionary<Tuple<ATItemSiteBuffer, ATItemSiteBuffer>, List<ATBomDetail>> _otherBomDetail { get; set; }

        public ATBom(string bomID, BomType bomType, double priority, DateTime effStartTime, DateTime effEndTime)
        {
            this.BomID = bomID;
            this.BomType = bomType;
            this.Priority = priority;

            this.EffStartTime = effStartTime;
            this.EffEndtime = effEndTime;

            this.IsInvalidBom = false;

            this.BomDetails = new List<ATBomDetail>();
            this.BomRoutes = new List<ATBomRouting>();
            this.SoItemSites = new Dictionary<ATItemSiteBuffer, ATItemSiteBuffer>();
            
            this.FactorInfos = new Dictionary<string, ATFactorValue>();
            this.FilterInfos = new Dictionary<string, ATFilterValue>();

            this.Property = new DynamicDictionary();
            this.CalendarInfo = new ATCalendarManager();

            this.ConstraintInfos = new List<ATConstraintInfo>();
            this.Constraints = new List<ATConstraint>();

            this._lowerBomDetail = new Dictionary<Tuple<ATItemSiteBuffer, ATItemSiteBuffer>, List<ATBomDetail>>();
            this._upperBomDetail = new Dictionary<Tuple<ATItemSiteBuffer, ATItemSiteBuffer>, List<ATBomDetail>>();
            this._otherBomDetail = new Dictionary<Tuple<ATItemSiteBuffer, ATItemSiteBuffer>, List<ATBomDetail>>();

            List<ATProperty> reservedProps = ATInputData.Properties.GetPropertyValueByCategory(PropertyCategory.Bom.ToString());
            if (reservedProps != null)
                reservedProps.ForEach(x => this.SetProperty(x.PropertyID, x.DefaultValue));
        }

        #region DataObject Interface
        public dynamic Property { get; internal set; }

        public ATCalendarManager CalendarInfo { get; internal set; }

        public List<ATConstraintInfo> ConstraintInfos { get; set; }

        public List<ATConstraint> Constraints { get; set; }

        public bool HasConstraint { get; set; }

        public void SetProperty(string propertyID, object value)
        {
            this.Property[propertyID] = value;
        }

        public void SetCalendar(string propertyID, ATCalendar calendar)
        {
            this.CalendarInfo.AddCalendar(calendar);
        }
        #endregion

        public override string ToString()
        {
            return ATUtil.CreateKey(this.BomID, this.BomType.ToString(), this.Priority);
        }

        public string InitFilterLogs()
        {
            return this.FactorObjectKey;
        }

        public string InitFactorLogs()
        {
            return this.FactorObjectKey;
        }


        // 캐싱 작업
        #region Split Case 함수
        public List<ATBomDetail> GetLowerBomDetail(ATItemSiteBuffer soItem, ATItemSiteBuffer itemSiteBuffer)
        {
            List<ATBomDetail> result = new List<ATBomDetail>();
            if (this.BomType != BomType.SplitBy)
                return result;

            Tuple<ATItemSiteBuffer, ATItemSiteBuffer> key = Tuple.Create(soItem, itemSiteBuffer);

            if (_lowerBomDetail.ContainsKey(key) == true)
                return _lowerBomDetail[key];

            HashSet<string> alts = new HashSet<string>();
            HashSet<string> altInfos;

            if (itemSiteBuffer.AltItemSiteBufferKeys.TryGetValue(this.BomID, out altInfos))
                alts.AddRange(altInfos);

            if (itemSiteBuffer.AltItemSiteBufferKeys.TryGetValue(itemSiteBuffer.Key, out altInfos))
                alts.AddRange(altInfos);

            var targetGrade = itemSiteBuffer.Item.Grade;
            foreach (var detail in this.BomDetails)
            {
                var grade = detail.ToItem.Grade;

                if (grade.CompareTo(targetGrade) <= 0)
                    continue;

                if (alts.Contains(detail.ToItemSiteBuffer.Key) == false)
                    result.Add(detail);
            }

            _lowerBomDetail.Add(key, result);

            return result;
        }

        public List<ATBomDetail> GetUpperBomDetail(ATItemSiteBuffer soItem, ATItemSiteBuffer itemSiteBuffer)
        {
            List<ATBomDetail> result = new List<ATBomDetail>();
            if (this.BomType != BomType.SplitBy)
                return result;

            Tuple<ATItemSiteBuffer, ATItemSiteBuffer> key = Tuple.Create(soItem, itemSiteBuffer);

            // 캐싱작업.
            if (_upperBomDetail.ContainsKey(key) == true)
                return _upperBomDetail[key];

            HashSet<string> alts = new HashSet<string>();

            #region Alt 정보 등록..?
            HashSet<string> altInfos;
            if (itemSiteBuffer.AltItemSiteBufferKeys.TryGetValue(this.BomID, out altInfos))
                alts.AddRange(altInfos);

            if (itemSiteBuffer.AltItemSiteBufferKeys.TryGetValue(itemSiteBuffer.Key, out altInfos))
                alts.AddRange(altInfos);
            #endregion

            var targetGrade = itemSiteBuffer.Item.Grade;
            foreach (var detail in this.BomDetails)
            {
                var grade = detail.ToItem.Grade;

                if (grade.CompareTo(targetGrade) <= 0)
                    result.Add(detail);
                else if(alts.Contains(detail.ToItemSiteBuffer.Key))
                    result.Add(detail);
            }

            _upperBomDetail.Add(key, result);

            return result;
        }

        public double UpperBomRatio(ATItemSiteBuffer soItem, ATItemSiteBuffer itemSiteBuffer, DateTime dateTime)
        {
            if (this.BomType != BomType.SplitBy)
                return 1;

            var uppers = GetUpperBomDetail(soItem, itemSiteBuffer);

            double binrate = 0; // upper.Sum(x => x.ToQty);

            foreach (var detail in uppers)
            {
                binrate += detail.GetToQty(dateTime);
            }

            binrate = Math.Round(binrate,6);

            return binrate;
        }

        public List<ATBomDetail> GetOtherBomDetail(ATItemSiteBuffer soItem, ATItemSiteBuffer itemSiteBuffer)
        {
            List<ATBomDetail> result = new List<ATBomDetail>();
            if (this.BomType != BomType.SplitCo)
                return result;

            Tuple<ATItemSiteBuffer, ATItemSiteBuffer> key = Tuple.Create(soItem, itemSiteBuffer);

            if (_otherBomDetail.ContainsKey(key) == true)
                return _otherBomDetail[key];

            ATBomDetail currentBomDetail = this.GetCurrentBomDetail(soItem, itemSiteBuffer);

            foreach (var detail in this.BomDetails)
            {
                if (detail.Equals(currentBomDetail))
                    continue;

                result.Add(detail);
            }

            _otherBomDetail.Add(key, result);

            return result;
        }

        public ATBomDetail GetCurrentBomDetail(ATItemSiteBuffer soItem, ATItemSiteBuffer itemSiteBuffer)
        {
            var result = this.BomDetails.Where(x => x.ToItemID == itemSiteBuffer.Item.ItemID).FirstOrDefault();

            return result;
        }

        /// <summary>
        /// 자신을 생성하는 과정에서 발생되는 부산물 정보
        /// Split-Co 의 경우 : 자신을 제외한 나머지 제품
        /// Split-By 의 경우 : 자신보다 낮은 등급의 제품
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal List<ATBomDetail> GetCoByBomDetail(ATItemSiteBuffer soItem, ATItemSiteBuffer itemSiteBuffer)
        {
            if( this.BomType == BomType.SplitCo)
            {
                return GetOtherBomDetail(soItem, itemSiteBuffer);
            }
            else
            {
                return GetLowerBomDetail(soItem, itemSiteBuffer);
            }
        }

        /// <summary>
        /// 자기 제품으로의 변경가능한 비율 정보
        /// Split-Co의 경우 자신에게 설정된 값만큼만 변경가능
        /// Split-By의 경우 동일 grade 및 상위 Grade를 자기 제품으로 변경가능
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal double GetBomRatio(ATItemSiteBuffer soItem, ATItemSiteBuffer itemSiteBuffer, DateTime dateTime)
        {
            ATBomDetail currentBomDetail = this.GetCurrentBomDetail(soItem, itemSiteBuffer);
            double toQty;

            if (this.BomType == BomType.SplitCo)
                toQty = currentBomDetail.GetToQty(dateTime);
            else
                toQty = this.UpperBomRatio(soItem, itemSiteBuffer, dateTime);
         
            return toQty;
        }
        #endregion
    }
}
