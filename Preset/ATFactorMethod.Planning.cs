using Mozart.SeePlan.Aleatorik.Planner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    #region Compare Rule Point
    /// <summary>
    /// Allocate Init 시 LotGroup내 Lot들 정렬
    /// </summary>
    /// <param name="bom"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    [RulePoint("CompareLotInGroup")]
    public delegate ATFactorValue CompareLotInGroup(IAPELot lot, ATWeightFactor factor);

    #region LFS
    /// <summary>
    /// LFS 시 장비 소팅
    /// </summary>
    /// <param name="bom"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    [RulePoint("CompareResourceOnLFS")]
    public delegate ATFactorValue CompareResourceOnLFS(PBFResource bucket, ATWeightFactor factor, APELotGroup lotGroup, PBFAllocateContext context);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bucket"></param>
    /// <param name="addBucket"></param>
    /// <param name="lotGroup"></param>
    /// <param name="factor"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [RulePoint("CompareAddResource")]
    public delegate ATFactorValue CompareAddResource(PBFResource bucket, PBFResource addBucket, APELotGroup lotGroup, ATWeightFactor factor, PBFAllocateContext context);

    /// <summary>
    /// LFS 시 LotGroup
    /// </summary>
    /// <param name="bom"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    [RulePoint("CompareLotGroupOnLFS")]
    public delegate ATFactorValue CompareLotGroupOnLFS(APELotGroup lotGroup, ATWeightFactor factor, PBFAllocateContext context);
    #endregion

    #region RFS
    /// <summary>
    /// RFS 시 장비 소팅
    /// </summary>
    /// <param name="bom"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    [RulePoint("CompareResourceOnRFS")]
    public delegate ATFactorValue CompareResourceOnRFS(PBFResource bucket, ATWeightFactor factor, PBFAllocateContext context);

    /// <summary>
    /// RFS 시 LotGroup
    /// </summary>
    /// <param name="factor"></param>
    /// <returns></returns>
    [RulePoint("CompareLotGroupOnRFS")]
    public delegate ATFactorValue CompareLotGroupOnRFS(APELotGroup lotGroup, PBFResource bucket, ATWeightFactor factor, PBFAllocateContext context);
    #endregion

    #endregion

    #region Filter Rule Point
    /// <summary>
    /// 현 Phase 내에 대상이 아닌 Lot을 수행 대상에서 제거.
    /// </summary>
    /// <param name="lot"></param>
    /// <returns></returns>
    [RulePoint("FilterLot")]
    public delegate ATFilterValue FilterLot(IAPELot lot, ATWeightFactor factor, int pahse);

    #region LFS
    /// <summary>
    /// LFS 시 Bucket Filter
    /// </summary>
    /// <param name="bom"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    [RulePoint("FilterResourceOnLFS")]
    public delegate ATFilterValue FilterResourceOnLFS(PBFResource bucket, ATWeightFactor factor, APELotGroup lotGroup, PBFAllocateContext context);


    /// <summary>
    /// LFS 시 Bucket Filter
    /// </summary>
    /// <param name="bom"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    [RulePoint("FilterLotGroupInLevel")]
    public delegate ATFilterValue FilterLotGroupInLevel(APELotGroup lotGroup, ATWeightFactor factor, PBFAllocateContext context);

    /// <summary>
    /// LFS 시 Bucket Filter
    /// </summary>
    /// <param name="bom"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    [RulePoint("FilterLotGroupOnLFS")]
    public delegate ATFilterValue FilterLotGroupOnLFS(APELotGroup lotGroup, ATWeightFactor factor, PBFAllocateContext context);
    #endregion

    #region RFS
    /// <summary>
    /// 
    /// </summary>
    /// <param name="bucket"></param>
    /// <param name="factor"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [RulePoint("FilterResourceOnRFS")]
    public delegate ATFilterValue FilterResourceOnRFS(PBFResource bucket, ATWeightFactor factor, PBFAllocateContext context);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="lotGroup"></param>
    /// <param name="bucket"></param>
    /// <param name="factor"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [RulePoint("FilterLotGroupOnRFS")]
    public delegate ATFilterValue FilterLotGroupOnRFS(APELotGroup lotGroup, PBFResource bucket, ATWeightFactor factor, PBFAllocateContext context);
    #endregion

    #endregion

    #region Get Rule Point
    /// <summary>
    /// 
    /// </summary>
    /// <param name="lot"></param>
    /// <returns></returns>
    [RulePoint("LotGroupKey")]
    public delegate string LotGroupKey(IAPELot lot);
    #endregion
}
