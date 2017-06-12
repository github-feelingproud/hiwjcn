using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lib.data;
using System.Linq.Expressions;
using Model.User;
using Lib.extension;
using StackExchange.Redis;
using System.Linq;
using Lib.helper;
using System.Data;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Hiwjcn.Test
{
    [TestClass]
    public class UnitTest6
    {
        [TestMethod]
        public void Jsontest()
        {
            try
            {
                var same = JsonHelper.HasSameStructure(new
                {
                    a = "",
                    b = new object[]
                    {
                        new { arr_1=1 },
                        new { arr_2="2"},
                        new { arr_3=new { this_is_final_level="最终" } }
                    },
                    c = new
                    {
                        x = "",
                        c = "",
                        d = new
                        {
                            cc = 1,
                            dk = ""
                        }
                    }
                }.ToJson(), new
                {
                    a = "",
                    b = "",
                    c = new
                    {
                        x = "",
                        c = "",
                        d = new
                        {
                            cc = 1,
                            dk = ""
                        }
                    }
                }.ToJson());
            }
            catch (Exception e)
            {
                //
            }
        }

        [TestMethod]
        public void permission()
        {
            var per = (int)(PermissionExample.协助管理员 | PermissionExample.终极管理员 | PermissionExample.普通用户);
            var valid = PermissionHelper.HasPermission(per, (int)PermissionExample.普通用户);
            valid = PermissionHelper.HasPermission(per, (int)PermissionExample.开店);
            PermissionHelper.AddPermission(ref per, (int)PermissionExample.开店);
            valid = PermissionHelper.HasPermission(per, (int)PermissionExample.开店);
            PermissionHelper.RemovePermission(ref per, (int)PermissionExample.终极管理员);
            valid = PermissionHelper.HasPermission(per, (int)PermissionExample.终极管理员);
        }

        class xxkkl : IDBTable
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public virtual int id { get; set; }
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
            public virtual string guid { get; set; }

            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public virtual string school_name { get; set; }
            public virtual int age { get; set; }
            [Column("fake_name")]
            public virtual string name { get; set; }

            [NotMapped]
            public virtual xxkkl father { get; set; }
        }

        [TestMethod]
        public void jklajlkjhsfasdfasf()
        {
            var model = new UserModel() { };
            var sql = model.GetInsertSql();
            sql = model.GetUpdateSql();

            var xx = new xxkkl();
            sql = xx.GetInsertSql();
            sql = xx.GetUpdateSql();
        }

        [TestMethod]
        public void nishishabi()
        {
            if (int.TryParse("lala", out var res))
            {
                Console.WriteLine(res.GetType().ToString());
            }
            Console.WriteLine(res.GetType().ToString());
        }

        [TestMethod]
        public void fasdfajlkjkhfasdjhf()
        {
            using (var con = new SqlConnection("User Iout=20"))
            {
                con.OpenIfClosedWithRetry(2);

                try
                {
                    var count = con.ExecuteScalar("select count(1) from t_inquiry where provinceid=@id and quoteuid = @uid",
                       new
                       {
                           id = "31",
                           uid = default(string)
                       });
                }
                catch (Exception e)
                {
                    //
                }
            }
        }

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
