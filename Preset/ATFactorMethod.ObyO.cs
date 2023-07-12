using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.ObyO.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    #region Compare Rule Point

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pegPart"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    [RulePoint("CompareAssemblyPart")]
    public delegate ATFactorValue CompareAssemblyPart(APEPegPart pegPart, ATWeightFactor factor);

    /// <summary>
    /// Obo 에서 Capa 우선순위
    /// </summary>
    /// <param name="capaInfo"></param>
    /// <param name="factor"></param>
    /// <returns></returns>
    [RulePoint("CompareCapacity")]
    public delegate ATFactorValue CompareCapacity(APECapacity capaInfo, ATWeightFactor factor, PBOAllocateContext context);
    #endregion

    #region Filter Rule Point
    /// <summary>
    /// Obo에서 할당 제외 대상 Arrange
    /// </summary>
    /// <param name="wip"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [RulePoint("FilterOperResource")]
    public delegate ATFilterValue FilterOperResource(ATOperResource arrange, ATWeightFactor factor, PBOAllocateContext context);

    /// <summary>
    /// Obo에서 할당 제외 대상 CapacityInfo
    /// </summary>
    /// <param name="wip"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    [RulePoint("FilterCapacity")]
    public delegate ATFilterValue FilterCapacity(APECapacity capacity, ATWeightFactor factor, PBOAllocateContext context);
    #endregion
}
