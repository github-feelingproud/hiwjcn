using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.core
{
    /// <summary>
    /// usages report,not prepared
    /// </summary>
    internal class LibStartUpHelper
    {
        private static Task t = null;

        static LibStartUpHelper()
        {
            //t = Task.Run(() => { });
        }

        public static void Dispose()
        {
            t?.Dispose();
        }
    }
}
