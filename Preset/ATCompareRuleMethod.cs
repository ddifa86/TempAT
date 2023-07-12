using Mozart.SeePlan.Aleatorik.Planner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik.Preset
{
    public class ATCompareRuleMethod
    {

        internal class FactorObjectComparer : IComparer<IFactorObject>
        {
            //private ATPeggingContext _context;
            object[] args;
            private ATWeightPreset _preset;

            public FactorObjectComparer(ATWeightPreset preset, params object[] args)
            {
                this.args = args;
                this._preset = preset;
            }
           

            public int Compare(IFactorObject x, IFactorObject y)
            {
                if (object.ReferenceEquals(x, y))
                    return 0;

                int cmp = 0;

                if (this._preset != null)
                {
                    foreach (ATWeightFactor factor in this._preset.FactorList)
                    {
                        var method = factor.Method;

                        if (method == null)
                            continue;

                        ATFactorValue xValue = null; 
                        if (x.FactorInfos.TryGetValue(factor.Name, out xValue) == false)
                        {
                            xValue = (ATFactorValue)method.DynamicInvoke(x, factor, args);
                            x.FactorInfos.Add(factor.Name, xValue);
                        }

                        ATFactorValue yValue = null; 
                        if (x.FactorInfos.TryGetValue(factor.Name, out yValue) == false)
                        {
                            yValue = (ATFactorValue)method.DynamicInvoke(y, factor, args);
                            y.FactorInfos.Add(factor.Name, yValue);
                        }

                        cmp = xValue.Value.CompareTo(yValue.Value);

                        if (cmp != 0)
                            return cmp;
                    }
                }

                cmp = x.FactorObjectKey.CompareTo(y.FactorObjectKey);

                return cmp;
            }
        }

        //public List<T> CompareRule<T>(IEnumerable<T> factorObject, ATWeightPreset preset, params object[] args) where T : IFactorObject
        //{
        //    factorObject.ForEach(x => x.FactorValues.Clear());


        //    factorObject.Sort(new FactorObjectComparer(preset, args));

        //    return factorObject;
        //}
         

        public List<T> FilterRule<T>(List<T> factorObject, ATWeightPreset preset, ref List<T> filterObject, params object[] args) where T : IFactorObject
        {
            try
            {
                factorObject.ForEach(x => x.FilterInfos.Clear());

                List<T> selectObjects = new List<T>();
                filterObject = new List<T>();

                if (preset == null)
                {
                    selectObjects.AddRange(factorObject);
                    return selectObjects;
                }

                foreach (var obj in factorObject)
                {
                    bool bFilter = false;
                    foreach (var factor in preset.FactorList)
                    {
                        Delegate method = factor.Method;

                        // Invoke사용하는 방안으로처리 ...? 속도 이슈가 없는지확인 필요.
                        var result =  (ATFilterValue)method.DynamicInvoke(obj, factor, args );

                        obj.FilterInfos.Add( factor.Name, result);

                        if (result.Value == true)
                        {
                            filterObject.Add(obj);
                            // filter사유 남기기.
                            bFilter = true;
                            break;
                        }
                    }

                    if (bFilter == false)
                        selectObjects.Add(obj);
                }

                return selectObjects;
            }
            finally
            {
            }
        }
    }
}
