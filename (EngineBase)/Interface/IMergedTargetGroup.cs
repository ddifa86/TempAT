using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    /// <summary>
    /// APEPeggingGroup
    /// </summary>
    public interface IMergedTargetGroup : ITargetGroup
    {
        IComparable Key { get; set; }

        /// <summary>
        /// Merge된 TargetGroup 의 리스트 입니다.
        /// </summary>
        List<ITargetGroup> Items { get; }

        void Merge(ITargetGroup targetGroup);
    }
}
