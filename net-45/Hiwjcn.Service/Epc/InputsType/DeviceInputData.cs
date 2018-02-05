using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPC.Service.InputsType
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

    public class CheckInputDataResult
    {
        public string ParamUID { get; set; }
        public string ParamName { get; set; }
        public bool StatusOk { get; set; }
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
