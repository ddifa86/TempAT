
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Inputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class BomHelper
    {
        private Dictionary<string, ATBom> _boms = new Dictionary<string, ATBom>();
        private Dictionary<string, List<ATBomDetail>> _bomDetails = new Dictionary<string, List<ATBomDetail>>();
        private Dictionary<string, ATRoute> _routes = new Dictionary<string, ATRoute>();
        private Dictionary<string, ATBomRouting> _bomRoutes = new Dictionary<string, ATBomRouting>();

        private HashedSet<ATOperation> _operationTypeOper = new HashedSet<ATOperation>();

        /// <summary>
        /// BomRoute 정보 등록
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool AddBomRoutes(ATBomRouting obj)
        {
            string key = obj.Bom.BomID + obj.RouteID;

            if (_bomRoutes.ContainsKey(key))
                return false;
            
            _bomRoutes.Add(key, obj);
            return true;
        }

        public void AddBomDetails(ATBomDetail obj)
        {
            if (_bomDetails.TryGetValue(obj.ToBufferID, out var value))
                value.Add(obj);
            else
                _bomDetails.Add(obj.ToBufferID, new List<ATBomDetail>() { obj });
        }

        public List<ATBomDetail> GetBomDetails(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            if (_bomDetails.TryGetValue(key, out var value))
                return value;
            else
                return new List<ATBomDetail>();
        }

        public Dictionary<string, List<ATBomDetail>> GetBomDetails()
        {
            return _bomDetails;
        }

        public ATBomRouting GetBomRouting(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            if (_bomRoutes.TryGetValue(key, out var value))
                return value;
            else
                return null;
        }

        public bool AddBom(ATBom bom)
        {
            if (_boms.ContainsKey(bom.BomID) == true)
                return false;

            _boms.Add(bom.BomID, bom);
            return true;
        }

        public bool AddRoute(ATRoute route)
        {
            if (_routes.ContainsKey(route.RouteID))
                return false;

            _routes.Add(route.RouteID, route);
            return true;
        }

        public ATBom GetBom(string bomid)
        {
            if (string.IsNullOrEmpty(bomid))
                return null;

            ATBom bom;
            if (_boms.TryGetValue(bomid, out bom) == false)
                return null;

            return bom;
        }

        public List<ATBom> GetBoms(BomType type)
        {
            var lst = _boms.Values.Where(x => x.BomType == type).ToList();

            return lst;
        }

        public List<ATBom> GetBoms()
        {
            return _boms.Values.ToList();
        }
       
        public ATRoute GetRoute(string routeid)
        {
            if (string.IsNullOrEmpty(routeid))
                return null;

            ATRoute route;
            if (_routes.TryGetValue(routeid, out route) == false)
                return null;

            return route;
        }

        public IEnumerable<ATRoute> GetRoute()
        {
            return _routes.Values;
        }

        public void RemoveRoute(string key)
        {
            _routes.Remove(key);
        }

        public void RemoveBom(string key)
        {
            _boms.Remove(key);
        }

        public ATRoute FindRoute(APEPegPart pegPart, ATBom bom)
        {
            var routes = bom.BomRoutes;
            var route = routes.FirstOrDefault()?.Route;

            return route;
        }

        public BomType GetChangeBomType(ATBom bom, ATOperation currentOper, bool isOut)
        {
            ATRoute route = bom.BomRoutes.FirstOrDefault().Route;
            if (isOut)
            {
                if (route.LastOper != currentOper)
                    return BomType.None;

                if (bom.BomType != BomType.Assembly)
                    return bom.BomType;

                return BomType.None;
            }
            else
            {
                if (bom.BomType != BomType.Assembly)
                    return BomType.None;

                if (route.FirstOper != currentOper)
                    return BomType.None;

                return bom.BomType;
            }
        }

        internal void RemoveEmptyBomDetailBoms()
        {
            List<ATBom> boms = new List<ATBom>(_boms.Values);

            foreach(var bom in boms)
            {
                if (bom.BomDetails.Count <= 0)
                {
                    _boms.Remove(bom.BomID);
                }
                else if (bom.BomType == BomType.Assembly && bom.BomDetails.Count == 1)
                {
                    // _boms.Remove(bom.BomID);

                    BOM_MASTER entity = new BOM_MASTER();
                    entity.BOM_ID = bom.BomID;

                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Info, ErrorReasonCode.AutoCorrectionData
                        , entity, ErrKey.BomMaster, "", "BOM_DETAIL of Assembly must be 2 or more rows");

                    bom.BomType = BomType.Normal;
                }

            }
        }

        internal void AddOperationTypeOper(ATOperation oper)
        {
            _operationTypeOper.Add(oper);
        }

        internal HashedSet<ATOperation> GetOperationTypeOper()
        {
            return _operationTypeOper;
        }
    }
}
