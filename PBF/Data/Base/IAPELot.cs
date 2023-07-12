using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik
{
    public interface IAPELot : ICloneable
    {
        /// <summary>
        /// Batch Key
        /// </summary>
        string FactorObjectKey { get; }

        bool IsReserved { get; set; }

        /// <summary>Get/Set plan quantity.</summary>
        double Qty { get; }

        /// <summary>
        /// The release time of entity to the factory.
        /// </summary>
        DateTime ReleaseTime { get; set; }

        /// <summary>
        /// The latest out time of previsous step
        /// </summary>
        DateTime LastStepTime { get; set; }

        /// <summary>
        /// Temporary qty for  stage calcuration
        /// </summary>
        double CurrentQty { get; set; }

        /// <summary>
        /// The current step id of the batch
        /// </summary>
        string CurrentOperID { get; }

        /// <summary>
        /// The current Step of the batch.
        /// </summary>
        ATOperation CurrentOper { get; }

        /// <summary>
        /// latest loading information
        /// </summary>
       // CbsLoadInfo LastPlan { get; }

        /// <summary>
        /// The Sample Lot of the Lot composing HandlingBatch.
        /// </summary>
        IAPELot Sample { get; }

        /// <summary>
        /// The Weight Factor information of the entity for Dispatching.
        /// </summary>
        //WeightInfo WeightInfo { get; }

        //CbsBucket ReservedBucket { get; set; }

        /// <summary>
        /// 작업물이 포함된 그룹의 키입니다. 
        /// 작업물이 그룹에 추가되면 해당 그룹의 키가 설정됩니다.
        /// 작업물이 그룹에서 제거되면 null로 설정됩니다.
        /// 작업물이 그룹에 포함되지 않은 경우 null입니다.
        /// </summary>
        string LotGroupKey { get; set; }



        ATOperResource CurrentArrange { get; set; }

        /// <summary>
        /// 작업물 할당 로직에서 사용되는 컨텍스트를 가져오거나 설정합니다.
        /// </summary>
        //AllocationContext Context { get; set; }

        /// <summary>
        ///Set the current Step of the entity as the first CbsStep.          
        /// </summary>
        /// <param name="now">Current time of Simulation.</param>
        /// <returns>설정된 첫공정 입니다.</returns>
        ATOperation MoveFirst(DateTime now);

        /// <summary>
        /// Set thi next step of the entity 
        /// </summary>
        /// <param name="now"></param>
        /// <returns></returns>
        ATOperation MoveNext(DateTime now);

        
        /// <summary>
        /// This is used to apply logics for a particular entity included in Batch. 
        /// </summary>
        /// <param name="action">This is the logic delegate to be applied to unit entity.</param>
        void Apply(Action<IAPELot, IAPELot> action);
    }
}
