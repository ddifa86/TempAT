using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.ObyO;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class PBOBackwardLogic
    {
        public static PBOBackwardLogic Instance
        {
            get
            {
                return ServiceLocator.Resolve<PBOBackwardLogic>();
            }
        }

        internal Dictionary<IComparable, APETargetGroup> GetTargetGroups(HashSet<APEPegPart> pegParts)
        {
            var factors = ATRuleAgent.Instance.CurrentRuleSet.GetRule(RulePoint.TargetGroupKey, CallType.Operation);

            Dictionary<IComparable, APETargetGroup> targetGroups = new Dictionary<IComparable, APETargetGroup>();

            foreach (var pp in pegParts)
            {
                IComparable key = null;
                if (factors == null || factors.FactorList.Count <= 0)
                {
                    key = pp.MoMaster.Key;
                }
                else
                {
                    foreach (var factor in factors.FactorList)
                    {
                        var method = factor.Method as TargetGroupKey;
                        if (key == null)
                            key = method(pp);
                        else
                            key += "@" + method(pp);
                    }
                }

                if (targetGroups.TryGetValue(key, out var tg) == false)
                {
                    tg = new APETargetGroup(key);
                    tg.CurrentOperation = pp.CurrentOperation;

                    targetGroups.Add(key, tg);
                }

                tg.Merge(pp);
            }

            return targetGroups;
        }

        public void CreateTarget(ITargetGroup part, bool isOut)
        {
            part.Apply((x, _) =>
            {
                var pegPart = x as APEPegPart; 
                BackwardCommonLogic.Instance.CreateTarget(pegPart, isOut);
            });
        }

        public List<ATSelectBomInfo> DoSelectBom(APETarget target, List<ATBom> boms, APESelectBomContext context)
        {
            List<ATSelectBomInfo> selectBoms = new List<ATSelectBomInfo>();

            foreach (var bom in boms)
            {
                if (target.RemainQty <= ATOption.Instance.MinimumAllocationQuantity)
                    break;

                ATSelectBomInfo info = new ATSelectBomInfo(target, bom, SelectType.Select, context);

                double availQty = PBOInterface.PegControl.AvailableQty(bom, target, boms, context);

                if (availQty <= ATOption.Instance.MinimumAllocationQuantity)
                    continue;

                info.Qty = availQty;

                PBOInterface.PegControl.OnSelectedBom(info, bom, target, boms, context);

                selectBoms.Add(info);

                target.RemainQty -= availQty;

                // 다른 봄에다가 분배여부.
                if (PBOInterface.PegControl.IsMoreSelecteBom(bom, target, boms, context) == false)
                    break;
            }

            return selectBoms;
        }
    }
}

