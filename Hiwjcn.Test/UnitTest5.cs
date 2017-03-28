using System;
using Lib.extension;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
