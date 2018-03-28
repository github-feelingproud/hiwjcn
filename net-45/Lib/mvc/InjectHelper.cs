using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Reflection;
using Lib.core;
using Lib.helper;

namespace Lib.mvc.test.inject
{
    /// <summary>
    /// 注入
    /// classname的格式：（namespace.classname,assemblyname）
    /// </summary>
    public class InjectAttribute : System.Attribute
    {
        public string ClassName { get; set; }
    }

    public class LoadNavigationBeforeActionExecutingAttribute : RunBeforeAttribute
    {
        public override void Run(System.Web.Mvc.Controller controller)
        {
            //dosomething
        }
    }

    public abstract class RunBeforeAttribute : System.Attribute
    {
        public abstract void Run(Controller controller);
    }

    /// <summary>
    /// 注入框架
    /// </summary>
    public class InjectHelper
    {
        private InjectHelper() { }

        private static readonly Dictionary<string, Assembly> AssemblyCache = new Dictionary<string, Assembly>();

        /// <summary>
        /// 查找缓存中的程序集
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static Assembly GetCachedAssembly(string name)
        {
            //查找缓存
            if (AssemblyCache.Keys.Contains(name))
            {
                return AssemblyCache[name];
            }
            Assembly ass = Assembly.Load(name);
            if (ass != null)
            {
                //添加缓存
                AssemblyCache[name] = ass;
            }
            return ass;
        }

        /// <summary>
        /// 注入对象，现在还不知道怎么实现方法的动态代理，学习中。。。
        /// </summary>
        /// <param name="obj"></param>
        public static void InjectController(object obj)
        {
            Type t = obj.GetType();
            List<PropertyInfo> list = ReflectionHelper.GetPropertyInfoList(t);
            if (!ValidateHelper.IsPlumpList(list))
            {
                return;
            }
            list.ForEach(pro =>
            {
                //查找注入标记的attribute
                List<InjectAttribute> attrs = ReflectionHelper.GetAttributes<InjectAttribute>(pro);
                if (!ValidateHelper.IsPlumpList(attrs)) { return; }
                InjectAttribute attr = attrs[0];
                if (attr == null) { return; }
                //切割得到类名和程序集
                string[] sp = ConvertHelper.GetString(attr.ClassName).Split(',').Select(x => x.Trim()).ToArray();
                if (sp.Length != 2) { return; }
                //查找缓存中的程序集
                Assembly ass = GetCachedAssembly(sp[1]);
                if (ass == null) { return; }
                //生成实例
                object value = ass.CreateInstance(sp[0]);
                //设置值
                pro.SetValue(obj, value, null);
            });
        }
    }

    #region 代理模式
    interface A
    {
        void Run(string arg);
    }

    class Aimp : A
    {
        public void Run(string arg)
        {
            //dosomething
        }
    }

    class Aproxy : A
    {
        private A Target { get; set; }

        public Aproxy(A t)
        {
            Target = t;
        }

        public void Run(string arg)
        {
            //dosomething before
            Target.Run(arg);
            //dosomething after
        }
    }

    class App
    {
        public App()
        {
            A bll = new Aimp();
            A proxy = new Aproxy(bll);

            //xxxxxxx
            proxy.Run("fasd");
        }
    }
    #endregion

}
