using Mozart.Collections;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.ObyO;
using Mozart.SeePlan.Aleatorik.ObyO.Data;
using Mozart.SeePlan.Aleatorik.Outputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    /// <summary>
    /// PegPart의 전체 Short 정보를 관리
    /// </summary>
    public class APEShortManager
    {
        public List<APEPegPart> InTargets { get; set; }

        public HashSet<ATBom> ShortBoms { get; set; }

        public Dictionary<ATBom, int> CumShortBoms { get; set; }

        public HashSet<ATResource> ShortResources { get; set; }

        public Dictionary<string, ATShortManager> ShortManagers { get; set; }

        public APEShortManager(ICollection<APEPegPart> pegParts)
        {
            this.InTargets = new List<APEPegPart>();
            this.ShortManagers = new Dictionary<string, ATShortManager>();

            foreach (var pegPart in pegParts)
            {
                ATShortManager shortManager = pegPart.SampleTarget.Demand.ShortManager;

                shortManager.RootPegPart = pegPart;
                shortManager.RootPegTarget = pegPart.SampleTarget;

                string key = pegPart.SampleTarget.Demand.ID;

                if (ShortManagers.ContainsKey(key) == false)
                    ShortManagers.Add(key, shortManager);
            }

            // 아래 정보들도 PegPart가 들고 있어야 할까??
            this.ShortBoms = new HashSet<ATBom>();
            this.CumShortBoms = new Dictionary<ATBom, int>();
            this.ShortResources = new HashSet<ATResource>();
        }

        public virtual bool IsRetryInPhase(Dictionary<IComparable, APETargetGroup> targetGroups, ITargetGroup pegPart, List<APEPegPart> pegParts, int retryCount)
        {
            return false;
        }

        public void AddShortInfo(ATShortInfo info, bool isBWShort)
        {
            string demandID = info.PegPart.SampleTarget.Demand.ID;

            ATShortManager shortManager;
            if (this.ShortManagers.TryGetValue(demandID, out shortManager) == false)
                return; // 없는 케이스

            if (isBWShort)
            {
                if (info.PegPart.CurShortInfo != null)
                    return;

                info.PegPart.CurShortInfo = info;
            }

            #region ShortInfo 등록
            if (info.PegPart.RootAssyInfo != null)
            {
                var key = info.PegPart.RootAssyInfo.Key;
                if (shortManager.AssyShortInfo.TryGetValue(info.ShortType, key, out Dictionary<ATItemSiteBuffer, List<ATShortInfo>> dic) == false)
                {
                    dic = new Dictionary<ATItemSiteBuffer, List<ATShortInfo>>();
                    shortManager.AssyShortInfo.Add(info.ShortType, key, dic);
                }

                if (dic.TryGetValue(info.PegPart.RootAssyInfo.FromISB, out List<ATShortInfo> lst) == false)
                {
                    lst = new List<ATShortInfo>();
                    dic.Add(info.PegPart.RootAssyInfo.FromISB, lst);
                }
                lst.Add(info);
            }
            else
            {
                if (shortManager.ShortInfos.TryGetValue(info.ShortType, out List<ATShortInfo> lst) == false)
                {
                    lst = new List<ATShortInfo>();
                    shortManager.ShortInfos.Add(info.ShortType, lst);
                }

                lst.Add(info);
            }

            #endregion

            if (info.Resource != null)
                this.ShortResources.Add(info.Resource);

            if (info.Bom != ATBom.NULL)
                this.ShortBoms.Add(info.Bom);

            // ShortBom 등록
            HashSet<ATBom> shortBoms = new HashSet<ATBom>();
            if (info.ShortLot == null)
            {
                var orgPegPartTarget = info.OperTarget;
                while (orgPegPartTarget != null)
                {
                    shortBoms.Add(orgPegPartTarget.CurrentBom);

                    orgPegPartTarget = orgPegPartTarget.Next;
                }
            }
            else
            {
                var shortLot = info.ShortLot;
                var orgLotTarget = shortLot.InitOperTarget;
                while (orgLotTarget != null)
                {
                    shortBoms.Add(orgLotTarget.CurrentBom);
                    orgLotTarget = orgLotTarget.Next;
                }

                foreach (var history in shortLot.AssemblyHistory)
                {
                    foreach (var partLot in history.PartLots)
                    {
                        var partLotTarget = partLot.InitOperTarget;
                        while (partLotTarget != null)
                        {
                            shortBoms.Add(partLotTarget.CurrentBom);

                            partLotTarget = partLotTarget.Next;
                        }
                    }
                }
            }

            foreach (var shortBom in shortBoms)
            {
                if (CumShortBoms.ContainsKey(shortBom) == false)
                    CumShortBoms.Add(shortBom, 0);

                CumShortBoms[shortBom] += 1;
            }
        }

        internal void ClearShortInfo(ShortType shortType = ShortType.All)
        {
            foreach (var shortManager in this.ShortManagers.Values)
            {
                if (shortType == ShortType.All)
                {
                    shortManager.ShortInfos.Clear();
                    shortManager.AssyShortInfo.Clear();
                }
                else
                {
                    if (shortManager.ShortInfos.ContainsKey(shortType))
                        shortManager.ShortInfos.Remove(shortType);

                    if (shortManager.AssyShortInfo.ContainsKey(shortType))
                        shortManager.AssyShortInfo.Remove(shortType);
                }
            }
        }

        public void ConvertToNextPhase()
        {
            foreach (var shortManager in ShortManagers.Values)
            {
                if (shortManager.ShortInfos.TryGetValue(ShortType.InPhase, out var shortInfos))
                {
                    if (shortManager.ShortInfos.TryGetValue(ShortType.NextPhase, out List<ATShortInfo> infos) == false)
                    {
                        infos = new List<ATShortInfo>();
                        shortManager.ShortInfos.Add(ShortType.NextPhase, infos);
                    }

                    foreach (var shortInfo in shortInfos)
                    {
                        shortInfo.ShortType = ShortType.NextPhase;
                        infos.Add(shortInfo);
                    }
                }

                if (shortManager.AssyShortInfo.TryGetValue(ShortType.InPhase, out var assyShortInfos))
                {
                    if (shortManager.AssyShortInfo.TryGetValue(ShortType.NextPhase, out Dictionary<string, Dictionary<ATItemSiteBuffer, List<ATShortInfo>>> aInfos) == false)
                    {
                        aInfos = new Dictionary<string, Dictionary<ATItemSiteBuffer, List<ATShortInfo>>>();
                        shortManager.AssyShortInfo.Add(ShortType.NextPhase, aInfos);
                    }

                    foreach (var assyShortInfo in assyShortInfos)
                    {
                        foreach (var info in assyShortInfo.Value)
                        {
                            foreach (var item in info.Value)
                            {
                                item.ShortType = ShortType.NextPhase;

                                if (aInfos.TryGetValue(assyShortInfo.Key, out Dictionary<ATItemSiteBuffer, List<ATShortInfo>> values) == false)
                                {
                                    values = new Dictionary<ATItemSiteBuffer, List<ATShortInfo>>();
                                    aInfos.Add(assyShortInfo.Key, values);
                                }

                                if (values.TryGetValue(item.PegPart.RootAssyInfo.FromISB, out List<ATShortInfo> infos) == false)
                                {
                                    infos = new List<ATShortInfo>();
                                    values.Add(item.PegPart.RootAssyInfo.FromISB, infos);
                                }

                                infos.Add(item);
                            }
                        }
                    }
                }

                shortManager.ShortInfos.Remove(ShortType.InPhase);
                shortManager.AssyShortInfo.Remove(ShortType.InPhase);
            }
        }

        public List<APEPegPart> CreateRetryShortPegPart(ShortType shortType)
        {
            List<APEPegPart> pegParts = new List<APEPegPart>();

            foreach (var shortManager in ShortManagers.Values)
            {
                if (shortManager.ShortInfos.TryGetValue(shortType, out var shortInfos) == false)
                    shortInfos = new List<ATShortInfo>();

                if (shortManager.AssyShortInfo.TryGetValue(shortType, out var assyShortInfos) == false)
                    assyShortInfos = new Dictionary<string, Dictionary<ATItemSiteBuffer, List<ATShortInfo>>>();

                double retryQty = shortInfos.Sum(x => x.Qty / x.BCumChangeRatio);

                foreach (var pair in assyShortInfos)
                {
                    double assyQty = 0;

                    foreach (var item in pair.Value)
                    {
                        // Assy Part의 경우, Short 난 정보 중, Max 수량.
                        double sumQty = item.Value.Sum(x => x.Qty / x.BCumChangeRatio);
                        assyQty = Math.Max(assyQty, sumQty);
                    }
                    retryQty += assyQty;
                }

                if (retryQty <= ATOption.Instance.MinimumAllocationQuantity)
                    continue;

                var pegPart = shortManager.RootPegPart;
                var moPlan = shortManager.RootPegTarget.MoPlan;
                moPlan.Qty = retryQty;

                APEPegPart newPegPart = new APEPegPart(pegPart.MoMaster, pegPart.MoMaster.ItemSiteBuffer);
                APETarget newPegTarget = new APETarget(newPegPart, moPlan, moPlan.ID);

                newPegPart.CurrentOperation = null;
                newPegPart.AddPegTarget(newPegTarget);
                newPegPart.Type = PegPartType.Short;
                newPegTarget.Group = newPegPart;

                pegParts.Add(newPegPart);
            }
            
            return pegParts;
        }

        internal void WriteShortReport(int retryCount)
        {
            foreach (var shortManager in this.ShortManagers.Values)
            {
                foreach (var shortInfo in shortManager.ShortInfos)
                {
                    foreach (var info in shortInfo.Value)
                    {
                        OutputWriter.Instance.WriteShortReport(info, retryCount);
                    }
                }

                foreach (var assyShortInfos in shortManager.AssyShortInfo)
                {
                    foreach (var assyShortInfo in assyShortInfos)
                    {
                        foreach (var info in assyShortInfo.Value)
                        {
                            OutputWriter.Instance.WriteShortReport(info, retryCount);
                        }
                    }
                }
            }
        }

        public virtual List<APEPegPart> CreateRetryPegPart(ShortType shortType, List<APEPegPart> remainPegParts)
        {
            List<APEPegPart> retryPegParts = new List<APEPegPart>();

            // KTR : 해당 코드와 같은 경우는 자제하였으면 좋겠음.
            // 추후 디버깅할 때의 어려움이 있음.
            // Short 난 PegPart와 InTarget으로 생성된 PegPart가 다른 MoID라는 것이 보장되나?
            // retryPegPart와 remainPegPart가 Merged 되어야 할 것 같은데?
            // 예를 들어서 Bom을 특정 버퍼에서 찢었는데,  TAT상 한쪽은 Short이 발생하고, 한쪽은 그냥 InTarget으로 만들어지면
            // 두개의 PegPart는 합쳐서 하나의 PegPart로 만들어줘야 하는 것 아닌가?
            retryPegParts = CreateRetryShortPegPart(shortType);

            if (remainPegParts != null && remainPegParts.Count > 0)
            {
                foreach (APEPegPart remain in remainPegParts)
                {
                    var moPlan = remain.SampleTarget.MoPlan;
                    moPlan.Qty = remain.SampleTarget.RemainQty;

                    APEPegPart newPegPart = new APEPegPart(remain.MoMaster, remain.MoMaster.ItemSiteBuffer);
                    APETarget newPegTarget = new APETarget(newPegPart, moPlan, moPlan.ID);

                    newPegPart.CurrentOperation = null;
                    newPegPart.AddPegTarget(newPegTarget);
                    newPegPart.Type = PegPartType.InTarget;
                    newPegTarget.Group = newPegPart;

                    retryPegParts.Add(newPegPart);
                }
            }

            return retryPegParts;
        }

        public void AddInTargetPegPart(APEPegPart pegPart)
        {
            this.InTargets.Add(pegPart);
        }

        public List<APEPegPart> GetInTargetPegParts()
        {
            return this.InTargets;
        }

        public void ClearInTargetPegParts()
        {
            this.InTargets.Clear();
        }
    }
}
