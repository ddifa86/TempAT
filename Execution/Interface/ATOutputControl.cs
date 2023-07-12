using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Planner;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    [FEComponent(FECategory.Execution, FEControl.OutputMapper, Root = FEProvider.Aleatorik)]
    public class ATOutputControl : IModelController
    {
        public static ATOutputControl Instance
        {
            get { return ServiceLocator.Resolve<ATOutputControl>(); }
        }

        public Type ControllerType => typeof(ATOutputControl);
        

        [FEAction]
        public virtual Outputs.BOM_NETWORK_INFO WriteBomNetwork(Outputs.BOM_NETWORK_INFO entity, ATBomDetail detail, ATItemSiteBuffer sop)
        {
            return entity;
        }

        //[FEAction]
        public virtual Outputs.ERROR_LOG WriteInputErrorLog(Outputs.ERROR_LOG entity)
        {
            return entity;
        }

        //[FEAction]
        public virtual Outputs.ITEM_SITE_BUFFER_ALT_INFO WriteItemSiteBufferAltInfo(Outputs.ITEM_SITE_BUFFER_ALT_INFO entity)
        {
            return entity;
        }

        //[FEAction]
        public virtual Outputs.EXECUTION_TIME_LOG WriteExecutionTimeLog(Outputs.EXECUTION_TIME_LOG entity)
        {
            return entity;
        }
        [FEAction]
        public virtual Outputs.PEG_INFO WritePegInfo(Outputs.PEG_INFO entity, ATOperTarget target, APEWip wip, double pegQty, double sumQty, APEPegContext context)
        {
            return entity;
        }
        [FEAction]
        public virtual Outputs.PSI_REPORT WritePsiReport(Outputs.PSI_REPORT entity)
        {
            return entity;
        }
        [FEAction]
        public virtual Outputs.RES_PLAN WriteResPlan(Outputs.RES_PLAN entity, IBucket bucket, IAPELot lot, APEPlanInfo planInfo, PlanInfoType writeType)
        {
            return entity;
        }

        [FEAction]
        public virtual Outputs.SHIPMENT_PLAN WriteShipmentPlan(Outputs.SHIPMENT_PLAN entity, ATDemand demand)
        {
            return entity;
        }

        [FEAction]
        public virtual Outputs.STAGE_OUT_PLAN WriteStageOutPlan(Outputs.STAGE_OUT_PLAN entity, APELot lot)
        {
            return entity;
        }

        [FEAction]
        public virtual Outputs.SHORT_REPORT WriteShortReport(Outputs.SHORT_REPORT entity, IAPELot lot, ATShortInfo shortInfo)
        {
            return entity;
        }

        [FEAction]
        public virtual Outputs.SHORT_REPORT WriteShortReportInLoading(Outputs.SHORT_REPORT entity)
        {
            return entity;
        }

        [FEAction]
        public virtual Outputs.UNPEG_INFO WriteUnpegInfo(Outputs.UNPEG_INFO entity, APEWip wip, ATUnpegReason reason)
        {
            return entity;
        }

        [FEAction]
        public virtual Outputs.UNPEG_INFO WriteUnpegInfoInLoading(Outputs.UNPEG_INFO entity, Inputs.WIP wip, ATUnpegReason reason)
        {
            return entity;
        }

        //[FEAction]
        public virtual Outputs.LOT_ASSEMBLY_LOG WriteBatchAssemblyLog(Outputs.LOT_ASSEMBLY_LOG entity)
        {
            return entity;
        }

        [FEAction]
        public virtual Outputs.LOT_HISTORY WriteLotHistory(Outputs.LOT_HISTORY entity, APELot lot)
        {
            return entity;
        }

        [FEAction]
        public virtual Outputs.LOT_ASSEMBLY_LOG WriteLotAssemblyLog(Outputs.LOT_ASSEMBLY_LOG entity, APELot assyLot, APELot partlot, ATBomDetail detail)
        {
            return entity;
        }

        [FEAction]
        public virtual Outputs.LOT_SPLIT_LOG WriteLotSplitLog(Outputs.LOT_SPLIT_LOG entity, Data.ATSplitInfo splitInfo)
        {
            return entity;
        }

        [FEAction]
        public virtual Outputs.COMPARE_BOM_LOG WriteCompareBomLog(Outputs.COMPARE_BOM_LOG entity, APESelectBomContext context)
        {
            return entity;
        }

        [FEAction]
        public virtual Outputs.ELAPSED_TIME_LOG WriteElapsedtimeLog(Outputs.ELAPSED_TIME_LOG entity)
        {
            return entity;
        }

        [FEAction]
        public virtual Outputs.INIT_DEMAND_LOG WriteInitDemandLog(Outputs.INIT_DEMAND_LOG entity)
        {
            return entity;
        }

        [FEAction]
        public virtual Outputs.INTARGET_PLAN WriteIntargetPlan(Outputs.INTARGET_PLAN entity, APEPegPart pegPart, ATOperTarget operTarget)
        {
            return entity;
        }

        [FEAction]
        public virtual Outputs.PROD_PLAN WriteProdPlan(Outputs.PROD_PLAN entity, APEPlanInfo info)
        {
            return entity;
        }

        [FEAction]
        public virtual Outputs.SHORT_PROD_PLAN WriteShortProdPlan(Outputs.SHORT_PROD_PLAN entity, APEPlanInfo planInfo)
        {
            return entity;
        }

     
        [FEAction]
        public virtual Outputs.TARGET_PLAN WriteTargetPlan(Outputs.TARGET_PLAN entity, APEPegPart pegPart, ATOperTarget operTarget)
        {
            return entity;
        }

        [FEAction]
        public virtual Outputs.REF_PROD_PLAN_LOG WriteRefProdPlan(Outputs.REF_PROD_PLAN_LOG entity)
        {
            return entity;
        }

        [FEAction]
        public virtual Outputs.ALLOCATION_LOG WriteAllocationLog(Outputs.ALLOCATION_LOG entity, PBFAllocateContext context)
        {
            return entity;
        }

        [FEAction]
        public virtual Outputs.SHORT_LOG WriteShortLog(Outputs.SHORT_LOG entity, ATLateInfo info)
        {
            return entity;
        }

        [FEAction]
        public virtual Outputs.CAPA_ALLOCATION_INFO WriteCapaAllocationInfo(Outputs.CAPA_ALLOCATION_INFO entity)
        {
            return entity;
        }
    }
}
 