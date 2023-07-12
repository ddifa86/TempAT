using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.TimeLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Planner
{
    /// <summary>
    /// 설비의 PM을 등록하고 관리하는 Manager 입니다
    /// </summary>
    public class ATPMManager : ATNonWorkingManager
    {
        public ATPMManager(PBFResource bucket)
            :base(bucket)
        {

        }

        internal void AddPMInfo(ATPMInfo pmInfo)
        {
            var period = new ATPMPeriod(pmInfo, pmInfo.StartTime, pmInfo.EndTime);
            
            AddNonWorkingTimes(period);
        }

        internal void AddPMInfos()
        {
            List<ATNonWorkingInfo> pmInfos =  this._bucket.Target.PMTimeInfoManager.NonWorkingInfos;

            foreach (var info in pmInfos)
            {
                ATPMInfo pm = info as ATPMInfo;
                this.AddPMInfo(pm);
            }
        }

        /// <summary>
        /// 설비의 PM List중 PM의 StartTime이 Parameter의 endTime 이전인 PM List를 반환합니다
        /// </summary>
        /// <param name="endTime"> 해당 날 이전의 PM을 조회하기 위한 날짜입니다. </param>
        /// <returns>설비의 PM List중 PM의 StartTime이 Parameter의 endTime 이전인 PM List입니다.</returns>
        internal List<ATPMPeriod> GetCurrPMList(DateTime endTime)
        {
            var lst = new List<ATPMPeriod>();
            APENonWorkingPeriod selectedPm = APENonWorkingPeriod.NULL;

            if (this._bucket.IsTimeCapa == false)
                return lst;

            if (this._nonWorkingTimes == null || this._nonWorkingTimes.Count() == 0)
                return lst;

            List<APENonWorkingPeriod> cur = GetNonWorkingTimes(endTime);
            if (cur.Count() == 0)
                return lst;

            lst = cur.Select(x => x as ATPMPeriod).ToList();

            return lst;
        }

        /// <summary>
        /// pmList중, 해당 시점에 수행할 PM을 선택합니다.
        /// </summary>
        /// <param name="pmList">후보 PM List 입니다.</param>
        /// <param name="isEndCycle">EndCycle 시점에 호출했는지 여부 입니다.</param>
        /// <returns>해당 시점에 수행 할 PM List입니다.</returns>
        internal List<ATPMPeriod> SelectPM(List<ATPMPeriod> pmList, bool isEndCycle)
        {
            // FixedPM은 무조건 선택, 그외에는 하나만 선택
            List<ATPMPeriod> selected = new List<ATPMPeriod>();

            bool isSelected = false;
            foreach (ATPMPeriod pm in pmList.OrderBy(x=>x.PmFlag).ThenBy(x=>x.PMPriority).ThenBy(x=>x.Start)) // reserve우선
            {
                if (pm.PmFlag == PMFlag.Canceled)
                    continue;

                //EndCycle시점에는, 이전 PM만 진행해야함. EndTime과 맞물리는 Start PM은 진행하면 안됨.
                if (isEndCycle == true && (pm.Start == this._bucket.EndTime || pm.PmFlag == PMFlag.Reserved))
                    continue;

                if (this._bucket.CurrentTime == this._bucket.EndTime) // 할당을 완료한 EndCycle 시점에 Select를 하면, Fixed Type만 진행 가능
                {
                    if (pm.PMPolicy == PmPolicy.Fix_Split || pm.PMPolicy == PmPolicy.Fix_None)
                        selected.Add(pm);

                    continue;
                }

                if (isSelected == false)
                    selected.Add(pm);
                else
                    pm.PmFlag = PMFlag.Canceled;
                
                if (pm.PMPolicy != PmPolicy.Fix_None && pm.PMPolicy != PmPolicy.Fix_Split)
                    isSelected = true;
            }

            return selected;
        }


        //internal APENonWorkingPeriod DoPM_back(ATPMPeriod todoPm)
        //{
        //    DateTime start = todoPm.Start;
        //    DateTime end = todoPm.End;

        //    if (todoPm.PmFlag == PMFlag.Reserved)
        //    {
        //        start = todoPm.ReservedStartTime;
        //        end = todoPm.ReservedEndTime;
        //    }

        //    var pmPeriod = (end - start).TotalSeconds;
        //    double usedCapa = pmPeriod;
        //    bool needReserve = false;

        //    #region 시간보정
        //    if (todoPm.PMPolicy != PmPolicy.Fixed)
        //    {
        //        if (todoPm.Start < this._bucket.CurrentTime)
        //        {
        //            start = this._bucket.CurrentTime;
        //            end = this._bucket.CurrentTime.AddSeconds(pmPeriod);
        //        }
        //    }

        //    // GetNonWorkingTime
        //    TimeSpan offTimes = TimeSpan.Zero;
        //    offTimes = this._bucket.GetCumNonWorkingTime(start, this._bucket.EndTime < end ? this._bucket.EndTime : end, false);

        //    end = end + offTimes;
        //    #endregion

        //    if (this._bucket.EndTime < end)
        //    {
        //        TimeSpan rollingOffTime = TimeSpan.Zero;
        //        rollingOffTime = this._bucket.GetCumNonWorkingTimeNoExtend(start, this._bucket.EndTime, LotSplitOption.None, false);

        //        todoPm.ExecutedEndTime = this._bucket.EndTime;
        //        usedCapa = (todoPm.ExecutedEndTime - start).TotalSeconds - (rollingOffTime.TotalSeconds);
        //        needReserve = true;
        //        end = this._bucket.EndTime;
        //    }

        //    todoPm.CurrentUsedCapa = usedCapa;

        //    if (todoPm.PmFlag != PMFlag.Reserved) // 예약되서 들어온 PM이 아니고 처음 들어온 PM에대해 Start 입력
        //        todoPm.ExecutedStartTime = start;

        //    if (needReserve)
        //    {
        //        double reserveCapa = pmPeriod - usedCapa;

        //        DateTime reservedEnd = end.AddSeconds(reserveCapa);
        //        todoPm.ReservedStartTime = this._bucket.EndTime;
        //        todoPm.ReservedEndTime = reservedEnd;
        //        todoPm.PmFlag = PMFlag.Reserved;
        //    }
        //    else
        //    {
        //        // PM 할당 끝
        //        todoPm.ExecutedEndTime = end;
        //        todoPm.PmFlag = PMFlag.Executed;
        //    }

        //    return todoPm;
        //}


        /// <summary>
        /// 지난 PM을 관리합니다.
        /// </summary>
        /// <param name="fence">해당 날짜 이전의 PM의 상태를 Update합니다.</param>
        /// <returns>지나간 PM중 수행해야 하는 PM List 입니다</returns>
        internal List<ATPMPeriod> UpdatePM(DateTime fence)
        { // 지나간 PM은 History로 옮기고 지움 (중복된것도 지워질 것), 작성도해야함

            var lst = _nonWorkingTimes.Values.Where(x => x.Start <= fence).ToList();
            List<ATPMPeriod> todoPM = new List<ATPMPeriod>();

            foreach (var info in lst)
            {
                ATPMPeriod pm = info as ATPMPeriod;

                if (pm.PmFlag == PMFlag.Reserved)
                    continue;

                if ((pm.PMPolicy == PmPolicy.Fix_Split || pm.PMPolicy == PmPolicy.Fix_None) && pm.PmFlag != PMFlag.Executed)
                {
                    todoPM.Add(pm);
                    continue;
                }

                this._nonWorkingTimesHistory.Add(info);
                this._nonWorkingTimes.Remove(info);
                
                //if (pm.PmFlag == PMFlag.Executed) // 수행됨
                //    OutputWriter.Instance.WriteNonWorkingPeriodResourcePlan(this._bucket, pm, "PM", pm.Name + "@" + pm.PMPriority);

                OutputWriter.Instance.WritePmPlanLog(this._bucket.BucketID, pm);
            }

            return todoPM;
        }

        /// <summary>
        /// at, fence 사이에 걸쳐있는 Fixed PM을 조회하고, 그중 가장 빠른 nonWorkingTime을 반환합니다.
        /// </summary>
        /// <param name="at"></param>
        /// <param name="fence"></param>
        /// <returns> at, fence 사이에 걸쳐있는 Fixed PM을 조회하고, 그중 가장 빠른 nonWorkingTime을 반환합니다 </returns>
        public APENonWorkingPeriod GetNextNonWorkingPeriod(DateTime at, DateTime fence, bool containFence, LotSplitOption option = LotSplitOption.None)
        {
            //at ~ fence 사이에 걸쳐져있는 Fixed PM
            foreach (var section in this._nonWorkingTimes.Values)
            {
                if ((containFence && fence < section.Start) || (containFence == false && fence <= section.Start))
                    return APENonWorkingPeriod.NULL;

                if (section.End <= at)
                    continue;

                if ((section as ATPMPeriod).PMPolicy != PmPolicy.Fix_None && (section as ATPMPeriod).PMPolicy != PmPolicy.Fix_Split)
                    continue;

                if (option != LotSplitOption.None && section.SplitOption != option)
                    continue;

                return section;
            }

            return APENonWorkingPeriod.NULL;
        }
    }
}
