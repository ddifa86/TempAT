using DryIoc;
using Mozart.Extensions;
using Mozart.SeePlan.DataModel;
using Mozart.SeePlan.Pegging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    public partial class APEPegPart : IFactorObject, IPropertyObject, ITargetGroup
    {
        #region 공통
        public ATMoMaster MoMaster { get; set; }

        public ATItemSiteBuffer CurrentItemSiteBuffer { get; set;}

        public ATBom NextBom { get; internal set; }
        
        public ATRoute NextRoute { get; internal set; }

        public ATBomDetail NextBomDetail { get; internal set; }

        public ATBom CurrentBom { get; internal set; }

        public ATRoute CurrentRoute { get; set; }

        public ATBomDetail CurrentBomDetail { get; set; }

        public Step Step { get; set; }

        public ATOperation CurrentOperation
        {
            get
            {
                return this.Step as ATOperation;
            }
            set
            {
                this.Step = value;
            }
        }

        public string CurrentItemSiteBufferID
        {
            get
            {
                return CurrentItemSiteBuffer.Key;
            }
        }

        public ATItem CurrentItem
        {
            get
            {
                return CurrentItemSiteBuffer.Item;
            }
        }

        public ATBuffer CurrentBuffer
        {
            get
            {
                return CurrentItemSiteBuffer.Buffer;
            }
        }

        public ATSite CurrentSite
        {
            get
            {
                return CurrentItemSiteBuffer.Site;
            }
        }

        public string CurrentSiteID
        {
            get
            {
                return CurrentItemSiteBuffer.SiteID;
            }
        }

        public List<ITarget> Targets { get; set; }

        public ITarget Sample
        {
            get 
            {
                return this.Targets.FirstOrDefault();
            }
        }

        public APETarget SampleTarget
        {
            get
            {
                return this.Sample as APETarget;
            }
        }
        #endregion

        private bool _hasRemainTarget = true;
        public bool HasRemainTarget
        {
            get
            {
                if(_hasRemainTarget)
                {
                    _hasRemainTarget = this.Targets.Sum(x => x.Qty) > 0;
                }

                return _hasRemainTarget;
            }
        }

        public string FactorObjectKey { get; set; }
      
        public string FactorValues { get; set; }

        public string FilterValues { get ; set ; }

        public PegPosition PegPosition { get; set; }

        public Status Status { get; set; }

        public PegPartType Type { get; set; }

        public bool IsRetryPegPart { get; set; }

        public APEPegPart()
        {
            this.Targets = new List<ITarget>();
            this.PegPosition = PegPosition.None;
            this.Type = PegPartType.None;
        }

        public APEPegPart(ATMoMaster mm, ATItemSiteBuffer itembuffer)
        {
            #region PegPart 상속 부분 처리
            this.MoMaster = mm;
            #endregion

            this.CurrentItemSiteBuffer = itembuffer;
            this.CurrentBom = ATBom.NULL;
            this.CurrentRoute = null;
            this.Status = Status.Normal;
            this.PegPosition = PegPosition.None;
            this.Type = PegPartType.None;

            this.Targets = new List<ITarget>();
            this.FactorInfos = new Dictionary<string, ATFactorValue>();
            this.FilterInfos = new Dictionary<string, ATFilterValue>();

            this.Property = new DynamicDictionary();
            this.CalendarInfo = new ATCalendarManager();
        }

        #region DataObject Interface
        public dynamic Property { get; internal set; }

        public ATCalendarManager CalendarInfo { get; internal set; }

        public void SetProperty(string propertyID, object value)
        {
            this.Property[propertyID] = value;
        }

        public void SetCalendar(string propertyID, ATCalendar calendar)
        {
            this.CalendarInfo.AddCalendar(calendar);
        }
        #endregion

        public override string ToString()
        {
            return this.CurrentItemSiteBuffer.Key + " => " + this.MoMaster.ItemSiteBuffer.Key ;
        }

        public virtual ITargetGroup Clone()
        {
            var clone = (APEPegPart)this.MemberwiseClone();

            var copied = this.CopyTargets(clone);
            clone.Targets = new List<ITarget>(copied.ToList());

            return clone;
        }

        public IEnumerable<APETarget> CopyTargets(APEPegPart pegPart)
        {
            foreach (APETarget pt in this.Targets)
            {
                yield return pt.Clone(pegPart);
            }
        }

        public string InitFilterLogs()
        {
            return this.FactorObjectKey;
        }

        public string InitFactorLogs()
        {
            return this.FactorObjectKey;
        }

        public void AddTargets(IEnumerable<ITarget> targets)
        {
            this.Targets.AddRange(targets);
        }

        public void RemoveTarget(ITarget target)
        {
            if (target != null)
                this.Targets.Remove(target);
        }

        public void Apply(Action<ITargetGroup, ITargetGroup> action)
        {
            if (action != null)
                action(this, null);
        }

        public List<ATBom> GetPrevBoms()
        {
            var boms = this.CurrentItemSiteBuffer.PrevBoms.Keys.ToList();

            return boms;
        }

        public void SetPegPosition(PegPosition position)
        {
            this.PegPosition = position;
        }
    }
}
