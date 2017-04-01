using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lib.data;
using System.Linq.Expressions;
using Model.User;
using Lib.extension;

namespace Hiwjcn.Test
{
    [TestClass]
    public class UnitTest6
    {
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
