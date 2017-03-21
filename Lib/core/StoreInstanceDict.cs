using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.core
{
    /// <summary>
    /// 存储实例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class StoreInstanceDict<T> : Dictionary<string, T>
    {
        public readonly object _locker = new object();
    }
}
