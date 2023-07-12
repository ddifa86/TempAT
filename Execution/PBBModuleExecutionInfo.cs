using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public partial class PBBModuleExecutionInfo : ModuleExecutionInfo
    {
        #region 필수 Input Data

        /// <summary>
        /// Stage 내부 활용 Demand 정보
        /// Key : DemandID
        /// </summary>
        /// 
        public List<ATDemand> Demands { get; internal set; }
        
        #endregion

        #region Module Execution Base Result

        public Dictionary<string, List<APEWip>> BinPeggedInfos { get;  set; }

        public List<APEWip> PeggedWips { get;  set; }

        public List<ATOperTarget> StageInTargets { get;  set; }

        public Dictionary<ATBuffer, List<ATOperTarget>> StageInAssemblyTargets { get;  set; }

        public List<APEWip> UnpeggedWips { get;  set; }

        public Dictionary<IComparable, List<ATOperTarget>> OperTargets { get; internal set; }

        public HashSet<APEPegPart> PegParts { get; set; }
        #endregion

        internal PBBModuleExecutionInfo(string key, ATStage stage, int sequence, string refKey)
            : base(key, ModuleType.PBB, stage, sequence, refKey)
        {
            // peggign 결과를 저장하는 정보들.  
            // 실행 결과를 저장하는 객체로 위치 변경 필요.
            this.StageInTargets = new List<ATOperTarget>();
            this.StageInAssemblyTargets = new Dictionary<ATBuffer, List<ATOperTarget>>();
            this.PeggedWips = new List<APEWip>();
            this.UnpeggedWips = new List<APEWip>();
            this.BinPeggedInfos = new Dictionary<string, List<APEWip>>();
            this.Demands = new List<ATDemand>();
            this.OperTargets = new Dictionary<IComparable, List<ATOperTarget>>();
            this.PegParts = new HashedSet<APEPegPart>();
        }

        internal virtual void PrepareInput()
        {
        }

        internal virtual void PrepareOutput()
        {
        }

        internal override void OnStarted()
        {
            base.OnStarted();
        }

        internal override void OnEnded()
        {
            base.OnEnded();
#warning TODO: Output 기록
            //if (this.Key == null)
            //{
            //    // TODO: Null 처리
            //    return;
            //}
            //var Info = LcdMPContext.Instance.GetExecutionInfo(this.Key);
            //var Myinfo = Info as PeggingModuleExecutionInfo;

            //if (this.stageInTarget != null)
            //    this.WriteStageInTarget(Myinfo);
            //if (this.operTarget != null)
            //    this.WriteOperTarget(Myinfo);
            //if (this.peggedWip != null)
            //    this.WritePeggedWip(Myinfo);
            //if (this.unpeggedWip != null)
            //    this.WriteUnPeggedWip(Myinfo);

#warning 모듈 정보 정리
            //this.Demands.Clear();

            //if (!MPContext.Instance.HasRelatedExecutionInfo(this))
            //{
            //    if (this.StageInTarget != null) this.StageInTarget.Clear();
            //    if (this.OperTarget != null) this.OperTarget.Clear();
            //    if (this.PeggedWip != null) this.PeggedWip.Clear();
            //    if (this.UnpeggedWip != null) this.UnpeggedWip.Clear();
            //}
        }

        internal override void OnPrepareInput()
        {
        }

        internal override void OnPrepareOutput()
        {
        }

        /// <summary>
        /// Pegging 결과..? 다른 위치로의 이동이 필요. ModueExecutionInfo .. ModueExecutionResult..?
        /// </summary>

        internal void AddUnpegWips(APEWip wip)
        {
            this.UnpeggedWips.Add(wip);
        }

        internal void AddPegWips(APEWip wip)
        {
            if (wip.CreationType == LotCreateType.SplitByBom)
            {
                // BinPegging 이력 정보 별도 저장
                // 추후 F/W에서 활용하기 위한 작업 진행.
                //wip.BinPegInfo = wip.BinPegInfo.ShallowCopy();
                //wip.BinPegInfo.Phase = context.Phase;
                //wip.BinPegInfo.PeggedQty = qty;

                string key = wip.SourceTargetID + wip.ItemID + wip.OperID;
                if (this.BinPeggedInfos.TryGetValue(key, out var value))
                    value.Add(wip);
                else
                    this.BinPeggedInfos.Add(key, new List<APEWip>() { wip });
            }
            else
            {
                this.PeggedWips.Add(wip);
            }
        }
    }

}

