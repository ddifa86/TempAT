using Mozart.SeePlan.Aleatorik.Data;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public class AssemblyLogic
    {
        public static AssemblyLogic Instance
        {
            get
            {
                return ServiceLocator.Resolve<AssemblyLogic>();
            }
        }

        public void OnAddPartLot(APELot partLot)
        {

        }

        public List<APELot> FilterAssyLot(List<APELot> possibleAssemblyLots, string soDemandID)
        {
            List<APELot> lots = new List<APELot>();
            foreach (var info in possibleAssemblyLots)
            {
                if (info.CurrentTarget.SODemand.ID != soDemandID)
                    continue;

                lots.Add(info);
            }

            //var solver = APESolver.Instance;
            //var fence = solver.NowDT.AddDays(210);

            //var lst = possibleAssemblyLots.Where(x => x.CurrentTarget.TargetDateTime < fence);

            //return lst.ToList();       
            return lots;
;
        }

        /// <summary>
        /// 지정된 부품 작업물을 조립에 사용할 수 있는지 여부를 반환합니다. 기본 반환값은 <see langword="true"/>입니다.
        /// </summary>
        /// <param name="part">조립 작업물입니다.</param>
        /// <param name="assemblyLot">조립 후 작업물입니다.</param>
        /// <param name="now">현재 시간입니다.</param>
        /// <returns>조립 가능 여부입니다.</returns>
        /// 
        public bool CanAssemblyPartLot(APELot part, APELot assemblyLot, DateTime now, Dictionary<ATBomDetail, List<APELot>> availablePartLot, KeyValuePair<ATBomDetail, List<APELot>> pairKey)
        {
            return FWInterface.AssemblyControl.CanAssemblyPartLot(part, assemblyLot, now, availablePartLot, pairKey);
        }

        public bool CanAssembleSmallBatch(APELot assemblyLot, double qty, DateTime now)
        {
            return FWInterface.AssemblyControl.CanAssembleSmallBatch(assemblyLot, qty, now);
        }
    }
}
