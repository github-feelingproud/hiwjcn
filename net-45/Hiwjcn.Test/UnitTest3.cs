using Lib.extension;
using Lib.helper;
using Lib.infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Hiwjcn.Core.Domain.User;

namespace Hiwjcn.Test
{
    [TestClass]
    public class UnitTest3
    {

        [TestMethod]
        public void strhide()
        {
            var list = new int[10].Select(x => Com.GetUUID().HideForSecurity()).ToList();
        }

        [TestMethod]
        public void TestMethod1()
        {
            var userType = typeof(UserEntity);
            //false
            var a = userType.IsAssignableTo_(typeof(IServiceBase<>));
            //false
            var b = userType.IsGenericType_(typeof(ServiceBase<>));
            //true
            var c = userType.BaseType.IsGenericType_(typeof(ServiceBase<>));

            //{Name = "ServiceBase`1" FullName = "Lib.infrastructure.ServiceBase`1"}
            var d = userType.BaseType.GetGenericTypeDefinition();
            //{Name = "IServiceBase`1" FullName = "Lib.infrastructure.IServiceBase`1"}
            var e = typeof(IServiceBase<>);
            //{Name = "ServiceBase`1" FullName = "Lib.infrastructure.ServiceBase`1"}
            var f = typeof(ServiceBase<>);
            //{Name = "ServiceBase`1" FullName = "Lib.infrastructure.ServiceBase`1[[Model.User.UserModel, Hiwjcn.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"}
            var m = typeof(ServiceBase<UserEntity>);

            //true
            var g = userType.IsAssignableTo_<UserEntity>();
            //true
            var h = userType.IsAssignableTo_(typeof(UserEntity));
        }
    }
}
