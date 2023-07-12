using Mozart.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    [Mozart.Task.Execution.FEBaseClassAttribute(Root = "PKG", Category = "PKG", IsTypeBinding = true, Mandatory = true, Description = null)]
    public partial class ATItemSiteBuffer : IPropertyObject, IConstraint
    {
        #region Property

        public ATSite Site { get; private set; }

        public ATItem Item { get; private set; }

        public ATBuffer Buffer { get; private set; }


        public string SiteID
        {
            get
            {
                return Site.SiteID;
            }
        }

        public string ItemID
        {
            get
            {
                return Item.ItemID;
            }
        }

        public string BufferID
        {
            get
            {
                return Buffer.BufferID;
            }

        }

        public string Key
        {
            get
            {
                return CommonHelper.CreateKey(Item.ItemID, Site.SiteID, Buffer.BufferID);
            }
        }

        public Dictionary<string, HashSet<ATBomDetailAlt>> AltItemSiteBuffers { get; set; }

        public Dictionary<string, HashSet<string>> AltItemSiteBufferKeys { get; set; }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public SortedDictionary<ATBom, List<ATBomDetail>> PrevBoms { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public SortedDictionary<ATBom, List<ATBomDetail>> NextBoms { get; private set; }

        public HashSet<ATItemSiteBuffer> OrgItemSiteBuffers { get; set; }

        public ATItemSiteBuffer(ATSite site, ATItem item, ATBuffer buffer, bool isInfiniteMaterial, bool isNoCarryOver, double inputLotSize)
        {
            this.Site = site;
            this.Item = item;
            this.Buffer = buffer;
            this.IsInfiniteMaterial = isInfiniteMaterial;
            this.IsNoCarryover = isNoCarryOver;
            this.InputLotSize = inputLotSize;

            this.PrevBoms = new SortedDictionary<ATBom, List<ATBomDetail>>(new ATBomPriorityComparer());
            this.NextBoms = new SortedDictionary<ATBom, List<ATBomDetail>>(new ATBomPriorityComparer());

            this.Property = new DynamicDictionary();
            this.WipQueue = new ATISBQueue(this);

            this.AltItemSiteBuffers = new Dictionary<string, HashSet<ATBomDetailAlt>>();
            this.AltItemSiteBufferKeys = new Dictionary<string, HashSet<string>>();

            this.ConstraintInfos = new List<ATConstraintInfo>();
            this.Constraints = new List<ATConstraint>();

            #region attributes
            this.IsUsablePath = BomPathType.N;

            this.MinCumTat = Int32.MaxValue;
            this.MaxCumTat = Int32.MaxValue;
            this.LateCumTat = Int32.MinValue;

            this.PrevResources = new Dictionary<ATBuffer, HashSet<ATResource>>();
            this.PrevBomsAll = new Dictionary<ATBuffer, HashSet<ATBom>>();
            this.PrevItemSiteBuffers = new Dictionary<ATBuffer, HashSet<ATItemSiteBuffer>>();
            this.NextItemSiteBuffers = new Dictionary<ATBuffer, HashSet<ATItemSiteBuffer>>();
            this.SoItemSiteBuffers = new HashSet<ATItemSiteBuffer>();
            this.PrevAssyItemSiteBuffers = new HashSet<ATItemSiteBuffer>();
            this.OrgItemSiteBuffers = new HashSet<ATItemSiteBuffer>();
            

            this.RefPlans = new Dictionary<string, List<APERefPlan>>();

            this.Property = new DynamicDictionary();
            this.CalendarInfo = new ATCalendarManager();

            List<ATProperty> reservedProps = ATInputData.Properties.GetPropertyValueByCategory(PropertyCategory.ItemSiteBuffer.ToString());
            if (reservedProps != null)
                reservedProps.ForEach(x => this.SetProperty(x.PropertyID, x.DefaultValue));
            #endregion
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

        public void AddPrevBom(ATBom prevBom)
        {
            if (this.PrevBoms.ContainsKey(prevBom) == false)
            {
                this.PrevBoms.Add(prevBom, prevBom.BomDetails.Where(x => x.ToItemSiteBuffer.Key == this.Key).ToList());
            }
        }

        public void AddNextbom(ATBom nextBom)
        {
            if (this.NextBoms.ContainsKey(nextBom) == false)
            {
                this.NextBoms.Add(nextBom, nextBom.BomDetails.Where(x => x.FromItemSiteBuffer.Key == this.Key).ToList());
            }
        }

        public List<ATBom> GetNextBom(BomType bomType)
        {
            return this.NextBoms.Keys.Where(x => x.BomType == bomType).ToList();
        }

        public List<ATBom> GetPrevBom(BomType bomType)
        {
            return this.PrevBoms.Keys.Where(x => x.BomType == bomType).ToList();
        }
        /*
        *좀 고민인 포인트..
        * ItemBuffer가 가지고 있는 Bom 정보는 실질적으로 자신의....BomDetial 정보를 가지도록 해야하나?
        * BOM에 대한 정보에서 자신에 해당하는 BomDetail 정보를 반환하게 하는게 맞는것인가..?
        * 해당 함수를 호출하는 횟수가 많은 것으로 예상되는데...
        * 
        * Pegging 시점에 Bom을 먼저 선택하고 => Bom에 해당하는 Detail을 선택 -> BomID에 Mapping되는 Route 를 찾는 구조로
        * 구성이 되어야 하는데 해당 부분은 일단 고민이 필요...?
       */

        #region ToString

        public override string ToString()
        {
            return this.Key;
        }
        #endregion //ToString
 
    }
}
