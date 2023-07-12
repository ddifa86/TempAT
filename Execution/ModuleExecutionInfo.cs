using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    [System.Diagnostics.DebuggerDisplayAttribute("{Key}/{StageID}")]
    public abstract class ModuleExecutionInfo
    {        
        #region Properties

        public string Key { get; private set; }

        public ModuleType ModuleType { get; private set; }        

        public ATStage Stage{get; private set;}

        public string StageID 
        { 
            get
            { 
                if(Stage == null)
                    return StringUtility.IdentityNull;

                return this.Stage.StageID;
            } 
        }

        public int Sequence { get; private set;}

        public string RefKey { get; private set;}

        public bool IsPBBModule { get { return this.ModuleType == ModuleType.PBB; } }

        public bool IsPBFModule { get { return this.ModuleType == ModuleType.PBF; } }

        public bool IsPBOModule { get { return this.ModuleType == ModuleType.PBO; } }

        public int PhaseCount { get; internal set; }

        public int CurPhase { get; set; }

        public int CurRetryCount { get; set; }

        internal Dictionary< int, ModuleExecutionOption> Options;

        public ATDataContext CustomInputs;

        public ATDataContext CustomResults;

        public List<ModuleExecutionInfo> RefModules { get; internal set; }

        public APEKpiSummary Kpi { get; internal set; }

        #endregion

        public ModuleExecutionInfo(string key, ModuleType moduleType, ATStage stage, int sequence, string refKey = null)
        {
            this.Key = key;
            this.ModuleType = moduleType;
            this.Stage = stage;
            this.Sequence = sequence;
            this.RefKey = refKey;


            this.Options = new Dictionary<int, ModuleExecutionOption>();
            this.RefModules = new List<ModuleExecutionInfo>();

            this.CustomResults = new ATDataContext();
            this.CustomInputs = new ATDataContext();
        }


        internal virtual void OnStarted()
        {
            Logger.MonitorInfo("Start Module : {0}", this.Key);
            Logger.MonitorInfo("Module Type : {0}", this.ModuleType.ToString());
            Logger.MonitorInfo("Ref Module : {0}", this.RefKey);
            Logger.MonitorInfo("Module Sequence : {0}", this.Sequence);

            ATOption.Instance.SetDefaultOptionValue();

           

            OnPrepareInput();

          
        }

        internal virtual void OnEnded()
        {
            

            OnPrepareOutput();

          
        }

        internal abstract void OnPrepareInput();

        internal abstract void OnPrepareOutput();

        internal virtual void OnDone()
        {
            
        }

        internal ModuleExecutionOption GetOption(int seq)
        {
            ModuleExecutionOption option;
            if (Options.TryGetValue(seq, out option) == false)
            {
                option = new ModuleExecutionOption();

                Options.Add(seq, option);
            }

            return option;
        }
    }
}
