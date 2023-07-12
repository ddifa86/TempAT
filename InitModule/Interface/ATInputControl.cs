using Mozart.Extensions;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mozart.SeePlan.Aleatorik.Data;

namespace Mozart.SeePlan.Aleatorik
{
    [FEComponent(FECategory.Execution, FEControl.InputMapper, Root = FEProvider.Aleatorik)]
    public class ATInputControl : IModelController
    {
        public static ATInputControl Instance
        {
            get { return ServiceLocator.Resolve<ATInputControl>(); }
        }

        #region IModelController 멤버
        Type IModelController.ControllerType
        {
            get { return typeof(ATInputControl); }
        }
        #endregion

        [FEAction]
        public virtual void Initialize(ATStage stage)
        {

        }

        [FEAction]
        public virtual void SetItemSiteBufferAttribute(ATItemSiteBuffer isb, ATBuffer firstBuffer)
        {

        }

        [FEAction]
        public virtual void Done(ATStage stage)
        {
#warning AltBom의 정보는 데이터를 생성해서 넣어야 하지 않을까..?
            ATInputData.ItemSiteBuffers.GenerationSplitByAltBom();
        }

        [FEAction]
        public virtual void SetCustomOption(ModuleExecutionOption info, object option)
        {

        }
    }
}
