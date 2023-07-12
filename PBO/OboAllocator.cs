using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Logic;
using Mozart.SeePlan.Aleatorik.Manager;
using Mozart.SeePlan.Aleatorik.ObyO.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.ObyO
{
    public class OboAllocator
    {
        public OboFactory _factory;
        public ATRuleSet RuleSet
        {
            get
            {
                return ATRuleAgent.Instance.CurrentRuleSet;
            }

        }
        public OboAllocator(OboFactory factory)
        {
            this._factory = factory;
        }

        public void DoAllocate(APELot lot)
        {
            if (PBOInterface.PlanControl.IsDummyOperation(lot) || lot.CurrentOper.OperType == OperType.Dummy || lot.CurrentOper.IsBuffer)
            {
                ATDummyAgent.Instance.DoAllocate(lot);
            }
            else
            {
                if (lot.CurrentOper.Arranges.Count() == 0)
                {
                    /// 옵션에 따라 처리가 필요함.
                    /// Dummy로라도 진행? 혹 기준정보 이슈로 Short.
                    if (ATOption.Instance.ApplyNoArrangeToDummy)
                    {
                        ATDummyAgent.Instance.DoAllocate(lot);
                    }
                    else
                    {
                        lot.IsShort = true;
                        lot.ShortCategory = "FW Short";
                        lot.ReasonName = "Not Found Capacity Data";

                        lot.AddShortInfo(LateCategory.Capacity, LateReason.NoOpResourceInfo.ToString(), lot.Qty, null, lot.LastStepTime, lot.LastStepTime);
                    }

                    return;
                }

                // GetLoadableEqpArrange 확인
                Allocate(lot);
            }
        }

        //public APEPlanInfo DummyPlanInfo(APELot lot)
        //{
        //    double tat = OboInterfaces.PlanControl.GetTat(lot);

        //    DateTime start = lot.LastStepTime;

        //    // 선행 로직이 여전히 필요한가..??
        //    if (lot.CurrentOper.IsBuffer && lot.CurrentItemSiteBuffer.IsNoCarryover == false)
        //    {
        //        ATBuffer buffer = lot.CurrentOper as ATBuffer;
        //        var prebuildDays = buffer.PreBuildDays;
        //        start = ATUtil.MaxTime(start, lot.CurrentTarget.TargetDateTime.AddDays(-prebuildDays));
        //    }

        //    DateTime lot_end_time = start.AddSeconds(tat);             
        //    APEPlanInfo info = new APEPlanInfo(null, null, null, lot, start, lot_end_time, lot_end_time, lot.Qty, lot.Qty, 1, 1, 1, string.Empty, PlanInfoType.Allocate);
        //    info.TotalTat = tat;

        //    lot.AddPlan(info);
        //    lot.AddVirtualLateInfo(LateCategory.Tat, LateReason.DummyOperationLate.ToString(), null, lot.LastStepTime, lot.LastStepTime, null);

        //    OboInterfaces.PlanControl.OnBucketAllocated(null, lot, info, null);

        //    return info;
        //}

        private List<ATOperResource> FilterOperResource(APELot lot, List<ATOperResource> arranges, List<ATOperResource> filterArranges, PBOAllocateContext context)
        {
            List<ATOperResource> selArrange = new List<ATOperResource>();
            var so = lot.CurrentTarget.SODemand;

            var preset = this.RuleSet.GetRule(RulePoint.FilterOperResource, CallType.Operation);

            if (preset == null)
                return arranges;

            foreach (var arr in arranges)
            {
                bool bFilter = false;
                foreach (var factor in preset.FactorList)
                {
                    var method = factor.Method as FilterOperResource;
                    var result = method(arr, factor, context);
                    if (result.Value == true)
                    {
                        filterArranges.Add(arr);
                        var bucket = arr.Oper.GetLoadableBucket(arr);
                        HashSet<IBucket> resources = null;

                        if (bucket != null)
                            resources = new HashSet<IBucket>() { bucket };

                        lot.AddVirtualLateInfo(LateCategory.Constraint, factor.Name, result.Description, lot.LastStepTime, lot.LastStepTime, resources);
                        bFilter = true;
                        break;
                    }
                }

                if (bFilter == false)
                    selArrange.Add(arr);
            }

            return selArrange;
        }

        private List<APECapacity> FilterCapaInfo(APELot lot, List<APECapacity> candidateCapacity, PBOAllocateContext context)
        {
            List<APECapacity> selCapacity = new List<APECapacity>();
            var so = lot.CurrentTarget.SODemand;

            var preset = this.RuleSet.GetRule(RulePoint.FilterCapacity, CallType.Operation);

            if (preset == null)
                return candidateCapacity;

            foreach (var capacity in candidateCapacity)
            {
                bool bFilter = false;
                foreach (var factor in preset.FactorList)
                {
                    var method = factor.Method as FilterCapacity;
                    var result = method(capacity, factor, context);

                    if (result.Value == true)
                    {
                        // ShortLog 처리 필요.
                        HashSet<IBucket> resources = null;

                        var bucket = capacity.Bucket;
                        if (bucket != null)
                            resources = new HashSet<IBucket>() { bucket };

                        lot.AddVirtualLateInfo(LateCategory.Constraint, factor.Name, result.Description, lot.LastStepTime, lot.LastStepTime, resources);
                        bFilter = true;
                        break;
                    }
                }

                if (bFilter == false)
                    selCapacity.Add(capacity);
            }

            return selCapacity;
        }


        public bool Allocate(APELot lot)
        {
            // GetArranges
            var arranges = lot.CurrentOper.Arranges;
            var so = lot.CurrentTarget.SODemand;

            APERefPlan refPlan = null;
            if (lot.CurrentTarget.CurRefPlan != null && (lot.CurrentTarget.CurRefPlan.Operation != null && lot.CurrentOperID == lot.CurrentTarget.CurRefPlan.Operation.OperID))
                refPlan = lot.CurrentTarget.CurRefPlan;

            #region SelectCapaInfo
            PBOAllocateContext context = new PBOAllocateContext(arranges, lot, refPlan);
            context.CapaStartTime = lot.LastStepTime;
            context.CapaEndTime = so.CalcDueDateTime.AddDays(so.MaxLateDays);

            // OnPrepareAllocate
            PBOInterface.PlanControl.OnPrepareVirtualAllocate(lot, context);

            // Filter Arranges => 캘린더가 존재하는 경우 반영하여 Filter 작업.
            List<ATOperResource> filterArranges = new List<ATOperResource>();
            var selArranges = FilterOperResource(lot, arranges, filterArranges, context);

            if ((selArranges == null || selArranges.Count() == 0) && ATOption.Instance.ApplyOverCapacity == false) 
            {
                lot.IsShort = true;
                lot.ShortCategory = "FW Short";
                lot.ReasonName = "Not Found Usable Capacity Data"; // 검증 필요

                lot.UpdateLateInfos(Status.Short);

                return false;
            }

            HashSet<PBOResource> candidateBucket = new HashSet<PBOResource>();
            List<APECapacity> candidateCapaInfos = new List<APECapacity>();
            List<APECapacity> loadableCapaInfos = new List<APECapacity>();

            // 대상이 되는 CapaInfo 모두 가져옴.
            foreach (var arr in selArranges)
            {
                var bucket = arr.Oper.GetLoadableBucket(arr);

                if (bucket == null)
                    continue;
                                
                if (candidateBucket.Add(bucket) == false)
                    continue;

                if (bucket.Target.IsInfinite == false)
                {
                    if (bucket.CapacityManager.HasCapaInfos == false)
                    {
                        lot.AddVirtualLateInfo(LateCategory.Capacity, LateReason.NoOpResourceInfo.ToString(), null, lot.LastStepTime, lot.LastStepTime);
                        continue;
                    }

                    var capainfos = bucket.CapacityManager.GetLoadableCapaInfos(context.CapaStartTime, context.CapaEndTime);

                    foreach (var capaInfo in capainfos)
                    {
                        ATQtyCapacity qtyCapa = capaInfo as ATQtyCapacity;

                        //sh Bucket어디에둬야하는가...
                        qtyCapa.Bucket = bucket;
                        qtyCapa.CurrentArrange = arr;

                        candidateCapaInfos.Add(qtyCapa);

                        if (capaInfo.Remain > ATOption.Instance.MinimumAllocationQuantity)
                            loadableCapaInfos.Add(qtyCapa);
                    }
                }
            }

            if (loadableCapaInfos.Count <= 0 && ATOption.Instance.ApplyOverCapacity == false)
            {
                lot.AddShortInfo(LateCategory.Capacity, LateReason.LackOfCapacity.ToString(), lot.Qty, null, lot.LastStepTime, lot.LastStepTime, new HashSet<IBucket>(candidateBucket));
                lot.IsShort = true;
                return false;
            }

            context.Buckets = candidateBucket;
            #endregion

            #region CompareCapaInfo            
            // Filter 수행
            context.CapaInfos = candidateCapaInfos;
            candidateCapaInfos = FilterCapaInfo(lot, candidateCapaInfos, context);
            context.CapaInfos = candidateCapaInfos;

            var preset = RuleSet.GetRule(RulePoint.CompareCapacity, CallType.Operation);
            if (preset != null)
                candidateCapaInfos.Sort(new OboCapaInfoComparer(preset, context));
            
            #endregion

            #region Virtual Allocate
            double remainQty = lot.Qty;
            double allocQty = 0;
            //bool isConstraintShort = false;
            //bool isResourceConstraint = false;

            List<OboCapaInfo> additionalCapas = new List<OboCapaInfo>();
            List<APEPlanInfo> additionalPlanInfos = new List<APEPlanInfo>();
            ATConstraintDetail minConstraint = null;

            foreach (var selCapa in candidateCapaInfos)
            {
                if (remainQty <= ATOption.Instance.MinimumAllocationQuantity)
                    break;

                // bucket type에 따라 Capa casting해야하는데 Bucket을 모르고 Capa를 가져와야하는상황..?
                ATQtyCapacity qtyCapa = selCapa as ATQtyCapacity;
                context.UsagePer = qtyCapa.CurrentArrange.GetUsagePer(qtyCapa.StartTime);
                context.SelectedBucket = qtyCapa.Bucket as PBOResource;

                if (PBOInterface.PlanControl.CheckForAvailableBeforeAllocation(lot, selCapa, context) == false)
                    continue;

                if (qtyCapa.Remain <= ATOption.Instance.MinimumAllocationQuantity)
                    continue;

                lot.CurrentArrange = qtyCapa.CurrentArrange;

                APEPlanInfo planInfo = null;
                
                additionalCapas.Clear();
                additionalPlanInfos.Clear();

                var bucket = qtyCapa.Bucket as PBOResource;
                double allocLotQty = remainQty;
                
                List<ATConstraintDetail> constraints = new List<ATConstraintDetail>();

                constraints.AddRange(lot.GetConstraintDetails(qtyCapa.CapaInfo.CalAttr.ApplyDate));
                constraints.AddRange(bucket.GetCurrentConstraintDetails(qtyCapa.CapaInfo.CalAttr.ApplyDate));

                if (constraints != null && constraints.Count() > 0)
                {
                    double minConstraintQty = double.MaxValue;

                    foreach (var detail in constraints)
                    {
                        double constraintUsagePer = PBOInterface.PlanControl.GetConstraintUsagePer(lot, bucket, detail.Constraint);
                        detail.Constraint.UsagePer = constraintUsagePer;

                        if (minConstraintQty > detail.RemainQty / constraintUsagePer)
                        {
                            minConstraintQty = detail.RemainQty / constraintUsagePer;
                            minConstraint = detail;
                        }
                    }
                    
                    allocLotQty = Math.Min(allocLotQty, minConstraintQty);

                    string reason = LateReason.ItemConstraint.ToString();
                    string desc = string.Format("Constraint ID : {0}", minConstraint.Constraint.ConstraintID);

                    if (minConstraint.Constraint.Property.Category == PropertyCategory.Resource.ToString())
                        reason = LateReason.ResourceConstraint.ToString();

                    lot.AddVirtualLateInfo(LateCategory.Constraint, reason, desc, minConstraint.Attribute.EffectiveStartTime, minConstraint.Attribute.EffectiveEndTime);
                }
                else
                {
                    string desc = string.Format("Calendar ID : {0}", selCapa.CapaInfo.CalAttr.CalendarID);
                    lot.AddVirtualLateInfo(LateCategory.Capacity, LateReason.LackOfResourceCapacity.ToString(), desc, selCapa.CapaInfo.StartTime, selCapa.CapaInfo.EndTime, new HashSet<IBucket>() { selCapa.Bucket });
                }
                if (allocLotQty <= ATOption.Instance.MinimumAllocationQuantity)
                    continue;

                double utilizationRate = bucket.Target.GetUtilization(qtyCapa.StartTime);
                double needCapacity = allocLotQty * context.UsagePer;
                double remainCapacity = Math.Round(selCapa.Remain, ATOption.Instance.MinimumAllocationValue);
                //if (selCapa.CurrentArrange.AddArrangeInfo.Count() > 0)
                //{
                    // Additional Resource 에 대한 처리 추가 필요.
                    //// remainCapacity 보정.
                    //foreach (var addArr in selCapa.CurrentArrange.AddArrangeInfo)
                    //{
                    //    additionalCapas.Add(addArr)
                    //}

                    // additionalCapa.add(secondCapa);
                // }

                double allocCapaQty = Math.Min(needCapacity, remainCapacity);

                if (context.UsagePer != 0)
                    allocLotQty = allocCapaQty / context.UsagePer;

                if (lot.CurrentOper.IsMBSOper)
                {
                    double remainder = allocLotQty % lot.CurrentOper.MBSValue;
                    if (remainder > ATOption.Instance.MinimumAllocationQuantity)
                    {
                        allocLotQty -= remainder;
                        allocCapaQty = allocLotQty * context.UsagePer;
                    }
                }

                if (allocLotQty <= ATOption.Instance.MinimumAllocationQuantity)
                    continue;

                planInfo = bucket.DoAllocated(lot, qtyCapa, allocCapaQty, allocLotQty, context.UsagePer, utilizationRate, refPlan, context);
                                
                Dictionary<ATConstraintDetail, double> usedConstraint = new Dictionary<ATConstraintDetail, double>();
                foreach (var detail in constraints)
                {
                    usedConstraint.Add(detail, detail.Constraint.UsagePer);
                }

                planInfo.AllocationInfo.UsedConstraints = usedConstraint;

                foreach (var detail in constraints)
                {
                    APEPlanInfo cloneInfo = planInfo.Clone() as APEPlanInfo;
                    cloneInfo.AllocationInfo.Type = PlanInfoType.Constraint;
                    cloneInfo.Description = detail.Constraint.ConstraintID;

                    detail.AddVirtualUsedQty(allocLotQty * detail.Constraint.UsagePer);
                    detail.Constraint.Plans.Add(cloneInfo);
                }

                if (additionalCapas.Count() > 0)
                {
                    //foreach (var secCapa in additionalCapas)
                    //{
                    //    var secPlan = secCapa.Bucket.DoAllocated()
                    //}
                }

                remainQty -= allocLotQty;
                allocQty += allocLotQty;

                // IsSplit
                if (remainQty > ATOption.Instance.MinimumAllocationQuantity)
                {
                    APELot splitLot = LotHelper.GenerateSplitLot(lot, remainQty);

                    // argument 가 필요함.
                    bool isRetry = false;

                    // ??
                    if (isRetry)
                    {
                        // 다시 처음부터 투입.
                        // 할당 이력 rollback.
                        OboLogic.Instance.CommitAllocate(splitLot, false);

                        // 재투입 작업
                        var retryLots = _factory.CreateRetryLot(splitLot, splitLot.Qty);
                        foreach (var retryLot in retryLots)
                        {
                            OutputWriter.Instance.WriteLotHistory(retryLot, retryLot.Qty, LifeCycle.Release.ToString(), retryLot.LastStepTime, lot.LotID);
                            _factory.ReleasedLots.Push(retryLot);
                        }

                        retryLots.ForEach(x => _factory.ReleasedLots.Push(x));
                    }
                    else 
                    {
                        if (lot.CurrentOper.IsMBSOper)
                        {
                            var splitLots = MBSLogic.Instance.SplitReAllocLot(lot, remainQty);
                            foreach (var sLot in splitLots)
                            {
                                OutputWriter.Instance.WriteLotHistory(sLot, sLot.Qty, LifeCycle.Release.ToString(), sLot.LastStepTime, lot.LotID);
                                ReleaseAgent.Instance.AddAlignQueue(sLot.CurrentOper, sLot);
                            }
                        }
                        else
                        {
                            so.LateInfoManager.CopyLateInfos(lot, splitLot);

                            // 다른 장비로 연속 진행.
                            // 바로 장비할당부터 시작하도록 RunWip으로 설정.
                            splitLot.LotState = LotState.Run;

                            OutputWriter.Instance.WriteLotHistory(splitLot, splitLot.Qty, LifeCycle.Release.ToString(), splitLot.LastStepTime, lot.LotID);
                            _factory.ReleasedLots.Push(splitLot);
                        }
                    }

                    remainQty = 0;
                }

                lot.AddPlan(planInfo);

                if (additionalPlanInfos.Count() > 0)
                {
                    //planInfo.AddPlanInfo = additionalPlanInfos;
                }

                PBOInterface.PlanControl.OnCompleteVirtualAllocated(lot, planInfo);
            }
            #endregion

            // 할당을 하나도 하지 않은 경우
            if (allocQty <= ATOption.Instance.MinimumAllocationQuantity)
            {
                // Arrange 여부
                if (ATOption.Instance.ApplyOverCapacity)
                {
                    APECapacity selCapa = candidateCapaInfos.FirstOrDefault();
                    double allocLotQty = remainQty;

                    if (selCapa != null)
                    {
                        ATQtyCapacity qtyCapa = selCapa as ATQtyCapacity;
                        PBOResource bucket = qtyCapa.Bucket as PBOResource;
                        double utilizationRate = bucket.Target.GetUtilization(qtyCapa.StartTime);

                        APEPlanInfo planInfo = bucket.DoAllocated(lot, qtyCapa, 0, allocLotQty, 0, utilizationRate, refPlan, context);
                        lot.AddPlan(planInfo);
                    }
                    else
                    {
                        ATDummyAgent.Instance.DoAllocate(lot);
                    }
                }
                else
                {
                    lot.IsShort = true;
                    lot.ShortCategory = "FW Short";

                    if (minConstraint != null)
                        lot.ReasonName = "Lack of Constraint Capacity";
                    else
                        lot.ReasonName = "Lack of Capacity";

                    lot.UpdateLateInfos(Status.Short);

                    return false;
                }
            }

            return true;
        }
    }
}
