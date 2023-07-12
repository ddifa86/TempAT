using Mozart.Task.Execution;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public static class RuleBuilder
    {
        //public static Tuple<string, string, Assembly> Build(IEnumerable<RuleFactor> factors,
        //    Assembly rulePointAssembly, IEnumerable<string> addtionalAssemblies = null,
        //    IEnumerable<string> additionalNamespaces = null)
        //{
        //    string code = string.Empty;
        //    string error = string.Empty;
        //    Assembly assembly = null;

        //    Dictionary<string, Type> ruleDefs = GetRuleDefTypes(rulePointAssembly);

        //    //var ns = new CodeNamespace("Mozart.CustomRules");

        //    foreach (var type in ruleDefs)
        //    {
        //        var td = new CodeTypeDeclaration(type.Value.Name)
        //        {
        //            Attributes = MemberAttributes.Public,
        //        };
        //        MethodInfo mi = type.Value.GetMethod("Invoke");

        //        foreach (var factor in factors.Where(f => f.RulePoint == type.Key))
        //        {
        //            var method = new CodeMemberMethod();
        //            method.Name = factor.Name;
        //            method.Attributes = MemberAttributes.Public;
        //            method.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(typeof(RuleFactorAttribute))));
        //            method.ReturnType = new CodeTypeReference(mi.ReturnType);
        //            foreach (var pi in mi.GetParameters())
        //            {
        //                method.Parameters.Add(new CodeParameterDeclarationExpression(pi.ParameterType, pi.Name));
        //            }
        //            method.Statements.Add(new CodeSnippetStatement(factor.Expression));
        //            td.Members.Add(method);
        //        }

        //        ns.Types.Add(td);
        //    }

        //    CodeNamespace gn = new CodeNamespace();
        //    gn.Imports.Add(new CodeNamespaceImport("System"));
        //    gn.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
        //    gn.Imports.Add(new CodeNamespaceImport("System.Data"));
        //    gn.Imports.Add(new CodeNamespaceImport("System.IO"));
        //    gn.Imports.Add(new CodeNamespaceImport("System.Linq"));
        //    gn.Imports.Add(new CodeNamespaceImport("System.Text"));
        //    if (additionalNamespaces != null)
        //    {
        //        foreach (var ans in additionalNamespaces)
        //        {
        //            gn.Imports.Add(new CodeNamespaceImport(ans));
        //        }
        //    }

        //    var cu = new CodeCompileUnit();
        //    cu.Namespaces.Add(gn);
        //    cu.Namespaces.Add(ns);

        //    var provider = CodeDomProvider.CreateProvider("CSharp");
        //    var sb = new StringBuilder();
        //    using (var sourceWriter = new StringWriter(sb))
        //    {
        //        provider.GenerateCodeFromCompileUnit(cu, sourceWriter, new CodeGeneratorOptions());
        //    }
        //    code = sb.ToString();

        //    // 컴파일러 파라미터 옵션 지정
        //    CompilerParameters cparams = new CompilerParameters();
        //    cparams.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
        //    cparams.ReferencedAssemblies.Add("System.dll");
        //    cparams.ReferencedAssemblies.Add("System.Core.dll");
        //    cparams.ReferencedAssemblies.Add("System.Data.dll");
        //    cparams.ReferencedAssemblies.Add("System.Data.DataSetExtensions.dll");
        //    cparams.ReferencedAssemblies.Add("System.Xml.dll");
        //    cparams.ReferencedAssemblies.Add("System.Xml.Linq.dll");

        //    if (addtionalAssemblies != null)
        //    {
        //        foreach (var asm in addtionalAssemblies)
        //        {
        //            cparams.ReferencedAssemblies.Add(asm);
        //        }
        //    }
        //    cparams.GenerateInMemory = true;
        //    //cparams.OutputAssembly = "CustomRules.dll";

        //    // 소스코드를 컴파일해서 EXE 생성
        //    CompilerResults results = provider.CompileAssemblyFromSource(cparams, code);
        //    if (!results.Errors.HasErrors)
        //    {
        //        assembly = results.CompiledAssembly;
        //    }

        //    // 컴파일 에러 있는 경우 표시
        //    if (results.Errors.Count > 0)
        //    {
        //        var errSb = new StringBuilder();
        //        foreach (var err in results.Errors)
        //        {
        //            string factorID = GetFactorID(code, err);

        //            errSb.Append(err.ToString());
        //            Logger.MonitorInfo(string.Concat(factorID, " : ", err.ToString()));
        //        }
        //        error = errSb.ToString();
        //    }

        //    return Tuple.Create(code, error, assembly);
        //}

        //private static string GetFactorID(string code, object err)
        //{
        //    int line = ((CompilerError)err).Line;
        //    var codes = code.Split('\r');
        //    for (int i = line; i > 0; i--)
        //    {
        //        if (codes[i].Contains("[Mozart.SeePlan.Aleatorik.RuleFactorAttribute()]"))
        //        {
        //            string lineStr = code.Split('\r')[i + 1];
        //            string sub = lineStr.Substring(0, lineStr.IndexOf('('));
        //            string factor = sub.Substring(sub.LastIndexOf(' '));
        //            return factor.Trim();
        //        }
        //    }
        //    return null;
        //}

        private static Dictionary<string, Type> GetRuleDefTypes(Assembly asm)
        {
            Module[] ruleDefModules = asm.GetModules();

            Dictionary<string, Type> ruleDefs = new Dictionary<string, Type>();

            foreach (Type t in ruleDefModules[0].GetTypes())
            {
                foreach (var obj in t.GetCustomAttributes(true))
                {
                    if (obj.GetType() == typeof(RulePointAttribute))
                    {
                        var point = obj as RulePointAttribute;
                        try
                        {
                            ruleDefs.Add(point.Name, t);
                        }
                        catch
                        {

                        }
                    }
                }
            }

            return ruleDefs;
        }
    }
}
