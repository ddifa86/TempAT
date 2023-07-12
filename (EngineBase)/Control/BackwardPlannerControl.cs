using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class BackwardPlannerControl
    {
        #region AlignQueue
        public virtual void OnSelectPart(ITargetGroup part)
        {
        }

        public virtual IComparable GetAlignKey(ITargetGroup part)
        {
            return null;
        }

        public virtual IComparable GetMergedTargetGroupKey(ITargetGroup part)
        {
            return null;
        }

        public virtual void OnSelectedEntities(List<IMergedTargetGroup> entities, IAlignQueue queue)
        {
        }

        public virtual void OnCompareEntities(List<IMergedTargetGroup> entities, IAlignQueue queue)
        {
        }
        #endregion

        #region Backward Logic

        public virtual void OnPushPart(ITargetGroup part)
        {

        }

        public virtual void OnSelectGroup(ITargetGroup part)
        {
        }

        /// <summary>
        /// 현재 TargetGroup을 여러개의 Target
        /// </summary>
        /// <param name="part"></param>
        /// <returns></returns>
        public virtual List<ITargetGroup> SplitTargetGroup(ITargetGroup part)
        {
            
            return new List<ITargetGroup> { part };
        }

        public virtual List<ITargetGroup> SelectBom(ITargetGroup part)
        {
            return new List<ITargetGroup> { part };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        public virtual void OnStepOut(ITargetGroup part)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pegPart"></param>
        public virtual void OnStepIn(ITargetGroup part)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        /// <param name="isOut"></param>
        /// <returns></returns>
        public virtual void CreateTarget(ITargetGroup part, bool isOut)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        public virtual void ApplyYield(ITargetGroup part)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        /// <param name="isOut"></param>
        public virtual void ApplyTat(ITargetGroup part, bool isOut)
        {
        }

        public virtual Step GetPrevStep(ITargetGroup part)
        {
            return part.Step;
        }

        /// <summary>
        /// pegPart의 종료 여부 판단
        /// </summary>
        /// <param name="part"></param>
        /// <returns>false이면 진행 종료 후 OnDone 호출</returns>
        public virtual bool IsFinished(ITargetGroup part)
        {
            return false;
        }

        public virtual void OnDone(ITargetGroup part)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="part"></param>
        /// <param name="isOut"></param>
        /// <returns></returns>
        public virtual List<object> GetPartChangeInfos(ITargetGroup part, bool isOut)
        {
            return null;
        }

        public virtual ITargetGroup ApplyPartChangeInfo(ITargetGroup part, object partChangeInfo, bool isOut)
        {
            return part;
        }

        #endregion

        #region Pegging
        public virtual void DoPegging(ITargetGroup part, bool isOut)
        {
        }
        #endregion
    }
}
