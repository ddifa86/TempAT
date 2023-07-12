using Mozart.SeePlan.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public interface ITargetGroup
    {
        // IComparable Key { get; } PegPart에서의 Key가 어떤 의미로 사용될지?? 

        List<ITarget> Targets { get; }

        ITarget Sample { get; }

        Step Step { get; set; }

        PegPosition PegPosition { get; set; }

        void AddTargets(IEnumerable<ITarget> targets);

        void RemoveTarget(ITarget target);

        void Apply(Action<ITargetGroup, ITargetGroup> action);

        void SetPegPosition(PegPosition position);

        ITargetGroup Clone();
    }
}
