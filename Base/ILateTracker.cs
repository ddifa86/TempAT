
using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Planner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public interface ILateTracker
    {
        ATDemand SoDemand { get; }

        Dictionary<string, ATLateInfo> VirtualLateInfos { get; }
    }

    public static class LateTrackerExtend
    {
        public static void AddShortInfo(this ILateTracker obj, LateCategory category, string reason, double qty, string desc, DateTime fromDate, DateTime toDate, HashSet<IBucket> resources = null)
        {
            if (qty <= ATOption.Instance.MinimumAllocationQuantity)
                return;

            int phase = ATExecutionContext.Instance.CurrentExecutionInfo.CurPhase;
            int retryCount = ATExecutionContext.Instance.CurrentExecutionInfo.CurRetryCount;

            var lateInfos = obj.SoDemand.LateInfoManager.LateInfos;

            if (obj is APELot)
            {
                var lot = obj as APELot;
                string refPlanID = lot.InitOperTarget.PegTarget?.PegPart?.CurRefPlan?.ID;

                string key = lot.CurrentItemSiteBuffer + lot.CurrentRoute?.RouteID + lot.CurrentOper?.OperID + category.ToString() + reason + Status.Short.ToString() + refPlanID + phase + retryCount;

                ATLateInfo info;
                if (lateInfos.TryGetValue(key, out info) == false)
                {
                    info = new ATLateInfo(obj.SoDemand, lot.CurrentBom, lot.CurrentOper, lot.CurrentItemSiteBuffer, Status.Short, category, reason, desc, phase, retryCount, fromDate, toDate, refPlanID);
                    lateInfos.Add(key, info);
                }

                info.ToDate = toDate;
                info.ShortQty += qty;
                info.Count += 1;
                if (resources != null)
                    info.Resources.AddRange(resources);

                info.AddShortLot(lot);
            }
            else if (obj is APETarget)
            {
                var pt = obj as APETarget;
                var pp = pt.PegPart;
                string refPlanID = pp.CurRefPlan?.ID;

                string key = pp.CurrentItemSiteBuffer + pp.CurrentOperation?.RouteID + pp.CurrentOperation?.OperID + category.ToString() + reason + Status.Short.ToString() + refPlanID + phase + retryCount;

                ATLateInfo info;
                if (lateInfos.TryGetValue(key, out info) == false)
                {
                    info = new ATLateInfo(obj.SoDemand, pp.CurrentBom, pp.CurrentOperation, pp.CurrentItemSiteBuffer, Status.Short, category, reason, desc, phase, retryCount, fromDate, toDate, refPlanID);
                    lateInfos.Add(key, info);
                }

                info.ToDate = toDate;
                info.ShortQty += qty;
                info.Count += 1;

                pp.Status = Status.Short;
            }
        }
        
        public static void AddVirtualLateInfo(this ILateTracker obj, LateCategory category, string reason, string desc, DateTime fromDate, DateTime toDate, HashSet<IBucket> resources = null)
        {
            int phase = ATExecutionContext.Instance.CurrentExecutionInfo.CurPhase;
            int retryCount = ATExecutionContext.Instance.CurrentExecutionInfo.CurRetryCount;

            if (obj is APELot)
            {
                var lot = obj as APELot;
 
                string refPlanID = lot.CurrentTarget.PegTarget?.PegPart?.CurRefPlan?.ID;

                string key = lot.CurrentItemSiteBuffer + lot.CurrentRoute?.RouteID + lot.CurrentOper?.OperID + category.ToString() + reason + Status.Short.ToString() + refPlanID + phase + retryCount;

                ATLateInfo info;
                if (obj.VirtualLateInfos.TryGetValue(key, out info) == false)
                {
                    info = new ATLateInfo(obj.SoDemand, lot.CurrentBom, lot.CurrentOper, lot.CurrentItemSiteBuffer, Status.Late, category, reason, desc, phase, retryCount, fromDate, toDate, refPlanID);
                    obj.VirtualLateInfos.Add(key, info);
                }

                info.Count += 1;
                info.ToDate = toDate;

                if (resources != null)
                    info.Resources.AddRange(resources);
            }
        }

        public static void UpdateLateInfos(this ILateTracker obj, Status status)
        {
            if (obj is APELot)
            {
                var lot = obj as APELot;

                if (obj.VirtualLateInfos != null && obj.VirtualLateInfos.Count() > 0)
                {
                    foreach (var virtualInfo in obj.VirtualLateInfos)
                    {
                        var lateInfo = virtualInfo.Value;
                        lateInfo.ShortType = status;

                        string key = lateInfo.GetKey();

                        ATLateInfo info;
                        if (obj.SoDemand.LateInfoManager.LateInfos.TryGetValue(key, out info) == false)
                        {
                            info = lateInfo;
                            obj.SoDemand.LateInfoManager.LateInfos.Add(key, info);
                        }

                        info.FromDate = ATUtil.MinTime(info.FromDate, virtualInfo.Value.FromDate);
                        info.ToDate = ATUtil.MaxTime(info.ToDate, virtualInfo.Value.ToDate);
                        info.ShortQty += lot.Qty;
                        info.Count += lateInfo.Count;
                        info.Resources.AddRange(lateInfo.Resources);
                        info.AddShortLot(lot);
                    }
                }
                else
                {
                    string refPlanID = lot.CurrentTarget.PegTarget.PegPart?.CurRefPlan?.ID;
                    var phase = ATExecutionContext.Instance.CurrentExecutionInfo.CurPhase;
                    int retryCount = ATExecutionContext.Instance.CurrentExecutionInfo.CurRetryCount;

                    string key = lot.CurrentItemSiteBuffer + lot.CurrentRoute?.RouteID + lot.CurrentOper?.OperID + LateCategory.Capacity + LateReason.LackOfCapacity.ToString() + status.ToString() + refPlanID + phase + retryCount;

                    ATLateInfo info;
                    if (obj.SoDemand.LateInfoManager.LateInfos.TryGetValue(key, out info) == false)
                    {
                        info = new ATLateInfo(obj.SoDemand, lot.CurrentBom, lot.CurrentOper, lot.CurrentItemSiteBuffer, status, LateCategory.Capacity, LateReason.LackOfCapacity.ToString(), null, phase, retryCount, DateTime.Now, DateTime.Now, refPlanID);
                        obj.SoDemand.LateInfoManager.LateInfos.Add(key, info);
                    }

                    info.ToDate = DateTime.Now;
                    info.ShortQty += lot.Qty;
                    info.Count += 1;
                    info.AddShortLot(lot);
                }
            }

            obj.ClearVirtualLateInfo();
        }

        public static void ClearVirtualLateInfo(this ILateTracker obj)
        {
            obj.VirtualLateInfos.Clear();
        }
    }
}
