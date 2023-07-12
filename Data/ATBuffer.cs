using Mozart.Data.Entity;
using Mozart.SeePlan.Cbsim;
using Mozart.SeePlan.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Data
{
    [Mozart.Task.Execution.FEBaseClassAttribute(Root = "PKG", Category = "PKG", IsTypeBinding = true, Mandatory = true, Description = null)]
    public partial class ATBuffer : ATOperation, IConstraint
    {
        #region Property

        public string BufferID { get; private set; }

        public ATStage Stage { get; private set; }

        public List<ATConstraintInfo> ConstraintInfos { get; set; }

        public List<ATConstraint> Constraints { get; set; }

        public bool HasConstraint { get; set; }

        #endregion

        public ATBuffer(string bufferID, int sequence, ATStage stage)
            :base(bufferID, sequence, OperType.Buffer, 0, 0, 1.0d,0)
        {
            this.BufferID = bufferID;
            this.Stage = stage;

            this.IsRefPlanBuffer = false;

            this.ConstraintInfos = new List<ATConstraintInfo>();
            this.Constraints = new List<ATConstraint>();

            List<ATProperty> reservedProps = ATInputData.Properties.GetPropertyValueByCategory(PropertyCategory.Buffer.ToString());
            if(reservedProps != null)
                reservedProps.ForEach(x => this.SetProperty(x.PropertyID, x.DefaultValue));
        }

        public override void SetProperty(string propertyID, object value)
        {
            this.Property[propertyID] = value;
        }
    }
}
