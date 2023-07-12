using System;
using System.Collections.Generic;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Cbsim;
using Mozart.Simulation.Engine;

namespace Mozart.SeePlan.Aleatorik
{

    public partial class PBFModuleExecutionInfo : ModuleExecutionInfo
    {
        private List<DateTime> timeStamp;

        /// <summary>
        /// 현재와 이전 Bucket 시작 시간의 목록입니다.
        /// </summary>
        internal List<DateTime> TimeStamp => this.timeStamp;

        #region 필수 Input

        public List<APEWip> Wips { get; internal set; }

        public List<ATOperTarget> InTargets { get; internal set; }

        public List<APEWip> UnpeggedWips { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public List<APEWip> IncomingWips { get; internal set; }

        /// <summary>
        /// 공정별 OperTarget 정보
        /// </summary>
        public Dictionary<IComparable, List<ATOperTarget>> OperTargets { get; internal set; }

        /// <summary>
        /// PBB 시점에 생성된 Assembly 공정별 Target 정보
        /// </summary>
        public Dictionary<ATBuffer, List<ATOperTarget>> AssemblyTargets { get; internal set; }

        /// <summary>
        /// PBB 시점에 SplitBy 에 생성된 Wip의 Pegging 정보
        /// </summary>
        public Dictionary<string, List<APEWip>> BinPeggedInfos { get; internal set; }
        #endregion

        /// <summary>
        /// StageOut된 Wip의 정보
        /// </summary>
        public List<APEWip> StageOutWips { get; internal set; }

        internal double AllocationKey { get; set; }

        internal PBFModuleExecutionInfo(string key, ATStage stage, int sequence, string refKey = null)
            : base(key, ModuleType.PBF, stage, sequence, refKey)
        {
            this.BinPeggedInfos = new Dictionary<string, List<APEWip>>();
            this.Wips = new List<APEWip>();
            this.InTargets = new List<ATOperTarget>();
            this.UnpeggedWips = new List<APEWip>();
            this.AssemblyTargets = new Dictionary<ATBuffer, List<ATOperTarget>>();
            this.IncomingWips = new List<APEWip>();
            this.StageOutWips = new List<APEWip>();
            this.OperTargets = new Dictionary<IComparable, List<ATOperTarget>>();
        }

        internal override void OnStarted()
        {
            base.OnStarted();
            this.timeStamp = new List<DateTime>();
        }

        internal override void OnEnded()
        {
            base.OnEnded();
 
        }

        internal override void OnDone()
        {
            this.timeStamp.Clear();
            this.timeStamp = null;
        }

        internal override void OnPrepareInput()
        {
         
        }

        internal override void OnPrepareOutput()
        {
             
        }


        internal void AddOutStockWip(APELot lot)
        {
            var wipInfo = new ATWipInfo(lot.LotID, lot.Qty, WipType.Inventory, LotState.Wait, lot.CurrentItemSiteBuffer.Site, lot.CurrentItemSiteBuffer.Item, lot.CurrentRoute, lot.CurrentOper, null, lot.LastStepTime, lot.LastStepTime, this.Stage);

            wipInfo.ItemSiteBuffer = lot.CurrentItemSiteBuffer;

            APEWip wip = new APEWip(wipInfo, LotCreateType.StageOut);
            wip.StageOutSalesOrder = lot.CurrentTarget.SODemand;

            this.StageOutWips.Add(wip);
        }

        //public List<OperTarget> GetOperTarget(string operID, string gubun)
        //{
        //    var key = Tuple.Create(operID, gubun);

        //    List<OperTarget> finds;
        //    if (this.operInTargetDic.TryGetValue(key, out finds))
        //        return finds;

        //    return new List<OperTarget>();
        //}

        //public List<OperTarget> GetStageInTarget(string operID, string gubun)
        //{
        //    return this.StageInTarget.FindAll(s => s.Gubun == gubun && s.OperID == operID && s.TargetQty > 0);
        //}

        /// <summary>
        /// 지정된 시간이 속한 버켓 구간의 시작 시간을 반환합니다.
        /// </summary>
        /// <param name="nowDt">현재 시간입니다.</param>
        /// <returns>버켓 구간의 시작 시간입니다.</returns>
        //internal DateTime GetBucketStartTime(DateTime nowDt)
        //{
        //    var planStartTime = BCPOption.Instance.PlanStartTime;
        //    if (nowDt < planStartTime)
        //        return DateTime.MinValue;

        //    var bucketStartTime = DateTime.MinValue;
        //    for (var i = this.TimeStamp.Count - 1; i >= 0; i--)
        //    {
        //        var stamp = this.TimeStamp[i];
        //        if (stamp == nowDt)
        //            return nowDt;

        //        if (stamp < nowDt)
        //        {
        //            bucketStartTime = stamp;
        //            break;
        //        }
        //    }

        //    if (bucketStartTime == DateTime.MinValue)
        //        bucketStartTime = planStartTime;

        //    var limit = 1000;
        //    while (bucketStartTime <= nowDt)
        //    {
        //        limit--;
        //        if (limit == 0)
        //            break;

        //        var cycleTime = CbsControl.GetBucketCycleTime(bucketStartTime);
        //        if (cycleTime <= Time.Zero)
        //            return DateTime.MinValue;

        //        var bucketEndTime = bucketStartTime.Add(cycleTime);
        //        if (nowDt < bucketEndTime)
        //            return bucketStartTime;

        //        bucketStartTime = bucketEndTime;
        //    }

        //    return DateTime.MinValue;
        //}
    }
}
