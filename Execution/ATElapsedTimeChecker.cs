using System;
using System.Collections.Generic;
using System.Linq;
using Mozart.Task.Execution;

namespace Mozart.SeePlan.Aleatorik
{
    //이 클래스는 
    //구현된 함수가 Simulation 시작 ~ 끝까지 얼마의 시간이 걸리는지 체크 하기위한 용도
    public class ATElapsedTimeChecker
    {
        public static ATElapsedTimeChecker Instance
        {
            get
            {
                return ServiceLocator.Resolve<ATElapsedTimeChecker>();
            }
        }

        internal Dictionary<string, TimeInfo> ElapsedTimeByType;

        Dictionary<string, System.Diagnostics.Stopwatch> timerSet = new Dictionary<string, System.Diagnostics.Stopwatch>();

        Dictionary<string, System.Diagnostics.Stopwatch> cumulativeTimerSet = new Dictionary<string, System.Diagnostics.Stopwatch>();

      

        public ATElapsedTimeChecker()
        {
            this.ElapsedTimeByType = new Dictionary<string, TimeInfo>();
        }

        #region Custom 집계

        public System.Diagnostics.Stopwatch GetTimer(string typeKey)
        {

            System.Diagnostics.Stopwatch timer;
            if (this.timerSet.TryGetValue(typeKey, out timer) == false)
                this.timerSet.Add(typeKey, timer = new System.Diagnostics.Stopwatch());

            return timer;
        }

        public void StartCustomTimer(string key)
        {
            var timer = this.GetTimer(key);
            if (timer == null)
                return;

            timer.Reset();
            timer.Start();
        }

        public void StopCustomTimer(string key, bool bWrite )
        {
            var timer = this.GetTimer(key);

            timer.Stop();

            float freq = System.Diagnostics.Stopwatch.Frequency / 10000000f;

            Logger.MonitorInfo(string.Format("\t\t+ {0} => elapsed : {1} \t\t now : {2} ", key,
                     new TimeSpan((long)(timer.ElapsedTicks / freq)), DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")
                    )
                );

            if (bWrite == true)
            {
                OutputWriter.Instance.WriteExecutionTimeLog(key, new TimeSpan((long)(timer.ElapsedTicks / freq)));
            }
        }

        #endregion

        #region 함수 누적 집계 용도
        public string GetKey(string typeKey)
        {
            if (ATExecutionContext.Instance.CurrentStage == null)
                return typeKey;

            string stageID = ATExecutionContext.Instance.CurrentStage.StageID;
            string moduleKey = ATExecutionContext.Instance.CurrentExecutionInfo.Key;
            int phase = ATExecutionContext.Instance.CurrentExecutionInfo.CurPhase;
            typeKey = stageID + "^" + moduleKey + "^" + phase + "^" + typeKey;

            return typeKey;
        }

        public System.Diagnostics.Stopwatch GetCumulativeTimer(string typeKey)
        {
            if (typeKey.StartsWith("*"))
                return null;

            typeKey = GetKey(typeKey);

            System.Diagnostics.Stopwatch timer;
            if (this.cumulativeTimerSet.TryGetValue(typeKey, out timer) == false)
                this.cumulativeTimerSet.Add(typeKey, timer = new System.Diagnostics.Stopwatch());

            return timer;
        }

        public void ResetTimer(string typeKey)
        {
            if (ATOption.Instance.WritePerformanceLog == false)
                return;

            var timer = GetCumulativeTimer(typeKey);
            if (timer == null)
                return;

            timer.Reset();
            timer.Start();
        }

        public void AddElapsedTime(string typeKey)
        {
            if (ATOption.Instance.WritePerformanceLog == false)
                return;

            var timer = GetCumulativeTimer(typeKey);
            if (timer == null)
                return;

            timer.Stop();

            TimeInfo info = null;
            string key = GetKey(typeKey);
            if (ElapsedTimeByType.TryGetValue(key, out info) == false)
                ElapsedTimeByType.Add(key, info = new TimeInfo());

            info.Count++;
            info.ElapsedTicks += timer.ElapsedTicks;
        }

        #endregion

        public void PrintElapsedTimes()//WriteElapsedTimes()
        {
            Logger.MonitorInfo("\t####     Analysis ElapsedTime     ####");

            float freq = System.Diagnostics.Stopwatch.Frequency / 10000000f;

            foreach (KeyValuePair<string, TimeInfo> entry in ElapsedTimeByType)
            {
                Logger.MonitorInfo(string.Format("\t\t+ {0} \t\t\t\t\t\t CallCount : {1} \t\t\t\tElapsed Time = {2}", entry.Key.Replace('^', '/'),  entry.Value.Count,  new TimeSpan((long)(entry.Value.ElapsedTicks / freq))));
            }

            ElapsedTimeByType.Clear();
        }

        public void CleartElapsedTimes()
        {
            ElapsedTimeByType.Clear();
        }        
    }
    public class TimeInfo
    {
        internal int Count;
        internal long ElapsedTicks;

        public TimeSpan ElapseTimes
        {
            get
            {
                float freq = System.Diagnostics.Stopwatch.Frequency / 10000000f;
                return new TimeSpan((long)( ElapsedTicks / freq));
            }
        }

        public TimeInfo()
        {
            Count = 0;
            ElapsedTicks = 0;
        }
    }
}
