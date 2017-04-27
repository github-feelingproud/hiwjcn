using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lib.data;
using System.Linq.Expressions;
using Model.User;
using Lib.extension;
using StackExchange.Redis;
using System.Linq;
using Lib.helper;

namespace Hiwjcn.Test
{
    [TestClass]
    public class UnitTest6
    {
        [TestMethod]
        public void jiami()
        {
            var str = "123";
            var s = SecureHelper.GetMD5(str);
            s = SecureHelper.GetSHA1(str);
            s = SecureHelper.GetSHA256(str);
            s = SecureHelper.GetHMACMD5(str, "1");
            s = SecureHelper.GetHMACSHA1(str, "1");
        }

        [TestMethod]
        public void time()
        {
            var now = DateTime.Now;

            now = now + TimeSpan.FromDays(5);

            now = now + TimeSpan.FromDays(-5);
        }

        public class a
        {
            public string name { get; set; }
            public int age { get { return 10; } }
        }

        public class b : a { }

        [TestMethod]
        public void map()
        {
            try
            {
                var obj = new a() { name = "wj" };
                var objb = obj.MapTo<b>();

                var mm = new b();
                MapEntity(ref mm, obj);
            }
            catch (Exception e)
            {
                //
            }
        }

        /// <summary>
        /// 原来的方法有问题
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="model"></param>
        /// <param name="notmap"></param>
        public static void MapEntity<T>(ref T entity, object model, string[] notmap = null)
        {
            if (model == null) { throw new Exception("对象为空"); }
            if (notmap == null) { notmap = new string[] { }; }

            var modelproperties = (model.GetType().GetProperties());

            var entityproperties = (entity.GetType().GetProperties());

            foreach (var pi in entityproperties)
            {
                if (notmap.Contains(pi.Name)) { continue; }

                //属性名和属性类型一样
                var modelpi = modelproperties
                    .Where(x => x.Name == pi.Name)
                    .Where(x => x.GetType().Name == pi.GetType().Name)
                    .FirstOrDefault();

                if (modelpi == null) { continue; }

                pi.SetValue(entity, modelpi.GetValue(model), null);
            }
        }

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
