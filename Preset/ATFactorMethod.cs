using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Pegging;
using System;
using System.Collections.Generic;

namespace Mozart.SeePlan.Aleatorik
{
    #region Compare Rule Point
    
    /// <summary>
    /// BOM 우선순위 정렬
    /// </summary>
    /// <param name="bom"></param>
    /// <param name="factor"></param>
    /// <param name="pegPart"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [RulePoint("CompareBom")]
    public delegate ATFactorValue CompareBom(ATBom bom, ATWeightFactor factor, APEPegPart pegPart, APESelectBomContext context);

    /// <summary>
    ///
    /// </summary>
    /// <param name="group"></param>
    /// <param name="factor"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [RulePoint("CompareTargetGroup")]
    public delegate ATFactorValue CompareTargetGroup(APETargetGroup targetGroup, ATWeightFactor factor, APEPegContext context);

    /// <summary>
    ///
    /// </summary>
    /// <param name="target"></param>
    /// <param name="factor"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [RulePoint("CompareTargetInGroup")]
    public delegate ATFactorValue CompareTargetInGroup(APETarget pegTarget, ATWeightFactor factor, APEPegContext context);

    /// <summary>
    ///
    /// </summary>
    /// <param name="target"></param>
    /// <param name="factor"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [RulePoint("CompareWip")]
    public delegate ATFactorValue CompareWip(APEWip planWip, ATWeightFactor factor, APEPegContext context);

    /// <summary>
    ///
    /// </summary>
    /// <param name="target"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    [RulePoint("ComparePeggingGroup")]
    public delegate ATFactorValue ComparePeggingGroup(APEPeggingGroup peggingGroup, ATWeightFactor factor);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="refPlan"></param>
    /// <param name="pegPart"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    [RulePoint("CompareRefProdPlan")]
    public delegate ATFactorValue CompareRefProdPlan(APERefPlan refPlan, ATWeightFactor factor, APEPegPart pegPart);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bom"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    [RulePoint("CompareLot")]
    public delegate ATFactorValue CompareLot(APELot lot, ATWeightFactor factor);
    #endregion

    #region Filter Rule Point
    /// <summary>
    ///
    /// </summary>
    /// <param name="target"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [RulePoint("FilterTargetGroup")]
    public delegate ATFilterValue FilterTargetGroup(APETargetGroup targetGroup, ATWeightFactor factor, APEPegContext context);


    /// <summary>
    ///
    /// </summary>
    /// <param name="wip"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [RulePoint("FilterWip")]
    public delegate ATFilterValue FilterWip(APEWip planWip, ATWeightFactor factor, APETarget pegTarget, APEPegContext context);

    /// <summary>
    ///
    /// </summary>
    /// <param name="wip"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [RulePoint("FilterBom")]
    public delegate ATFilterValue FilterBom(ATBom bom, ATWeightFactor factor, APEPegPart pegPart);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pegPart"></param>
    /// <returns></returns>
    [RulePoint("FilterTarget")]
    public delegate ATFilterValue FilterTarget(APEPegPart pegPart, ATWeightFactor factor);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="refPlan"></param>
    /// <param name="pegPart"></param>
    /// <returns></returns>
    [RulePoint("FilterRefProdPlan")]
    public delegate ATFilterValue FilterRefProdPlan(APERefPlan refPlan, ATWeightFactor factor, APEPegPart pegPart);
    #endregion

    #region Get Rule Point
    /// <summary>
    ///
    /// </summary>
    /// <param name="wip"></param>
    /// <returns></returns>
    [RulePoint("WipKey")]
    public delegate List<string> WipKey(APEWip planWip);

    /// <summary>
    ///
    /// </summary>
    /// <param name="group"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [RulePoint("PeggingKey")]
    public delegate List<string> PeggingKey(ITargetGroup targetGroup, APEPegContext context);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="part"></param>
    /// <returns></returns>
    [RulePoint("TargetGroupKey")]
    public delegate string TargetGroupKey(APEPegPart pegPart);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="part"></param>
    /// <returns></returns>
    [RulePoint("PeggingGroupKey")]
    public delegate string PeggingGroupKey(APEPegPart pegPart);

    #endregion
}
