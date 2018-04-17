using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reactive;
using System.Globalization;
using System.Linq;
using Lib.extension;
using Lib.helper;
using Lib.io;
using System.IO;
using System.Text;
using System.Xml;
using Lib.net;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;

namespace Hiwjcn.Test
{
    [TestClass]
    public class UnitTest9
    {
        [TestMethod]
        public void jlkjl()
        {
            AssemblyName assemblyName = new AssemblyName("assemblyName");
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("PersonModule", "Person.dll");
            TypeBuilder typeBuilder = moduleBuilder.DefineType("Person", TypeAttributes.Public);
            //*【不同点1】
            MethodBuilder sayHelloMethod = typeBuilder.DefineMethod("SayHello", MethodAttributes.Public, null, null);
            ILGenerator ilOfSayHello = sayHelloMethod.GetILGenerator();
            ilOfSayHello.Emit(OpCodes.Ldstr, "蝈蝈");
            ilOfSayHello.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }));
            ilOfSayHello.Emit(OpCodes.Call, typeof(Console).GetMethod("ReadLine"));
            //没有返回值所有加OpCodes.Pop
            ilOfSayHello.Emit(OpCodes.Pop);
            ilOfSayHello.Emit(OpCodes.Ret);
            Type personType = typeBuilder.CreateType();
            //assemblyBuilder.Save("Person.dll");
            object obj = Activator.CreateInstance(personType);
            MethodInfo methodInfo = personType.GetMethod("SayHello");
            //【不同点2】
            methodInfo.Invoke(obj, null);
        }

        [TestMethod]
        public void TestMfasdfasdfasdfasedfethod1()
        {
            var x = Lib.net.HttpClientHelper.Get("http://www.baidu.com/");
        }

        [TestMethod]
        public void TestMfasdfasdfasdfasdfethod1()
        {
            try
            {
                var dom = new XmlDocument();
                dom.Load("d:\\xml.txt");

                var node = dom.ChildNodes.AsEnumerable_<XmlNode>().Where(x => x.Name == "s:Envelope").FirstOrDefault().ChildNodes.AsEnumerable_<XmlNode>().Where(x => x.Name == "s:Body").FirstOrDefault();
                var p = node.FirstChild.FirstChild.ChildNodes.AsEnumerable_<XmlNode>().Select(x => new { x.Name, x.InnerText }).ToList();

            }
            catch (Exception e)
            {
                //
            }
        }

        [TestMethod]
        public void TestMethod1()
        {
            var culs = new string[] { "da-DK", "en-CA", "fr-CH", "de-DE", "he-IL", "ja-JP", "kok" };
            var now = DateTime.Now;
            var list = culs.Select(x => now.ToString(new CultureInfo(x).DateTimeFormat)).ToList();
        }

        [TestMethod]
        public void fasdfsd()
        {
            foreach (var b in Com.Range(65).Batch(10))
            {
                var str = ",".Join_(b);
            }
        }
    }
}
