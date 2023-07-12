using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public interface IPegControl
    {
        public bool IsReSelectBom(APETarget target, List<ATBom> boms, APESelectBomContext context);

        public void OnPrepareSelectBom(APEPegPart pegPart, List<ATBom> boms, APESelectBomContext context);

        public double AvailableQty(ATBom bom, APETarget target, List<ATBom> boms, APESelectBomContext context);

        public void OnSelectedBom(ATSelectBomInfo info, ATBom bom, APETarget target, List<ATBom> boms, APESelectBomContext context);

        public bool IsMoreSelecteBom(ATBom bom, APETarget target, List<ATBom> boms, APESelectBomContext context);

        public bool IsShortPegPart(APEPegPart pegPart);

        public void OnApplyRefPlan(APEPegPart pegPart, APERefPlan refPlan);

        public void OnStepOut(ITargetGroup part);

        public void OnStepIn(APEPegPart pegPart);

        public void OnCreateTargetGroup(List<APETargetGroup> groups, ATOperation oper);

        public void OnStartLevel(int curLevel, int maxLevel, List<APETargetGroup> targetGroups, APEPegContext context, bool isRun);

        public void OnPrepareVirtualPegging(APETargetGroup targetGroup, List<APEWip> candidateWips, APEPegContext contxt, bool isRun);

        public void OnEndLevel(int curLevel, int maxLevel, List<APETargetGroup> targetGroups, APEPegContext context, bool isRun);

        public void OnCompleteWipPegging(ATOperTarget operTarget, APETarget target, APEWip wip, double pegQty, APEPegContext context);
    }
}
