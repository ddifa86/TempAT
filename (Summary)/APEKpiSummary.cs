using Mozart.SeePlan.Aleatorik.Data;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class APEKpiSummary
    {
        internal Dictionary<string, double> KpiDic_Sum { get; set; }

        internal Dictionary<string, string> KpiDic_Avg { get; set; }

        public APEKpiSummary()
        {
            this.KpiDic_Sum = new Dictionary<string, double>();
            this.KpiDic_Avg = new Dictionary<string, string>();
        }

        internal string GetKey(KpiCategory category, KpiIndex index, DateTime date, bool useTimeKey, string etcIndex = "")
        {
            string timeKey = "-";
            string indexStr = index.ToString();

            if (useTimeKey)
                timeKey = ATUtil.ToMonth(date);

            if (index == KpiIndex.ETC)
                indexStr = etcIndex;

            string key = category + "@" + indexStr + "@" + timeKey;
            
            return key;
        }

        internal void AddDictionary(KpiCategory category, KpiIndex index, DateTime date, bool useTimeKey, double value, CalcType calcType, string etcIndex = "")
        {
            string key = GetKey(category, index, date, useTimeKey, etcIndex);

            if (calcType == CalcType.Average)
            {
                string outValue;
                if (KpiDic_Avg.TryGetValue(key, out outValue) == false)
                    KpiDic_Avg.Add(key, "1@" + value);
                else
                {
                    double cnt = double.Parse(outValue.Split('@')[0]);
                    double sum = double.Parse(outValue.Split('@')[1]);

                    cnt += 1;
                    sum += value;

                    KpiDic_Avg[key] = cnt + "@" + sum;
                }
                    
            }
            else // sum
            {
                double outValue;
                if (KpiDic_Sum.TryGetValue(key, out outValue) == false)
                    KpiDic_Sum.Add(key, value);
                else
                    KpiDic_Sum[key] += value;
            }
        }

        internal void AddRtfQty(APELot lot)
        {
            var so = lot.CurrentTarget.SODemand;

            var demandKey = GetKey(KpiCategory.RTF, KpiIndex.DEMAND_QTY, so.DueDateTime, true);
            double demandQty = 0d;

            if (ATExecutionContext.Instance.CurrentExecutionInfo.IsPBFModule 
                && so.DueDateTime >= ATOption.Instance.PlanEndTime)
            {
                // DEMAND KEY가 PLAN END를 감안하였을 때, 대상이 아닌 경우 제외.
                // PBF의 경우 PLAN END를 고려하여 집계대상이 아닌 경우 RTF 집계 대상에서 제외.
                return;
            }
            
            this.KpiDic_Sum.TryGetValue(demandKey, out demandQty);
            this.AddDictionary(KpiCategory.RTF, KpiIndex.TOTAL_RTF_QTY, lot.LastStepTime, true, lot.CurrentQty, CalcType.Sum);
            this.AddDictionary(KpiCategory.RTF, KpiIndex.LATENESS_RTF_QTY, so.DueDateTime, true, lot.CurrentQty, CalcType.Sum);

            if (demandQty > 0)
                this.AddDictionary(KpiCategory.RTF, KpiIndex.LATENESS_RTF_RATE, so.DueDateTime, true, (lot.CurrentQty / demandQty) * 100, CalcType.Sum);

            if (lot.LastStepTime <= so.CalcDueDateTime)
            {
                this.AddDictionary(KpiCategory.RTF, KpiIndex.ONTIME_RTF_QTY, so.DueDateTime, true, lot.CurrentQty, CalcType.Sum);

                if (demandQty > 0)
                    this.AddDictionary(KpiCategory.RTF, KpiIndex.ONTIME_RTF_RATE, so.DueDateTime, true, (lot.CurrentQty / demandQty) * 100, CalcType.Sum);
            }
        }

        internal void AddPegResult(double pegQty)
        {
            this.AddDictionary(KpiCategory.PEG_RESULT, KpiIndex.PEG_QTY, DateTime.MinValue, false, pegQty, CalcType.Sum);
            this.AddDictionary(KpiCategory.PEG_RESULT, KpiIndex.PEG_COUNT, DateTime.MinValue, false, 1, CalcType.Sum);
        }
        
        internal void AddUnPegResult(double unpegQty)
        {
            this.AddDictionary(KpiCategory.PEG_RESULT, KpiIndex.UNPEG_QTY, DateTime.MinValue, false, unpegQty, CalcType.Sum);
            this.AddDictionary(KpiCategory.PEG_RESULT, KpiIndex.UNPEG_COUNT, DateTime.MinValue, false, 1, CalcType.Sum);
        }

        internal void AddStageInQty(double inTargetQty, DateTime date)
        {
            this.AddDictionary(KpiCategory.RELEASE_QTY, KpiIndex.STAGE_IN_QTY, date, true, inTargetQty, CalcType.Sum);
        }

        internal void AddUtil(string resourceGroupName, DateTime date, double utilRatio)
        {
            if (string.IsNullOrEmpty(resourceGroupName) == true)
                return;

            this.AddDictionary(KpiCategory.RESOURCE, KpiIndex.ETC, date, true, utilRatio, CalcType.Average, resourceGroupName);
        }

        internal void AddSetup(DateTime date)
        {
            this.AddDictionary(KpiCategory.RESOURCE, KpiIndex.SETUP_COUNT, date, true, 1, CalcType.Sum);
            this.AddDictionary(KpiCategory.RESOURCE, KpiIndex.TOTAL_SETUP_COUNT, DateTime.MinValue, false, 1, CalcType.Sum);
        }
    }
}
