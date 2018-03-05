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
                code.AppendLine($"public virtual async {this.ReturnType(m)} {m.Name}({this.ParamList(m)})");
                code.AppendLine("{");
                code.AppendLine($"return ({this.ReturnType(m)}){this.Request(m)};");
                code.AppendLine("}");
                code.AppendLine();
            }
            code.AppendLine("}");

            using (CSharpCodeProvider provider = new CSharpCodeProvider())
            {
                var options = new CompilerParameters();
                options.GenerateInMemory = true;

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

                Type codeType = result.CompiledAssembly.GetType(cls);
                //codeType.InvokeMember("Run", BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, null, null, null);
                this.Instance = (T)Activator.CreateInstance(codeType);

            }
        }

        private string ReturnType(MethodInfo m)
        {
            throw new NotImplementedException();
        }

        private string ParamList(MethodInfo m)
        {
            throw new NotImplementedException();
        }

        public static readonly HttpClient _client = new HttpClient();

        private object Request(MethodInfo m)
        {
            throw new NotImplementedException();
        }

        public T Instance { get; private set; }
    }
}
