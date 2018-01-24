using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lib.extension;
using Lib.core;
using Lib.helper;
using Lib.ioc;
using Lib.data;
using org.apache.zookeeper;
using System.Threading.Tasks;
using Lib.mvc;
using static org.apache.zookeeper.ZooDefs;
using Lib.distributed;
using Hiwjcn.Core.Domain.User;
using Lib.infrastructure.entity.user;
using System.Reflection;

namespace Hiwjcn.Test
{
    [TestClass]
    public class UnitTest8
    {
        [TestMethod]
        public void TestMethod1()
        {
            var json = new _().ToJson();
        }

        [TestMethod]
        public void fasdsfahhfa()
        {
            var props = typeof(UserEntity).GetProperties().ToList();

            var data = props.Select(x => new
            {
                x,
                attrs = x.GetCustomAttributes(true)
            }).ToList();

            var data2 = props.Select(x => new
            {
                x,
                attrs = x.GetCustomAttributes(false)
            }).ToList();


            var data3 = props.Select(x => new
            {
                x,
                attrs = CustomAttributeExtensions.GetCustomAttributes(x)
            }).ToList();

            var data4 = props.Select(x => new
            {
                x,
                attrs = CustomAttributeExtensions.GetCustomAttributes(x, true)
            }).ToList();

            var data5 = props.Select(x => new
            {
                x,
                attrs = CustomAttributeExtensions.GetCustomAttributes(x, false)
            }).ToList();
        }
    }
}
