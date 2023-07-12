using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public enum PlanType
    {
        Backward,
        Forward
    }

    public enum ModuleType
    {
        Global,
        PBB,        
        PBF,
        PBO,
        None
    }

    public enum RuleSetType
    {
        Buffer,
        Operation,
        ResourceGroup,
        DefaultRuleset,
        None
    }

    public enum CapacityType
    {
        Time,
        Quantity,
        Count,
        None
    }

    /// <summary>
    /// PlanPosition Enumerations
    /// </summary>
    public enum PlanPosition
    {
        Wait,
        Out,
        Run
    }

    public enum LifeCycle
    {
       Creation,
       Release,
       Change,
       Split,
       Assembly,
       Disposal,
       Remain
    }

    public enum AllocateType
    {
        LotFirstSelection,
        ResourceFirstSelection,
        Reserve
    }


    public enum LotSplitOption
    {
        //
        // 요약:
        //     작업물을 분할하지 않고, 작업 시간을 지연시킵니다.
        KeepAndDelay = 0,

        //
        // 요약:
        //     작업물을 분할합니다.
        Split = 1,
        
        /// <summary>
        /// 
        /// </summary>
        None = 2
    }

    public enum BomType
    {
        /// <summary>
        /// 서로 같거나 다른 둘 이상의 아이템이 하나로 합쳐지는 것 (ASSEMBLY)
        /// </summary>
        Assembly,
        /// <summary>
        /// 하나의 아이템이 들어가 하나 이상의 아이템들로 나누어지는 것 (HYBRID, BIN, CHANGE)
        /// </summary>
        SplitCo,
        /// <summary>
        /// 
        /// </summary>
        SplitBy,
        /// <summary>
        /// 아이템 변경 없이 버퍼만 이동하는 것 (MOVE)
        /// </summary>
        Normal,
        None
    }

    public enum PmPolicy
    {
        Push,
        Fix_Split,
        Fix_None
    }

    public enum PMFlag
    {
        Reserved = 0,
        Waiting = 1,
        Canceled = 2,
        Executed = 3
    }

    public enum SetupPolicy
    {
        None,
        Extend,
        Push,
        Fix
    }
    public enum ConstraintPolicy
    {
        None,
        Cumulative
    }

    public enum CallType
    { 
        Operation,
        Init,
        Level,
        Phase,
        ResGroup,
        None
    }

    public enum RulePointType
    {
        ListKey,
        MergeKey,
        Compare,
        Filter,
        None
    }

    public enum ComparisonOperatorType
    {
        None,
        IsLessThan,
        IsLessThanOrEqualTo,
        IsGreaterThan,
        IsGreaterThanOrEqualTo,
        Equals,
        DoesNotEqual
    }



    public enum FactorType
    { 
        Predefined,
        Custom,
        None
    }

    public enum ItemType
    { 
        Product,
        Material,
        None
    }

    public enum LogicalOperationType
    { 
        None,
        And,
        Or
    }

    public enum LotState
    { 
        Run,
        Wait,
        Hold,
        Out,
        None

    }

    public enum Status
    { 
        Normal,
        Late,
        Short
    }

    public enum OperType
    { 
        Operation,
        Buffer,
        Dummy,
        None
    }

    public enum PegProcedureType
    {
        TargetFirstSelected,
        WipFirstSelected
    }

    public enum PegType
    { 
        Normal,
        Kit,
        Bin
    }

    public enum PresetType
    { 
        Buffer,
        Operation,
        Resource,
        AllocationGroup,
        None
    }

    public enum ResourceCategory
    { 
        Resource,
        AddResource,
        SetupResource,
        Constraint,
        None
    }

    public enum SortType
    { 
        WeightSum,
        WeightSorted,
        None
    }

    public enum UomType
    {
        Month,
        Week,
        Day,
        Shift,
        Hour,
        Minute,
        Second,
        None
    }

    public enum LotCreateType
    {
        
        Normal,   // InTarget, MBS Lot  
        Creation, // Lot 최초 생성
        Assembly, // 조립된 Lot       
        SplitByBom, // Bom에 의해 Split된 Lot
        SplitByCapacity, //Capa에 의해 Split된 Lot
        Retrying,   // 재시도하는 Lot
        StageOut, // Stage Out으로부터 생성된 Wip
        Incoming,  // Stage Out되면서 생성된 Lot
        Wip        // 재공으로 생성된 Lot
    }

    public enum PropertyCategory
    {
        Customer,
        SalesOrder,
        Buffer,
        Item,
        ItemSiteBuffer,
        Site,
        Bom,
        BomRouting,
        Wip,
        Resource,
        RoutingOperation,
        OperationResource,
        OperationAdditionalResource,
        None
    }

    public enum ArrangeType
    {
        Main,
        Additinal
    }

    public enum PegPosition
    {
        None,
        InPosition,
        SelectBom,
        StepOut,
        Run,
        Wait,
        StepIn,
        MovePrev
    }

    public enum ShortCategory
    {
        PERSIST,
        BOM,
        TAT,
        ETC,
        REF_PLAN
    }

    public enum RetryType
    {
        None,
        Retry,      // 바로 재수행.
        ReExecute   // 모든 Demand 완료 후 재수행.
    }

    public enum ShortType
    { 
        /// <summary>
        /// 현재 Phase 내에서 Short 난 정보
        /// </summary>
        InPhase,
        /// <summary>
        /// 다음 Phase 에서 수행될 Short 정보
        /// </summary>
        NextPhase,
        All
    }

    public enum BomPathType
    {
        /// <summary>
        /// B/W 진행 시 끊어질 수 있는 BOM
        /// </summary>
        D,
        /// <summary>
        /// B/W 진행 시 어느 경로로도 끊어지지 않는 BOM
        /// </summary>
        Y,
        /// <summary>
        /// B/W 진행이 불가능한 BOM
        /// </summary>
        N
    }

    public enum ErrorSeverity
    {
        Critical,
        Warning ,
        Info,
        None
    }

    public enum ErrorReasonCode
    {
        /// <summary>
        /// - 컬럼이 참조하는 기준정보 Master 정보가 없는 경우 (bom detail 에서 사용하는 bomid 가 bom master 에 없다거나 하는 유형)
        /// - 기본처리 : reference 가 되는 master 가 없는 정보임으로 내부적으로 사용불가, 데이터를 버리고 정보를 기록
        /// - 조합 key 로 처리되는 부분을 포함해야 함 (itemsitebuffer 와 같은 항목, item, site, buffer 각각 master 가 있어도 itemsitebuffer 에 등록이 안된 경우에 해당)
        /// </summary>
        NotFoundReferredData,

        /// <summary>
        /// - Null 허용하지 않는 데이터컬럼에 Null 값이 들어오는 경우
        /// - 기본처리 : 해당 컬럼에 Null 을 허용하지 않는 다는 의미는 컬럼이 중요 의미를 가지는 것임으로 해당 데이터는 버리고, 정보를 기록
        /// </summary>
        Null,

        /// <summary>
        /// - 예약어가 필요한 컬럼에 값이 상이한 값이 들어간 경우
        /// - 기본처리 : 해당데이터를 버림, 버린 데이터 정보를 알려줘야함
        /// </summary>
        MismatchReservedWord,

        /// <summary>
        /// - 동일한 Key 의 데이터가 중복입력된 경우
        /// - 기본처리 : 나중에 들어온 값을 버리는 방식으로 처리, 즉 검출된 시점에 버린 데이터를 알려줘야함
        /// </summary>
        DataDuplication,

        /// <summary>
        /// - 특정 입력 컬럼값이 지정된 범위를 벗어나는 경우, 예를 들어 음수를 허용하지 않는컬럼에 음수값이 들어왔다거나 하는 등
        /// - 기본처리 : 시스템 Default 값으로 처리하고 문제 데이터를 알려줘야함 (default 값으로 처리된 내용도 포함)
        /// </summary>
        OutOfRange,

        /// <summary>
        /// -. 컬럼 값에 허용하지 않는 정보가 들어온 경우
        /// -. 기본처리 : Type의 Default 값으로 처리하고 문제 데이터를 알려줘야합. Default 값으로 처리된 내용도 포함.
        /// </summary>
        DataTypeMisMatch,

        /// <summary>
        /// 모델구성, 엔진실행을 위한 기본 데이터가 전혀 입력되지 않은 경우 발생(RECORD COUNT = 0)
        /// - 기본처리 : 시스템 shutdown
        /// - 모델관련 주요 input : STAGE, ITEM_MASTER, ITEM_SITE_BUFFER_MASTER, SALES_ORDER, BOM_MASTER, BOM_DETAIL, BOM_ROUTING, ROUTING_MASTER,
        /// - 시나리오 관련 주요 Input : RULE_MASTER, RULESET_MASTER, FACTOR_MASTER, RULE_ACTION_MAP, EXECUTION_PLAN
        /// </summary>
        NotFoundKeyInput,

        /// <summary>
        /// - INPUT_ERROR_CONFIG 와 같은 기준정보 입력 테이블의 데이터 중 내부 데이터명칭 등과 맞지 않아 로직 실행이 안되는 경우
        /// - 기본처리 : 해당 데이터 제외 및 메시지 기록
        /// </summary>
        IncompatibleConfig,

        /// <summary>
        ///  모듈 및 전체 엔진 실행의 주요 Input 이 없는 경우
        /// - 기본처리 : 메시지만 출력 (Warning) 하고 실행. 그러나 실행해도 결과가 없을 것임으로 해당 모듈의 실행을 종료하는 방법도 고려가능함
        /// - PBO 에서 PegPart 가 없는 경우, PBB 에서 PegPart 가 없는 경우, PBF 에서 Lot 이 없거나 OperTarget 이 하나도 없는 경우
        /// </summary>
        NotAvailableMainInput,

        /// <summary>
        /// - 엔진에 정의된 내용과 Input 데이터로 정의된 Rule 이 서로 상이한 경우, RuleFactor 가 사전 Rule 과 다른 형식이라던가 하는 등의 소스와 설정의 불일치 할 수 있는 것과 같이 Input data 로딩시점에는 확인이 불가능하고 엔진 실행시에만 확인할 수 있는 유형.
        /// - 기본처리 : 메시지만 출력하고 해당 데이터를 버림
        /// </summary>
        IncompatibleRule,

        /// <summary>
        /// 엔진에서 제공하지 않는 데이터 매칭
        /// </summary>
        MisMatchReferenceData,

        /// <summary>
        /// - 데이터 로딩이 안될정도의 Error는 아니지만, Info 레벨로 알릴 필요가 있는 경우.
        /// </summary>
        AutoCorrectionData,

        /// <summary>
        /// SO_ITEM 생산시 투입이 불가한 BomPath 구성을 가진 경우.
        /// </summary>
        InvalidBomPath,

        /// <summary>
        /// Reference Object가 존재하지만, 문제가 있는 경우.
        /// </summary>
        ChainError,

        /// <summary>
        /// 법률 위반. (에러 코드가 명확하지 않은 경우)
        /// </summary>
        LawViolation,

        /// <summary>
        /// 
        /// </summary>
        None
    }

    public enum DataType
    {
        Int,
        Double,
        DateTime,
        String,
        None
    }

    public enum SetupWay
    {
        DetailSetup,
        OneWaySetup,
        NormalSetup,
        MultiSetup
    }

    public enum PlanInfoType
    {
        Allocate,
        Setup,
        PM,
        Constraint,
        NonWorking
    }

    public enum CalcType
    { 
        Sum,
        Average
    }

    public enum KpiCategory
    {
        INPUT,
        PEG_RESULT,
        RELEASE_QTY,
        RESOURCE,
        RTF
    }

    public enum KpiIndex
    {
        TOTAL_DEMAND_QTY,
        TOTAL_DEMAND_COUNT,
        TOTAL_WIP_QTY,
        TOTAL_WIP_COUNT,
        PEG_QTY,
        PEG_COUNT,
        UNPEG_QTY,
        UNPEG_COUNT,
        STAGE_IN_QTY,
        DEMAND_QTY,
        TOTAL_RTF_QTY,
        ONTIME_RTF_QTY,
        ONTIME_RTF_RATE,
        LATENESS_RTF_QTY,
        LATENESS_RTF_RATE,
        SETUP_COUNT,
        TOTAL_SETUP_COUNT,
        ETC
    }

    public enum NonWorkingType
    { 
        OffTime,
        PM
    }

    public enum SetupProperty
    { 
        ITEM_ID,
        BOM_ID,
        ROUTING_ID,
        OPERATION_ID
    }

    public enum SelectType
    {
        Select,
        Retry,
        Short
    }

    public enum PegPartType
    { 
        None,
        Short,
        InTarget
    }
}
