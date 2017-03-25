using System;
using System.Linq;
using System.Linq.Expressions;
using Hiwjcn.Core.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Model.User;

namespace Hiwjcn.Test
{
    [TestClass]
    public class UnitTest4
    {
        public class node
        {
            public node left { get; set; }

            public node right { get; set; }
        }

        public void FindNode(node parent, node left, node right)
        {
            if (left != null)
            {

                FindNode(left, left.left, left.right);
            }
            if (right != null)
            {
                FindNode(right, right.left, right.right);
            }
        }

        [TestMethod]
        public void TestMethod1()
        {
            Expression<Func<UserModel, bool>> ex = x =>
            x.PassWord.Length == 16
            && (x.Email.Contains("gmail.com") || x.Flag > 0)
            && x.NickName == "wj";

            var body = (BinaryExpression)ex.Body;

            var left = (ParameterExpression)body.Left;

            var node = new node();




        }
    }
}
