using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class FEProvider
    {
        public const string Aleatorik = "Aleatorik";
    }

    public class FECategory
    {
        public const string Execution = "Execution";
        public const string DataModel = "DataModel";

        public const string PlanByBackward = "PlanByBackward";
        public const string PlanByForward = "PlanByForward";
        public const string PlanByOrder = "PlanByOrder";
    }

    public class FEControl
    {
        #region Execution

        public const string ExecutionModule = "ExecutionModule";
        public const string InputMapper = "InputMapper";
        public const string OutputMapper = "OutputMapper";

        #endregion

        #region OrderBy
        public const string PBOInitialize = "PBOInitialize";
        public const string PBOModule = "PBOModule";
        public const string PBOPegger = "PBOPegger";
        public const string PBOPlanner = "PBOPlanner";
        #endregion

        #region Backward
        public const string PBBInitialize = "PBBInitialize";
        public const string PBBModule = "PBBModule";
        public const string PBBPegger = "PBBPegger";
        #endregion

        #region Forward

        public const string PBFInitialize = "PBFInitialize";
        public const string PBFModule = "PBFModule";

        public const string Factory = "Factory";
        
        public const string Bucket = "Bucket";
        public const string Allocator = "Allocator";
        public const string Assembly = "Assembly";


        #endregion


    }
}
