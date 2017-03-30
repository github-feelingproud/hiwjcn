using System;
using Lib.extension;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Castle.DynamicProxy;
using Hiwjcn.Bll;
using Castle.Core.Interceptor;

namespace Hiwjcn.Test
{
    public enum UserStatusEnum : int
    {
        正常用户 = 0,
        异常用户 = 8,
        禁用账户 = 9
    }

    [TestClass]
    public class UnitTest5
    {
        [TestMethod]
        public void TestMethod1()
        {
            var data = Enum.GetNames(typeof(UserStatusEnum));

            var items = UserStatusEnum.异常用户.GetItems<int>();
        }

        public class wj
        {
            public virtual void print()
            {
                var i = 0;

                var j = 0;
            }
        }

        public class inter : Castle.Core.Interceptor.IInterceptor
        {
            public void Intercept(IInvocation invocation)
            {
                var name = invocation.Method.Name;

                invocation.Proceed();

            }
        }

        [TestMethod]
        public void proxy()
        {
            var p = new ProxyGenerator();
            var px = p.CreateClassProxy<wj>(new inter());

            px.print();
        }
    }
}
