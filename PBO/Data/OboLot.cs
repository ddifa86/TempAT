using Mozart.SeePlan.Aleatorik.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.ObyO.Data
{

    // 추후 해당 정보로 변경 작업 진행 필요.
    //public class OboLot
    //{
    //    public string LotID;
    //    public string Key
    //    {
    //        get { return LotID; }
    //    }

    //    public double Qty { get; set; }

    //    public DateTime ReleaseTime { get; set; }
    //    public DateTime LastStepTime { get; set; }
    //    public double CurrentQty { get; set; }


    //    public string CurrentOperID => this.CurrentOper.OperID;

    //    public ATOperation CurrentOper { get; set; }

    //    public ATOperation InitOper { get; set; }

    //    public ATRoute CurrentRoute { get; set; }

    //    public ATBom CurrentBom { get; set; }

    //    public ATOperTarget CurrentTarget { get; set; }

    //    public List<APEPlanInfo> Plans { get; internal set; }

    //    public List<APEPlanInfo> CapaPlans { get; internal set; }

    //    public APEPlanInfo LastPlan
    //    {
    //        get
    //        {
    //            if (Plans.Count() == 0)
    //                return null;

    //            return Plans.Last();
    //        }
    //    }

    //    public APEPlanInfo FirstPlan
    //    {
    //        get
    //        {
    //            if (Plans.Count() == 0)
    //                return null;

    //            return Plans.First();
    //        }
    //    }


    //    public ATPlanWip Wip;

    //    public ATItemSiteBuffer CurrentItemSiteBuffer {get; set;}

    //    public LotState LotState { get; set; }

    //    public string CreateType { get; set; }

    //    public ATOperTarget InitOperTarget { get; set; }

    //    public List<ATPlanWip> PeggedWips { get; set; }

    //    public double PreBuildDays { get; set; }
    //    //public APELot OrgLot;

    //    public OboLot(string lotID, double qty, ATOperation oper, ATOperTarget target, ATPlanWip wip, string createType)
    //    {
    //        this.LotID = lotID;
    //        this.Qty = qty;
    //        this.CurrentQty = qty;

    //        this.CurrentOper = null;
    //        this.InitOper = oper;

    //        this.CurrentTarget = target;
    //        this.Wip = wip;

    //        if (wip != null)
    //        {
    //            ATItemSiteBuffer itembuffer = ItemSiteBufferHelper.GetItemSite(wip.WipInfo.SiteID, wip.ItemID, wip.Buffer.BufferID);
    //            this.CurrentItemSiteBuffer = itembuffer;

    //            // 임시
    //            this.CurrentItemSiteBuffer = target.CurrentItemBuffer; //CurrentBomDetail.FromItemSiteBuffer;

    //            this.CurrentBom = target.CurrentBom;
    //            this.CurrentRoute = target.CurrentBomRoute;

    //            this.LastStepTime = ATUtil.MaxTime(wip.AvailableTime, ATOption.Instance.PlanStartTime);
    //        }
    //        else
    //        {
    //            this.CurrentItemSiteBuffer = target.CurrentItemBuffer; // target.CurrentBomDetail.FromItemSiteBuffer;

    //            this.CurrentBom = target.CurrentBom;
    //            this.CurrentRoute = target.CurrentBomRoute;

    //            this.LastStepTime = ATOption.Instance.PlanStartTime; //target.TargetDateTime;
    //        }

    //        this.LotState = wip != null ? wip.State : LotState.Wait;

    //        this.CreateType = createType;

    //        this.Plans = new List<APEPlanInfo>();
    //        this.CapaPlans = new List<APEPlanInfo>();


    //        this.InitOperTarget = target;
            
    //        //this.IsOrgLot = false;
            
    //        //this.ShortItemSiteBuffer = null;
    //    }




    //    public object Clone()
    //    {
    //        APELot clone = (APELot)this.MemberwiseClone();
    //        return clone;
    //    }

    //    public ATOperation MoveFirst(DateTime now)
    //    {
    //        // FindBom
    //        this.CurrentOper = this.InitOper;

    //        return this.CurrentOper;
    //    }

    //    public ATOperation MoveNext(DateTime now)
    //    {
    //        ATOperation oper = null;

    //        if (this.CurrentOper.IsBuffer)
    //        {
    //            // Buffer In 시점에 CurrentBom & CurrentRoute 설정.
    //            if (this.CurrentRoute == null)
    //                return null;

    //            oper = this.CurrentRoute.FirstOper as ATOperation;
    //        }
    //        else
    //        {
    //            oper = this.CurrentOper.GetDefaultNextStep() as ATOperation;

    //            if (oper == null && this.CurrentBom != null)
    //            {
    //                oper = this.CurrentBom.ToBuffer;
    //            }

    //        }

    //        return oper;
    //    }
    //}
}
