using Mozart.SeePlan.Aleatorik.Data;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public sealed class RuleManager
    {
        private Assembly _ruleAssembly;

        //Test Merge
        private RuleManager()
        {
            SetRules();
        }

        private static readonly Lazy<RuleManager> _instance = new Lazy<RuleManager>(() => new RuleManager());

        public static RuleManager Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private Dictionary<Type, List<object>> _rulesByPoint = new Dictionary<Type, List<object>>();
        private Dictionary<Tuple<string, string>, Delegate> _rulesByFactor = new Dictionary<Tuple<string, string>, Delegate>();

        private Dictionary<string, Type> _rulePoint = new Dictionary<string, Type>();

        public void SetAssembly(Assembly assembly)
        {
            this._ruleAssembly = assembly;

            SetRules();
        }

        private void SetRules()
        {
            _rulesByPoint.Clear();
            _rulesByFactor.Clear();
            _rulePoint.Clear();

            var rulePointAsm = Assembly.GetAssembly(this.GetType());
            if (rulePointAsm != null)
            {
                Module[] ruleDefModules = rulePointAsm.GetModules();
                // [RulePoint] delegate list를 가져온다.
                _rulePoint = GetRulePoints(ruleDefModules[0]);
                RegisterPredefinedRuleFactor(ruleDefModules[0], _rulePoint);

                if (_ruleAssembly != null)
                {
                    // [RuleFactor] method를 Rule List에 등록한다.
                    Module[] ruleModules = _ruleAssembly.GetModules();
                    RegisterRuleFactor(ruleModules[0], _rulePoint);
                }
            }
        }

        internal void AddRuleSet(Assembly customAssembly)
        {
            if (customAssembly != null)
            {
                Module[] ruleDefModules = customAssembly.GetModules();

                // [RulePoint] delegate list를 가져온다.
                RegisterPredefinedRuleFactor(ruleDefModules[0], _rulePoint);

                Module[] ruleModules = customAssembly.GetModules();
                // [RuleFactor] method를 Rule List에 등록한다.
                RegisterRuleFactor(ruleModules[0], _rulePoint);

                string path = ModelContext.Current.TaskContext.Get("#private-path").ToString();

                var rulePath = Path.Combine(path, "CustomRulesV2.dll");

                if (File.Exists(rulePath))
                {
                    var file = Assembly.LoadFile(rulePath);
                    if (file != null)
                    {
                        Module[] customRuleModules = file.GetModules();
                        RegisterRuleFactor(customRuleModules[0], _rulePoint);
                    }
                }
            }
        }

        private Dictionary<string, Type> GetRulePoints(Module module)
        {
            Dictionary<string, Type> rulePointss = new Dictionary<string, Type>();

            foreach (Type t in module.GetTypes())
            {
                foreach (var obj in t.GetCustomAttributes(true))
                {
                    // [RulePoint] Attribute가 붙은 delegate만 탐색
                    if (obj.GetType() == typeof(RulePointAttribute))
                    {
                        var point = (RulePointAttribute)obj;
                        rulePointss.Add(point.Name, t);
                    }
                }
            }

            return rulePointss;
        }

        /// <summary>
        /// 엔진 내 선언된 RulePoint 정보 등록.
        /// </summary>
        /// <param name="module"></param>
        /// <param name="rulePoints"></param>
        private void RegisterPredefinedRuleFactor(Module module, Dictionary<string, Type> rulePoints)
        {
            foreach (Type t in module.GetTypes())
            {
                foreach (MethodInfo minfo in t.GetMethods())
                {
                    foreach (var obj in minfo.GetCustomAttributes(true))
                    {
                        // [RuleFactor] Attribute가 붙은 함수만 검색
                        if (obj.GetType() == typeof(RuleFactorAttribute))
                        {
                            var factor = obj as RuleFactorAttribute;
                            if (factor.RulePoint != null)
                            {
                                var point = rulePoints.Where(r => r.Value.Equals(factor.RulePoint)).Select(r => new { r.Key, r.Value }).FirstOrDefault();
                                if (point != null)
                                {
                                    // Rule Factor 저장
                                    if (!_rulesByPoint.TryGetValue(point.Value, out List<object> rules))
                                    {
                                        rules = new List<object>();
                                        _rulesByPoint.Add(point.Value, rules);
                                    }

                                    var tuple = Tuple.Create(point.Key, minfo.Name);
                                    if (_rulesByFactor.ContainsKey(tuple) == false)
                                    {
                                        var func = Delegate.CreateDelegate(point.Value, null, minfo);
                                        rules.Add(func);
                                        _rulesByFactor.Add(tuple, func);
                                    }
                                    else
                                    {
                                        Inputs.FACTOR_MASTER entity = new Inputs.FACTOR_MASTER();
                                        entity.RULE_POINT = tuple.Item1;
                                        entity.FACTOR_ID = tuple.Item2;

                                        OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Critical, ErrorReasonCode.IncompatibleRule, entity,
                                                ErrKey.FactorMaster, "", string.Format("Factor Duplication predefined (Rule Point : {0}, Factor : {1}, Factor Type : {2})", tuple.Item1, tuple.Item2, "Predefined"));
                                    }
                                    
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  구현되어있는 Factor 함수를 RulePoint 별로 등록.
        /// </summary>
        /// <param name="module"></param>
        /// <param name="rulePoints"></param>
        private void RegisterRuleFactor(Module module, Dictionary<string, Type> rulePoints)
        {
            if (module.ScopeName.Contains("CustomRule") == false)
                return;

            foreach (Type t in module.GetTypes())
            {
                if (t.CustomAttributes == null || t.CustomAttributes.Count() == 0)
                    continue;

                var ruleInfo = t.GetCustomAttribute(typeof(RuleInfo));
                if (ruleInfo == null)
                    continue;
                    
                if ((ruleInfo as RuleInfo).ProjectID != "" && (ruleInfo as RuleInfo).ProjectID != AleatorikGlobalParameters.Instance.ProjectID)
                    continue;

                //var rulePoint = rulePoints.Where(r => r.Value.Name == (ruleInfo as RuleInfo).RulePoint).Select(r => new { r.Key, r.Value }).FirstOrDefault();
                // var rulePoint = rulePoints.FirstOrDefault();

                foreach (MethodInfo minfo in t.GetMethods())
                {
                    foreach (var obj in minfo.GetCustomAttributes(true))
                    {
                        // [RuleFactor] Attribute가 붙은 함수만 검색
                        if (obj.GetType() == typeof(RuleFactorAttribute))
                        {

                            var rulePoint = rulePoints.Where(r => r.Value.Name == (obj as RuleFactorAttribute).RulePoint.Name).Select(r => new { r.Key, r.Value }).FirstOrDefault();
                            // Rule Factor 저장
                            if (!_rulesByPoint.TryGetValue(rulePoint.Value, out List<object> rules))
                            {
                                rules = new List<object>();
                                _rulesByPoint.Add(rulePoint.Value, rules);
                            }

                            var tuple = Tuple.Create(rulePoint.Key, minfo.Name);
                            if (_rulesByFactor.ContainsKey(tuple) == false)
                            {
                                var func = Delegate.CreateDelegate(rulePoint.Value, null, minfo);
                                rules.Add(func);
                                _rulesByFactor.Add(tuple, func);
                            }
                            else
                            {
                                Inputs.FACTOR_MASTER entity = new Inputs.FACTOR_MASTER();
                                entity.RULE_POINT = tuple.Item1;
                                entity.FACTOR_ID = tuple.Item2;

                                OutputWriter.Instance.WriteErrorLog(ModuleType.None, "", ErrorSeverity.Critical, ErrorReasonCode.IncompatibleRule, entity,
                                    ErrKey.FactorMaster, "", string.Format("Factor Duplication predefined (Rule Point : {0}, Factor : {1}, Factor Type : {2})", tuple.Item1, tuple.Item2, "Custom"));
                            }

                            continue;
                        }
                    }
                }



                //// 정의된 Rule Factor의 delegate가 있으면
                //if (rulePoint != null)
                //{
                   
                //}
            }
        }

        private Tuple<string, Type> GetRulePoint(Dictionary<string, Type> rulePoints, Type ruleFactor)
        {
            var rulePoint = rulePoints.Where(r => r.Value.Name == ruleFactor.Name).Select(r => new { r.Key, r.Value }).FirstOrDefault();
            if (rulePoint != null)
            {
                return Tuple.Create(rulePoint.Key, rulePoint.Value);
            }
            else
            {
                foreach (MethodInfo minfo in ruleFactor.GetMethods())
                {
                    foreach (var obj in minfo.GetCustomAttributes(true))
                    {
                        if (obj.GetType() == typeof(RuleFactorAttribute))
                        {
                            var factor = obj as RuleFactorAttribute;
                            if (factor.RulePoint != null)
                            {
                                var point = rulePoints.Where(r => r.Value.Equals(factor.RulePoint)).Select(r => new { r.Key, r.Value }).FirstOrDefault();
                                if (point != null)
                                {
                                    return Tuple.Create(point.Key, point.Value);
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        public Dictionary<string, RulePointType> GetMethods<RulePointType>() where RulePointType : Delegate
        {
            if (_rulesByPoint.TryGetValue(typeof(RulePointType), out List<object> value))
            {
                IEnumerable<RulePointType> rules = value.Select(r => (RulePointType)r);
                return rules.ToDictionary(x => x.Method.Name);
            }
            return new Dictionary<string, RulePointType>();
        }

        public Delegate GetMethod(string rulePoint, string ruleFactor)
        {
            if (_rulesByFactor.TryGetValue(Tuple.Create(rulePoint, ruleFactor), out Delegate func))
            {
                return func;
            }
            return null;
        }
    }
}
