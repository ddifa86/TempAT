using Mozart.SeePlan.Aleatorik.DataModel;
using Mozart.SeePlan.Aleatorik.Data;
using Mozart.SeePlan.Aleatorik.Outputs;
using Mozart.Task.Execution.Persists;
using Mozart.SeePlan.Aleatorik.Persists;
using Mozart.SeePlan.Aleatorik.Inputs;
using Mozart.Task.Execution;
using Mozart.Extensions;
using Mozart.Collections;
using Mozart.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using Mozart.SeePlan.DataModel;
using System.Reflection;
using System.Data;
using System.Linq.Expressions;
using System.Globalization;
using Mozart.Data.Entity;
using CollectionExtensions = Mozart.Extensions.CollectionExtensions;

namespace Mozart.SeePlan.Aleatorik.Logic
{
    [FeatureBind()]
    public partial class PersistInputs 
    {
        public void OnAction_STAGE(Task.Execution.Persists.IPersistContext context)
        {
            ATElapsedTimeChecker.Instance.StartCustomTimer("Engine_Total");
            ATElapsedTimeChecker.Instance.StartCustomTimer("DataLoad");

            List<ERROR_LOG> ErrLogs = new List<ERROR_LOG>();

            if (AleatorikInputMart.Instance.STAGE_MASTER.Rows.Count() == 0)
            {
                OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Critical, ErrorReasonCode.NotFoundKeyInput,
                        null, "STAGE_MASTER@" + ErrKey.StageMaster, "", ErrDesc.NotFoundKeyInput("STAGE_MASTER"));
            }

            // 함수를 보여줄 필요가 있을까?? 추후 논의..
            foreach (var entity in AleatorikInputMart.Instance.STAGE_MASTER.Rows)
            {
                var stage = ObjectMapper.CreateStage(entity);

                ATExecutionContext.Instance.AddStage(stage);
            }
        }

        public void OnAction_ALLOCATION_GROUP(IPersistContext context)
        {
            // 함수를 보여줄 필요가 있을까?? 추후 논의..
            foreach (var entity in AleatorikInputMart.Instance.ALLOCATION_GROUP_MASTER.Rows)
            {
                var stage = ATExecutionContext.Instance.GetStage(entity.STAGE_ID);
                if (stage == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.AllocationGroup, "STAGE_MASTER@STAGE_ID", ErrDesc.NotFoundReferredData("STAGE_ID", "STAGE"));

                    continue;
                }

                var type = ATUtil.StringToEnum<AllocateType>(entity.ALLOCATION_TYPE, AllocateType.LotFirstSelection);

                var obj = ObjectMapper.CreateAllocationGroup(entity, stage, type);

                ATInputData.AllocationGroups.AddAlloctionGroup(obj);
            }
        }


        public void OnAction_BUFFER_MASTER(IPersistContext context)
        {
            if (AleatorikTempMart.Instance.BUFFER_MASTER.Rows.Count == 0)
            {
                OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Critical, ErrorReasonCode.NotFoundKeyInput,
                        null, "BUFFER_MASTER@" + ErrKey.BufferMaster, "", ErrDesc.NotFoundKeyInput("BUFFER_MASTER"));
            }

            foreach (var entity in AleatorikTempMart.Instance.BUFFER_MASTER.Rows)
            {
                List<ERROR_LOG> errLogs = new List<ERROR_LOG>();

                var stage = ATExecutionContext.Instance.GetStage(entity.STAGE_ID);
                if (stage == null)
                {
                    errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.BufferMaster, "STAGE_MASTER@STAGE_ID", ErrDesc.NotFoundReferredData("STAGE_ID", "STAGE")));
                }

                if(stage.Buffers.Values.Where(x=>x.Sequence == entity.BUFFER_SEQ).FirstOrDefault() != null)
                {
                    errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.DataDuplication,
                        entity, ErrKey.BufferMaster));
                }

                if (errLogs.Count() != 0)
                {
                    ErrorHelper.AddErrLog(entity, ErrLogKeySchema.Buffer, ErrLogKeyColumn.Buffer, errLogs);
                    continue;
                }

                ATBuffer obj = ObjectMapper.CreateBuffer(entity, stage);

                if (!DerivedHelper.CallAfterLoadHandler<BUFFER_MASTER>(entity, obj))
                    continue;

                stage.Buffers.Add(obj.BufferID, obj);
                stage.BufferRoute.Opers.Add(obj);
                ATInputData.ItemSiteBuffers.AddBuffer(obj);
            }

            foreach (var stage in ATExecutionContext.Instance.Stages)
            {
                //stage의 buffer에 oper가 없으면 stage를 없애도록 validation 처리 필요
                stage.BufferRoute.Opers.Sort(
                        delegate (Step x, Step y)
                        {
                            if (object.ReferenceEquals(x, y))
                                return 0;

                            int cmp = (x as ATOperation).Sequence.CompareTo((y as ATOperation).Sequence);

                            return cmp;
                        }
                    );

                stage.BufferRoute.LinkOpers();
            }
        }


        public void OnAction_EXECUTION_OPTION_CONFIG(IPersistContext context)
        {
            if (AleatorikInputMart.Instance.SCENARIO_OPTION_CONFIG.Rows.Count == 0)
            {
                OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Critical, ErrorReasonCode.NotFoundKeyInput,
                       null, "SCENARIO_OPTION_CONFIG@" + ErrKey.ScenarioOptionConfig, "", ErrDesc.NotFoundKeyInput("SCENARIO_OPTION_CONFIG"));
            }

            foreach (var entity in AleatorikInputMart.Instance.SCENARIO_OPTION_CONFIG.Rows)
            {
                if (entity.MODULE_ID != ModuleType.Global.ToString())
                    continue;

                ATOption.Instance.SetGlobalOption(entity.OPTION_ID, entity.OPTION_VALUE);

                OutputWriter.Instance.WriteExecutionOptionConfigLog(entity);
            }
        }

        public void OnAction_ITEM_MASTER(IPersistContext context)
        {
            if (AleatorikTempMart.Instance.ITEM_MASTER.Rows.Count() == 0)
            {
                OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Critical, ErrorReasonCode.NotFoundKeyInput,
                       null, "ITEM_MASTER@" + ErrKey.ItemMaster, "", ErrDesc.NotFoundKeyInput("ITEM_MASTER")) ;
            }

            // Item을 하나로 묶어서 관리를 해야하나.. Product / Matrial을 나누어서 관리해야하나..
            // Type별로 관리해야하는건 맞겠지..?
            foreach (var entity in AleatorikTempMart.Instance.ITEM_MASTER.Rows)
            {
                var type = ATUtil.StringToEnum<ItemType>(entity.ITEM_TYPE, ItemType.None);
                List<ERROR_LOG> errLogs = new List<ERROR_LOG>();

                if (type == ItemType.None)
                {
                    errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                        entity, ErrKey.ItemMaster, "", ErrDesc.MismatchReservedWord("ITEM_TYPE", ATUtil.GetEnumProperty<ItemType>())));
                }

                if (errLogs.Count() != 0)
                {
                    ErrorHelper.AddErrLog(entity, ErrLogKeySchema.Item, ErrLogKeyColumn.Item, errLogs);
                    continue;
                }

                ATItem obj = ObjectMapper.CreateItem(entity, type, entity.ITEM_GRADE, entity.ITEM_NAME
                        ,entity.ITEM_GROUP ,entity.ITEM_UNIT ,entity.PROCUREMENT_TYPE ,entity.PROD_TYPE ,entity.ITEM_SIZE
                    );
                if (!DerivedHelper.CallAfterLoadHandler<ITEM_MASTER>(entity, obj))
                {
                    continue;
                }

                ATInputData.ItemSiteBuffers.AddItem(obj);

            }
        }



        public void OnAction_BOM_MASTER(IPersistContext context)
        {
            if (AleatorikTempMart.Instance.BOM_MASTER.Rows.Count == 0)
            {
                OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Critical, ErrorReasonCode.NotFoundKeyInput, null,
                    "BOM_MASTER@" + ErrKey.BomMaster, "", ErrDesc.NotFoundKeyInput("BOM_MASTER"));
            }

            foreach (var entity in AleatorikTempMart.Instance.BOM_MASTER.Rows)
            {
                List<ERROR_LOG> errLogs = new List<ERROR_LOG>();

                var type = ATUtil.StringToEnum<BomType>(entity.BOM_TYPE, BomType.None);
                if (type == BomType.None)
                {
                    errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord, entity, ErrKey.BomMaster
                        , "",  ErrDesc.MismatchReservedWord("BOM_TYPE", ATUtil.GetEnumProperty<BomType>())));
                }

                if (entity.EFF_START_DATE >= entity.EFF_END_DATE)
                {
                    errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.OutOfRange, entity, ErrKey.BomMaster
                         , "", ErrDesc.OutOfRange_datetime("EFF_START_TIME", "EFF_END_TIME")));

                }
                if (errLogs.Count() != 0)
                {
                    ErrorHelper.AddErrLog(entity, ErrLogKeySchema.Bom, ErrLogKeyColumn.Bom, errLogs);
                    continue;
                }

                ATBom obj = ObjectMapper.CreateBom(entity, type);
                if (!DerivedHelper.CallAfterLoadHandler<BOM_MASTER>(entity, obj))
                {
                    continue;
                }

                ATInputData.Boms.AddBom(obj);
            }
        }

        public void OnAction_BOM_DETAIL(IPersistContext context)
        {
            if (AleatorikTempMart.Instance.BOM_DETAIL.Rows.Count == 0)
            {
                OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Critical, ErrorReasonCode.NotFoundKeyInput, 
                    null, "BOM_DETAIL@" + ErrKey.BomDetail, "", ErrDesc.NotFoundKeyInput("BOM_DETAIL"));
            }

            foreach (var entity in AleatorikTempMart.Instance.BOM_DETAIL.Rows)
            {
                List<ERROR_LOG> errLogs = new List<ERROR_LOG>();

                #region
                ATBom bom = ATInputData.Boms.GetBom(entity.BOM_ID);
                if (bom == null)
                {
                    if (ErrorHelper.IsErrorObject(ErrLogKeySchema.Bom, entity.BOM_ID) == true) // chain
                    {
                        errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.ChainError,
                           entity, ErrKey.BomDetail, "BOM_MASTER@BOM_ID", ErrDesc.ChainError()));
                    }
                    else
                    {
                        errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                           entity, ErrKey.BomDetail, "BOM_MASTER@BOM_ID", ErrDesc.NotFoundReferredData("BOM_ID", "BOM_MASTER")));
                    }

                    bom = new ATBom("tmp", BomType.None, 0, DateTime.Now, DateTime.Now);
                }

                ATItemSiteBuffer fromISB = ATInputData.ItemSiteBuffers.GetItemSite(entity.FROM_SITE_ID, entity.FROM_ITEM_ID, entity.FROM_BUFFER_ID);
                if (fromISB == null)
                {
                    bom.IsInvalidBom = true;

                    if (ErrorHelper.IsErrorObject(ErrLogKeySchema.ItemSiteBuffer, entity.FROM_ITEM_ID, entity.FROM_SITE_ID, entity.FROM_BUFFER_ID) == true) // chain
                    {
                        errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.ChainError,
                            entity, ErrKey.BomDetail, "ITEM_SITE_BUFFER_MASTER@SITE_ID,ITEM_ID,BUFFER_ID", ErrDesc.ChainError()));

                    }
                    else
                    {
                        errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                            entity, ErrKey.BomDetail, "ITEM_SITE_BUFFER_MASTER@SITE_ID,ITEM_ID,BUFFER_ID", ErrDesc.NotFoundReferredData("FROM_SITE_ID,FROM_ITEM_ID,FROM_BUFFER_ID", "ITEM_SITE_BUFFER_MASTER")));
                    }
                }

                ATItemSiteBuffer toISB = ATInputData.ItemSiteBuffers.GetItemSite(entity.TO_SITE_ID, entity.TO_ITEM_ID, entity.TO_BUFFER_ID);
                if (toISB == null)
                {
                    bom.IsInvalidBom = true;
                    if (ErrorHelper.IsErrorObject(ErrLogKeySchema.ItemSiteBuffer, entity.TO_ITEM_ID, entity.TO_SITE_ID, entity.TO_BUFFER_ID) == true) // chain
                    {
                        errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.ChainError, 
                            entity, ErrKey.BomDetail, "ITEM_SITE_BUFFER_MASTER@SITE_ID,ITEM_ID,BUFFER_ID", ErrDesc.ChainError()));
                    }
                    else
                    {
                        errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData, 
                            entity, ErrKey.BomDetail, "ITEM_SITE_BUFFER_MASTER@SITE_ID,ITEM_ID,BUFFER_ID", ErrDesc.NotFoundReferredData("TO_SITE_ID,TO_ITEM_ID,TO_BUFFER_ID", "ITEM_SITE_BUFFER_MASTER")));
                    }
                }

                if (entity.FROM_QTY <= ATOption.Instance.MinimumAllocationQuantity)
                {
                    bom.IsInvalidBom = true;
                    errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.OutOfRange, 
                        entity, ErrKey.BomDetail, "", ErrDesc.OutOfRange_greater("FROM_QTY")));
                }

                if (entity.TO_QTY <= ATOption.Instance.MinimumAllocationQuantity)
                {
                    bom.IsInvalidBom = true;
                    errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.OutOfRange, 
                        entity, ErrKey.BomDetail, "", ErrDesc.OutOfRange_greater("TO_QTY")));
                }
                #endregion

                if (errLogs.Count() != 0)
                {
                    ErrorHelper.AddErrLog(entity, ErrLogKeySchema.BomDetail, ErrLogKeyColumn.BomDetail, errLogs);
                    continue;
                }

                ATBomDetail obj = ObjectMapper.CreateBomDetail(entity, bom, fromISB.Site, fromISB.Item, fromISB.Buffer, toISB.Site, toISB.Item, toISB.Buffer);
                if (!DerivedHelper.CallAfterLoadHandler<BOM_DETAIL>(entity, obj))
                {
                    bom.IsInvalidBom = true;
                    continue;
                }

