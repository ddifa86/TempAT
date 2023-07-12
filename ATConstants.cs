using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public struct ATConstants
    {
        #region PREFIX
        public const string MBS_LOT_PREFIX = "M";
        public const string BIN_WIP_PREFIX = "BI";
        public const string WIP_BATCH_PREFIX = "W";
        public const string TARGET_BATCH_PREFIX = "Z";
        public const string SPLIT_BATCH_PREFIX = "S";
        public const string ASSY_BATCH_PREFIX = "ASSY";

        #endregion

        #region TABLE NAME
        public const string TBL_RULE_MASTER = "RULE_MASTER";
        #endregion

        #region COLUMN
        public const string COL_RULE_ID = "RULE_ID";
        #endregion

        #region TIME UOM
        public const int Day = 86400;
        public const int Hour = 3600;
        public const int Minute = 60;
        #endregion

        #region ETC
        public const string DefaultKey = "Key";
        #endregion
    }
}
