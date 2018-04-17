using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiwjcn.Service.Epc.InputsType
{
    /// <summary>
    /// app提交的数据结构
    /// </summary>
    public class DeviceInputData
    {
        public class DataModel
        {
            public string ParamUID { get; set; }

            public string ValueJson { get; set; }
        }

        public string OrgUID { get; set; }

        public string UserUID { get; set; }

        public string DeviceUID { get; set; }

        public List<DataModel> Data { get; set; }
    }

    /// <summary>
    /// 点检返回数据
    /// </summary>
    public class CheckInputDataResult
    {
        /// <summary>
        /// 参数UID
        /// </summary>
        public string ParamUID { get; set; }

        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParamName { get; set; }

        /// <summary>
        /// 状态是否正常
        /// </summary>
        public bool StatusOk { get; set; }

        /// <summary>
        /// 异常提示
        /// </summary>
        public List<string> Tips { get; set; }
    }

    public abstract class ValueBase<T>
    {
        public T Value { get; set; }
    }

    public class BoolValue : ValueBase<bool> { }

    public class StringValue : ValueBase<string> { }

    public class SelectValue : ValueBase<string[]> { }

    public class NumberValue : ValueBase<double> { }
}
