using Mozart.SeePlan.Pegging;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik.Pegger
{
    public partial class BWPeggerPegControl : IModelController
    {

        #region IModelController 멤버
        Type IModelController.ControllerType
        {
            get { return typeof(BWPeggerPegControl); }
        }
        #endregion

    }
}