#warning Bin(Yield)Portion Calendar 반영
                if (string.IsNullOrEmpty(entity.CALENDAR_ID) == false)
                {
                    // BIN PORTION CALENDAR 반영
                    var calendar = ATInputData.Calendars.GetCalendar(entity.CALENDAR_ID);

                    if (calendar != null)
                    {
                        obj.Calendar = calendar;
                        foreach (var detail in calendar.Details.Values)
                        {
                            var period = AleatorikGlobalParameters.Instance.period;
                            var startTime = detail.EffectiveStartTime;

                            if (startTime < AleatorikGlobalParameters.Instance.start_time)
                                startTime = AleatorikGlobalParameters.Instance.start_time;

                            var endTime = detail.EffectiveEndTime;

                            if (endTime > AleatorikGlobalParameters.Instance.start_time.AddDays(period))
                                endTime = AleatorikGlobalParameters.Instance.start_time.AddDays(period);

                            var attr = detail.GetAttribte(ATReservedCode.YIELD).FirstOrDefault();
                            if (attr == null)
                                continue;

                            while (startTime < endTime)
                            {
                                if (detail.IsEffectiveTime(startTime) == false)
                                {
                                    startTime = startTime.AddDays(1);
                                    continue;
                                }

                                string applyDate = startTime.ToString(ATUtil.DateFormat);

                                if (obj.ToQtyOnCalendar.ContainsKey(applyDate) == false)
                                    obj.ToQtyOnCalendar.Add(applyDate, attr.Value);


                                startTime = startTime.AddDays(1);
                            }
                        }
                    }
                    else
                    { 
                        //Calendar 잘못된 값으로 설정한 경우
                        errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                                entity, ErrKey.BomDetail, "CALENDAR_MASTER@CALENDAR_ID", ErrDesc.NotFoundReferredData("CALENDAR_ID","CALENDAR_MASTER")));

                        ErrorHelper.AddErrLog(entity, ErrLogKeySchema.BomDetail, ErrLogKeyColumn.BomDetail, errLogs);

                        continue;
                    }
                }

                bom.BomDetails.Add(obj);

                if (bom.MainBomDetail == null)
                {
                    bom.MainBomDetail = obj;
                }
                else
                {
                    if (obj.ToQty > bom.MainBomDetail.ToQty)
                        bom.MainBomDetail = obj;
                }

                ATInputData.Boms.AddBomDetails(obj);
            }

            // BOM이 Normal이 아닌데 BomDetail이 1개인 경우나 유효하지 않음 Bom 지우기
            foreach (var bom in ATInputData.Boms.GetBoms())
            {
                if (bom.IsInvalidBom == true)
                    bom.BomDetails.Clear();
            }

            // ItemBuffer 생성 작업.
            // ItemBuffer는 Inbound 작업을 하지 않고 내부적으로 생성하는 주요 객체
            // 추후 생성하는 위치에 대한 고려 필요.
            foreach (var bomDetails in ATInputData.Boms.GetBomDetails())
            {
                foreach (var detail in bomDetails.Value)
                {
                    var bom = detail.Bom;

                    if (bom.IsInvalidBom)
                        continue;

                    detail.FromItemSiteBuffer.AddNextbom(bom);
                    detail.ToItemSiteBuffer.AddPrevBom(bom);

                    HashSet<ATItemSiteBuffer> fromIsbs;
                    if (bom.PrevItemSiteBuffers.TryGetValue(detail.FromBuffer, out fromIsbs) == false)
                    {
                        fromIsbs = new HashSet<ATItemSiteBuffer>();
                        bom.PrevItemSiteBuffers.Add(detail.FromBuffer, fromIsbs);
                    }
                    fromIsbs.Add(detail.FromItemSiteBuffer);

                    HashSet<ATItemSiteBuffer> nextIsbs;
                    if (bom.NextItemSiteBuffers.TryGetValue(detail.ToBuffer, out nextIsbs) == false)
                    {
                        nextIsbs = new HashSet<ATItemSiteBuffer>();
                        bom.NextItemSiteBuffers.Add(detail.ToBuffer, nextIsbs);
                    }
                    nextIsbs.Add(detail.ToItemSiteBuffer);

                    if (bom.BomType == BomType.Assembly)
                        detail.ToItemSiteBuffer.PrevAssyItemSiteBuffers.Add(detail.ToItemSiteBuffer);
                }
            }

            //BomDetail 없는 Bom 삭제
            ATInputData.Boms.RemoveEmptyBomDetailBoms();
        }

        public void OnAction_ROUTING_MASTER(IPersistContext context)
        {
            if (AleatorikTempMart.Instance.ROUTING_MASTER.Rows.Count() == 0)
            {
                OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Critical, ErrorReasonCode.NotFoundKeyInput,
                        null, "ROUTING_MASTER@" + ErrKey.RoutingMaster, "", ErrDesc.NotFoundKeyInput("ROUTING_MASTER"));
            }

            foreach (var entity in AleatorikTempMart.Instance.ROUTING_MASTER.Rows)
            {
                List<ERROR_LOG> errLogs = new List<ERROR_LOG>();

                if (entity.EFF_START_DATE >= entity.EFF_END_DATE)
                {
                    errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.OutOfRange,
                      entity, ErrKey.RoutingMaster, "", ErrDesc.OutOfRange_datetime("EFF_START_DATE", "EFF_END_DATE")));
                }

                if (errLogs.Count() != 0)
                {
                    ErrorHelper.AddErrLog(entity, ErrLogKeySchema.Routing, ErrLogKeyColumn.Routing, errLogs);

                    continue;
                }

                var obj = ObjectMapper.CreateRoute(entity);

                if (!DerivedHelper.CallAfterLoadHandler<ROUTING_MASTER>(entity, obj))
                {
                    continue;
                }

                ATInputData.Boms.AddRoute(obj);
            }
        }

        public void OnAction_BOM_ROUTING(IPersistContext context)
        {
            if (AleatorikTempMart.Instance.BOM_ROUTING.Rows.Count() == 0)
            {
                OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Critical, ErrorReasonCode.NotFoundKeyInput,
                      null, "BOM_ROUTING@" + ErrKey.BomRouting, "", ErrDesc.NotFoundKeyInput("BOM_ROUTING"));
            }

            foreach (var entity in AleatorikTempMart.Instance.BOM_ROUTING.Rows)
            {
                List<ERROR_LOG> errLogs = new List<ERROR_LOG>();

                var bom = ATInputData.Boms.GetBom(entity.BOM_ID);
                if (bom == null)
                {
                    if (ErrorHelper.IsErrorObject(ErrLogKeySchema.Bom, entity.BOM_ID) == true) // chainError
                    {
                        errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.ChainError,
                            entity, ErrKey.BomRouting, "BOM_MASTER@BOM_ID", ErrDesc.ChainError()));
                    }
                    else
                    {
                        errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                            entity, ErrKey.BomRouting, "BOM_MASTER@BOM_ID", ErrDesc.NotFoundReferredData("BOM_ID", "BOM_MASTER")));
                    }
                }

                var route = ATInputData.Boms.GetRoute(entity.ROUTING_ID);
                if (route == null)
                {
                    if (ErrorHelper.IsErrorObject(ErrLogKeySchema.Routing, entity.ROUTING_ID) == true) // chainError
                    {
                        errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.ChainError,
                            entity, ErrKey.BomRouting, "ROUTE_MASTER@ROUTING_ID^BOM_ROUTING@BOM_ID,ROUTING_ID", ErrDesc.ChainError()));
                    }
                    else
                    {
                        errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                            entity, ErrKey.BomRouting, "ROUTE_MASTER@ROUTING_ID^BOM_ROUTING@BOM_ID,ROUTING_ID"
                            , ErrDesc.NotFoundReferredData("ROUTING_ID", "ROUTE_MASTER") + "^" + ErrDesc.NotFoundReferredData("BOM_ID,ROUTING_ID", "BOM_ROUTING")));
                    }
                }

                if (errLogs.Count() != 0)
                {
                    ErrorHelper.AddErrLog(entity, ErrLogKeySchema.BomRouting, ErrLogKeyColumn.BomRouting, errLogs);

                    continue;
                }

                var obj = ObjectMapper.CreateBomRouting(entity, bom, route);

                if (!DerivedHelper.CallAfterLoadHandler<BOM_ROUTING>(entity, obj))
                {
                    continue;
                }

                bom.AddBomRoute(obj);

                route.Bom = bom;

                ATInputData.Boms.AddBomRoutes(obj);
            }

            // Route에 Bom 정보가 Mapping되지 않은 경우. 기준정보 이슈로 제거
            var routes = ATInputData.Boms.GetRoute();
            List<ATRoute> del = new List<ATRoute>();
            foreach (var route in routes)
            {
                if (route.Bom == null)
                {
                    //ROUTING_MASTER entity = new ROUTING_MASTER();
                    //entity.ROUTING_ID = route.RouteID;

                    //OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                    //    entity, "ROUTING_ID", "BOM_ROUTING@ROUTING_ID", ErrDesc.NotFoundReferredData("ROUTING_ID", "BOM_ROUTING"));

                    del.Add(route);
                    continue;
                }
            }

            var boms = ATInputData.Boms.GetBoms();
            List<ATBom> delBoms = new List<ATBom>();
            foreach (var bom in boms)
            {
                if (bom.BomRoutes == null || bom.BomRoutes.Count() == 0)
                {
                    //BOM_MASTER entity = new BOM_MASTER();
                    //entity.BOM_ID = bom.BomID;

                    //OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                    //    entity, ErrKey.BomRouting, "BOM_ROUTING@BOM_ID", ErrDesc.NotFoundReferredData("BOM_ID", "BOM_ROUTING")); 

                    delBoms.Add(bom);
                    continue;
                }
            }


            del.ForEach(x => ATInputData.Boms.RemoveRoute(x.RouteID));
            delBoms.ForEach(x => ATInputData.Boms.RemoveBom(x.BomID));
        }

        public void OnAction_WIP(IPersistContext context)
        {
            //OutputWriter.Instance.WritePlanIndex("-", KpiCategory.INPUT.ToString(), KpiIndex.TOTAL_WIP_QTY.ToString(), "-", ""
            //    , AleatorikTempMart.Instance.WIP.Rows.Sum(x => x.WIP_QTY));

            //OutputWriter.Instance.WritePlanIndex("-", KpiCategory.INPUT.ToString(), KpiIndex.TOTAL_WIP_COUNT.ToString(), "-", ""
            //    , AleatorikTempMart.Instance.WIP.Rows.Count());

            foreach (var entity in AleatorikTempMart.Instance.WIP.Rows)
            {
                ATRoute route = null;
                ATOperation oper = null;
                ATResource resource = null;
                ATBuffer buffer = null;
                List<ERROR_LOG> errLogs = new List<ERROR_LOG>();

                WipType wipType = ATUtil.StringToEnum<WipType>(entity.WIP_TYPE, WipType.None);
                if (wipType == WipType.None)
                {
                    ATUnpegReason reason = new ATUnpegReason(UnpegCategory.InvalidData, "Invalid WIP_TYPE column", string.Format("WIP_ID : {0}", entity.WIP_ID));
                    ATInputData.Wips.AddPersistUnpegWip(entity, reason);

                    errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                       entity, ErrKey.Wip, "", ErrDesc.MismatchReservedWord("WIP_TYPE", ATUtil.GetEnumProperty<WipType>())));
                }

                LotState lotState = ATUtil.StringToEnum<LotState>(entity.WIP_STATUS, LotState.None);
                if (lotState == LotState.None)
                {
                    ATUnpegReason reason = new ATUnpegReason(UnpegCategory.InvalidData, "Invalid WIP_STATUS column", string.Format("WIP_ID : {0}", entity.WIP_ID));
                    ATInputData.Wips.AddPersistUnpegWip(entity, reason);

                    errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                       entity, ErrKey.Wip, "", ErrDesc.MismatchReservedWord("WIP_STATUS", ATUtil.GetEnumProperty<LotState>())));
                }

                if (lotState == LotState.Run)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Info, ErrorReasonCode.AutoCorrectionData,
                        entity, ErrKey.Wip, "", "Not yet provide STATUS = Run. Engine correct it to STATUS = Wait");
                }

                if (entity.WIP_QTY <= 0)
                {
                    ATUnpegReason reason = new ATUnpegReason(UnpegCategory.InvalidData, "Invalid WIP_QTY column", string.Format("WIP_ID : {0}", entity.WIP_ID));
                    ATInputData.Wips.AddPersistUnpegWip(entity, reason);

                    errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.OutOfRange,
                            entity, ErrKey.Wip, "", ErrDesc.OutOfRange_greater("LOT_QTY")));
                }

                ATStage stage = ATExecutionContext.Instance.GetStage(entity.STAGE_ID);
                if (stage == null)
                {
                    ATUnpegReason reason = new ATUnpegReason(UnpegCategory.InvalidData, "Not found stage info", string.Format("LOT_ID : {0}", entity.WIP_ID));
                    ATInputData.Wips.AddPersistUnpegWip(entity, reason);

                    errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                    entity, ErrKey.Wip, "STAGE@STAGE_ID", ErrDesc.NotFoundReferredData("STAGE_ID", "STAGE")));
                }

                ATSite site = ATInputData.ItemSiteBuffers.GetSite(entity.SITE_ID);
                if (site == null)
                {
                    ATUnpegReason reason = new ATUnpegReason(UnpegCategory.InvalidData, "Not In Site Master", string.Format("SITE {0}", entity.SITE_ID));
                    ATInputData.Wips.AddPersistUnpegWip(entity, reason);

                    errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                            entity, ErrKey.Wip, "SITE_MASTER@SITE_ID", ErrDesc.NotFoundReferredData("SITE_ID", "SITE_MASTER")));
                }

                ATItem item = ATInputData.ItemSiteBuffers.GetItem(entity.ITEM_ID);
                if (item == null)
                {
                    ATUnpegReason reason = new ATUnpegReason(UnpegCategory.InvalidData, "Not In Item Master", string.Format("ITEM {0}", entity.ITEM_ID));
                    ATInputData.Wips.AddPersistUnpegWip(entity, reason);

                    if (ErrorHelper.IsErrorObject(ErrLogKeySchema.Item, entity.ITEM_ID) == true) // chainError
                    {
                        errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.ChainError,
                            entity, ErrKey.Wip, "ITEM_MASTER@ITEM_ID", ErrDesc.ChainError()));
                    }
                    else
                    {
                        errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                            entity, ErrKey.Wip, "ITEM_MASTER@ITEM_ID", ErrDesc.NotFoundReferredData("ITEM_ID", "ITEM_MASTER")));
                    }
                }

                if (wipType == WipType.Inventory)
                {
                    if (string.IsNullOrEmpty(entity.ROUTING_ID) == false)
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Info, ErrorReasonCode.AutoCorrectionData,
                            entity, ErrKey.Wip, "", "If WIP_TYPE is Inventory, ROUTING_ID must be Null");
                    }

                    if (string.IsNullOrEmpty(entity.OPER_ID) == false)
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Info, ErrorReasonCode.AutoCorrectionData,
                            entity, ErrKey.Wip, "", "If WIP_TYPE is Inventory, OPER_ID must be Null");
                    }
                }


                if (wipType == WipType.Inventory || wipType == WipType.Dummy)
                {
                    oper = ATInputData.ItemSiteBuffers.GetBuffer(entity.BUFFER_ID);

                    if (oper == null)
                    {
                        ATUnpegReason reason = new ATUnpegReason(UnpegCategory.InvalidData, "Not In Buffer Master", string.Format("BUFFER_ID : {0}", entity.BUFFER_ID));
                        ATInputData.Wips.AddPersistUnpegWip(entity, reason);

                        if (ErrorHelper.IsErrorObject(ErrLogKeySchema.Buffer, entity.BUFFER_ID) == true) // chainError
                        {
                            errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.ChainError,
                                entity, ErrKey.Wip, "BUFFER_MASTER@BUFFER_ID", ErrDesc.ChainError()));
                        }
                        else
                        {
                            errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                                entity, ErrKey.Wip, "BUFFER_MASTER@BUFFER_ID", ErrDesc.NotFoundReferredData("BUFFER_ID", "BUFFER_MASTER")));
                        }
                    }
                }
                else
                {
                    route = ATInputData.Boms.GetRoute(entity.ROUTING_ID);
                    if (route == null)
                    {
                        ATUnpegReason reason = new ATUnpegReason(UnpegCategory.InvalidData, "Not In Routing Master", string.Format("ROUTING_ID : {0}", entity.ROUTING_ID));
                        ATInputData.Wips.AddPersistUnpegWip(entity, reason);

                        if (ErrorHelper.IsErrorObject(ErrLogKeySchema.Routing, entity.ROUTING_ID) == true) // chainError
                        {
                            errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.ChainError,
                                entity, ErrKey.Wip, "ROUTING_MASTER@ROUTING_ID", ErrDesc.ChainError()));
                        }
                        else
                        {
                            errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                                entity, ErrKey.Wip, "ROUTING_MASTER@ROUTING_ID", ErrDesc.NotFoundReferredData("ROUTING_ID", "ROUTING_MASTER")));
                        }
                    }

                    oper = route != null ? route.FindOper(entity.OPER_ID) : null;
                    if (oper == null)
                    {
                        ATUnpegReason reason = new ATUnpegReason(UnpegCategory.InvalidData, "Not In Routing Operation", string.Format("ROUTING_ID : {0}, OPER_ID : {1}", entity.ROUTING_ID, entity.OPER_ID));
                        ATInputData.Wips.AddPersistUnpegWip(entity, reason);

                        if (ErrorHelper.IsErrorObject(ErrLogKeySchema.RoutingOper, entity.ROUTING_ID, entity.OPER_ID) == true) // chainError
                        {
                            errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.ChainError,
                                entity, ErrKey.Wip, "ROUTING_OPER@ROUTING_ID,OPER_ID", ErrDesc.ChainError()));
                        }
                        else
                        {
                            errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                                entity, ErrKey.Wip, "ROUTING_OPER@ROUTING_ID,OPER_ID", ErrDesc.NotFoundReferredData("ROUTING_ID,OPER_ID", "ROUTING_OPER")));
                        }
                    }

                    if (lotState == LotState.Run)
                    {
                        resource = ATInputData.Resources.GetResource(entity.RES_ID);

                        if (string.IsNullOrEmpty(entity.RES_ID) == false && resource == null)
                        {
                            OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Info, ErrorReasonCode.NotFoundReferredData,
                                entity, ErrKey.Wip, "RES_MASTER@RES_ID", ErrDesc.NotFoundReferredData("RES_ID", "RES_MASTER")); 
                        }
                    }
                }

                if (oper != null && oper.IsBuffer)
                {
                    buffer = oper as ATBuffer;
                }
                else if (route != null)
                {
                    buffer = route.Bom.BomDetails.First().FromBuffer;
                    if (route.Bom.BomType == BomType.Assembly)
                        buffer = route.Bom.BomDetails.First().ToBuffer;
                }

                var itemsite = ATInputData.ItemSiteBuffers.GetItemSite(entity.SITE_ID, entity.ITEM_ID, entity.BUFFER_ID);

                // 여기 고민
                if (itemsite == null)
                {
                    ATUnpegReason reason = new ATUnpegReason(UnpegCategory.InvalidData, "Not In Item Site Buffer Info", string.Format("ITEM_SITE_BUFFER : {0}_{1}_{2}", entity.ITEM_ID, entity.SITE_ID, entity.BUFFER_ID));
                    ATInputData.Wips.AddPersistUnpegWip(entity, reason);

                    if (ErrorHelper.IsErrorObject(ErrLogKeySchema.ItemSiteBuffer, entity.ITEM_ID, entity.SITE_ID, entity.BUFFER_ID) == true) // chainError
                    {
                        errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.ChainError,
                            entity, ErrKey.Wip, "ITEM_SITE_BUFFER_MASTER@ITEM_ID,SITE_ID,BUFFER_ID", ErrDesc.ChainError()));
                    }
                    else
                    {
                        errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                            entity, ErrKey.Wip, "ITEM_SITE_BUFFER_MASTER@ITEM_ID,SITE_ID,BUFFER_ID", ErrDesc.NotFoundReferredData("ITEM_ID,SITE_ID,BUFFER_ID", "ITEM_SITE_BUFFER_MASTER")));
                    }
                }

                if (errLogs.Count() != 0)
                {
                    ErrorHelper.AddErrLog(entity, ErrLogKeySchema.Wip, ErrLogKeyColumn.Wip, errLogs);

                    continue;
                }

                var obj = ObjectMapper.CreateWipInfo(
                        entity
                    , wipType
                    , lotState
                    , site
                    , item, route, oper, resource
                    , stage
                    , buffer
                    );

                // itemsite에 Wip 수량 정보 등록
                if (!DerivedHelper.CallAfterLoadHandler<WIP>(entity, obj))
                {
                    continue;
                }
                
                ATInputData.Wips.AddWipInfo(obj);
            }
        }

        public void OnAction_FACTOR_MASTER(IPersistContext context)
        {
            //if (AleatorikTempMart.Instance.FACTOR_MASTER.Rows.Count == 0)
            //{
            //    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Critical, ErrorReasonCode.NotFoundKeyInput,
            //            null, "FACTOR_MASTER@" + ErrKey.FactorMaster, "", ErrDesc.NotFoundKeyInput("FACTOR_MASTER"));
            //}

            List<ATFactor> factors = new List<ATFactor>();

            foreach (var entity in AleatorikTempMart.Instance.FACTOR_MASTER.Rows)
            {
                FactorType factorType = ATUtil.StringToEnum<FactorType>(entity.FACTOR_TYPE, FactorType.None);

                if (factorType == FactorType.None)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                        entity, ErrKey.FactorMaster, "", ErrDesc.MismatchReservedWord("FACTOR_TYPE", ATUtil.GetEnumProperty<FactorType>()));

                    continue;
                }

                if (factorType == FactorType.Predefined && string.IsNullOrEmpty(entity.FACTOR_SCRIPT) == false)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Info, ErrorReasonCode.AutoCorrectionData,
                        entity, ErrKey.FactorMaster, "", "If FACTOR_TYPE is Predefined, FACTOR_SCRIPT must be Null");
                }


                RulePoint rulePoint = ATUtil.StringToEnum<RulePoint>(entity.RULE_POINT, RulePoint.None);
                if (rulePoint == RulePoint.None)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                        entity, ErrKey.FactorMaster, "", ErrDesc.MismatchReservedWord("RULE_POINT", ATUtil.GetEnumProperty<RulePoint>()));

                    continue;
                }

                ATFactor factor = new ATFactor(entity.FACTOR_ID, rulePoint, entity.DESCRIPTION, factorType, entity.FACTOR_SCRIPT);

                factors.Add(factor);
            }

            CreateFactorMethod(context.ModelContext, factors);

            foreach (var factor in factors)
            {
                factor.Method = RuleManager.Instance.GetMethod(factor.RulePoint.ToString(), factor.FactorID);

                if (factor.Method == null)
                {
                    FACTOR_MASTER entity = new FACTOR_MASTER();
                    entity.FACTOR_ID = factor.FactorID;
                    entity.RULE_POINT = factor.RulePointID;

                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.IncompatibleRule,
                        entity, ErrKey.FactorMaster, "", "fail to function mapping");

                    continue;
                }

                ATRuleAgent.Instance.AddFactor(factor);
            }
        }

        public void OnAction_PRESET_MASTER(IPersistContext context)
        {
            foreach (var entity in AleatorikTempMart.Instance.RULE_MASTER.Rows)
            {
                // 아래 조건 및 RulePoint 변수는 불필요하지 않을까...??
                RulePoint rulePoint = ATUtil.StringToEnum<RulePoint>(entity.RULE_POINT, RulePoint.None);
                if (rulePoint == RulePoint.None)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                        entity, ErrKey.RuleMaster, "", ErrDesc.MismatchReservedWord("RULE_POINT", ATUtil.GetEnumProperty<RulePoint>()));

                    continue;
                }

                ATRule rule = ATRuleAgent.Instance.GetRule(entity.RULE_POINT);
                if (rule == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                        entity, ErrKey.RuleMaster, "", ErrDesc.MismatchReservedWord("RULE_POINT", ATUtil.GetEnumProperty<RulePoint>()));

                    continue;
                }

                SortType sortType = ATUtil.StringToEnum<SortType>(entity.SORT_TYPE, SortType.None);
                if (sortType == SortType.None && (rule.RulePointType != RulePointType.ListKey && rule.RulePointType != RulePointType.MergeKey))
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                        entity, ErrKey.RuleMaster, "", ErrDesc.MismatchReservedWord("SORT_TYPE", ATUtil.GetEnumProperty<SortType>()));

                    continue;
                }

                var obj = new ATWeightPreset(rule, entity.RULE_ID, rulePoint, sortType);
                
                ATRuleAgent.Instance.AddWeightPreset(obj);
            }
        }

        public void OnAction_PRESET_FACTOR_MAP(IPersistContext context)
        {
            HashSet<string> validDupl = new HashSet<string>();

            foreach (var entity in AleatorikTempMart.Instance.RULE_FACTOR.Rows)
            {
                var rule = ATRuleAgent.Instance.GetPreset(entity.RULE_ID);
                if (rule == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.RuleFactor, "RULE_MASTER@RULE_ID", ErrDesc.NotFoundReferredData("RULE_ID", "RULE_MASTER"));

                    continue;
                }

                string key = rule.RulePointID + entity.FACTOR_ID;
                var factor = ATRuleAgent.Instance.GetFactor(key);
                if (factor == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.RuleFactor, "FACTOR_MASTER@FACTOR_ID", ErrDesc.NotFoundReferredData("FACTOR_ID", "FACTOR_MASTER"));

                    continue;
                }

                if (factor.RulePoint != rule.RulePoint)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Critical, ErrorReasonCode.IncompatibleRule,
                        entity, ErrKey.RuleFactor, "FACTOR_MASTER@FACTOR_ID", string.Format("RULE_LOGIC_POINT : {0}, FACTOR_LOGIC_POINT : {1}", rule.RulePoint.ToString(), factor.RulePoint.ToString()));

                    continue;
                }

                string validationKey = entity.RULE_ID + "@" + entity.FACTOR_ID + "@" + entity.FACTOR_SEQ;
                if (rule.SortType == SortType.WeightSorted && validDupl.Contains(validationKey) == true)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.IncompatibleRule,
                        entity, ErrKey.RuleFactor, "RULE_MASTER@RULE_ID,SORT_TYPE", "If SORT_TYPE is WeightSorted, FACTOR_SEQ is not allowed to be duplicated");

                    continue;
                }

                validDupl.Add(validationKey);

                if (rule.SortType == SortType.WeightSum)
                {
                    if (entity.FACTOR_WEIGHT < 0)
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.OutOfRange,
                       entity, ErrKey.RuleFactor, "RULE_MASTER@RULE_ID,SORT_TYPE", ErrDesc.OutOfRange_above("FACTOR_WEIGHT"));

                        continue;
                    }

                    if (entity.FACTOR_SEQ != 0)
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Info, ErrorReasonCode.AutoCorrectionData,
                       entity, ErrKey.RuleFactor, "RULE_MASTER@RULE_ID,SORT_TYPE", "If SORT_TYPE is WeightSum, FACTOR_SEQ must be 0");
                    }
                    
                }
                else if (rule.SortType == SortType.WeightSorted)
                {
                    entity.FACTOR_WEIGHT = 1; // Factor 구현시 SortType에 따라 분기를 작성하지 않기 위해 1로 초기화

                    //if (entity.WEIGHT != 0)
                    //{
                    //    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Info, ErrorReasonCode.AutoCorrectionData,
                    //   entity, ErrKey.PresetFactorMap, "PRESET_MASTER@PRESET_ID,SORT_TYPE", "If SORT_TYPE is WeightSorted, WEIGHT must be 0");
                    //}
                }

                ATWeightFactor wfactor = new ATWeightFactor(factor, entity.FACTOR_WEIGHT, entity.FACTOR_SEQ, entity.FACTOR_VALUE);

                rule.FactorList.Add(wfactor);

                OutputWriter.Instance.WritePresetFactorMapLog(entity);
            }
        }

        public void OnAction_BUFFER_PROPERTY_VALUE(IPersistContext context)
        {
            foreach (var entity in AleatorikTempMart.Instance.BUFFER_PROP_VALUE.Rows)
            {
                var buffer = ATInputData.ItemSiteBuffers.GetBuffer(entity.BUFFER_ID);
                if (buffer == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.BufferPropValue, "BUFFER_MASTER@BUFFER_ID", "BUFFER_ID must be contained in BUFFER_MASTER");
                    continue;
                }

                SetProperties(entity, buffer, entity.PROP_ID, entity.PROP_VALUE, entity.CALENDAR_ID, ErrKey.BufferPropValue);
            }
        }



        public void OnAction_ITEM_PROPERTY_VALUE(IPersistContext context)
        {
            foreach (var entity in AleatorikTempMart.Instance.ITEM_PROP_VALUE.Rows)
            {
                var item = ATInputData.ItemSiteBuffers.GetItem(entity.ITEM_ID);
                if (item == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.ItemPropValue, "ITEM_MASTER@ITEM_ID", ErrDesc.NotFoundReferredData("ITEM_ID", "ITEM_MASTER"));

                    continue;
                }

                SetProperties(entity, item, entity.PROP_ID, entity.PROP_VALUE, entity.CALENDAR_ID, ErrKey.ItemPropValue);
            }
        }


        public void OnAction_CALENDAR_MASTER(IPersistContext context)
        {
            foreach (var entity in AleatorikTempMart.Instance.CALENDAR_MASTER.Rows)
            {
                var obj = ObjectMapper.CreateCalendar(entity);

                if (ATInputData.Calendars.AddCalendar(entity.CALENDAR_ID, obj) == false)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.DataDuplication, entity, ErrKey.CalendarMaster);

                    continue;
                }
            }
        }


        public void OnAction_CALENDAR_DETAIL(IPersistContext context)
        {
            foreach (var entity in AleatorikTempMart.Instance.CALENDAR_DETAIL.Rows)
            {
                if (entity.EFF_START_DATE > entity.EFF_END_DATE)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.OutOfRange,
                        entity, ErrKey.CalendarDetail, "", ErrDesc.OutOfRange_datetime("EFF_START_DATE", "EFF_END_DATE"));
                }

                ATCalendar calendar = ATInputData.Calendars.GetCalendar(entity.CALENDAR_ID);
                if (calendar == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.CalendarDetail, "CALENDAR_MASTER@CALENDAR_ID", ErrDesc.NotFoundReferredData("CALENDAR_ID", "CALENDAR_MASTER"));
                    continue;
                }

                var type = ATUtil.StringToEnum<CalendarPatternType>(entity.PATTERN_TYPE, CalendarPatternType.None);

                if (type == CalendarPatternType.None)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                        entity, ErrKey.CalendarDetail, "", ErrDesc.MismatchReservedWord("PATTERN_TYPE", ATUtil.GetEnumProperty<CalendarPatternType>()));

                    continue;
                }

                var obj = ObjectMapper.CreateCalendarDetail(entity, calendar, type);

                // 중복 체크
                if (calendar.AddDetail(obj) == false)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.DataDuplication,
                        entity, ErrKey.CalendarDetail);

                    continue;
                }
            }
        }

        public void OnAction_CALENDAR_BASED_ATTRIBUTES(IPersistContext context)
        {
            HashSet<string> validateDuplecate = new HashSet<string>();

            foreach (var entity in AleatorikTempMart.Instance.CALENDAR_BASED_ATTR.Rows)
            {
                var detail = ATInputData.Calendars.GetCalendarDetail(entity.CALENDAR_ID, entity.PATTERN_ID);

                if (detail == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.CalendarBasedAttr, "CALENDAR_DETAIL@CALENDAR_ID,PATTERN_ID", ErrDesc.NotFoundReferredData("CALENDAR_ID", "CALENDAR_DETAIL"));

                    continue;
                }

                // Calendar 값 보정
                var valueType = ATUtil.StringToEnum<DataType>(entity.ATTR_DATA_TYPE, DataType.None);
                AdjustCalendarType(detail.Calendar, entity, ref valueType);

                if (valueType == DataType.None)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                        entity, ErrKey.CalendarBasedAttr, "", ErrDesc.MismatchReservedWord("ATTR_DATA_TYPE", ATUtil.GetEnumProperty<DataType>()));

                    continue;
                }

                if (string.IsNullOrEmpty(entity.ATTR_VALUE))
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.Null,
                        entity, ErrKey.CalendarBasedAttr, "", ErrDesc.Null("ATTR_VALUE"));

                    continue;
                }

                //OffTime의 경우 4개의 파라미터가 있어야함
                if (entity.ATTR_TYPE == ATReservedCode.OFF_TIME)
                {
                    var timeInfos = entity.ATTR_VALUE.Split(';');
                    var infos = timeInfos.Where(x => x.Split(',').Count() != 4).FirstOrDefault();
                    if (infos != null)
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.DataTypeMisMatch,
                            entity, ErrKey.CalendarBasedAttr, "", "Follow the format of #OffTime. e.g., VALUE = 00:00:00,24:00:00,OffTimeName,N");

                        continue;
                    }
                }
                else if (entity.ATTR_TYPE == ATReservedCode.WORK_TIME)
                {
                    var timeInfos = entity.ATTR_VALUE.Split(';');
                    var infos = timeInfos.Where(x => x.Split(',').Count() != 2).FirstOrDefault();
                    if (infos != null)
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.DataTypeMisMatch,
                            entity, ErrKey.CalendarBasedAttr, "", "Follow the format of #WorkTime. e.g., VALUE = 00:00:00,24:00:00");

                        continue;
                    }
                }

                var obj = ObjectMapper.CreateCalendarAttribute(entity, detail, detail.PatternSeq, valueType);

                // 입력된 데이터가 유효하지 않은 경우
                if (obj.IsVaild == false)
                {
                    //obj.Value = obj.Type.GetDefaultValue().ToString();

                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.DataTypeMisMatch,
                        entity, ErrKey.CalendarBasedAttr, "", ErrDesc.DataTypeMisMatch("ATTR_VALUE", obj.Type.GetDefaultValue().ToString()));

                    continue;
                }

                string validationKey = entity.CALENDAR_ID + "@" + entity.PATTERN_ID + "@" + entity.ATTR_TYPE;
                if (validateDuplecate.Contains(validationKey) == true)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.DataDuplication,
                        entity, ErrKey.CalendarBasedAttr, "", "");

                    continue;
                }

                validateDuplecate.Add(validationKey);

                // Option : PlanStartTime + Period 까지만 calendar 등록.
                // 내부 로직의 경우 => Daily로 Internal Pattern을 통하여 값을 가져올 수 있도록 설정
                // 속도 이슈 해결을 위한 처리 작업.
                if (detail.Calendar.IsNeedDailyPatternCode())
                {
                    var startTime = detail.EffectiveStartTime;
                    var endTime = detail.EffectiveEndTime;

                    while (startTime < endTime)
                    {
                        bool isOffTime = detail.IsEffectiveTime(startTime) == false;
                        string applyDate = startTime.ToString(ATUtil.DateFormat);
                        ATCalendarAttribute attribute = obj.ShallowCopy();

                        attribute.ApplyDate = applyDate;

                        attribute.EffectiveStartTime = startTime;
                        attribute.EffectiveEndTime = detail.GetEffectiveEndTime(startTime);

                        startTime = attribute.EffectiveEndTime;

                        if (isOffTime && attribute.CalendarType == ATReservedCode.CAPACITY) // capacity가 설정되지 않은 날은 통째로 OffTime으로 지정
                        {
                            if (attribute.Attribute == ATReservedCode.CAPACITY)
                            {
                                attribute.Value = 0;
                            }
                            else if (attribute.Attribute == ATReservedCode.WORK_TIME)
                            {
                                var offTimeAttr = attribute.ShallowCopy();
                                offTimeAttr.Attribute = ATReservedCode.OFF_TIME;
                                offTimeAttr.Type = typeof(string);
                                // offTimeAttr.Value = "00:00:00,24:00:00," + ATReservedCode.OFF_TIME + ",Y";
                                offTimeAttr.Value = attribute.Value + "," + ATReservedCode.OFF_TIME + ",Y";
                                detail.AddAttribute(offTimeAttr);
                            }
                        }

                        detail.AddAttribute(attribute);
                    }
                }
                else
                {
                    detail.AddAttribute(obj);
                }
            }
        }

        public void OnAction_SITE_MASTER(IPersistContext context)
        {
            if (AleatorikTempMart.Instance.SITE_MASTER.Rows.Count == 0)
            {
                OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Critical, ErrorReasonCode.NotFoundKeyInput,
                      null, "SITE_MASTER@" + ErrKey.SiteMaster, "", ErrDesc.NotFoundKeyInput("SITE_MASTER"));
            }

            foreach (var entity in AleatorikTempMart.Instance.SITE_MASTER.Rows)
            {
                var obj = ObjectMapper.CreateSite(entity.SITE_ID, entity.SITE_NAME);
                if (!DerivedHelper.CallAfterLoadHandler<SITE_MASTER>(entity, obj))
                {
                    continue;
                }

                ATInputData.ItemSiteBuffers.AddSite(obj);
            }
        }

        public void OnAction_SALES_ORDER(IPersistContext context)
        {
            //OutputWriter.Instance.WritePlanIndex("-", KpiCategory.INPUT.ToString(),  KpiIndex.TOTAL_DEMAND_QTY.ToString(), "-", ""
            //    ,  AleatorikTempMart.Instance.DEMAND.Rows.Sum(x => x.DEMAND_QTY));

            //OutputWriter.Instance.WritePlanIndex("-", KpiCategory.INPUT.ToString(), KpiIndex.TOTAL_DEMAND_COUNT.ToString(), "-", ""
            //    , AleatorikTempMart.Instance.DEMAND.Rows.Count());

            if (AleatorikTempMart.Instance.DEMAND.Rows.Count == 0)
            {
                OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Critical, ErrorReasonCode.NotFoundKeyInput,
                      null, "SALES_ORDER@" + ErrKey.Demand, "", ErrDesc.NotFoundKeyInput("SALES_ORDER"));
            }

            foreach (var entity in AleatorikTempMart.Instance.DEMAND.Rows)
            {
                ATInputData.Demands.AddRowDemand(entity);

                if (string.IsNullOrEmpty(ATOption.Instance.DemandItems) == false
                        && ATOption.Instance.DemandItems.Contains(entity.ITEM_ID) == false)
                    continue;

                if (ATOption.Instance.DemandDueDate < entity.DUE_DATE)
                    continue;

                ATBuffer buffer = ATInputData.ItemSiteBuffers.GetBuffer(entity.BUFFER_ID);
                if (buffer == null)
                {
                    string reason = "InvalidBuffer";
                    string ReasonDetail = string.Format("BufferID : {0}", entity.BUFFER_ID);

                    OutputWriter.Instance.WriteShortLog(entity, reason, ReasonDetail);
                    OutputWriter.Instance.WriteShortReport(entity, reason);
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.Demand, "BUFFER_MASTER@BUFFER_ID", ErrDesc.NotFoundReferredData("BUFFER_ID", "BUFFER_MASTER"));

                    continue;

                }

                var stage = buffer.Stage;
                if (stage == null)
                {
                    string reason = "InvalidBuffer";
                    string ReasonDetail = string.Format("BufferID : {0}", entity.BUFFER_ID);

                    OutputWriter.Instance.WriteShortLog(entity, reason, ReasonDetail);
                    OutputWriter.Instance.WriteShortReport(entity, reason);
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.Demand, "BUFFER_MASTER@BUFFER_ID", ErrDesc.NotFoundReferredData("BUFFER_ID", "BUFFER_MASTER"));

                    continue;
                }

                var bufferID = string.IsNullOrEmpty(entity.BUFFER_ID) ? stage.BufferRoute.LastOper.StepID : entity.BUFFER_ID;

                var lastBuffer = ATInputData.ItemSiteBuffers.GetBuffer(bufferID);
                if (lastBuffer == null)
                {
                    string reason = "InvalidBuffer";
                    string ReasonDetail = string.Format("BufferID : {0}", entity.BUFFER_ID);

                    OutputWriter.Instance.WriteShortLog(entity, reason, ReasonDetail);
                    OutputWriter.Instance.WriteShortReport(entity, reason);
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.Demand, "BUFFER_MASTER@BUFFER_ID", ErrDesc.NotFoundReferredData("BUFFER_ID", "BUFFER_MASTER"));

                    continue;
                }

                var customer = ATInputData.Customers.GetCustomer(entity.CUST_ID);
                if (string.IsNullOrEmpty(entity.CUST_ID) == false && customer == null)
                {
                    string reason = "InvalidCustomer";
                    string ReasonDetail = string.Format("CustomerID : {0}", entity.CUST_ID);

                    OutputWriter.Instance.WriteShortLog(entity, reason, ReasonDetail);
                    OutputWriter.Instance.WriteShortReport(entity, reason);
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData, 
                        entity, ErrKey.Demand, "CUSTOMER@CUSTOMER_ID", ErrDesc.NotFoundReferredData("CUSTOMER_ID", "CUSTOMER"));

                    continue;
                }

                ATItem item = ATInputData.ItemSiteBuffers.GetItem(entity.ITEM_ID);
                if (item == null)
                {
                    string reason = "InvalidItem";
                    string ReasonDetail = string.Format("ItemID : {0}", entity.ITEM_ID);

                    OutputWriter.Instance.WriteShortLog(entity, reason, ReasonDetail);
                    OutputWriter.Instance.WriteShortReport(entity, reason);
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.Demand, "ITEM_MASTER@ITEM_ID", ErrDesc.NotFoundReferredData("ITEM_ID", "ITEM_MASTER"));

                    continue;
                }

                ATSite site = ATInputData.ItemSiteBuffers.GetSite(entity.SITE_ID);
                if (site == null)
                {
                    string reason = "InvalidSite";
                    string ReasonDetail = string.Format("SiteID : {0}", entity.SITE_ID);

                    OutputWriter.Instance.WriteShortLog(entity, reason, ReasonDetail);
                    OutputWriter.Instance.WriteShortReport(entity, reason);
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.Demand, "SITE_MASTER@SITE_ID", ErrDesc.NotFoundReferredData("SITE_ID", "SITE_MASTER")); 


                    continue;
                }

                ATItemSiteBuffer itemBuffer = ATInputData.ItemSiteBuffers.GetItemSite(entity.SITE_ID, entity.ITEM_ID, bufferID);
                if (itemBuffer == null)
                {
                    string reason = "InvalidItemSiteBuffer";
                    string ReasonDetail = string.Format("ItemSiteBuffer : {0}", entity.ITEM_ID + "@" + entity.SITE_ID + "@" + entity.BUFFER_ID);

                    OutputWriter.Instance.WriteShortLog(entity, reason, ReasonDetail);
                    OutputWriter.Instance.WriteShortReport(entity, reason);
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.Demand, "ITEM_SITE_BUFFER_MASTER@SITE_ID,ITEM_ID,BUFFER_ID", ErrDesc.NotFoundReferredData("ITEM_ID,SITE_ID,BUFFER_ID", "ITEM_SITE_BUFFER_MASTER"));

                    continue;
                }

                if (stage.Demands.ContainsKey(entity.DEMAND_ID) == true)
                {
                    string reason = "DuplicateSO";
                    string ReasonDetail = string.Format("SoID : {0}", entity.DEMAND_ID);

                    OutputWriter.Instance.WriteShortLog(entity, reason, ReasonDetail);
                    OutputWriter.Instance.WriteShortReport(entity, reason);
                    
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.DataDuplication,
                        entity, ErrKey.Demand);

                    continue;
                }

                if (entity.DEMAND_QTY <= 0)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.OutOfRange,
                            entity, ErrKey.Demand, "", ErrDesc.OutOfRange_above("QTY"));

                    continue;
                }

                if (entity.MAX_EARLINESS_DAY < 0)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.OutOfRange,
                            entity, ErrKey.Demand, "", ErrDesc.OutOfRange_greater("MAX_EARLINESS_DAYS"));

                    continue;
                }

                if (entity.MAX_LATENESS_DAY < 0)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.OutOfRange,
                            entity, ErrKey.Demand, "", ErrDesc.OutOfRange_greater("MAX_LATENESS_DAYS"));

                    continue;
                }

