using Mozart.Simulation.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public class ATSetupInfo
    {
        public string SetupID { get; set; }

        public List<ATSetupDetail> SetupInfos { get; set; }

        public HashSet<string> Properties { get; set; }

        public ATSetupInfo(string setupID)
        {
            this.SetupID = setupID;
            this.SetupInfos = new List<ATSetupDetail>();
            this.Properties = new HashSet<string>();
        }
    }

    public class ATSetupDetail
    {
        public string SetupID { get; internal set; }

        public string FromCondition { get; internal set; }

        public string ToCondition { get; internal set; }

        public Time SetupTime { get; internal set; }

        public int Priority { get; internal set; }

        public string SetupType { get; internal set; }

        public SetupWay SetupWay { get; internal set; }

        public List<ATResource> SetupResources { get; internal set; }

        public bool HasSetupResource
        { 
            get
            {
                return this.SetupResources.Count > 0;
            }
        }

        public ATSetupDetail(string setupID, string fromCondition, string toCondition, double setupTime, string setupType, int priority, List<ATResource> setupResources, bool isMultiSetup)
        {
            this.SetupID = setupID;
            this.FromCondition = fromCondition;
            this.ToCondition = toCondition;
            this.SetupTime = ATUtil.UomToTime(setupTime, ATOption.Instance.TimeUOM);
            this.SetupType = setupType;
            this.Priority = priority;
            this.SetupResources = setupResources;

            if (isMultiSetup)
                this.SetupWay = SetupWay.MultiSetup;
            else if (string.IsNullOrEmpty(fromCondition) == false && string.IsNullOrEmpty(toCondition) == false)
                this.SetupWay = SetupWay.DetailSetup;
            else if (string.IsNullOrEmpty(fromCondition) && string.IsNullOrEmpty(toCondition))
                this.SetupWay = SetupWay.NormalSetup;
            else
                this.SetupWay = SetupWay.OneWaySetup;
        }

        public bool IsNeedSetup(Dictionary<string, string> lastKeyInfo, Dictionary<string, string> currentKeyInfo)
        {
            if (this.SetupWay == SetupWay.MultiSetup)
            {
                var types = this.SetupType.Split('|');
                var fromConditions = this.FromCondition.Split('|');
                var toConditions = this.ToCondition.Split('|');

                for (int i = 0; i < types.Count(); i++)
                {
                    var type = types[i];
                    var subTypes = type.Split('&');
                    string lastKey = null;
                    string currentKey = null;

                    foreach (var subType in subTypes)
                    {
                        if (string.IsNullOrEmpty(lastKey))
                            lastKey = lastKeyInfo[subType];
                        else
                            lastKey += "&" + lastKeyInfo[subType];

                        if (string.IsNullOrEmpty(currentKey))
                            currentKey = currentKeyInfo[subType];
                        else
                            currentKey += "&" + currentKeyInfo[subType];
                    }

                    if (fromConditions[i] == lastKey && toConditions[i] == currentKey)
                        return true;
                }
            }
            else
            {
                string lastKey = lastKeyInfo[SetupType];
                string currentKey = currentKeyInfo[SetupType];

                switch (this.SetupWay)
                {
                    case SetupWay.DetailSetup:
                        if (lastKey == FromCondition && currentKey == ToCondition)
                            return true;

                        break;
                    case SetupWay.OneWaySetup:
                        if (lastKey == FromCondition || currentKey == ToCondition)
                            return true;

                        break;
                    case SetupWay.NormalSetup:
                        if (lastKey != currentKey)
                            return true;

                        break;
                    default:
                        return false;
                }
            }

            return false;
        }
    }
}
