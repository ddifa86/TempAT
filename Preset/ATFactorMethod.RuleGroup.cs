using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Pegging;
using System;
using System.Collections.Generic;

namespace Mozart.SeePlan.Aleatorik.Preset
{

    [RulePoint("WriteTargetPlan")]
    public delegate Outputs.TARGET_PLAN WriteTargetPlan(Outputs.TARGET_PLAN entity, APEPegPart pegPart, ATOperTarget operTarget);

}
