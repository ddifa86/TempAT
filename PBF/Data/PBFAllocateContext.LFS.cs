using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public partial class PBFAllocateContext : BaseContext
    {

        /// <summary>
        /// 현재 할당중인 장비 그룹 정보
        /// </summary>
        public PBFResourceGroup SelectedBucketGroup { get; internal set; }

        internal ATWeightPreset CompareLotGroupOnLFSPreset;
        internal ATWeightPreset CompareResourceOnLFSPreset;
        internal ATWeightPreset CompareAddResourcePreset;
        internal ATWeightPreset FilterLotGroupInLevelPreset;
        internal ATWeightPreset FilterLotGroupOnLFSPreset;
        internal ATWeightPreset FilterResourceOnLFSPreset;

        /// <summary>
        /// 현재 Level에서 할당 대상인 LotGroup 정보
        /// </summary>
        public List<APELotGroup> SelectedLotGroupInLevel { get; internal set; }

    }
}
