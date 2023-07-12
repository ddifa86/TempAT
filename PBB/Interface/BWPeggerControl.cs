using Mozart.SeePlan.Pegging;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Pegger;
using Mozart.SeePlan.Aleatorik.Logic;

namespace Mozart.SeePlan.Aleatorik.Pegger
{
    [FEComponent(FECategory.PlanByBackward, FEControl.PBBPegger, Root = FEProvider.Aleatorik)]
    public partial class PBBPeggerControl : IModelController, IPegControl
    {
        #region IModelController 멤버
        Type IModelController.ControllerType
        {
            get { return typeof(PBBPeggerControl); }
        }

        #endregion

        #region IPegControl
        [FEAction]
        public virtual void OnStepOut(ITargetGroup part)
        {
        }

        [FEAction]
        public virtual void OnStepIn(APEPegPart pegPart)
        {
        }

        [FEAction]
        public virtual bool IsReSelectBom(APETarget target, List<ATBom> boms, APESelectBomContext context)
        {
            return true;
        }

        [FEAction]
        public virtual void OnPrepareSelectBom(APEPegPart pegPart, List<ATBom> boms, APESelectBomContext context)
        {
        }

        [FEAction]
        public virtual double AvailableQty(ATBom bom, APETarget target, List<ATBom> boms, APESelectBomContext context)
        {
            return target.Qty;
        }

        [FEAction]
        public virtual void OnSelectedBom(ATSelectBomInfo info, ATBom bom, APETarget target, List<ATBom> boms, APESelectBomContext context)
        {
        }

        [FEAction]
        public virtual bool IsMoreSelecteBom(ATBom bom, APETarget target, List<ATBom> boms, APESelectBomContext context)
        {
            return false;
        }

        [FEAction]
        public virtual bool IsShortPegPart(APEPegPart pegPart)
        {
            if (pegPart.Status == Status.Short)
                return true;

            return false;
        }

        [FEAction]
        public virtual void OnApplyRefPlan(APEPegPart pegPart, APERefPlan refPlan)
        {
        }

        [FEAction]
        public virtual void OnCreateTargetGroup(List<APETargetGroup> groups, ATOperation oper)
        {
        }

        [FEAction]
        public virtual void OnStartLevel(int curLevel, int maxLevel, List<APETargetGroup> targetGroups, APEPegContext context, bool isRun)
        {
        }

        [FEAction]
        public virtual void OnPrepareVirtualPegging(APETargetGroup targetGroup, List<APEWip> candidateWips, APEPegContext contxt, bool isRun)
        {
        }

        [FEAction]
        public virtual void OnEndLevel(int curLevel, int maxLevel, List<APETargetGroup> targetGroups, APEPegContext context, bool isRun)
        {
        }

        [FEAction]
        public virtual void OnCompleteWipPegging(ATOperTarget operTarget, APETarget target, APEWip wip, double pegQty, APEPegContext context)
        {
        }
        #endregion

        [FEAction]
        public virtual List<ITargetGroup> CustomSplitPegPart(ITargetGroup pegPart)
        {
            List<ITargetGroup> pegParts = new List<ITargetGroup>() { pegPart };

            return pegParts;
        }

        [FEAction]
        public virtual void OnCompleteWipPegging(APETargetGroup targetGroup, APETarget target, APEWip peggedWip, double peggedQty, APEPegContext contxt, bool isRun)
        {

        }

        [FEAction]
        public virtual void OnAddPeggingGroup(APEPegPart pegPart, APEPeggingGroup peggingGroup)
        {

        }

        [FEAction]
        public virtual void OnSelectedPeggingGroup(APEPeggingGroup peggingGroup)
        {

        }
    }
}
