using Mozart.Data.Entity;
using Mozart.SeePlan.Aleatorik.Outputs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public static class ErrorHelper
    {
        //key : Schema + @ + KeyColumn들 
        // CommonHelper.createkey()
        public static Dictionary<string, ERROR_LOG> ErrLogs = new Dictionary<string, ERROR_LOG>();

        public static void AddErrLog(IEntityObject entity, string errLogKeySchema, string errLogKeyColumn, List<ERROR_LOG> errLogs)
        {
            if (errLogs.Count() == 0 || errLogs.FirstOrDefault() == null)
                return;

            if (entity == null)
                return;

            //schemaName을 가져올 수 있지만, 참조찾기를 위해 명시적으로 파라미터로 받음
            //Type type = entity.GetType();
            //string schemaName = type.Name;

            string errLogKey =  OutputWriter.GetErrorTargetData(entity, errLogKeyColumn.Split(','));
            errLogKey = errLogKeySchema + "@" + errLogKey;

            if (ErrLogs.ContainsKey(errLogKey) == true)
                return;

            ERROR_LOG noChainError = errLogs.Where(x => x.REASON_CODE != ErrorReasonCode.ChainError.ToString()).FirstOrDefault();

            if (noChainError != null) // ChainError가 아닌게 있으면, 해당 Error 우선
                ErrLogs.Add(errLogKey, noChainError);
            else
                ErrLogs.Add(errLogKey, errLogs.FirstOrDefault());

        }

        public static bool IsErrorObject(params object[] args)
        {
            string key = CommonHelper.CreateKey(args);

            if (ErrLogs.ContainsKey(key) == true)
                return true;

            return false;
        }

        public static void WriteError()
        {
            foreach (ERROR_LOG err in ErrLogs.Values)
            {
                OutputWriter.Instance.WriteErrorLog(err);
            }
        }

        public static void ClearErrLogs()
        {
            ErrLogs.Clear();
        }

    }

    public struct ErrLogKeyColumn
    {
        public const string Buffer = "BUFFER_ID";
        public const string Item = "ITEM_ID";
        public const string ItemSiteBuffer = "ITEM_ID,SITE_ID,BUFFER_ID";
        public const string Bom = "BOM_ID";
        public const string BomDetail = "BOM_ID,FROM_ITEM_ID,FROM_SITE_ID,FROM_BUFFER_ID,TO_ITEM_ID,TO_SITE_ID,TO_BUFFER_ID";
        public const string Routing = "ROUTING_ID";
        public const string BomRouting = "ROUTING_ID,BOM_ID";
        public const string RoutingOper = "ROUTING_ID,OPER_ID";
        public const string Wip = "WIP_ID";

    }

    public struct ErrLogKeySchema
    {
        public const string Buffer = "BUFFER_MASTER";
        public const string Item = "ITEM_MASTER";
        public const string ItemSiteBuffer = "ITEM_SITE_BUFFER_MASTER";
        public const string Bom = "BOM_MASTER";
        public const string BomDetail = "BOM_DETAIL";
        public const string Routing = "ROUTING_MASTER";
        public const string BomRouting = "BOM_ROUTING";
        public const string RoutingOper = "ROUTING_OPER";
        public const string Wip = "WIP";

    }
}
