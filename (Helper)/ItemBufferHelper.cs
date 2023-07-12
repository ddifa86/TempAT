
using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    [FeatureBind()]
    public partial class ItemSiteBufferHelper
    {
        private Dictionary<string, ATItem> Items = new Dictionary<string, ATItem>();
        private Dictionary<string, ATSite> Sites = new Dictionary<string, ATSite>();
        private Dictionary<string, ATBuffer> Buffers = new Dictionary<string, ATBuffer>();

        public Dictionary<string, ATItemSiteBuffer> ItemSiteBuffers = new Dictionary<string, ATItemSiteBuffer>();
        public Dictionary<string, List<ATItemSiteBuffer>> ItemSiteBufferByBuffer = new Dictionary<string, List<ATItemSiteBuffer>>();

        public Dictionary<string, List<ABomNetwork>> ABomNetworks = new Dictionary<string, List<ABomNetwork>>();
        public Dictionary<string, List<ABomNetwork>> ABomNetworksByCurrentItemSiteBuffer = new Dictionary<string, List<ABomNetwork>>();

        public bool AddSite(ATSite site)
        {
            if (Sites.ContainsKey(site.SiteID) == true)
                return false;

            Sites.Add(site.SiteID, site);

            return true;
        }

        public bool AddBuffer(ATBuffer buffer)
        {
            if (Buffers.ContainsKey(buffer.BufferID) == true)
                return false;

            Buffers.Add(buffer.BufferID, buffer);

            return true;
        }

        public bool AddItem(ATItem item)
        {
            if (Items.ContainsKey(item.ItemID) == true)
                return false;

            Items.Add(item.ItemID, item);

            return true;
        }

        public bool AddItemSiteBuffer(ATItemSiteBuffer itemSiteBuffer)
        {
            bool isSave = true;

            string key = ATUtil.CreateKey(itemSiteBuffer.SiteID, itemSiteBuffer.ItemID, itemSiteBuffer.BufferID);  // itemSiteBuffer.Key;  // string siteid, string itemID, string bufferID

            if (ItemSiteBuffers.ContainsKey(key) == true)
                isSave = false;
            else
                ItemSiteBuffers.Add(key, itemSiteBuffer);

            if (ItemSiteBufferByBuffer.TryGetValue(itemSiteBuffer.BufferID, out var values))
                values.Add(itemSiteBuffer);
            else
                ItemSiteBufferByBuffer.Add(itemSiteBuffer.BufferID, new List<ATItemSiteBuffer>() { itemSiteBuffer });

            return isSave;
        }

        public void AddItemSiteBufferNode(ABomNetwork itemSiteBufferNode)
        {
            if (ABomNetworks.TryGetValue(itemSiteBufferNode.RootKey, out var values))
                values.Add(itemSiteBufferNode);
            else
                ABomNetworks.Add(itemSiteBufferNode.RootKey, new List<ABomNetwork>() { itemSiteBufferNode });

            if (ABomNetworksByCurrentItemSiteBuffer.TryGetValue(itemSiteBufferNode.Key, out var value))
                value.Add(itemSiteBufferNode);
            else
                ABomNetworksByCurrentItemSiteBuffer.Add(itemSiteBufferNode.Key, new List<ABomNetwork>() { itemSiteBufferNode });
        }

        public ATSite GetSite(string siteID)
        {
            if (string.IsNullOrEmpty(siteID))
                return null;

            ATSite site = null;
            if (Sites.TryGetValue(siteID, out site) == false)
            {
                //site = ObjectMapper.CreateSite(siteID); // new ATSite(siteID, string.Empty);
                //AddSite(site);
            }

            return site;
        }

        public ATItem GetItem(string itemID)
        {
            if (string.IsNullOrEmpty(itemID))
                return null;

            ATItem item;
            if (Items.TryGetValue(itemID, out item) == false)
            {
                return null;
            }

            return item;
        }

        public IEnumerable<ATItem> GetItems()
        {
            return Items.Values;
        }
      
        public ATBuffer GetBuffer(string bufferid)
        {
            if (string.IsNullOrWhiteSpace(bufferid))
                return null;

            ATBuffer buffer;
            if (Buffers.TryGetValue(bufferid, out buffer) == false)
            {
                return null;
            }

            return buffer;
        }

        public IEnumerable<ATBuffer> GetBuffers()
        {
            return Buffers.Values;
        }

        // ItemBuffer를 생성 및 등록하는 별도의 위치가 필요한가..?
        // BomDetial 등록 이후 해당 작업이 필요해 보이긴함..

        public ATItemSiteBuffer GetItemSite(string siteid, string itemID, string bufferID)
        {
            string key = ATUtil.CreateKey(siteid, itemID, bufferID);

            ATItemSiteBuffer itemSite;
            if (ItemSiteBuffers.TryGetValue(key, out itemSite) == false)
            {
                return null;
            }

            return itemSite;
        }

        public ATItemSiteBuffer GetItemSite(ATSite site, ATItem item, ATBuffer buffer)
        {
            string key = ATUtil.CreateKey(site.SiteID, item.ItemID, buffer.BufferID);

            ATItemSiteBuffer itemSite;
            if (ItemSiteBuffers.TryGetValue(key, out itemSite) == false)
                return null;

            return itemSite;
        }

        public IEnumerable<ATItemSiteBuffer> GetItemSiteBuffers()
        {
            return ItemSiteBuffers.Values;
        }

        public List<ATItemSiteBuffer> GetItemSiteBuffers(string bufferID)
        {
            if (string.IsNullOrEmpty(bufferID))
                return null;

            if (ItemSiteBufferByBuffer.TryGetValue(bufferID, out var values))
                return values;
            else
                return new List<ATItemSiteBuffer>();
        }

        public Dictionary<string, List<ABomNetwork>> GetItemSiteBufferNodes()
        {
            return ABomNetworks;
        }

        public List<ABomNetwork> GetABomNetwork(bool isRoot, string key)
        {
            List<ABomNetwork> retValue = new List<ABomNetwork>();
            List<ABomNetwork> value;

            if (isRoot)
            {
                if (ABomNetworks.TryGetValue(key, out value))
                    retValue = value;
            }
            else
            {
                if (ABomNetworksByCurrentItemSiteBuffer.TryGetValue(key, out value))
                    retValue = value;
            }
            
            return retValue;
        }

        internal void GenerationSplitByAltBom()
        {
            //Dictionary<ATItemSiteBuffer, Dictionary<ATItemSiteBuffer, ATBomDetailAlt>> altBoms = new Dictionary<ATItemSiteBuffer, Dictionary<ATItemSiteBuffer, ATBomDetailAlt>>();
            var boms = ATInputData.Boms.GetBoms(BomType.SplitBy);

            foreach (var bom in boms)
            {
              var altBoms = CreateBomDetailAlt(bom.BomDetails);

                if (altBoms.Count <= 0)
                    continue;

                foreach (var altBom in altBoms)
                {
                    ATItemSiteBuffer isb = altBom.Key;
                    List<ATBomDetailAlt> list = new List<ATBomDetailAlt>(altBom.Value.Values);
                    list.Sort((x, y) => y.AltItem.Grade.CompareTo(x.AltItem.Grade));

                    double grade = int.MaxValue;
                    double priority = 0;

                    HashSet<ATBomDetailAlt> altList = new HashSet<ATBomDetailAlt>();
                    foreach (var alt in list)
                    {
                        if (grade > alt.AltItem.Grade)
                        {
                            priority++;
                            grade = alt.AltItem.Grade;
                        }
                        alt.Priority = priority;
                        altList.Add(alt);
                    }

                    HashSet<ATBomDetailAlt> values;
                    if (isb.AltItemSiteBuffers.TryGetValue(isb.Key, out values) == false)
                    {
                        values = new HashSet<ATBomDetailAlt>();
                        isb.AltItemSiteBuffers.Add(isb.Key, values);
                    }
                    values.AddRange(altList);

                    HashSet<string> keys;
                    if (isb.AltItemSiteBufferKeys.TryGetValue(isb.Key, out keys) == false)
                    {
                        keys = new HashSet<string>();
                        isb.AltItemSiteBufferKeys.Add(isb.Key, keys);
                    }

                    var alts = altList.Select(x => x.AltItemSiteBuffer.Key);
                    keys.AddRange(alts);
                }
            }
        }

        private Dictionary<ATItemSiteBuffer, Dictionary<ATItemSiteBuffer, ATBomDetailAlt>> CreateBomDetailAlt(List<ATBomDetail> details)
        {
            Dictionary<ATItemSiteBuffer, Dictionary<ATItemSiteBuffer, ATBomDetailAlt>> altBoms = new Dictionary<ATItemSiteBuffer, Dictionary<ATItemSiteBuffer, ATBomDetailAlt>>();

            details.Sort((x, y) => y.ToItem.Grade.CompareTo(x.ToItem.Grade));

            foreach (ATBomDetail detail in details)
            {
                double priority = 0;
                foreach (var alt in details)
                {
                    if (object.Equals(alt, detail))
                        continue;

                    if (detail.ToItem.Grade <= alt.ToItem.Grade)
                        continue;

                    priority++;

                    ATBomDetailAlt altbom = new ATBomDetailAlt(detail.Bom, detail.ToSite, detail.ToItem, detail.ToBuffer, alt.ToSite, alt.ToItem, alt.ToBuffer, priority);
                    ATItemSiteBuffer key = detail.ToItemSiteBuffer;

                    Dictionary<ATItemSiteBuffer, ATBomDetailAlt> alts;
                    if (altBoms.TryGetValue(key, out alts) == false)
                        altBoms.Add(key, alts = new Dictionary<ATItemSiteBuffer, ATBomDetailAlt>());

                    if (alts.ContainsKey(alt.ToItemSiteBuffer) == false)
                        alts.Add(alt.ToItemSiteBuffer, altbom);
                }
            }
            
            return altBoms;
        }
    }
}
