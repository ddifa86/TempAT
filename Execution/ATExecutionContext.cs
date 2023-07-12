
using Mozart.Simulation.Engine;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;
 

namespace Mozart.SeePlan.Aleatorik
{
    public class ATExecutionContext
    {
        #region Enums

        /// <summary>
        /// BCP 패키지 실행 상태를 나타냅니다.
        /// </summary>
        public enum ExecutionStep
        {
            /// <summary>
            /// 패키지를 초기화하는 중입니다.
            /// </summary>
            INITIALIZE,
            /// <summary>
            /// 패키지 내 모듈을 초기화하는 중입니다.
            /// </summary>
            MODULE_INIT,
            /// <summary>
            /// 패키지내 모듈을 실행하는 중입니다.
            /// </summary>
            MODULE_RUN,
            /// <summary>
            /// 패키지 실행을 완료한 상태입니다.
            /// </summary>
            DONE,
        }

        #endregion

        #region Variables

        private List<ModuleExecutionInfo> executionInfos;
        private List<ATStage> stages;
        private Dictionary<string, ATStage> stageDic;
        private Dictionary<string, object> options;

        private Dictionary<string, ATAllocationGroup> allocationGroups;

        #endregion

        #region Properties

        public static ATExecutionContext Instance
        {
            get
            {
                return ServiceLocator.Resolve<ATExecutionContext>();
            }
        }

        public Dictionary<string, object> Options
        {
            get
            {
                if (options == null)
                    options = new Dictionary<string, object>();

                return options;
            }
        }

        public List<ModuleExecutionInfo> ExecutionInfos
        {
            get
            {
                if (this.executionInfos == null)
                    this.executionInfos = new List<ModuleExecutionInfo>();

                return this.executionInfos;
            }
        }

        public ModuleExecutionInfo CurrentExecutionInfo { get; private set; }

        public ModuleExecutionInfo PrevExecutionInfo
        {
            get
            {
                if (this.CurrentExecutionInfo == null)
                    return null;

                var index = this.ExecutionInfos.IndexOf(this.CurrentExecutionInfo);
                if (index == 0)
                    return null;

                return this.GetExecutionInfo(index - 1);
            }
        }

        public ModuleExecutionInfo NextExecutionInfo
        {
            get
            {
                if (this.CurrentExecutionInfo == null)
                    return null;

                var index = this.ExecutionInfos.IndexOf(this.CurrentExecutionInfo);
                if (index == this.executionInfos.Count - 1)
                    return null;

                return this.GetExecutionInfo(index + 1);
            }
        }

        public List<ATStage> Stages
        {
            get
            {
                if (this.stages == null)
                {
                    this.stages = new List<ATStage>();
                    this.stageDic = new Dictionary<string, ATStage>();
                }

                return stages;
            }
        }


        public Dictionary<string, ATStage> StageDic
        {
            get
            {
                if (this.stageDic == null)
                {
                    this.stages = new List<ATStage>();
                    this.stageDic = new Dictionary<string, ATStage>();
                }

                return stageDic;
            }
        }

        public ATStage CurrentStage
        {
            get
            {
                if (this.CurrentExecutionInfo == null)
                    return null;

                return this.CurrentExecutionInfo.Stage;
            }
        }

        public string CurrentVersion
        {
            get
            {
                return "-";
            }
        }

        public Dictionary<string, ATAllocationGroup> AllocationGroups
        {
            get
            {
                if (this.allocationGroups == null)
                {
                    this.allocationGroups = new Dictionary<string, ATAllocationGroup>();
                }
                return allocationGroups;
            }
        }
        //Stage 내 Level 정보를 관리하는 부분..
        //public Process CurrentProcess
        //{
        //    get
        //    {
        //        if (this.CurrentStage == null)
        //            return null;

        //        return this.CurrentExecutionInfo.Stage.Process;
        //    }
        //}

        public ATStage PrevStage
        {
            get
            {
                var currentStage = this.CurrentStage;
                if (currentStage == null)
                    return null;

                return this.GetPrevStage(currentStage);
            }
        }

        public ATStage NextStage
        {
            get
            {
                var currentStage = this.CurrentStage;
                if (currentStage == null)
                    return null;

                return this.GetNextStage(currentStage);
            }
        }

        /// <summary>
        /// 현재 패키지 실행 상태를 가져옵니다.
        /// </summary>
        public ExecutionStep CurrentExecutionStep { get; internal set; }

        #endregion

        #region Constructors

        public ATExecutionContext()
        {
        }

        #endregion

        #region Methods

        public void AddExecutionInfo(ModuleExecutionInfo info)
        {
            int index = this.ExecutionInfos.BinarySearch(info, ModuleExecutionComparer.Default);
            if (index < 0)
                index = ~index;

            this.ExecutionInfos.Insert(index, info);
        }

        public ModuleExecutionInfo GetExecutionInfo(int index)
        {
            return this.GetExecutionInfo(index, false);
        }
 
        internal ModuleExecutionInfo GetExecutionInfo(int index, bool setCurrent)
        {
            try
            {
                var info = this.ExecutionInfos[index];

                if (setCurrent)
                    this.CurrentExecutionInfo = info;

                return info;
            }
            catch
            {
                return null;
            }
        }

        public ModuleExecutionInfo GetExecutionInfo(string key)
        {
            return this.ExecutionInfos.Where(x => x.Key == key).FirstOrDefault();
        }

       

        internal bool HasRelatedExecutionInfo(ModuleExecutionInfo info)
        {
            return this.ExecutionInfos.Any(x => x.RefKey == info.Key && x.Sequence > info.Sequence);
        }

        public void AddStage(ATStage stage)
        {
            int index = this.Stages.BinarySearch(stage, StageComparer.Default);
            if (index < 0)
                index = ~index;

            this.Stages.Insert(index, stage);
            this.stageDic.Add(stage.StageID, stage);

            return ;
        }

        public ATStage GetStage(string stageID)
        {
            if (string.IsNullOrEmpty(stageID))
                return null;


            ATStage stage;
            this.StageDic.TryGetValue(stageID, out stage);

            return stage;
        }
 

        public ATStage GetPrevStage(ATStage stage)
        {
            var index = this.Stages.IndexOf(stage);
            if (index == 0)
                return null;

            return this.Stages[index - 1];
        }
 

        public ATStage GetNextStage(ATStage stage)
        {
            var index = this.Stages.IndexOf(stage);
            if (index == this.Stages.Count - 1)
                return null;

            return this.Stages[index + 1];
        }
         

        #endregion

        #region Inner Class

        internal class ModuleExecutionComparer : IComparer<ModuleExecutionInfo>
        {
            public static ModuleExecutionComparer Default = new ModuleExecutionComparer();

            public int Compare(ModuleExecutionInfo x, ModuleExecutionInfo y)
            {
                if (object.ReferenceEquals(x, y))
                    return 0;

                int cmp = x.Sequence.CompareTo(y.Sequence);
                if (cmp == 0)
                    cmp = x.Key.CompareTo(y.Key);

                return cmp;
            }
        }

        internal class StageComparer : IComparer<ATStage>
        {
            public static StageComparer Default = new StageComparer();

            public int Compare(ATStage x, ATStage y)
            {
                if (object.ReferenceEquals(x, y))
                    return 0;

                //var cmp = x.Sequence.CompareTo(y.Sequence);
                //if (cmp == 0)
                //    cmp = x.StageID.CompareTo(y.StageID);
                //return cmp;

                return 0;
            }
        }

        #endregion
    }
}
