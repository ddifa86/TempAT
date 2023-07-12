using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{

    public class APELine
    {
        //internal ATAssemblyManager _assyManager;
        //internal ATReleaseManager _releaseManager;
        // LotQueueManager
        // ReleaseManager
        // ResourceManager
        // Allocator
        // Transfer

        internal FactoryControl FactoryControl;
        //internal AssemblyInterface AssemblyControl;


        public APELine()
        {            
            var current = ATExecutionContext.Instance.CurrentExecutionInfo.ModuleType;
            if (current == ModuleType.PBO)
            {
                FactoryControl = ObyO.PBOInterface.PlanControl;
                //
            }
            else if (current == ModuleType.PBF)
            {
                FactoryControl = Planner.FWInterface.LotControl;
                //
            }

          //  _assyManager = new ATAssemblyManager(this);
          //  _releaseManager = new ATReleaseManager(this);
        }

        internal virtual void MoveFirst(APELot lot)
        {
            //if (lot.LotType == LotCreateType.Normal)
            //    lot.LastStepTime = this.Factory.NowDT;

            //APEWipAgent.Instance.ReleasedLot(lot);

            //FWInterface.LotControl.OnReleasedLot(lot);

            //#region Assembly 시점에 처리
            ////if (lot.AssemblyInfo != null)
            ////{
            ////    foreach (var detail in lot.AssemblyInfo.PartInfo.Keys)
            ////    {
            ////        var dic = lot.AssemblyInfo.PartInfo[detail];

            ////        foreach (var pairkey in dic)
            ////        {
            ////            var partLot = pairkey.Key;
            ////            var usedQty = pairkey.Value;

            ////            lot.Plans.AddRange(partLot.Plans);
            ////            lot.CapaPlans.AddRange(partLot.CapaPlans);

            ////            // PartLot들이 Split되어 Assembly에 기여하더라도 최초에 한번 기록하면 문제가 없을 것으로 보임
            ////            partLot.Plans.Clear();
            ////            partLot.CapaPlans.Clear();

            ////            // partLot의 LifeCycle 출력
            ////            OutputWriter.Instance.WriteLotHistory(partLot, usedQty, LifeCycle.Assembly.ToString(), lot.LastStepTime, string.Format("Assembly Lot : {0}({1})", lot.LotID, lot.Qty));

            ////            // partLot의 Assembly 정보 출력.
            ////            OutputWriter.Instance.WriteLotAssemblyLog(lot, detail, partLot, usedQty, lot.LastStepTime);

            ////            // 사용된 partlot의 처리 작업
            ////            APEAssemblyAgent.Instance.UpdatePartLot(partLot, usedQty);
            ////        }
            ////    }

            ////    lot.AssemblyInfo = null;
            ////}
            //#endregion

            //LotState position = LotState.Wait;

            //if (lot.IsRunWip)
            //{
            //    lot.MoveFirst(Factory.NowDT);

            //    //// 초기 RUNTAT 반영 필요 혹은 PROCESS / TACTTIME을 고려한 OUT 시간 설정 필요
            //    FWDummyBucketLogic.Instance.DoAllocate(lot);

            //    lot.LotState = LotState.Out;
            //}

            //MoveNext(lot, position);
        }

        public virtual void MoveNext(APELot lot, LotState position = LotState.Run)
        {
        }

        // MoveFirset

        // MoveNext

        // OnStepOut
        // PartChange
        // 수율반영
        //

        // IsFinished

        // OnStepIn

        // PartChange
    }
}
