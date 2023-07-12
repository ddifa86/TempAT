using DryIoc;
using Mozart.Extensions;
using Mozart.SeePlan.Aleatorik.Data;

using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    [FEComponent(FECategory.Execution, FEControl.ExecutionModule, Root = FEProvider.Aleatorik)]
    public class ATExecutionControl : IModelController
    {
        public static ATExecutionControl Instance
        {
            get { return ServiceLocator.Resolve<ATExecutionControl>(); }
        }

        #region IModelController 멤버
        Type IModelController.ControllerType
        {
            get { return typeof(ATExecutionControl); }
        }
        #endregion

 
 
    }
}
