using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Lib.helper;
using System.Net;
using System.Net.Http;
using System.Web.Compilation;

namespace Lib.rpc
{
    public class WebApiClient<T>
    {
        public WebApiClient()
        {
            //思路
            var tp = typeof(T);
            var cls = $"_cls_{Com.GetUUID()}";
            var code = new StringBuilder();
            code.AppendLine($"public class {cls}: {tp.Namespace}.{tp.Name}");
            code.AppendLine("{");
            foreach (var m in tp.GetMethods())
            {
                var return_type = this.ReturnType(m);

                var param_list = this.ParamList(m);

                var a = string.Join(",", param_list.Select(x => $"{x.ParameterType.FullName} {x.Name}"));

                code.AppendLine($"public virtual async System.Threading.Tasks.Task<{return_type.FullName}> {m.Name}({a})");
                code.AppendLine("{");
                code.AppendLine("var data=new System.Collections.Generic.Dictionary<string, object>();");
                foreach (var p in param_list)
                {
                    code.AppendLine($"data[\"{p.Name}\"]={p.Name};");
                }
                code.AppendLine($"return await Lib.rpc.webapihelper.Request<{return_type.FullName}>(data);");
                code.AppendLine("}");
                code.AppendLine();
            }
            code.AppendLine("}");

            using (var provider = new CSharpCodeProvider())
            {
                var options = new CompilerParameters();
                options.GenerateInMemory = true;
                var ass = AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic).Select(x => x.Location).ToArray();
                options.ReferencedAssemblies.AddRange(ass);

                var result = provider.CompileAssemblyFromSource(options, code.ToString());

                if (result.Errors.HasErrors)
                {
                    var str = new StringBuilder();
                    foreach (CompilerError eoor in result.Errors)
                    {
                        str.AppendFormat("{0} {1}", eoor.Line, eoor.ErrorText);
                    }
                    throw new Exception($"动态编译错误：{str.ToString()}");
                }

                var codeType = result.CompiledAssembly.GetType(cls);
                //codeType.InvokeMember("Run", BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, null, null, null);
                this.Instance = (T)Activator.CreateInstance(codeType);

            }
        }

        private Type ReturnType(MethodInfo m)
        {
            var t = m.ReturnType;
            if (!t.IsGenericType || t.BaseType != typeof(Task)) { throw new Exception("must be async function"); }
            var data_types = t.GetGenericArguments().ToList();
            if (data_types.Count != 1) { throw new Exception("data type"); }
            return data_types.First();
        }

        private List<ParameterInfo> ParamList(MethodInfo m)
        {
            var ps = m.GetParameters().ToList();
            return ps;
        }

        public static readonly HttpClient _client = new HttpClient();

        private object Request(MethodInfo m)
        {
            throw new NotImplementedException();
        }

        public T Instance { get; private set; }
    }
}
