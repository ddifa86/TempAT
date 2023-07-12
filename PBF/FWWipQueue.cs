using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public class FWWipQueue
    {
        /// <summary>
        /// Key : lotgroupkey,
        /// 
        /// </summary>
        private Dictionary<string, APELotGroup> _lotGroups;

        public bool HasLotGroup
        {
            get
            {
                return this._lotGroups.Count() > 0;
            }
        }

        public FWWipQueue()
        {
            this._lotGroups = new Dictionary<string, APELotGroup>();
        }

        public void AddLot(string key, IAPELot lot)
        {
            APELotGroup group;
            if (_lotGroups.TryGetValue(key, out group) == false)
            {
                group = new APELotGroup(key);
                _lotGroups.Add(key, group);
            }

            group.AddLot(lot, FWFactory.Instance.DefaultLotInGroupPreset);
        }

        public void RemoveLotGroup(string key)
        {
            if (_lotGroups.ContainsKey(key))
                _lotGroups.Remove(key);
        }

        public void RemoveLot(string key, IAPELot lot, bool isRemoveNoContents)
        {
            APELotGroup group = null;
            if (_lotGroups.TryGetValue(key, out group) == false)
            {
                return;
            }

            group.RemoveLot(lot);

            // 컨텐츠가 없어지면 해당 그룹 삭제
            if (isRemoveNoContents && group.HasContents == false)
            {
                _lotGroups.Remove(key);
            }

            if (isRemoveNoContents == false && group.HasContents == false)
            {
                //_lotGroups.Remove(key);
            }
        }

        public List<APELotGroup> GetLotGroups()
        {
            return _lotGroups.Values.ToList();
        }

        public APELotGroup GetLotGroup(string key)
        {
            APELotGroup group = null;
            if (_lotGroups.TryGetValue(key, out group) == false)
            {
                return null;
            }

            return group;
        }

        public int GetLotCount()
        {
            return _lotGroups.Count();
        }

        //public void RemoveLotGroup(string key)
        //{
        //    if (_lotGroups.ContainsKey(key))
        //        _lotGroups.Remove(key);
        //}
    }
}
