
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Inputs;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public partial class ObjectMapper
    {
        public static ObjectMapper Instance
        {
            get
            {
#if BUILD_PKG
                CheckInstance();
#endif

                return ServiceLocator.Resolve<ObjectMapper>();
            }
        }

        #region Method

        private static void CheckInstance()
        {
            var baseType = typeof(ObjectMapper);
            if (ServiceLocator.IsRegistered(baseType))
                return;

            foreach (var assy in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assy.GetTypes())
                {
                    if (type.IsSubclassOf(baseType))
                    {
                        var instance = (IModelController)Activator.CreateInstance(type);
                        ServiceLocator.RegisterController(instance);
                        return;
                    }
                }
            }

        }

        internal static T Create<T>() where T : new()
        {
            if (IsRegistered(typeof(T)))
                return TypeRegistry.Create<T>();
            else
                return new T();
        }

        internal static T Create<T>(params object[] args) where T : class
        {
            var t = TypeRegistry.Resolve(typeof(T)) ?? typeof(T);
            var instance = Activator.CreateInstance(t, args);

            return (T)instance;
        }


        internal static bool IsRegistered(Type type)
        {
            return TypeRegistry.Resolve(type) != null;
        }
        #endregion

        #region Persist

        internal static ATStage CreateStage(Inputs.STAGE_MASTER entity)
        {
            var info = ObjectMapper.Create<ATStage>(
                entity.STAGE_ID,
                entity.DESCRIPTION
                );


          

            return info;
        }

        internal static ATAllocationGroup CreateAllocationGroup(Inputs.ALLOCATION_GROUP_MASTER entity, ATStage stage, AllocateType type)
        {
            var info = ObjectMapper.Create<ATAllocationGroup>(
                entity.ALLOCATION_GROUP_ID,
                entity.ALLOCATION_GROUP_SEQ,
                stage,
                type
                );

            return info;
        }


        internal static ATBuffer CreateBuffer(Inputs.BUFFER_MASTER entity, ATStage stage)
        {
            var info = ObjectMapper.Create<ATBuffer>(
                entity.BUFFER_ID,
                entity.BUFFER_SEQ,
                stage
                );

            ATPersistHelper.SetDefaultPropertyValue(info, PropertyCategory.Buffer.ToString());

            return info;
        }

        internal static ATDemand CreateDemand(Inputs.DEMAND entity, string soID, double qty, string stageID, ATItemSiteBuffer itemsite, ATCustomer customer)
        {
            var info = ObjectMapper.Create<ATDemand>(
                stageID,
                soID,
                entity.SITE_ID,
                itemsite,
                customer,
                entity.DUE_DATE,
                qty,
                entity.DEMAND_PRIORITY,
                entity.CUST_ID,
                entity.MAX_LATENESS_DAY,
                entity.MAX_EARLINESS_DAY
                );

            info.DemandType = entity.DEMAND_TYPE;

            info.IsRtfTarget = "Y";

            ATPersistHelper.SetDefaultPropertyValue(info, PropertyCategory.SalesOrder.ToString());

            return info;
        }

        internal static ATDemand CreateDemand(ATDemand demand, string soID, double qty, ATItemSiteBuffer itemsite)
        {
            var info = ObjectMapper.Create<ATDemand>(
                demand.StageID,
                soID,
                demand.SiteID,
                itemsite,
                demand.DueDateTime,
                qty,
                demand.Priority,
                demand.CustomerID,
                demand.MaxLateDays,
                demand.MaxEarlyDays
                );

            info.IsRtfTarget = demand.IsRtfTarget;

            ATPersistHelper.SetDefaultPropertyValue(info, PropertyCategory.SalesOrder.ToString());

            return info;
        }

        internal static APERefPlan CreateRefProductionPlan(Inputs.REF_PROD_PLAN entity, ATItemSiteBuffer itemsite, string type, ATStage stage, string soID)
        {
            var info = ObjectMapper.Create<APERefPlan>( 
                entity.REF_PLAN_VER,
                itemsite,
                entity.DUE_DATE,
                entity.DEMAND_QTY,
                type,
                stage,
                soID
                );

            return info;
        }

        internal static ATItem CreateItem(Inputs.ITEM_MASTER entity, ItemType type, int grade, string name, string group, string unit, string procurementType, string prodType, string itemSize)
        {
            ATItem info = null;

            info = ObjectMapper.Create<ATItem>(
                    entity.ITEM_ID,
                    type,
                    grade, 
                    name,
                    group, 
                    unit,
                    procurementType, 
                    prodType, 
                    itemSize
                    );

            ATPersistHelper.SetDefaultPropertyValue(info, PropertyCategory.Item.ToString());

            return info;
        }

        internal static ATBom CreateBom(Inputs.BOM_MASTER entity, BomType bomtype)
        {
            ATBom info = null;

            info = ObjectMapper.Create<ATBom>(
                    entity.BOM_ID
                    , bomtype
                    , entity.BOM_PRIORITY
                    , entity.EFF_START_DATE
                    , entity.EFF_END_DATE
                    );

            ATPersistHelper.SetDefaultPropertyValue(info, PropertyCategory.Bom.ToString());

            return info;
        }


        internal static ATBomDetail CreateBomDetail(Inputs.BOM_DETAIL entity,
            ATBom bom, ATSite fromSite, ATItem fromitem, ATBuffer frombuffer, ATSite toSite, ATItem toitem, ATBuffer tobuffer)
        {
            ATBomDetail info = null;

            info = ObjectMapper.Create<ATBomDetail>(
                bom
                , fromSite
                , fromitem
                , frombuffer
                , Math.Round( entity.FROM_QTY , 4)
                , toSite
                , toitem
                , tobuffer
                , Math.Round( entity.TO_QTY, 4)
                );

          

            return info;
        }

        internal static ATRoute CreateRoute(Inputs.ROUTING_MASTER entity)
        {
            ATRoute info = null;

            info = ObjectMapper.Create<ATRoute>(
               entity.ROUTING_ID
               , entity.EFF_START_DATE
               , entity.EFF_END_DATE
                    );

            return info;
        }

        internal static ATOperation CreateRouteOper(Inputs.ROUTING_OPER entity, OperType operType, double yield, ATCalendar tatCal, ATCalendar yieldCal, double multiLotSize)
        {
            ATOperation info = null;

            info = ObjectMapper.Create<ATOperation>(
               entity.OPER_ID
               , entity.OPER_SEQ
               , operType
               , entity.WAIT_TAT
               , entity.RUN_TAT
               , yield
               , multiLotSize
               );

            info.SetTatCal(tatCal);
            info.SetYieldCal(yieldCal);

            return info;
        }

        internal static ATBomRouting CreateBomRouting(Inputs.BOM_ROUTING entity, ATBom bom, ATRoute route)
        {
            ATBomRouting info = null;

            info = ObjectMapper.Create<ATBomRouting>(
                bom
                , route
                , entity.ROUTING_PRIORITY
               );

            ATPersistHelper.SetDefaultPropertyValue(info, PropertyCategory.BomRouting.ToString());

            return info;
        }


        internal static ATWipInfo CreateWipInfo(Inputs.WIP entity,
            WipType lotType, LotState lotState,
            ATSite site,
            ATItem item,
            ATRoute route,
            ATOperation oper,
            ATResource resource,
            ATStage stage,
            ATBuffer buffer
            )
        {
            ATWipInfo info = null;

            info = ObjectMapper.Create<ATWipInfo>(
              entity.WIP_ID,
              entity.WIP_QTY,
              lotType,
              lotState,
              site,
              item,
              route,
              oper,
              resource,
              entity.AVAILABLE_DATETIME,
              entity.TRACK_IN_DATETIME,
              stage
               );

            ATPersistHelper.SetDefaultPropertyValue(info, PropertyCategory.Wip.ToString());

            //
            var itemsite = ATInputData.ItemSiteBuffers.GetItemSite(site.SiteID, item.ItemID, buffer.BufferID);

            if (itemsite != null)
            {
                info.ItemSiteBuffer = itemsite;
            }


            return info;
        }

        internal static ATCalendar CreateCalendar(Inputs.CALENDAR_MASTER entity)
        {
            ATCalendar info;

            info = ObjectMapper.Create<ATCalendar>
                (
                entity.CALENDAR_ID,
                entity.CALENDAR_TYPE
                )
                ;

            return info;

        }

        internal static ATCalendar CreateCalendar(string calendarID, string calendarType)
        {
            ATCalendar info;

            info = ObjectMapper.Create<ATCalendar>
                (
                calendarID,
                calendarType
                )
                ;

            return info;
        }

        internal static ATCalendarDetail CreateCalendarDetail(Inputs.CALENDAR_DETAIL entity, ATCalendar calendar
                , CalendarPatternType patternType)
        {
            ATCalendarDetail info;

            info = ObjectMapper.Create<ATCalendarDetail>
                (
                calendar,
                entity.PATTERN_ID,
                entity.EFF_START_DATE,
                entity.EFF_END_DATE,
                patternType,
                entity.PATTERN_VALUE,
                entity.PATTERN_PRIORITY
                )
                ;

            return info;

        }

        internal static ATCalendarDetail CreateCalendarDetail(ATCalendar calendar, string patternSeq, DateTime startTime, DateTime endTime
            , CalendarPatternType patternType, string patternValue )
        {
            ATCalendarDetail info;

            info = ObjectMapper.Create<ATCalendarDetail>
                (
                calendar,
                patternSeq,
                startTime,
                endTime, 
                patternType,
                patternValue,
                0
                )
                ;

            return info;
        }

        internal static ATCalendarAttribute CreateCalendarAttribute(Inputs.CALENDAR_BASED_ATTR entity, ATCalendarDetail detail, string internalPatternSeq, DataType dataType )
        {
            ATCalendarAttribute info;

            info = ObjectMapper.Create<ATCalendarAttribute>
                (
                detail,
                entity.ATTR_TYPE,
                entity.ATTR_VALUE,
                dataType,
                internalPatternSeq
                )
                ;

            return info;
        }

        internal static ATCalendarAttribute CreateCalendarAttribute(ATCalendarDetail detail, string attribute, string value, DataType dataType, string patternSeq)
        {
            ATCalendarAttribute info;

            info = ObjectMapper.Create<ATCalendarAttribute>
                (
                detail,
                attribute,
                value,
                dataType,
                patternSeq
                )
                ;

            return info;

        }

        internal static ATResource CreateResource(Inputs.RES_MASTER entity, ResourceType restype, ResourceCategory category,
            CapacityType capatype, CapacityMode capaMode, ATResourceGroup resGroup, string resGroupID,
            double utilRate, ATCalendar capaCal, ATCalendar utilRateCal, ATCalendar pmCal, ATSetupInfo setup)
        {
            ATResource info;

            info = ObjectMapper.Create<ATResource>
                (
                entity.RES_ID,
                category,
                restype,
                entity.RES_SITE_ID,
                capatype,
                capaMode,
                resGroup,
                resGroupID,
                utilRate
                )
                ;

            info.CreateCapaInfo(capaCal);
            info.SetUtilRateCal(utilRateCal);
            info.SetPmInfo(pmCal);
            info.SetSetupInfo(setup);

            ATPersistHelper.SetDefaultPropertyValue(info, PropertyCategory.Resource.ToString());

            return info;
        }

        internal static ATResource CreateResource(Inputs.RES_MASTER entity, ResourceCategory category)
        {
            ATResource info;

            info = ObjectMapper.Create<ATResource>
                (
                entity.RES_ID,
                category
                )
                ;

            ATPersistHelper.SetDefaultPropertyValue(info, PropertyCategory.Resource.ToString());

            return info;
        }

        internal static ATResourceGroup CreateResGroup(Inputs.RES_GROUP_MASTER entity, ATAllocationGroup allocGroup, bool isReSorting)
        {
            ATResourceGroup info;

            info = ObjectMapper.Create<ATResourceGroup>
                (
                entity.RES_GROUP_ID,
                entity.RES_GROUP_SEQ,
                allocGroup,
                isReSorting
                )
                ;

            return info;
        }

        internal static ATOperResource CreateArrange(Inputs.OPER_RES entity, ATRoute route, ATOperation oper, ATResource resource, ATCalendar usagePerCal, ATCalendar flowTimeCal)
        {
            ATOperResource info;

            info = ObjectMapper.Create<ATOperResource>
                (
                route,
                oper,
                resource,
                entity.FLOW_TIME,
                entity.USAGE_PER,
                entity.RES_PRIORITY,
                ArrangeType.Main,
                PropertyCategory.OperationResource
                )
                ;
            
            info.SetUsagePerCal(usagePerCal);
            info.SetFlowTimeCal(flowTimeCal);

            return info;
        }

        internal static ATOperResource CreateAddionalArrange(ATOperResource arrange, ATResource addResource, OPER_ADD_RES entity)
        {
            ATOperResource info;

            info = ObjectMapper.Create<ATOperResource>
                (
                arrange.Route,
                arrange.Oper,
                addResource,
                0,
                entity.USAGE_PER,
                arrange.Priority,
                ArrangeType.Additinal,
                PropertyCategory.OperationAdditionalResource
                )
                ;

            return info;
        }


        internal static ATSite CreateSite(string siteID, string siteName)
        {
            var info = ObjectMapper.Create<ATSite>(
                siteID,
                siteName
                );

            ATPersistHelper.SetDefaultPropertyValue(info, PropertyCategory.Site.ToString());

            return info;
        }

        internal static ATItemSiteBuffer CreateItemSite(ATSite site, ATItem item, ATBuffer buffer, bool isInfiniteMaterial, bool isNocarryOver, double InputLotSize)
        {
            var info = ObjectMapper.Create<ATItemSiteBuffer>(
                site,
                item,
                buffer,
                isInfiniteMaterial,
                isNocarryOver,
                InputLotSize
                    );

            ATPersistHelper.SetDefaultPropertyValue(info, PropertyCategory.ItemSiteBuffer.ToString());

            return info;
        }

        internal static ATCustomer CreateCustomer(string customerID, string customerName, double priority)
        {
            var info = ObjectMapper.Create<ATCustomer>(
                customerID,
                customerName,
                priority
                );

            ATPersistHelper.SetDefaultPropertyValue(info, PropertyCategory.Customer.ToString());

            return info;
        }

        internal static ATSetupDetail CreateSetupInfo(SETUP entity, string fromCondition, string toCondition, string setupType,
            List<ATResource> setupResources, bool isMultiSetup)
        {
            var info = ObjectMapper.Create<ATSetupDetail>(entity.SETUP_ID, 
                fromCondition, 
                toCondition, 
                entity.SETUP_TIME, 
                setupType, 
                entity.SETUP_PRIORITY, 
                setupResources, 
                isMultiSetup);

            return info;
        }

        internal static ATConstraintInfo CreateConstraintInfo(CONSTRAINT entity, ATCalendar calendar, ATProperty property, ConstraintPolicy policy)
        {
            var info = ObjectMapper.Create<ATConstraintInfo>(entity.CONSTRAINT_ID, property, entity.PROP_VALUE, calendar, policy);

            return info;
        }

        #endregion
    }
}
