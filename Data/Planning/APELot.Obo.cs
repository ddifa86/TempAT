
using Mozart.SeePlan.Aleatorik.ObyO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public partial class APELot : IAPELot, IDisposable
    {
        /// <summary>
        /// 장비 할당된 이력 정보.
        /// 추후 RollBack 작업 시 활용.
        /// </summary>
        public List<APEPlanInfo> CapaPlans { get; set; }

        public bool IsShort { get; set; }

        public string ShortCategory { get; set; }

        public string ReasonName { get; set; }

        /// <summary>
        /// 가상 Pegging 한 Wip 정보
        /// </summary>
        public List<APEWip> VirtualPegWips = new List<APEWip>();

        public List<ATSplitInfo> SplitInfos = new List<ATSplitInfo>();

        // Assembly의 경우 PlanInfo 적는 부분 변경.
        public List<ATAssemblyInfo> AssemblyHistory = new List<ATAssemblyInfo>();

        /// <summary>
        ///  Lot별 선행가능 일수.
        /// </summary>
        public double PreBuildDays { get; set; }

        public List<APELot> ChildLots = new List<APELot>();

        public List<APELot> MoLots = new List<APELot>();

        public bool IsMBSSplitLot = false;

        public HashSet<APERefPlan> RefPlans = new HashSet<APERefPlan>();

        public HashSet<string> OrgLotKeys = new HashSet<string>();
    }
}
