using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.core
{
    /// <summary>
    /// 
    /// </summary>
    public class FieldInfoAttribute : Attribute
    {
        public FieldInfoAttribute()
        { }
    }
}

namespace System
{
    /// <summary>
    /// 只有obsolete属性可以生成一个编译器错误，其他的，比如notnull主要是给resharp等开发工具产生提示用的
    /// he only attribute that can cause the compiler to generate an error is the ObsoleteAttribute. It is because this attribute's behavior is hard-coded into the compiler.    
    /// Attributes like the NotNull attribute are generally meant for tools(like ReSharper) to generate warnings or errors while writing code.Please read about this particular attribute here.   
    /// You can also use tools like PostSharp to issue additional build-time errors.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    internal sealed class NotNullAttribute : Attribute
    {
        public NotNullAttribute() { }
    }
}