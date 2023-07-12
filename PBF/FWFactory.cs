using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Mozart.Simulation.Engine;
using Mozart.Task.Execution;
using Mozart.Threading;
using Mozart.Extensions;
using Mozart.SeePlan.Simulation;
using Mozart.SeePlan.DataModel;
using Mozart.SeePlan.Aleatorik.Data;
 

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public partial class FWFactory : ActiveObject
    {
        public static FWFactory Instance
        {
            get { return ServiceLocator.Resolve<FWFactory>(false); }
        }

        #region Basic
        /// <summary>
        /// Simulator object
        /// </summary>
        public FWExecutor Module { get; private set; }

        /// <summary>
        /// Model Context. 
        /// </summary>
        public ModelContext Context
        {
            get { return this.Module.Context; }
        }
        /// <summary>
        /// Default Logger of Model. 
        /// </summary>
        public IModelLog Logger
        {
            get { return this.Module.Logger; }
        }
        #endregion

        internal Dictionary<string, FWLine> Lines;

        public FWLine DefaultLine { get; internal set; }

        public Time CurrentHorizon { get; internal set; }

        public DateTime NextStartTime { get; internal set; }

        public DateTime EngineStartTime { get; internal set; }

        public List<FWAllocGroup> AllocGroup { get; internal set; }

        #region RuleSet
        internal ATRuleSet DefaultRuleSet
        {
            get
            {
                return ATRuleAgent.Instance.CurrentRuleSet;
            }
        }

        internal ATWeightPreset DefaultLotInGroupPreset;

        internal ATWeightPreset DefaultLotGroupKeyPreset;

        #endregion

        internal FWFactory(Coordinator clock)
            : base(clock)
        {
            this.Module = (FWExecutor)clock.Experiment;
            clock.StopCondition = StepEnded;

            // 추후 라인이 여러개 생성하여 수행하는 경우 감안.
            this.DefaultLine = new FWLine(this, StringUtility.IdentityNull);
            Lines = new Dictionary<string, FWLine>();
            Lines.Add(StringUtility.IdentityNull, this.DefaultLine);
        }

        private bool StepEnded(object sender)
        {
            //ysh
            var clock = (Coordinator)sender;
            return clock.Now == clock.StopTime;
        }

        protected override void OnCreate()
        {
            this.OnConstruct(this.Engine);

            base.OnCreate();
        }

        protected virtual void OnConstruct(Coordinator co)
        {
            ServiceLocator.RegisterInstance<FWFactory>(this);

            // Factory Time 기준으로 다음 공장 시작 시간 보정 작업.
            this.CheckCompensation();
        }

        protected override void OnStart()
        {
            var nowDt = this.NowDT;          
            
            this.EngineStartTime = this.NowDT.StartTimeOfDayT();

            var delay = Time.MaxValue;
            var shiftStart = ShopCalendar.ShiftStartTimeOfDayT(nowDt);

            if (shiftStart == nowDt)
                delay = Time.Zero;
            else
                delay = shiftStart.AddHours(FactoryConfiguration.Current.ShiftHours) - nowDt;


            this.AddTimeout(Time.Zero, (o, e) => this.Run());
        }

        public void Initialize(List<FWAllocGroup> allocGroup)
        {
            this.AllocGroup = allocGroup;
            
            this.DefaultLotInGroupPreset = this.DefaultRuleSet.GetRule(RulePoint.CompareLotInGroup, CallType.Operation);

            this.DefaultLotGroupKeyPreset = this.DefaultRuleSet.GetRule(RulePoint.LotGroupKey, CallType.Operation);
        }
         
        private void Run()
        {
            this.CurrentHorizon = GetBucketCycleTime(NowDT);
            this.NextStartTime = GetNextWorkingStartTime(NowDT, this.CurrentHorizon); 
            // 로그 남기기.
            this.Step(line => line.RunAllocate());
        }

        private void Step(Action<FWLine> action)
        {
            //this.AgentManager.Run();
            var now = this.NowDT;

            //ATElapsedTimeChecker.Instance.StartCustomTimer("#CycleTime");
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            var list = this.Lines.Values.ToList();

            if (list.Count > 1 && ATOption.Instance.ApplyThreadedLineAllocation)
            {
                var n = 0;
                var tasks = new System.Threading.Tasks.Task[list.Count];
                foreach (var line in list)
                {
                    tasks[n] = new System.Threading.Tasks.Task(() => action(line));
                    tasks[n++].Start();
                }

                System.Threading.Tasks.Task.WaitAll(tasks);
            }
            else
            {
                foreach (var line in list)
                {
                    action(line);
                }
            }

            timer.Stop();

            float freq = System.Diagnostics.Stopwatch.Frequency / 10000000f;

            Logger.MonitorInfo(string.Format("\t\t+ {0} => elapsed : {1} \t\t now : {2} ", "#CycleTime",
                     new TimeSpan((long)(timer.ElapsedTicks / freq)), now.ToString("yyyy/MM/dd HH:mm:ss")
                    )
                );

            //ATElapsedTimeChecker.Instance.StopCustomTimer("#CycleTime", false);

            this.Rolling();
        }


        public virtual Time GetBucketCycleTime(DateTime now)
        {
            var horizon = Time.FromMinutes(ATOption.Instance.BucketCycleTimeMinutes);

            return horizon; 
        }

        public virtual DateTime GetNextWorkingStartTime(DateTime now, Time horizon)
        {
            var next = (DateTime)(DateUtility.GetStartTimeOfInterval(now, (TimeSpan)horizon) + horizon);
            var wc = FactoryConfiguration.Current.GetWorkCalendar();

            var adj = wc.NextWorkingTime(next);
            if (next != adj)
            {
                var t = next;
                while (t < adj)
                    t = (DateTime)(t + horizon);
                next = (DateTime)(t - horizon);
            }

            return next;
        }


        public virtual void StartCycle()
        {
            // bcp의 endrolling 로직 반영 구간
            // 이번 Cycle 구간의 정보 처리
            // Batch Release => 여기다가 구현할지 별도의 AgentManager를 만들어서 관리하지 고민 필요.
            ATElapsedTimeChecker.Instance.ResetTimer("Planning_StartCycle");
            try
            {
                FWInterface.LotControl.OnStartCycle(this.NowDT, this.NextStartTime);

                var line = this.DefaultLine;
                var stage = ATExecutionContext.Instance.CurrentStage;
                foreach (var resource in stage.Resources)
                {
                    PBFResource bucket = resource.Bucket as PBFResource;
                       
                    bucket.StartCycle(this.NowDT, this.NextStartTime);
                    
                    FWInterface.BucketControl.OnStartCycle(bucket, this.NowDT, this.NextStartTime);
                }

                // 기타 cycle이 시작되기 전에 설정해야하는 부분들
                // BOH 기준의 정보 등을 출력.
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("Planning_StartCycle");
            }
        }


        public virtual void EndCycle()
        {
            ATElapsedTimeChecker.Instance.ResetTimer("Planning_EndCycle");
            try
            {
                FWInterface.LotControl.OnEndCycle(this.NowDT, this.NextStartTime);

                var line = this.DefaultLine;
                var stage = ATExecutionContext.Instance.CurrentStage;
                foreach (var resource in stage.Resources)
                {
                    PBFResource bucket = resource.Bucket as PBFResource;
                    if (bucket.EndTime == this.NextStartTime)
                        bucket.EndCycle(this.NowDT, this.NextStartTime);
                }
            }
            finally
            {
                ATElapsedTimeChecker.Instance.AddElapsedTime("Planning_EndCycle");
            }
            // EOH 정보 출력

            // 장비가 하루동안 진행한 정보 처리
        }


        private void Rolling()
        {
            this.Lines.Values.ForEach ( x=> x.Rolling (this.NowDT, this.NextStartTime));
            this.AddTimeout(this.NextStartTime - this.NowDT, (o, e) => this.Run());
        }

        protected override void OnDone()
        {
            base.OnDone();
        }

        #region Compenstation
        internal void CheckCompensation()
        {
            var now = this.NowDT;

            var duration = FWFactory.Instance.GetBucketCycleTime(now);
            Time remainToNext = DateUtility.GetSpanToNext(now, duration);
            float elapsedTime = (float)duration.TotalHours - (float)remainToNext.TotalHours;

            if (elapsedTime != duration.TotalHours)
                this.Compensate(now, elapsedTime / (float)duration.TotalHours);
        }

        private void Compensate(DateTime now, float portion)
        {
           // this.constraintManager.Compensate(now, portion);
        }
        #endregion
    }
}
