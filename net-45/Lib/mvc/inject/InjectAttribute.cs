using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lib.mvc.inject
{
    /// <summary>
    /// 注入
    /// classname的格式：（namespace.classname,assemblyname）
    /// </summary>
    public class InjectAttribute : System.Attribute
    {
        public string ClassName { get; set; }
    }
}