#warning Hynix : Incremental 로직
                double soQty = entity.DEMAND_QTY;

                var demand = ObjectMapper.CreateDemand(entity, entity.DEMAND_ID, soQty, stage.StageID, itemBuffer, customer);
                if (!DerivedHelper.CallAfterLoadHandler<DEMAND>(entity, demand))
                {
                    continue;
                }

                stage.Demands.Add(demand.ID, demand);

                ATInputData.Demands.AddDemand(demand);
            }
        }

        public void OnAction_RULE(IPersistContext context)
        {
            if (AleatorikTempMart.Instance.RULE_POINT_MASTER.Rows.Count == 0)
            {
                OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Critical, ErrorReasonCode.NotFoundKeyInput,
                      null, "RULE_POINT_MASTER@" + ErrKey.RulePointMaster, "", ErrDesc.NotFoundKeyInput("RULE_POINT_MASTER"));
            }

            foreach (var entity in AleatorikTempMart.Instance.RULE_POINT_MASTER.Rows)
            {
                var rulepoint = ATUtil.StringToEnum<RulePoint>(entity.RULE_POINT, RulePoint.None);
                if (rulepoint == RulePoint.None)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                        entity, ErrKey.RulePointMaster, "", ErrDesc.MismatchReservedWord("RULE_POINT", ATUtil.GetEnumProperty<RulePoint>()));

                    continue;
                }

                var calltype = ATUtil.StringToEnum<CallType>(entity.CALL_TYPE, CallType.None);
                if (calltype == CallType.None)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                        entity, ErrKey.RulePointMaster, "", ErrDesc.MismatchReservedWord("CALL_TYPE", ATUtil.GetEnumProperty<CallType>()));

                    continue;
                }

                var rulePointType = ATUtil.StringToEnum<RulePointType>(entity.RULE_POINT_TYPE, RulePointType.None);
                if (rulePointType == RulePointType.None)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                        entity, ErrKey.RulePointMaster, "", ErrDesc.MismatchReservedWord("RULE_POINT_TYPE", ATUtil.GetEnumProperty<CallType>()));

                    continue;
                }

                bool isactive = entity.ACTIVE_YN == "Y";

                ATRule rule = new ATRule(rulepoint, entity.RULE_NAME, calltype, rulePointType, isactive, entity.DESCRIPTION);
                
                ATRuleAgent.Instance.AddRule(rule);
            }
        }

        public void OnAction_RULESET(IPersistContext context)
        {
            if (AleatorikTempMart.Instance.RULESET_MASTER.Rows.Count == 0)
            {
                OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Critical, ErrorReasonCode.NotFoundKeyInput,
                      null, "RULESET_MASTER@" + ErrKey.RuleSetMaster, "", ErrDesc.NotFoundKeyInput("RULESET_MASTER"));
            }

            foreach (var entity in AleatorikTempMart.Instance.RULESET_MASTER.Rows)
            {
                if (entity.MAX_LEVEL <= 0)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.OutOfRange,
                        entity, ErrKey.RuleSetMaster, "", ErrDesc.OutOfRange_greater("LEVEL_COUNT"));

                    continue;
                }

                var moduletype = ATUtil.StringToEnum<ModuleType>(entity.MODULE_TYPE, ModuleType.None);

                if (moduletype == ModuleType.None)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                        entity, ErrKey.RuleSetMaster, "", ErrDesc.MismatchReservedWord("MODULE_TYPE", ATUtil.GetEnumProperty<ModuleType>()));

                    continue;
                }

                ATRuleSet obj = new ATRuleSet(entity.RULESET_ID, entity.MAX_LEVEL, entity.DESCRIPTION, moduletype);

                ATRuleAgent.Instance.AddRuleSet(obj);
            }
        }

        public void OnAction_RULEACTION_MAP(IPersistContext context)
        {
            foreach (var entity in AleatorikTempMart.Instance.RULESET_CONFIG.Rows)
            {
                var ruleset = ATRuleAgent.Instance.GetRuleSet(entity.RULESET_ID);
                if (ruleset == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.RuleSetConfig, "RULESET_MASTER@RULESET_ID", ErrDesc.NotFoundReferredData("RULESET_ID","RULESET_MASTER"));

                    continue;
                }

                if (entity.LEVEL_NO > ruleset.Level || entity.LEVEL_NO < 0)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.OutOfRange,
                        entity, ErrKey.RuleSetConfig, "", ErrDesc.OutOfRange_between("LEVEL_NO", "0", ruleset.Level.ToString()));

                    continue;
                }

                var rule = ATRuleAgent.Instance.GetRule(entity.RULE_POINT);
                if (rule == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                        entity, ErrKey.RuleSetConfig, "RULE_POINT_MASTER@RULE_POINT", ErrDesc.NotFoundReferredData("RULE_POINT", ATUtil.GetEnumProperty<RulePoint>()));

                    continue;
                }

                object target = ATRuleAgent.Instance.GetPreset(entity.RULE_ID);

                if (target != null && (target as ATWeightPreset).RulePointID != rule.RulePointID)
                    target = null;

                if (target == null)
                {
                    string refData = ATConstants.COL_RULE_ID;
                    string refTable = ATConstants.TBL_RULE_MASTER;

                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.RuleSetConfig, refTable + "@" + refData, ErrDesc.NotFoundReferredData(refData, refTable));

                    continue;
                }

                ATRuleSetConfig obj = new ATRuleSetConfig(ruleset, entity.LEVEL_NO, rule, target);

                string key = rule.CallType == CallType.Level ? entity.RULE_POINT + entity.LEVEL_NO : entity.RULE_POINT;

                ruleset.AddRuleSetConfig(key, obj);

                OutputWriter.Instance.WriteRuleActionMapLog(entity);
            }

            var rulesets = ATRuleAgent.Instance.GetRuleSet();

            foreach (var ruleset in rulesets)
            {
                string checkInfo = ruleset.GetInvalidLog();

                if (string.IsNullOrEmpty(checkInfo) == false)
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Info, ErrorReasonCode.IncompatibleRule, null, ErrKey.RuleSetMaster, null, checkInfo);
            }
        }

        public void OnAction_EXECUTION_RULESET_CONFIG(IPersistContext context)
        {
            foreach (var entity in AleatorikTempMart.Instance.SCENARIO_RULESET_CONFIG.Rows)
            {
                var scenario = AleatorikInputMart.Instance.GlobalParameters.PlanScenario;

                //if (entity.SCENARIO_ID != scenario)
                //{
                //    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                //        entity, ErrKey.ScenarioRulesetConfig, "", ErrDesc.MismatchReservedWord("SCENARIO_ID", scenario));

                //    continue;
                //}

                var info = ATExecutionContext.Instance.GetExecutionInfo(entity.MODULE_ID);
                if (info == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.ScenarioRulesetConfig, "SCENARIO_CONFIG@MODULE_ID", ErrDesc.NotFoundReferredData("MODULE_ID", "SCENARIO_CONFIG"));
                    continue;
                }

                var ruleSet = ATRuleAgent.Instance.GetRuleSet(entity.RULESET_ID);
                if (ruleSet == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.ScenarioRulesetConfig, "RULESET_MASTER@RULESET_ID", ErrDesc.NotFoundReferredData("RULESET_ID", "RULESET_MASTER")); 
                    continue;
                }

                RuleSetType ruleSetType = ATUtil.StringToEnum<RuleSetType>(entity.TARGET_CATEGORY, RuleSetType.None);
                if (ruleSetType == RuleSetType.None)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                        entity, ErrKey.ScenarioRulesetConfig, "", ErrDesc.MismatchReservedWord("TARGET_CATEGORY", ATUtil.GetEnumProperty<RuleSetType>()));
                    continue;
                }

                ATScenarioRuleSetConfig obj = new ATScenarioRuleSetConfig(entity.MODULE_ID, ruleSetType.ToString(), entity.TARGET_ID, entity.PHASE_NO, ruleSet);

                ATRuleAgent.Instance.AddScenarioRuleSetConfig(obj);
            }
        }

        private void onActionResMaster(bool isSetupResLoad)
        {
            foreach (var entity in AleatorikTempMart.Instance.RES_MASTER.Rows)
            {
                ResourceCategory category = ATUtil.StringToEnum<ResourceCategory>(entity.RES_CATEGORY, ResourceCategory.None);

                if (isSetupResLoad && category != ResourceCategory.SetupResource)
                    continue;    

                if (category == ResourceCategory.None)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                        entity, ErrKey.ResMaster, "", ErrDesc.MismatchReservedWord("RES_CATEGORY", ATUtil.GetEnumProperty<ResourceCategory>()));
                    continue;
                }

                CapacityType capaType = ATUtil.StringToEnum<CapacityType>(entity.CAPA_TYPE, CapacityType.None);
                if (category == ResourceCategory.SetupResource)
                    capaType = CapacityType.Time;

                if (capaType == CapacityType.None)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                        entity, ErrKey.ResMaster, "", ErrDesc.MismatchReservedWord("CAPA_TYPE", ATUtil.GetEnumProperty<CapacityType>()));

                    continue;
                }

                ResourceType resType = ATUtil.StringToEnum<ResourceType>(entity.RES_TYPE, ResourceType.None);
                if (resType == ResourceType.None && category == ResourceCategory.Resource)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                        entity, ErrKey.ResMaster, "", ErrDesc.MismatchReservedWord("RES_TYPE", ATUtil.GetEnumProperty<ResourceType>()));

                    continue;
                }

                bool infinityCapaYn = ATUtil.BoolYN(entity.INFINITY_CAPA_YN, false);
                CapacityMode capaMode = infinityCapaYn == true ? CapacityMode.Infinite : CapacityMode.Finite;
                // CapacityMode capaMode = ATUtil.StringToEnum<CapacityMode>(entity.INFINITY_CAPA_YN, CapacityMode.None);
                //if (capaMode == CapacityMode.None && category == ResourceCategory.Resource)
                //{
                //    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                //        entity, ErrKey.ResMaster, "", ErrDesc.MismatchReservedWord("INFINITY_CAPA_YN", ATUtil.GetEnumProperty<CapacityMode>()));

                //    continue;
                //}

                ATResourceGroup resGroup = ATInputData.Resources.GetResourceGroup(entity.RES_GROUP_ID);
                if (resGroup == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.ResMaster, "RES_GROUP_MASTER@RES_GROUP_ID", ErrDesc.NotFoundReferredData("RES_GROUP_ID", "RES_GROUP_MASTER"));

                    continue;
                }

                ATCalendar capaCal = null;
                ATCalendar utilCal = null;
                if (category == ResourceCategory.AddResource || category == ResourceCategory.Resource || category == ResourceCategory.SetupResource)
                {
                    capaCal = ATInputData.Calendars.GetCalendar(entity.CAPA_CALENDAR_ID);
                    if (capaCal == null)
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.ResMaster, "CALENDAR_MASTER@CALENDAR_ID", ErrDesc.NotFoundReferredData("CAPA_CALENDAR_ID", "CALENDAR_MASTER"));

                        continue;
                    }
                    else if (capaCal.CalendarType != ATReservedCode.CAPACITY)
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.LawViolation,
                            entity, ErrKey.ResMaster, "CALENDAR_MASTER@CALENDAR_ID",
                            "Only #Capacity calendar can be value of CAPA_CALENDAR_ID column in RES_MASTER table. Replace it with CALENDAR_TYPE = ‘#Capacity’ in CALENDAR_MASTER table.");

                        continue;
                    }

                    utilCal = ATInputData.Calendars.GetCalendar(entity.UTIL_RATE_CALENDAR_ID);
                    if (utilCal == null)
                    { // 설정 했는데 없는 Calendar
                        if (entity.UTIL_RATE_CALENDAR_ID.IsNullOrEmpty() == false)
                        {
                            OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Info, ErrorReasonCode.NotFoundReferredData,
                                entity, ErrKey.ResMaster, "CALENDAR_MASTER@CALENDAR_ID", ErrDesc.NotFoundReferredData("UTIL_RATE_CALENDAR_ID", "CALENDAR_MASTER"));
                        }
                    }
                    else if (utilCal.CalendarType != ATReservedCode.UTILIZATION_RATE)
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Info, ErrorReasonCode.LawViolation,
                            entity, ErrKey.ResMaster, "CALENDAR_MASTER@CALENDAR_ID",
                            "Only #UtilizationRate calendar can be value of UTIL_RATE_CALENDAR_ID column in RES_MASTER table. Replace it with CALENDAR_TYPE = '#UtilizationRate' in CALENDAR_MASTER table.");

                        utilCal = null;
                    }
                }

                ATCalendar pmCal = null;
                ATSetupInfo setup = null;
                if (category == ResourceCategory.Resource && capaType == CapacityType.Time)
                {
                    pmCal = ATInputData.Calendars.GetCalendar(entity.PM_ID);
                    if (entity.PM_ID.IsNullOrEmpty() == false && pmCal == null)
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Info, ErrorReasonCode.NotFoundReferredData,
                            entity, ErrKey.ResMaster, "PM@PM_ID", ErrDesc.NotFoundReferredData("PM_ID", "PM"));
                    }

                    setup = ATInputData.Setups.GetSetupInfos(entity.SETUP_ID);
                    if (entity.SETUP_ID.IsNullOrEmpty() == false && setup == null)
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Info, ErrorReasonCode.NotFoundReferredData,
                            entity, ErrKey.ResMaster, "SETUP@SETUP_ID", ErrDesc.NotFoundReferredData("SETUP_ID", "SETUP"));
                    }
                }
                else
                {
                    if (entity.PM_ID.IsNullOrEmpty() == false)
                    {
                        if (category != ResourceCategory.Resource || capaType != CapacityType.Time)
                        {
                            OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Info, ErrorReasonCode.LawViolation,
                                entity, ErrKey.ResMaster, "", "PM_ID can be set only when RES_CATEGORY = 'Resource' and CAPA_TYPE = 'Time'.");
                        }
                    }

                    if (entity.SETUP_ID.IsNullOrEmpty() == false)
                    {
                        if (category != ResourceCategory.Resource || capaType != CapacityType.Time)
                        {
                            OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Info, ErrorReasonCode.LawViolation,
                                entity, ErrKey.ResMaster, "", "SETUP_ID can be set only when RES_CATEGORY = 'Resource' and CAPA_TYPE = 'Time'.");
                        }
                    }
                }

                ATStage stage = ATExecutionContext.Instance.GetStage(resGroup.AllocationGroup.Stage.StageID);

                double utilRate = entity.UTIL_RATE <= 0 || entity.UTIL_RATE > 1 ? 1 : entity.UTIL_RATE;
                ATResource obj = ObjectMapper.CreateResource(entity, resType, category, capaType, capaMode, resGroup, entity.RES_GROUP_ID,
                    utilRate, capaCal, utilCal, pmCal, setup);

                if (ATInputData.Resources.AddResource(obj) == false)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.DataDuplication,
                        entity, ErrKey.ResMaster);

                    continue;
                }

                if (resGroup.AddResource(obj))
                {
                    stage.AddAllocationGroup(resGroup.AllocationGroup);
                    stage.Resources.Add(obj);
                }
                else
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord, null, "RES_MASTER@RES_ID,RES_GROUP_ID", entity.RES_ID + "@" + entity.RES_GROUP_ID);
                }
            }

            var resourceGroups = ATInputData.Resources.GetResourceGroups().ToList();

            while (resourceGroups.Count() > 0)
            {
                var resourceGroup = resourceGroups.First();
                resourceGroups.Remove(resourceGroup);

                if (resourceGroup.IsInvalidResourceGroup)
                {
                    var stage = resourceGroup.AllocationGroup.Stage;
                    foreach (var resource in resourceGroup.Resources.Values)
                    {
                        stage.Resources.Remove(resource);
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord, null, "RES_MASTER@RES_ID,RES_GROUP_ID", resource.ResourceID + "@" + resource.ResGroupID);
                    }

                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord, null, "RES_GROUP_MASTER@RES_GROUP_ID", resourceGroup.GroupID);

                    resourceGroup.Resources.Clear();
                    resourceGroup.AllocationGroup.RemoveResourceGroup(resourceGroup);
                    ATInputData.Resources.RemoveResourceGroup(resourceGroup);
                }
            }
        }

        public void OnAction_RESOURCE_MASTER(IPersistContext context)
        {
            // setupResource만 먼저 loading 하고, Setup entity loading 후에 나머지 resource loading 함
            onActionResMaster(true);
        }

        public void OnAction_RESOURCE_GROUP(IPersistContext context)
        {
            foreach (var entity in AleatorikTempMart.Instance.RES_GROUP_MASTER.Rows)
            {
                ATAllocationGroup allocGroup;
                allocGroup = ATInputData.AllocationGroups.GetAllocationGroup(entity.ALLOCATION_GROUP_ID);
                if (allocGroup == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.ResGroupMaster, "ALLOCATION_GROUP@ALLOCATION_GROUP_ID", ErrDesc.NotFoundReferredData("ALLOCATION_GROUP_ID", "ALLOCATION_GROUP"));
                    continue;
                }

                bool isReSorting = ATUtil.BoolYN(entity.RE_SORT_YN, false); 

                var obj = ObjectMapper.CreateResGroup(entity, allocGroup, isReSorting);

                ATInputData.Resources.AddResourceGroup(obj);

                allocGroup.AddResourceGroup(obj);
            }
        }

        public void OnAction_OPERATION_RESOURCE(IPersistContext context)
        {
            foreach (var entity in AleatorikTempMart.Instance.OPER_RES.Rows)
            {
                var route = ATInputData.Boms.GetRoute(entity.ROUTING_ID);
                if (route == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.OperRes, "ROUTING_MASTER@ROUTING_ID^BOM_ROUTING@BOM_ID,ROUTING_ID"
                        , ErrDesc.NotFoundReferredData("ROUTING_ID","ROUTING_MASTER") + "^" + ErrDesc.NotFoundReferredData("BOM_ID,ROUTING_ID", "BOM_ROUTING"));

                    continue;
                }

                var oper = route.FindOper(entity.OPER_ID);
                if (oper == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.OperRes, "ROUTING_OPER@OPER_ID", ErrDesc.NotFoundReferredData("OPER_ID", "ROUTING_OPER"));
                    continue;
                }

                var resource = ATInputData.Resources.GetResource(entity.RES_ID);
                if (resource == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.OperRes, "RES_MASTER@RES_ID", ErrDesc.NotFoundReferredData("RES_ID", "RES_MASTER"));
                    continue;
                }

                if (entity.FLOW_TIME < 0)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.OutOfRange,
                        entity, ErrKey.OperRes, "", ErrDesc.OutOfRange_greater("FLOW_TIME"));

                    continue;

                }

                if (entity.USAGE_PER < 0)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.OutOfRange,
                        entity, ErrKey.OperRes, "", ErrDesc.OutOfRange_greater("USAGE_PER"));

                    continue;
                }

                if (oper.OperType == OperType.Dummy)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Info, ErrorReasonCode.AutoCorrectionData,
                        entity, ErrKey.OperRes, "", "If OPER_TYPE is Dummy, OPERATION cannot have RESOURCE");
                }

                //entity.USAGE_PER_CALENDAR_ID
                #region Calendar 설정
                var usagePerCal = ATInputData.Calendars.GetCalendar(entity.USAGE_PER_CALENDAR_ID);
                if (entity.USAGE_PER_CALENDAR_ID.IsNullOrEmpty() == false)
                {
                    if (usagePerCal == null)
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Info, ErrorReasonCode.NotFoundReferredData,
                            entity, ErrKey.OperRes, "CALENDAR_MASTER@CALENDAR_ID", ErrDesc.NotFoundReferredData("USAGE_PER_CALENDAR_ID", "CALENDAR_MASTER"));
                    }
                    else if (usagePerCal.CalendarType != ATReservedCode.USAGE_PER)
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Info, ErrorReasonCode.LawViolation,
                            entity, ErrKey.OperRes, "CALENDAR_MASTER@CALENDAR_ID",
                            "Only #UsagePer calendar can be value of USAGE_PER_CALENDAR_ID column in OPER_RES table. Replace it with CALENDAR_TYPE = ‘#UsagePer’ in CALENDAR_MASTER table.");
                    }
                }

                var flowTimeCal = ATInputData.Calendars.GetCalendar(entity.FLOW_TIME_CALENDAR_ID);
                if (entity.FLOW_TIME_CALENDAR_ID.IsNullOrEmpty() == false)
                {
                    if (flowTimeCal == null)
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Info, ErrorReasonCode.NotFoundReferredData,
                            entity, ErrKey.OperRes, "CALENDAR_MASTER@CALENDAR_ID", ErrDesc.NotFoundReferredData("FLOW_TIME_CALENDAR_ID", "CALENDAR_MASTER"));
                    }
                    else if (flowTimeCal.CalendarType != ATReservedCode.FLOW_TIME)
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Info, ErrorReasonCode.LawViolation,
                            entity, ErrKey.OperRes, "CALENDAR_MASTER@CALENDAR_ID",
                            "Only #FlowTime calendar can be value of FLOW_TIME_CALENDAR_ID column in OPER_RES table. Replace it with CALENDAR_TYPE = ‘#FlowTime’ in CALENDAR_MASTER table.");
                    }
                }
                #endregion

                var obj = ObjectMapper.CreateArrange(entity, route, oper, resource, usagePerCal, flowTimeCal);

                resource.AddArrange(obj);
                oper.Arranges.Add(obj);
                oper.Resources.Add(resource);

                route.Bom.Resource.Add(resource);

                HashSet<ATResource> lst;
                if (route.Bom.PrevResources.TryGetValue(route.Bom.ToBuffer, out lst) == false)
                    route.Bom.PrevResources.Add(route.Bom.ToBuffer, lst = new HashSet<ATResource>());

                lst.Add(resource);

                if (string.IsNullOrEmpty(route.ResourceIDs))
                    route.ResourceIDs += resource.ResourceID;
                else
                    route.ResourceIDs += "/" + resource.ResourceID;
            }

            var opers = ATInputData.Boms.GetOperationTypeOper();
            foreach (var oper in opers)
            {
                if (oper.Arranges.Count() != 0)
                    continue;

                //PLAN_VERSION,ROUTING_ID,OPERATION_ID

                ROUTING_OPER entity = new ROUTING_OPER();
                entity.ROUTING_ID = oper.RouteID;
                entity.OPER_ID = oper.OperID;

                OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                       entity, ErrKey.RoutingOper,
                        "OPER_RES@ROUTING_ID,OPER_ID,RES_ID",
                        ErrDesc.NotFoundReferredData("ROUTING_ID,OPER_ID", "OPER_RES")
                       );
            }
        }

        public void OnAction_OPERATION_ADDITIONAL_RESOURCE(IPersistContext context)
        {
            foreach (var entity in AleatorikTempMart.Instance.OPER_ADD_RES.Rows)
            {
                var route = ATInputData.Boms.GetRoute(entity.ROUTING_ID);
                if (route == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.OperAddRes, "ROUTING_MASTER@ROUTING_ID^BOM_ROUTING@BOM_ID,ROUTING_ID"
                        , ErrDesc.NotFoundReferredData("ROUTING_ID", "ROUTING_MASTER") + "^" + ErrDesc.NotFoundReferredData("BOM_ID,ROUTING_ID", "BOM_ROUTING"));

                    continue;
                }

                var oper = route.FindOper(entity.OPER_ID);
                if (oper == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.OperAddRes, "ROUTING_OPER@ROUTING_ID,OPER_ID", ErrDesc.NotFoundReferredData("ROUTING_ID", "ROUTING_OPER"));

                    continue;
                }

                var resource = ATInputData.Resources.GetResource(entity.RES_ID);
                if (resource == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.OperAddRes, "RES_MASTER@RES_ID", ErrDesc.NotFoundReferredData("RES_ID", "RES_MASTER")); 

                    continue;
                }

                var addResource = ATInputData.Resources.GetResource(entity.ADD_RES_ID);
                if (addResource == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.OperAddRes, "RES_MASTER@RES_ID", ErrDesc.NotFoundReferredData("ADD_RES_ID", "RES_MASTER")); 
                    continue;
                }

                if (resource.CapaType != addResource.CapaType)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData
                        , entity, ErrKey.OperAddRes, "RES_MASTER@RES_ID,CAPA_TYPE", ErrDesc.NotFoundReferredData("RES_ID,CAPA_TYPE", "RES_MASTER"));
                    continue;
                }

                ATOperResource arrange = resource.GetArrange(entity.ROUTING_ID, entity.OPER_ID);
                if (arrange == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.OperAddRes, "OPER_RES@ROUTING_ID,OPER_ID,RES_ID", ErrDesc.NotFoundReferredData("ROUTING_ID,OPER_ID,RES_ID", "OPER_RES"));

                    continue;
                }

                if (entity.USAGE_PER < 0)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.OutOfRange,
                        entity, ErrKey.OperAddRes, "", ErrDesc.OutOfRange_greater("USAGE_PER"));

                    continue;
                }

                var obj = ObjectMapper.CreateAddionalArrange(arrange, addResource, entity);
                    
                // 추가 Arrange 정보 등록.
                arrange.SetAddArrangeInfo(addResource.ResGroupID, obj);
            }
        }

        public void OnAction_CUSTOMER(IPersistContext context)
        {
            foreach (var entity in AleatorikTempMart.Instance.CUST_MASTER.Rows)
            {
                var obj = ObjectMapper.CreateCustomer(entity.CUST_ID, entity.CUST_NAME, entity.CUST_PRIORITY);
                if (!DerivedHelper.CallAfterLoadHandler<CUST_MASTER>(entity, obj))
                {
                    continue;
                }
                
                ATInputData.Customers.AddCustomer(obj);
            }
        }

        public void OnAction_CUSTOMER_PROPERTY_VALUE(IPersistContext context)
        {
            foreach (var entity in AleatorikTempMart.Instance.CUST_PROP_VALUE.Rows)
            {                
                var customer = ATInputData.Customers.GetCustomer(entity.CUST_ID);
                if (customer == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.CustPropValue, "CUST_MASTER@CUST_ID", ErrDesc.NotFoundReferredData("CUST_ID", "CUST_MASTER"));

                    continue;
                }

                SetProperties(entity, customer, entity.PROP_ID, entity.PROP_VALUE, entity.CALENDAR_ID, ErrKey.CustPropValue);
            }
        }


        public void OnAction_SALES_ORDER_PROPERTY_VALUE(IPersistContext context)
        {
            foreach (var entity in AleatorikTempMart.Instance.DEMAND_PROP_VALUE.Rows)
            {
                var so = ATInputData.Demands.GetDemandByID(entity.DEMAND_ID);
                if (so == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.DemandPropValue, "DEMAND@DEMAND_ID", ErrDesc.NotFoundReferredData("DEMAND_ID", "DEMAND"));

                    continue;
                }

                SetProperties(entity, so, entity.PROP_ID, entity.PROP_VALUE, entity.CALENDAR_ID, ErrKey.DemandPropValue);
            }
        }


        public void OnAction_WIP_PROPERTY_VALUE(IPersistContext context)
        {
            foreach (var entity in AleatorikTempMart.Instance.WIP_PROP_VALUE.Rows)
            {                
                var wip = ATInputData.Wips.GetWipInfo(entity.WIP_ID);
                if (wip == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.WipPropValue, "WIP@WIP_ID" , ErrDesc.NotFoundReferredData("WIP_ID", "WIP")); 
                    continue;
                }

                SetProperties(entity, wip, entity.PROP_ID, entity.PROP_VALUE, entity.CALENDAR_ID, ErrKey.WipPropValue);
            }
        }

        public void OnAction_ROUTING_OPERATION(IPersistContext context)
        {
            if (AleatorikTempMart.Instance.ROUTING_OPER.Rows.Count == 0)
            {
                OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Critical, ErrorReasonCode.NotFoundKeyInput,
                      null, "ROUTING_OPER@" + ErrKey.RoutingOper, "", ErrDesc.NotFoundKeyInput("ROUTING_OPER"));
            }

            foreach (var entity in AleatorikTempMart.Instance.ROUTING_OPER.Rows)
            {
                List<ERROR_LOG> errLogs = new List<ERROR_LOG>();

                var route = ATInputData.Boms.GetRoute(entity.ROUTING_ID);
                if (route == null)
                {
                    if (ErrorHelper.IsErrorObject(ErrLogKeySchema.Routing, entity.ROUTING_ID) == true) // chainError
                    {
                        errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.ChainError,
                            entity, ErrKey.RoutingOper, "ROUTE_MASTER@ROUTING_ID^BOM_ROUTING@BOM_ID,ROUTING_ID"
                            , ErrDesc.NotFoundReferredData("ROUTING_ID", "ROUTING_MASTER") + "^" + ErrDesc.ChainError()));
                    }
                    else
                    {
                        errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                            entity, ErrKey.RoutingOper, "ROUTE_MASTER@ROUTING_ID^BOM_ROUTING@BOM_ID,ROUTING_ID"
                            , ErrDesc.NotFoundReferredData("ROUTING_ID", "ROUTING_MASTER") + "^" + ErrDesc.NotFoundReferredData("BOM_ID,ROUTING_ID", "BOM_ROUTING")));
                    }
                }

                OperType optype = ATUtil.StringToEnum<OperType>(entity.OPER_TYPE, OperType.None);
                if (optype == OperType.None)
                {
                    errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                        entity, ErrKey.RoutingOper, "", ErrDesc.MismatchReservedWord("OPER_TYPE", ATUtil.GetEnumProperty<OperType>())));
                }

                if (entity.WAIT_TAT < 0)
                {
                    errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.OutOfRange,
                            entity, ErrKey.RoutingOper, "", ErrDesc.OutOfRange_greater("WAIT_TAT")));
                }

                if (entity.RUN_TAT < 0)
                {
                    errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.OutOfRange,
                            entity, ErrKey.RoutingOper, "", ErrDesc.OutOfRange_greater("RUN_TAT")));
                }

                if (entity.OPER_YIELD < 0 || entity.OPER_YIELD > 1)
                {
                    errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.OutOfRange,
                            entity, ErrKey.RoutingOper, "", ErrDesc.OutOfRange_between("OPER_YIELD", "0","1")));
                }

                ATCalendar tatCal = ATInputData.Calendars.GetCalendar(entity.TAT_CALENDAR_ID);
                if (entity.TAT_CALENDAR_ID.IsNullOrEmpty() == false)
                {
                    if (tatCal == null)
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Info, ErrorReasonCode.NotFoundReferredData,
                            entity, ErrKey.RoutingOper, "", ErrDesc.NotFoundReferredData("TAT_CALENDAR_ID", "CALENDAR_MASTER"));
                    }
                    else if(tatCal.CalendarType != ATReservedCode.TAT)
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Info, ErrorReasonCode.LawViolation, entity, ErrKey.RoutingOper, "",
                            "Only #Tat calendar can be value of TAT_CALENDAR_ID column in ROUTING_OPER table. Replace it with CALENDAR_TYPE = '#Tat' in CALENDAR_MASTER table.");

                        tatCal = null;
                    }
                }

                ATCalendar yieldCal = ATInputData.Calendars.GetCalendar(entity.YIELD_CALENDAR_ID);
                if (entity.YIELD_CALENDAR_ID.IsNullOrEmpty() == false)
                {
                    if (yieldCal == null)
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Info, ErrorReasonCode.NotFoundReferredData,
                            entity, ErrKey.RoutingOper, "", ErrDesc.NotFoundReferredData("YIELD_CALENDAR_ID", "CALENDAR_MASTER"));
                    }
                    else if(yieldCal.CalendarType != ATReservedCode.YIELD)
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Info, ErrorReasonCode.LawViolation, entity, ErrKey.RoutingOper, "",
                            "Only #Yield calendar can be value of YIELD_CALENDAR_ID column in ROUTING_OPER table. Replace it with CALENDAR_TYPE = ‘#Yield’in CALENDAR_MASTER table.");

                        yieldCal = null;
                    }
                }

                double multiLotSize = entity.MULTI_LOT_SIZE < 0 ? 0 : entity.MULTI_LOT_SIZE;
                
                if (errLogs.Count() != 0)
                {
                    ErrorHelper.AddErrLog(entity, ErrLogKeySchema.RoutingOper, ErrLogKeyColumn.RoutingOper, errLogs);

                    continue;
                }

                double yield = entity.OPER_YIELD;
                var obj = ObjectMapper.CreateRouteOper(entity, optype, yield, tatCal, yieldCal, multiLotSize );

                if (!DerivedHelper.CallAfterLoadHandler<ROUTING_OPER>(entity, obj))
                {
                    continue;
                }

                route.AddOper(obj);

                if (obj.OperType == OperType.Operation)
                {
                    ATInputData.Boms.AddOperationTypeOper(obj);
                }
            }

            var routes = ATInputData.Boms.GetRoute();
            List<ATRoute> del = new List<ATRoute>();

            foreach (var route in routes)
            {
                if (route.Opers.Count() == 0)
                {
                    ROUTING_MASTER entity = new ROUTING_MASTER();
                    entity.ROUTING_ID = route.RouteID;

                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.RoutingMaster, "ROUTING_OPERATION@ROUTING_ID", ErrDesc.NotFoundReferredData("ROUTING_ID,OPERATION_ID", "ROUTING_OPERATION"));

                    del.Add(route);
                }

                route.Opers.Sort(

                    delegate (Step x, Step y)
                    {
                        if (object.ReferenceEquals(x, y))
                            return 0;

                        int cmp = (x as ATOperation).Sequence.CompareTo((y as ATOperation).Sequence);

                        return cmp;
                    }
                );

                route.LinkOpers();
            }

            del.ForEach(x => ATInputData.Boms.RemoveRoute(x.RouteID));
        }

        public void OnAction_BOM_PROPERTY_VALUE(IPersistContext context)
        {
            foreach (var entity in AleatorikTempMart.Instance.BOM_PROP_VALUE.Rows)
            {                
                var bom = ATInputData.Boms.GetBom(entity.BOM_ID);
                if (bom == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.BomPropValue, "BOM_MASTER@BOM_ID", ErrDesc.NotFoundReferredData("BOM_ID", "BOM_MASTER"));
                    continue;
                }

                SetProperties(entity, bom, entity.PROP_ID, entity.PROP_VALUE, entity.CALENDAR_ID, ErrKey.BomPropValue);
            }
        }

        public void OnAction_EndPersist(IPersistContext context)
        {
            if (ATPersistHelper.HasErrorData)
            {
                throw new Exception(string.Format("Invalid Data Error"));
            }
        }

        public void OnAction_ITEM_SITE_PROPERTY_VALUE(IPersistContext context)
        {
            foreach (var entity in AleatorikTempMart.Instance.ITEM_SITE_BUFFER_PROP_VALUE.Rows)
            {
                var itemBuffer = ATInputData.ItemSiteBuffers.GetItemSite(entity.SITE_ID, entity.ITEM_ID, entity.BUFFER_ID);
                if (itemBuffer == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.ItemSiteBufferPropValue, "ITEM_SITE_BUFFER_MASTER@ITEM_ID,SITE_ID,BUFFER_ID", ErrDesc.NotFoundReferredData("ITEM_ID,SITE_ID,BUFFER_ID", "ITEM_SITE_BUFFER_MASTER"));
                    continue;
                }

                SetProperties(entity, itemBuffer, entity.PROP_ID, entity.PROP_VALUE, entity.CALENDAR_ID, ErrKey.ItemSiteBufferPropValue);
            }
        }

        public void OnAction_ITEM_SITE_MASTER(IPersistContext context)
        {
            if (AleatorikTempMart.Instance.ITEM_SITE_BUFFER_MASTER.Rows.Count == 0)
            {
                OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Critical, ErrorReasonCode.NotFoundKeyInput,
                      null, "ITEM_SITE_BUFFER_MASTER@" + ErrKey.ItemSiteBufferMaster, "", ErrDesc.NotFoundKeyInput("ITEM_SITE_BUFFER_MASTER"));
            }

            foreach (var entity in AleatorikTempMart.Instance.ITEM_SITE_BUFFER_MASTER.Rows)
            {
                List<ERROR_LOG> errLogs = new List<ERROR_LOG>();

                ATSite site = ATInputData.ItemSiteBuffers.GetSite(entity.SITE_ID);
                if (site == null)
                {
                    errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.ItemSiteBufferMaster, "SITE_MASTER@SITE_ID", ErrDesc.NotFoundReferredData("SITE_ID", "SITE_MASTER")));

                }

                ATItem item = ATInputData.ItemSiteBuffers.GetItem(entity.ITEM_ID);
                if (item == null)
                {
                    if (ErrorHelper.IsErrorObject(ErrLogKeySchema.Item, entity.ITEM_ID) == true) // chainError
                    {
                        errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.ChainError,
                            entity, ErrKey.ItemSiteBufferMaster, "ITEM_MASTER@ITEM_ID", ErrDesc.ChainError()));
                    }
                    else
                    {
                        errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                            entity, ErrKey.ItemSiteBufferMaster, "ITEM_MASTER@ITEM_ID", ErrDesc.NotFoundReferredData("ITEM_ID", "ITEM_MASTER")));
                    }
                }

                ATBuffer buffer = ATInputData.ItemSiteBuffers.GetBuffer(entity.BUFFER_ID);
                if (buffer == null)
                {
                    if (ErrorHelper.IsErrorObject(ErrLogKeySchema.Buffer, entity.BUFFER_ID) == true)
                    {
                        errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.ChainError,
                            entity, ErrKey.ItemSiteBufferMaster, "BUFFER_MASTER@BUFFER_ID", ErrDesc.ChainError()));
                    }
                    else
                    {
                        errLogs.Add(OutputWriter.Instance.SetErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                            entity, ErrKey.ItemSiteBufferMaster, "BUFFER_MASTER@BUFFER_ID", ErrDesc.NotFoundReferredData("BUFFER_ID", "BUFFER_MASTER")));
                    }
                }

                bool noCarryOver = ATUtil.BoolYN(entity.NO_CARRY_YN, false);
                bool isInfiniteMaterial = ATUtil.BoolYN(entity.INFINITY_MATERIAL_YN, false);
                if (item != null && item.ItemType != ItemType.Material && isInfiniteMaterial == true)
                {
                    isInfiniteMaterial = false;
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Info, ErrorReasonCode.AutoCorrectionData, entity, ErrKey.ItemSiteBufferMaster, "",
                        "INFINITY_MATERIAL_YN can't be applied to PROD_TYPE = ‘Material’. Remove the INFINITY_MATERIAL_YN value");
                }

                if (errLogs.Count() != 0)
                {
                    ErrorHelper.AddErrLog(entity, ErrLogKeySchema.ItemSiteBuffer, ErrLogKeyColumn.ItemSiteBuffer, errLogs);
                    
                    continue;
                }

                ATItemSiteBuffer obj = ObjectMapper.CreateItemSite(site, item, buffer, isInfiniteMaterial, noCarryOver, entity.INPUT_LOT_SIZE);
                
                if (!DerivedHelper.CallAfterLoadHandler<ITEM_SITE_BUFFER_MASTER>(entity, obj))
                {
                    continue;
                }

                ATInputData.ItemSiteBuffers.AddItemSiteBuffer(obj);
            }
        }

        public void OnAction_BOM_DETAIL_ALT(IPersistContext context)
        {
            foreach (var entity in AleatorikTempMart.Instance.BOM_DETAIL_ALT.Rows)
            {
                #region
                ATBom bom = ATInputData.Boms.GetBom(entity.BOM_ID);
                if (bom == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.BomDetailAlt, "BOM_MASTER@BOM_ID", ErrDesc.NotFoundReferredData("BOM_ID", "BOM_MASTER"));

                    continue;
                }

                ATItem item = ATInputData.ItemSiteBuffers.GetItem(entity.ITEM_ID);
                if (item == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.BomDetailAlt, "ITEM_MASTER@ITEM_ID", ErrDesc.NotFoundReferredData("ITEM_ID", "ITEM_MASTER"));

                    continue;
                }

                ATSite site = ATInputData.ItemSiteBuffers.GetSite(entity.SITE_ID);
                if (site == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.BomDetailAlt, "SITE_MASTER@SITE_ID", ErrDesc.NotFoundReferredData("SITE_ID", "SITE_MASTER"));
                    continue;
                }

                ATBuffer buffer = ATInputData.ItemSiteBuffers.GetBuffer(entity.BUFFER_ID);
                if (buffer == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.BomDetailAlt, "BUFFER_MASTER@BUFFER_ID", ErrDesc.NotFoundReferredData("BUFFER_ID", "BUFFER_MASTER"));
                    continue;
                }

                ATItem altItem = ATInputData.ItemSiteBuffers.GetItem(entity.ALT_ITEM_ID);
                if (altItem == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.BomDetailAlt, "ITEM_MASTER@ITEM_ID", ErrDesc.NotFoundReferredData("ALT_ITEM_ID", "ITEM_MASTER"));
                    continue;
                }

                ATSite altSite = ATInputData.ItemSiteBuffers.GetSite(entity.ALT_SITE_ID);
                if (altSite == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.BomDetailAlt, "SITE_MASTER@SITE_ID", ErrDesc.NotFoundReferredData("ALT_SITE_ID", "SITE_MASTER"));  
                    continue;
                }

                ATBuffer altBuffer = ATInputData.ItemSiteBuffers.GetBuffer(entity.ALT_BUFFER_ID);
                if (altBuffer == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.BomDetailAlt, "BUFFER_MASTER@BUFFER_ID", ErrDesc.NotFoundReferredData("ALT_BUFFER_ID", "BUFFER_MASTER"));
                    continue;
                }

                ATItemSiteBuffer altISB = ATInputData.ItemSiteBuffers.GetItemSite(entity.ALT_SITE_ID, entity.ALT_ITEM_ID, entity.ALT_BUFFER_ID);
                if(altISB == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.BomDetailAlt, "ITEM_SITE_BUFFER_MASTER@SITE_ID,ITEM_ID,BUFFER_ID", ErrDesc.NotFoundReferredData("ALT_SITE_ID,ALT_ITEM_ID,ALT_BUFFER_ID", "ITEM_SITE_BUFFER_MASTER"));
                    continue;
                }

                ATItemSiteBuffer isb = ATInputData.ItemSiteBuffers.GetItemSite(entity.SITE_ID, entity.ITEM_ID, entity.BUFFER_ID);
                if (altISB == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.BomDetailAlt, "ITEM_SITE_BUFFER_MASTER@SITE_ID,ITEM_ID,BUFFER_ID", ErrDesc.NotFoundReferredData("SITE_ID,ITEM_ID,BUFFER_ID", "ITEM_SITE_BUFFER_MASTER"));
                    continue;
                }
                #endregion

                ATBomDetailAlt alt = new ATBomDetailAlt(bom, site, item, buffer, altSite, altItem, altBuffer, entity.ALT_PRIORITY);
                var fromItemSiteBuffer = alt.FromItemSiteBuffer;

                if (fromItemSiteBuffer.AltItemSiteBuffers.ContainsKey(alt.BomID))
                {
                    var altItemSiteBuffer = fromItemSiteBuffer.AltItemSiteBuffers[alt.BomID];
                    var altItemSiteBufferKey = fromItemSiteBuffer.AltItemSiteBufferKeys[alt.BomID];

                    altItemSiteBuffer.Add(alt);
                    altItemSiteBufferKey.Add(alt.AltItemSiteBuffer.Key);
                }
                else
                {
                    var altInfos = new HashSet<ATBomDetailAlt>();
                    var altKeys = new HashedSet<string>();

                    altInfos.Add(alt);
                    fromItemSiteBuffer.AltItemSiteBuffers.Add(alt.BomID, altInfos);

                    altKeys.Add(alt.AltItemSiteBuffer.Key);
                    fromItemSiteBuffer.AltItemSiteBufferKeys.Add(alt.BomID, altKeys);
                }
            }
        }

        public void OnAction_PROPERTY_MASTER(IPersistContext context)
        {
            foreach (var entity in AleatorikTempMart.Instance.PROP_MASTER.Rows)
            {
                var category = ATUtil.StringToEnum<PropertyCategory>(entity.PROP_CATEGORY, PropertyCategory.None);
                if (category == PropertyCategory.None)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                        entity, ErrKey.PropMaster, "", ErrDesc.MismatchReservedWord("PROP_CATEGORY", ATUtil.GetEnumProperty<PropertyCategory>()) );
                    continue;
                }

                DataType type = ATUtil.StringToEnum<DataType>(entity.DATA_TYPE, DataType.None);

                if (type == DataType.None)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                        entity, ErrKey.PropMaster, "", ErrDesc.MismatchReservedWord("DATA_TYPE",  ATUtil.GetEnumProperty<DataType>()));
                    
                    continue;
                }

                if (type == DataType.Int || type == DataType.Double || type == DataType.DateTime)
                {
                    if (string.IsNullOrEmpty(entity.DEFAULT_VALUE) == true)
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.DataTypeMisMatch,
                            entity, ErrKey.PropMaster, "", ErrDesc.MismatchReservedWord("DEFAULT_VALUE", "If PROPERTY_TYPE is Int or Double, DEFAULT_VALUE must be not Null"));
                    }
                    
                }

                if (string.IsNullOrEmpty(entity.RESERVED_WORD) == false)
                {
                    if (string.IsNullOrEmpty(entity.DEFAULT_VALUE) == true || entity.RESERVED_WORD.Contains(entity.DEFAULT_VALUE) == false)
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                           entity, ErrKey.PropMaster, "", ErrDesc.MismatchReservedWord("DEFAULT_VALUE", entity.RESERVED_WORD));

                        continue;
                    }
                }

                if (string.IsNullOrEmpty(entity.DEFAULT_VALUE))
                {
                    if (type == DataType.DateTime)
                        entity.DEFAULT_VALUE = ATUtil.DateMinValue.ToString();
                    else if (type == DataType.Int || type == DataType.Double)
                        entity.DEFAULT_VALUE = "0";
                    else
                        entity.DEFAULT_VALUE = string.Empty;
                }

                ATProperty obj = new ATProperty(category.ToString(), entity.PROP_ID, type, entity.DESCRIPTION,
                            entity.DEFAULT_VALUE, entity.RESERVED_WORD);

                if (ATInputData.Properties.AddProperty(obj) == false)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.DataDuplication,
                        entity, ErrKey.PropMaster);

                    continue;
                }
            }
        }

        public void OnAction_SITE_PROPERTY_VALUE(IPersistContext context)
        {
            foreach (var entity in AleatorikTempMart.Instance.SITE_PROP_VALUE.Rows)
            {
                var site = ATInputData.ItemSiteBuffers.GetSite(entity.SITE_ID);
                if (site == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.SitePropValue, "SITE_MASTER@SITE_ID", ErrDesc.NotFoundReferredData("SITE_ID", "SITE_MASTER"));

                    continue;
                }

                SetProperties(entity, site, entity.PROP_ID, entity.PROP_VALUE, entity.CALENDAR_ID, ErrKey.SitePropValue);
            }
        }

        public void OnAction_RESOURCE_PROPERTY_VALUE(IPersistContext context)
        {
            foreach (var entity in AleatorikTempMart.Instance.RES_PROP_VALUE.Rows)
            {
                var resource = ATInputData.Resources.GetResource(entity.RES_ID);
                if (resource == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.ResPropValue, "RES_MASTER@RESOURCE_ID", ErrDesc.NotFoundReferredData("RES_ID", "RES_MASTER"));
                    
                    continue;
                }

                if (resource.ResCategory == ResourceCategory.AddResource && entity.PROP_ID == ATReservedCode.CAPACITY)
                {
                    var calendar = ATInputData.Calendars.GetCalendar(entity.CALENDAR_ID);
                    foreach (ATCalendarDetail detail in calendar.Details.Values)
                    {
                        foreach (string attribute in detail.Attributes.Keys)
                        {
                            if (attribute == ATReservedCode.OFF_TIME && detail.Attributes[attribute] != null && detail.Attributes[attribute].Count() > 0)
                            {
                                // errStr
                                OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.IncompatibleConfig,
                                    entity, ErrKey.ResPropValue, "CALENDAR_BASED_ATTR@ATTR_TYPE", "Can't set #OffTime in AddResource");

                                continue;
                            }
                        }
                    }
                }

                SetProperties(entity, resource, entity.PROP_ID, entity.PROP_VALUE, entity.CALENDAR_ID, ErrKey.ResPropValue);
            }


            var stages = ATExecutionContext.Instance.Stages;

            foreach (var stage in stages)
            {
                foreach (var resource in stage.Resources)
                {
                    if (resource.CapaInfos.Count() != 0)
                        continue;

                    RES_PROP_VALUE entity = new RES_PROP_VALUE();
                    entity.RES_ID = resource.ResourceID;
                    entity.PROP_ID = "#Capacity";

                    string detail = ErrDesc.NotFoundReferredData("#Capacity", "RES_PROP_VALUE");

                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                           entity, ErrKey.ResPropValue, "CALENDAR_MASTER@CALENDAR_ID", detail);
                }
            }
        }



        public void OnAction_BOM_ROUTING_PROPERTY_VALUE(IPersistContext context)
        {
            foreach (var entity in AleatorikTempMart.Instance.BOM_ROUTING_PROP_VALUE.Rows)
            {
                string key = entity.BOM_ID + entity.ROUTING_ID;
                var bomRouting = ATInputData.Boms.GetBomRouting(key);
                if (bomRouting == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.BomRoutingPropValue, "BOM_ROUTING@BOM_ID,ROUTING_ID", ErrDesc.NotFoundReferredData("BOM_ID,ROUTING_ID", "BOM_ROUTING"));
                    continue;
                }

                SetProperties(entity, bomRouting, entity.PROP_ID, entity.PROP_VALUE, entity.CALENDAR_ID, ErrKey.BomRoutingPropValue);
            }
        }

        public void OnAction_REF_PRODUCTION_PLAN(IPersistContext context)
        {
            foreach (var entity in AleatorikTempMart.Instance.REF_PROD_PLAN.Rows)
            {
                var stage = ATExecutionContext.Instance.GetStage(entity.STAGE_ID);
                if (stage == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.RefProdPlan, "STAGE_MASTER@STAGE_ID", ErrDesc.NotFoundReferredData("STAGE_ID", "STAGE"));
                    continue;
                }
                  
                ATItemSiteBuffer itemBuffer = ATInputData.ItemSiteBuffers.GetItemSite(entity.SITE_ID, entity.ITEM_ID, entity.BUFFER_ID);

                if (itemBuffer == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.RefProdPlan, "ITEM_SITE_BUFFER_MASTER@SITE_ID,ITEM_ID,BUFFER_ID", ErrDesc.NotFoundReferredData("ITEM_ID,SITE_ID&BUFFER_ID", "ITEM_SITE_BUFFER_MASTER"));
                    continue;
                }

                ATBom bom = null;
                if (string.IsNullOrEmpty(entity.BOM_ID) == false)
                {
                    bom = ATInputData.Boms.GetBom(entity.BOM_ID);

                    if (bom == null)
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                            entity, ErrKey.RefProdPlan, "BOM_MASTER@BOM_ID", ErrDesc.NotFoundReferredData("BOM_ID", "BOM_MASTER")); 
                        continue;
                    }
                }

                ATRoute route = null;
                ATOperation oper = null;
                if (bom != null && string.IsNullOrEmpty(entity.ROUTING_ID) == false
                    && string.IsNullOrEmpty(entity.OPER_ID) == false)
                {
                    route = ATInputData.Boms.GetRoute(entity.ROUTING_ID);

                    if (route == null)
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                            entity, ErrKey.RefProdPlan, "ROUTING_MASTER@ROUTING_ID^BOM_ROUTING@BOM_ID,ROUTING_ID",
                            ErrDesc.NotFoundReferredData("BOM_ID,ROUTING_ID", "BOM_ROUTING") + "^" + ErrDesc.NotFoundReferredData("ROUTING_ID", "ROUTING_MATER")); 
                        continue;
                    }

                    oper = route.FindOper(entity.OPER_ID);
                    if (oper == null)
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                            entity, ErrKey.RefProdPlan, "ROUTING_OPER@ROUTING_ID,OPER_ID", ErrDesc.NotFoundReferredData("ROUTING_ID,OPER_ID", "ROUTING_OPER"));
                        continue;
                    }
                }

                ATResource res = null;
                if (route != null && oper != null && string.IsNullOrEmpty(entity.RES_ID) == false)
                {
                    res = ATInputData.Resources.GetResource(entity.RES_ID);

                    if (res == null)
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                            entity, ErrKey.RefProdPlan, "RES_MASTER@RES_ID", ErrDesc.NotFoundReferredData("RES_ID", "RES_MASTER"));

                        continue;
                    }

                    if (oper.Resources.Contains(res) == false)
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                            entity, ErrKey.RefProdPlan, "OPER_RES@ROUTING_ID,OPER_ID,RES_ID", ErrDesc.NotFoundReferredData("ROUTING_ID,OPER_ID,RES_ID", "OPER_RES"));

                        continue;
                    }
                }

                if (entity.DEMAND_QTY <= 0)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.OutOfRange,
                            entity, ErrKey.RefProdPlan, "", ErrDesc.OutOfRange_above("DEMAND_QTY"));

                    continue;
                }

                string type = entity.REF_TYPE;
                var refPlan = ObjectMapper.CreateRefProductionPlan(entity, itemBuffer, type, stage, entity.DEMAND_ID);

                refPlan.Bom = bom;
                refPlan.Route = route;
                refPlan.Operation = oper;
                refPlan.Resource = res;

                if (!DerivedHelper.CallAfterLoadHandler<REF_PROD_PLAN>(entity, refPlan))
                {
                    
                    continue;
                }

                itemBuffer.Buffer.IsRefPlanBuffer = true;

                if (itemBuffer.RefPlans.TryGetValue(type, out List<APERefPlan> lst) == false)
                {
                    lst = new List<APERefPlan>();
                    itemBuffer.RefPlans.Add(type, lst);
                }

                lst.Add(refPlan);

                ATInputData.RefPlans.AddRefPlan(refPlan);
            }
        }

        public void OnAction_EXECUTION_PLAN(IPersistContext context)
        {
            if (AleatorikInputMart.Instance.SCENARIO_CONFIG.Rows.Count == 0)
            {
                OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Critical, ErrorReasonCode.NotFoundKeyInput,
                        null, "SCENARIO_CONFIG@" + ErrKey.ScenarioConfig, "", ErrDesc.NotFoundKeyInput("SCENARIO_CONFIG"));
            }

            HashSet<string> seqValid = new HashSet<string>();

            foreach (var entity in AleatorikInputMart.Instance.SCENARIO_CONFIG.Rows)
            {
                var scenario = AleatorikGlobalParameters.Instance.PlanScenario;
                if (entity.SCENARIO_ID != scenario)
                    continue;

                var stage = ATExecutionContext.Instance.GetStage(entity.STAGE_ID);
                if (stage == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                       entity, ErrKey.ScenarioConfig, "STAGE_MASTER@STAGE_ID", ErrDesc.NotFoundReferredData("STAGE_ID", "STAGE"));

                    continue;
                }

                string seqValidKey = entity.SCENARIO_ID + "@" + entity.MODULE_ID + "@" + entity.MODULE_SEQ;
                if (seqValid.Contains(seqValidKey) == true)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.IncompatibleConfig,
                        entity, ErrKey.ScenarioConfig, "", "Each record must have different SEQUENCE for same STAGE_ID");

                    continue;
                }

                // Pegging/Planning 모듈만 실행 가능
                var moduleType = ATUtil.StringToEnum<ModuleType>(entity.MODULE_TYPE, ModuleType.None);
                ModuleExecutionInfo info = null;
                switch (moduleType)
                {
                    case ModuleType.PBO:
                        info = new PBOModuleExecutionInfo(entity.MODULE_ID, stage, entity.MODULE_SEQ, entity.REF_MODULE_ID);
                        break;

                    case ModuleType.PBB:
                        info = new PBBModuleExecutionInfo(entity.MODULE_ID, stage, entity.MODULE_SEQ, entity.REF_MODULE_ID);
                        break;

                    case ModuleType.PBF:
                        info = new PBFModuleExecutionInfo(entity.MODULE_ID, stage, entity.MODULE_SEQ, entity.REF_MODULE_ID);
                        break;

                    case ModuleType.None:
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                            entity, ErrKey.ScenarioConfig, "", ErrDesc.MismatchReservedWord("MODULE_TYPE", ATUtil.GetEnumProperty<ModuleType>()));

                        break;
                }

                if (info == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.ScenarioConfig, "SCENARIO_CONFIG@MODULE_ID", ErrDesc.NotFoundReferredData("REF_MODULE_ID", "SCENARIO_CONFIG"));

                    continue;
                }

                info.PhaseCount = entity.MAX_PHASE;

                if (!DerivedHelper.CallAfterLoadHandler<SCENARIO_CONFIG>(entity, info))
                {
                    continue;
                }
                
                ATExecutionContext.Instance.AddExecutionInfo(info);
            }
        }

        public void OnAction_OPERATION_RESOURCE_PROPERTY_VALUE(IPersistContext context)
        {
            foreach (var entity in AleatorikTempMart.Instance.OPER_RES_PROP_VALUE.Rows)
            {
                var resource = ATInputData.Resources.GetResource(entity.RES_ID);
                if (resource == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.OperResPropValue, "RES_MASTER@RES_ID", ErrDesc.NotFoundReferredData("RES_ID","RES_MASTER"));
                    continue;
                }

                ATOperResource arrange = resource.GetArrange(entity.ROUTING_ID, entity.OPER_ID);
                if (arrange == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.OperResPropValue, "OPER_RES@ROUTING_ID,OPER_ID,RES_ID", ErrDesc.NotFoundReferredData("ROUTING_ID,OPER_ID,RES_ID", "OPER_RES")); 
                    continue;
                }

                SetProperties(entity, arrange, entity.PROP_ID, entity.PROP_VALUE, entity.CALENDAR_ID, ErrKey.OperResPropValue);
            }
        }

        public void OnAction_ROUTING_OPERATION_PROPERTY_VALUE(IPersistContext context)
        {
            foreach (var entity in AleatorikTempMart.Instance.ROUTING_OPER_PROP_VALUE.Rows)
            {
                ATRoute route = ATInputData.Boms.GetRoute(entity.ROUTING_ID);
                if (route == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.RoutingOperPropValue, "ROUTING_MASTER@ROUTING_ID^BOM_ROUTING@BOM_ID,ROUTING_ID"
                        , ErrDesc.NotFoundReferredData("ROUTING_ID","ROUTING_MASTER") + "^" + ErrDesc.NotFoundReferredData("BOM_ID,ROUTING_ID", "BOM_ROUTING"));

                    continue;
                }

                ATOperation oper = route.FindOper(entity.OPER_ID);
                if (oper == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.RoutingOperPropValue, "ROUTING_OPER@ROUTING_ID,OPER_ID", ErrDesc.NotFoundReferredData("ROUTING_ID,OPER_ID", "ROUTING_OPER"));
                    continue;
                }

                SetProperties(entity, oper, entity.PROP_ID, entity.PROP_VALUE, entity.CALENDAR_ID, ErrKey.RoutingOperPropValue);
            }
        }

        public void OnAction_OPERATION_ADDITIONAL_RESOURCE_PROPERTY_VALUE(IPersistContext context)
        {
            foreach (var entity in AleatorikTempMart.Instance.OPER_ADD_RES_PROP_VALUE.Rows)
            {
                var route = ATInputData.Boms.GetRoute(entity.ROUTING_ID);
                if (route == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.OperAddPropValue, "ROUTING_MASTER@ROUTING_ID^BOM_ROUTING@BOM_ID,ROUTING_ID"
                        , ErrDesc.NotFoundReferredData("ROUTING_ID", "ROUTING_MASTER") + "^" + ErrDesc.NotFoundReferredData("BOM_ID,ROUTING_ID", "BOM_ROUTING"));

                    continue;
                }

                var oper = route.FindOper(entity.OPER_ID);
                if (oper == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.OperAddPropValue, "ROUTING_OPER@ROUTING_ID,OPER", ErrDesc.NotFoundReferredData("ROUTING_ID,OPER_ID", "ROUTING_OPER"));

                    continue;
                }

                var resource = ATInputData.Resources.GetResource(entity.RES_ID);
                if (resource == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.OperAddPropValue, "RES_MASTER@RES_ID", ErrDesc.NotFoundReferredData("RES_ID", "RES_MASTER"));

                    continue;
                }

                var addResource = ATInputData.Resources.GetResource(entity.ADD_RES_ID);
                if (addResource == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.OperAddPropValue, "RES_MASTER@RES_ID", ErrDesc.NotFoundReferredData("ADD_RES_ID", "RES_MASTER"));
                    continue;
                }

                ATOperResource arrange = resource.GetArrange(entity.ROUTING_ID, entity.OPER_ID);

                if (arrange == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.OperAddPropValue, "OPER_RES@ROUTING_ID,OPER_ID,RES_ID", ErrDesc.NotFoundReferredData("ROUTING_ID,OPER_ID,RES_ID", "OPER_RES"));

                    continue;
                }

                // 추가 Arrange 정보 등록.
                ATOperResource addArr = arrange.GetAddArrangeInfo(addResource.ResGroupID, entity.ADD_RES_ID);

                SetProperties(entity, addArr, entity.PROP_ID, entity.PROP_VALUE, entity.CALENDAR_ID, ErrKey.OperAddPropValue);
            }
        }

        public void OnAction_SETUP_INFO(IPersistContext context)
        {
            foreach (var entity in AleatorikTempMart.Instance.SETUP.Rows)
            {
                string setupType = entity.SETUP_CONDITION.TrimData();
                string fromCondition = entity.FROM_CONDITION_VALUE.TrimData();
                string toCondition = entity.TO_CONDITION_VALUE.TrimData();
                
                #region Validation Setup Type
                HashSet<string> setupProperties = ATInputData.Setups.GetSetupProperties(setupType);
                bool isMultiSetup = setupProperties.Count > 1;
                bool isInvalid = false;

                foreach (var prop in setupProperties)
                {
                    if (string.IsNullOrEmpty(prop))
                    {
                        // 에러 로그 사유 수정 필요
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                            entity, ErrKey.Setup, "SETUP@SETUP_CONDITION", ErrDesc.MismatchReservedWord("SETUP_CONDITION", ATUtil.GetEnumProperty<SetupProperty>()));

                        isInvalid = true;
                        break;
                    }

                    if (prop.StartsWith(ATReservedCode.RESERVED_PREFIX_CODE))
                    {
                        if (ATInputData.Setups.IsReservedSetupProperty(prop) == false)
                        {
                            OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord, 
                                entity, ErrKey.Setup, "SETUP@SETUP_CONDITION", ErrDesc.MismatchReservedWord("SETUP_CONDITION", ATUtil.GetEnumProperty<SetupProperty>()));

                            isInvalid = true;
                            break;
                        }
                    }
                    else
                    {
                        var property = ATInputData.Properties.GetPropertyByID(prop);
                        if (property == null)
                        {
                            OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                                entity, ErrKey.Setup, "PROP_MASTER@PROP_ID", ErrDesc.NotFoundReferredData("SETUP_CONDITION", "PROP_MASTER"));

                            isInvalid = true;
                            break;
                        }

                        property.IsSetupProperty = true;
                    }
                }

                if (isInvalid)
                    continue;
                #endregion

                #region Validation Condition
                if (isMultiSetup)
                {
                    string keySeparator = ATInputData.Setups.GetUsedSeparator(setupType);
                    string fromSeparator = ATInputData.Setups.GetUsedSeparator(fromCondition);
                    string toSeparator = ATInputData.Setups.GetUsedSeparator(toCondition);

                    if (keySeparator != fromSeparator)
                    {
                        // 사유 수정 필요
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord, 
                            entity, ErrKey.Setup, "SETUP@SETUP_CONDITION", ErrDesc.MismatchReservedWord("SETUP_CONDITION", ATUtil.GetEnumProperty<SetupProperty>()));

                        continue;
                    }

                    if (keySeparator != toSeparator)
                    {
                        // 사유 수정 필요
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord, 
                            entity, ErrKey.Setup, "SETUP@SETUP_CONDITION", ErrDesc.MismatchReservedWord("SETUP_CONDITION", ATUtil.GetEnumProperty<SetupProperty>()));

                        continue;
                    }

                    var fromValues = ATInputData.Setups.GetSetupValues(fromCondition);
                    var toValues = ATInputData.Setups.GetSetupValues(toCondition);
                    if (fromValues.Contains(string.Empty))
                    {
                        // 사유 수정 필요
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord, 
                            entity, ErrKey.Setup, "SETUP@SETUP_CONDITION", ErrDesc.MismatchReservedWord("SETUP_CONDITION", ATUtil.GetEnumProperty<SetupProperty>()));

                        continue;
                    }

                    if (toValues.Contains(string.Empty))
                    {
                        // 사유 수정 필요
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord, 
                            entity, ErrKey.Setup, "SETUP@SETUP_CONDITION", ErrDesc.MismatchReservedWord("SETUP_CONDITION", ATUtil.GetEnumProperty<SetupProperty>()));

                        continue;
                    }
                }
                #endregion

                List<ATResource> setupResources = new List<ATResource>();

                if (string.IsNullOrEmpty(entity.SETUP_RES_ID) == false)
                {
                    var resources = entity.SETUP_RES_ID.Replace(" ", "").Split(',');
                    foreach (var res in resources)
                    {
                        var resource = ATInputData.Resources.GetResource(res);
                        if (resource == null || resource.CapaType != CapacityType.Time)
                        {
                            OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                                entity, ErrKey.Setup, "RES_MASTER@RES_ID", ErrDesc.NotFoundReferredData("RES_ID", "RES_MASTER"));
                            continue;
                        }

                        setupResources.Add(resource);
                    }
                }

                if (entity.SETUP_TIME < 0)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.OutOfRange,
                                entity, ErrKey.Setup, "", ErrDesc.OutOfRange_greater("SETUP_TIME"));

                    continue;
                }

                var obj = ObjectMapper.CreateSetupInfo(entity, fromCondition, toCondition, setupType, setupResources, isMultiSetup);
                ATInputData.Setups.AddSetupInfo(obj, setupProperties);
            }

            onActionResMaster(false);
        }

        #region methods
        private void CreateFactorMethod(Task.Execution.ModelContext context, List<ATFactor> factors)
        {
            //var ruleFactors = factors.Where(f => f.FactorType != FactorType.Predefined).Select(f => new RuleFactor
            //{
            //    RulePoint = f.RulePoint.ToString(),
            //    Name = f.FactorID,
            //    Expression = f.Expression,
            //});


            //var rulePointAssembly = Assembly.GetAssembly(this.GetType());
            //List<string> additionalAssemblys = new List<string>();
            //additionalAssemblys.Add(rulePointAssembly.Location);
            //additionalAssemblys.Add(typeof(CollectionExtensions).Assembly.Location);
            //additionalAssemblys.Add(typeof(RuleFlow.RuleBase).Assembly.Location);
            //additionalAssemblys.Add(typeof(SeePlan.DataModel.WeightFactor).Assembly.Location);

            //List<string> additionalNamespaces = new List<string>()
            //{
            //    "Mozart.RuleFlow",
            //    "Mozart.SeePlan",
            //    "Mozart.SeePlan.Cbsim",
            //    "Mozart.SeePlan.Pegging",
            //    "Mozart.SeePlan.Simulation",
            //    "Mozart.SeePlan.Aleatorik",
            //    "Mozart.Extensions",
            //};

            var assemblys = context["#assembly"] as Assembly[];

            Assembly custAssembly = null;
            if (assemblys != null)
            {
                custAssembly = assemblys[0];
            }

            //var result = RuleBuilder.Build(ruleFactors, rulePointAssembly, additionalAssemblys, additionalNamespaces);
            RuleManager.Instance.SetAssembly(null);

            RuleManager.Instance.AddRuleSet(custAssembly);
        }

        public void AdjustCalendarType(ATCalendar calendar, CALENDAR_BASED_ATTR entity, ref DataType ValueType)
        {
            if (calendar.IsNeedDailyPatternCode() == false)
                return;

            ValueType = ATUtil.StringToEnum<DataType>(entity.ATTR_DATA_TYPE, DataType.String);

            string attr = entity.ATTR_TYPE;
            
            if (attr == ATReservedCode.YIELD
                || attr == ATReservedCode.RUNTAT
                || attr == ATReservedCode.WAITTAT
                || attr == ATReservedCode.UTILIZATION_RATE
                || attr == ATReservedCode.CAPACITY
                || attr == ATReservedCode.USAGE_PER
                || attr == ATReservedCode.CONSTRAINT
                )
            {
                entity.ATTR_DATA_TYPE = "Double";
                ValueType = DataType.Double;
            }

            if (attr == ATReservedCode.WORK_TIME
                || attr == ATReservedCode.OFF_TIME
                || attr == ATReservedCode.SETUP_ID
                )
            {
                entity.ATTR_DATA_TYPE = "String";
                ValueType = DataType.String;
            }

        }

        public static bool SetSetupProperty(ATProperty property, ISetupProperty entity, string value)
        {
            if (property.IsSetupProperty)
                entity.SetSetupProperty(property.PropertyID, value);

            return true;
        }

        public static void SetConstraintProperty(IConstraint entity, string key, string value)
        {
            string propKey = key + "@" + value;
            var infos = ConstraintHelper.GetConstraintInfoByCondition(propKey);
            if (infos != null)
            {
                foreach (var info in infos)
                {
                    entity.ConstraintInfos.Add(info);
                }

                ATConstraintAgent.Instance.AddConstraintObject(entity);
                entity.HasConstraint = true;
            }
        }

        private ATProperty SetProperties(IEntityObject entity, IPropertyObject obj, string propertyId, string value, string calendarId, string errKey)
        {
            var property = ATInputData.Properties.GetPropertyByID(propertyId);
            if (property == null)
            {
                OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                    entity, errKey, "PROPERTY_MASTER@PROPERTY_ID", ErrDesc.NotFoundReferredData("PROPERTY_ID", "PROPERTY_MASTER"));
                
                return null;
            }

            if (string.IsNullOrEmpty(property.ReservedWord) == false && property.ReservedWord.Contains(value) == false)
            {
                OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.MismatchReservedWord,
                    entity, errKey, "PROPERTY_MASTER@PROPERTY_ID", ErrDesc.MismatchReservedWord("VALUE", property.ReservedWord));

                return property;
            }

            if (string.IsNullOrEmpty(calendarId) == false)
            {
                var calendar = ATInputData.Calendars.GetCalendar(calendarId);

                if (calendar == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, errKey, "CALENDAR_MASTER@CALENDAR_ID",  ErrDesc.NotFoundReferredData("CALENDAR_ID","CALENDAR_MASTER"));

                    return property;
                }

                obj.SetCalendar(propertyId, calendar);
            }
            else
            {
                object propValue;
                if (value == null || string.IsNullOrEmpty(value))
                    propValue = property.DefaultValue;
                else
                {
                    try
                    {
                        propValue = Convert.ChangeType(value, property.Type);
                    }
                    catch
                    {
                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.DataTypeMisMatch, entity,
                            errKey, "PROPERTY_MASTER@PROPERTY_ID", ErrDesc.DataTypeMisMatch(propertyId, property.Type.ToString()));
                        return null;
                    }
                }
                
                obj.SetProperty(property.PropertyID, propValue);
            }

            if (obj is IConstraint)
                SetConstraintProperty(obj as IConstraint, propertyId, value);

            if (obj is ISetupProperty)
                SetSetupProperty(property, obj as ISetupProperty, value);

            return property;
        }

        #endregion

        public void OnAction_CONSTRAINT_INFO(IPersistContext context)
        {
            foreach (var entity in AleatorikTempMart.Instance.CONSTRAINT.Rows)
            {
                var property = ATInputData.Properties.GetPropertyByID(entity.PROP_ID);
                if (property == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.Constraint, "PROP_MASTER@PROP_ID", ErrDesc.NotFoundReferredData("PROP_ID", "PROP_MASTER"));

                    continue;
                }

                ATCalendar calendar = ATInputData.Calendars.GetCalendar(entity.CALENDAR_ID);
                if (calendar == null)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.NotFoundReferredData,
                        entity, ErrKey.Constraint, "CALENDAR_MASTER@CALENDAR_ID", ErrDesc.NotFoundReferredData("CALENDAR_ID", "CALENDAR_MASTER")); 
                    continue;
                }

                ConstraintPolicy policy = ATUtil.StringToEnum<ConstraintPolicy>(entity.CONSTRAINT_POLICY, ConstraintPolicy.None);

                var obj = ObjectMapper.CreateConstraintInfo(entity, calendar, property, policy);

                if (ConstraintHelper.AddConstraintInfo(obj) == false)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Warning, ErrorReasonCode.DataDuplication,
                        entity, ErrKey.Constraint);

                    continue;
                }
            }
        }

        public void OnAction_PM_PLAN(IPersistContext context)
        {
            foreach (var entity in AleatorikTempMart.Instance.PM.Rows)
            {
                #region  Create Calendar

                ATCalendar calendar = ATInputData.Calendars.GetCalendar(entity.PM_ID);

                if (calendar == null)
                {
                    calendar = ObjectMapper.CreateCalendar(entity.PM_ID, ATReservedCode.PM);
                    ATInputData.Calendars.AddCalendar(entity.PM_ID, calendar);
                }
                    
                #endregion

                #region Create CalendarDetail

                var type = ATUtil.StringToEnum<CalendarPatternType>(entity.PATTERN_TYPE, CalendarPatternType.EveryNDays);
                var calendarDetail = ObjectMapper.CreateCalendarDetail(calendar, entity.PM_PRIORITY.ToString(), entity.EFF_START_DATE, entity.EFF_END_DATE, type, entity.PATTERN_VALUE);
                calendar.AddDetail(calendarDetail);

                #endregion

                #region Create CalendarAttr
                string attrValue = entity.PM_TIME.ToString() + "@" + entity.PM_POLICY + "@" + entity.PM_POLICY_VALUE + "@" + entity.PM_START_HOUR;
                var attr = ObjectMapper.CreateCalendarAttribute(calendarDetail, ATReservedCode.PM, attrValue, DataType.String, string.Empty);

                var startTime = calendarDetail.EffectiveStartTime;
                var endTime = calendarDetail.EffectiveEndTime;

                while (startTime < endTime)
                {
                    if (calendarDetail.IsEffectiveTime(startTime) == false)
                    {
                        startTime = startTime.AddDays(1);
                        continue;
                    }

                    string applyDate = startTime.ToString(ATUtil.DateFormat);
                    var attribute = attr.ShallowCopy();
                    attribute.ApplyDate = applyDate;

                    attribute.EffectiveStartTime = startTime;
                        
                    // attribute.EffectiveEndTime = endTime > effEndTime ? effEndTime : endTime;
                    attribute.EffectiveEndTime = calendarDetail.GetEffectiveEndTime(startTime);
                        
                    startTime = attribute.EffectiveEndTime; //startTime.AddDays(1);

                    calendarDetail.AddAttribute(attribute);
                }

                #endregion
            }
        }

        public void OnAction_EndLog(IPersistContext context)
        {
            ATElapsedTimeChecker.Instance.StartCustomTimer("Generate_BomNetwork");
        }

        public void OnAction_SUPPLY_CHAIN_CONFIG(IPersistContext context)
        {
            if (AleatorikInputMart.Instance.FACTORY_CONFIG.Rows.Count <= 0)
            {
                OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Critical, ErrorReasonCode.NotFoundKeyInput, null, "FACTORY_CONFIG@" + ErrKey.FactoryConfig, "", ErrDesc.NotFoundKeyInput("FACTORY_CONFIG"));
            }
            else
            {
                var entity = AleatorikInputMart.Instance.FACTORY_CONFIG.Rows.LastOrDefault();

                var factoryStartTime = entity.FACTORY_START_HOUR;
                if (factoryStartTime < 0 || factoryStartTime > 24)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Critical, ErrorReasonCode.OutOfRange, 
                        entity, ErrKey.FactoryConfig, "", ErrDesc.OutOfRange_between("FACTORY_START_HOUR", "0", "24"));

                    return;
                }

                DayOfWeek startDayOfWeek;
                if (ATUtil.StringToDayOfWeek(entity.START_DAY_OF_WEEK, out startDayOfWeek) == false)
                {
                    OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Critical, ErrorReasonCode.MismatchReservedWord, 
                        entity, ErrKey.FactoryConfig, "", ErrDesc.MismatchReservedWord("START_DAY_OF_WEEK", DayOfWeek.Monday.ToString()));

                    return;
                }

                var target = Mozart.SeePlan.FactoryConfiguration.Current;
                FactoryTimeInfo timeInfo = new FactoryTimeInfo();
                timeInfo.ShiftNames = new string[]
                {
                "A",
                "B",
                "C"
                };
                timeInfo.ShiftHours = 8F;
                target.TimeInfo = timeInfo;

                timeInfo.StartOffset = new TimeSpan(factoryStartTime, 0, 0);
                timeInfo.StartOfWeek = startDayOfWeek;
                target.TimeInfo = timeInfo;
            }
        }

        public void OnAction_PLAN_MASTER(IPersistContext context)
        {
            if (AleatorikInputMart.Instance.PLAN_CONFIG.Rows.Count <= 0)
            {
                OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Critical, ErrorReasonCode.NotFoundKeyInput, null, "PLAN_CONFIG@" + ErrKey.PlanConfig, "", ErrDesc.NotFoundKeyInput("PLAN_CONFIG"));
            }
            else
            {
                var entity = AleatorikInputMart.Instance.PLAN_CONFIG.Rows.LastOrDefault();

                AleatorikGlobalParameters.Instance.PlanScenario = entity.SCENARIO_ID;
                AleatorikGlobalParameters.Instance.ProjectID = entity.PROJECT_ID;
                AleatorikGlobalParameters.Instance.start_time = entity.PLAN_START_DATE.StartTimeOfDay();
                AleatorikGlobalParameters.Instance.period = entity.PLAN_PERIOD;

                ATOption.Instance.PlanStartTime = entity.PLAN_START_DATE.StartTimeOfDay();
                ATOption.Instance.PlanPeriod = entity.PLAN_PERIOD;
                ATOption.Instance.PlanEndTime = ATOption.Instance.PlanStartTime.AddDays(entity.PLAN_PERIOD);
            }
        }

        public void OnAction_EXECUTION_RESULT(IPersistContext context)
        {
        }
    }
}
