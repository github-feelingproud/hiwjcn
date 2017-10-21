using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reactive;
using System.Globalization;
using System.Linq;

namespace Hiwjcn.Test
{
    [TestClass]
    public class UnitTest9
    {
        [TestMethod]
        public void TestMethod1()
        {
            var culs = new string[] { "da-DK", "en-CA", "fr-CH", "de-DE", "he-IL", "ja-JP", "kok" };
            var now = DateTime.Now;
            var list = culs.Select(x => now.ToString(new CultureInfo(x).DateTimeFormat)).ToList();
        }
    }
}
