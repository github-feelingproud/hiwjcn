using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lib.data;
using System.Linq.Expressions;
using Model.User;
using Lib.extension;
using StackExchange.Redis;

namespace Hiwjcn.Test
{
    [TestClass]
    public class UnitTest6
    {
        [TestMethod]
        public void redisconstr()
        {
            var option = new ConfigurationOptions()
            {
                Ssl = true,
                Proxy = Proxy.Twemproxy,
                AllowAdmin = true
            };

            var str = option.ToString();
        }

        [TestMethod]
        public void TestMethod1()
        {
            try
            {
                Expression<Func<UserModel, bool>> ex = x => x.NickName == "wj";

                var sql = ExpressionToSql.Convert(ex);
            }
            catch (Exception e)
            {
                e.AddErrorLog();
            }
        }
    }
}
