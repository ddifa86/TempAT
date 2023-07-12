using Mozart.SeePlan.TimeLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    public class APENonWorkingPeriod : DateTimeInterval
    {
        public static readonly APENonWorkingPeriod NULL = new APENonWorkingPeriod( "Dummy", DateTime.MinValue, TimeSpan.Zero);

        /// <summary>
        /// Interval 명
        /// </summary>
        public string Name { get; private set; }

        public LotSplitOption SplitOption { get; set; }

        internal bool IsWrite { get; set; }

        //
        // 요약:
        //     지정된 구간 시작 시간과 끝 시간 및 작업물 분할 방식을 사용해서 새로운 시간 구간 정보를 생성합니다.
        //
        // 매개 변수:
        //   startTime:
        //     구간 시작 시간입니다.
        //
        //   endTime:
        //     구간 끝 시간입니다.
        //
        //   option:
        //     작업물 분리 방식입니다.
        public APENonWorkingPeriod(string name, DateTime startTime, DateTime endTime, LotSplitOption option = LotSplitOption.KeepAndDelay)
            :base(startTime, endTime)
        {
            this.SplitOption = option;
            this.Name = name;
        }

        //
        // 요약:
        //     지정된 구간 시작 시간과 구간의 길이 및 작업물 분할 방식을 사용해서 새로운 시간 구간 정보를 생성합니다.
        //
        // 매개 변수:
        //   startTime:
        //     구간 시작 시간입니다.
        //
        //   duration:
        //     구간의 길이입니다.
        //
        //   option:
        //     작업물 분할 방식입니다.
        public APENonWorkingPeriod(string name, DateTime startTime, TimeSpan duration, LotSplitOption option = LotSplitOption.KeepAndDelay)
            :base(startTime, duration)
        {
            this.SplitOption = option;
            this.Name = name;
        }

        public override string ToString()
        {
            return this.Name + " : " + base.ToString();
        }
    }
}
