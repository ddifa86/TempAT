//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Mozart.SeePlan.Aleatorik.Data;

//namespace Mozart.SeePlan.Aleatorik
//{
//    // dlrjteh durl 
//    public class OperTargetHelper
//    {
//        public static ATOperTarget GetOperTarget(string pathID, string curitemsitebuffer, string routeid, string operid, string isOut)
//        {
//            var target = ATDataModel.Instance.GetView<ATOperTarget>(ATReservedCode.OPERTARGET_KEY, pathID, curitemsitebuffer, routeid, operid, isOut);

//            target = target.Where(x => x.RemainQty > ATOption.Instance.MinimumAllocationQuantity);

//            return target.FirstOrDefault();
//        }
//    }
//}
