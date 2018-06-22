using Lib.helper;
using System;
using System.Runtime.Serialization;

namespace Lib.rpc
{
    /// <summary>
    /// 操作返回值
    /// </summary>
    [Serializable]
    [DataContract]
    public class OperationResult<T>
    {
        [DataMember]
        public string ErrorCode { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }

        [DataMember]
        public bool Success { get; set; } = false;

        [DataMember]
        public Exception Ex { get; set; }

        [DataMember]
        public T Result { get; set; }

        /// <summary>
        /// 如果有异常就抛出
        /// </summary>
        public OperationResult<T> ThrowIfException()
        {
            if (this.Ex != null) { throw this.Ex; }
            if (ValidateHelper.IsPlumpString(this.ErrorMessage) || ValidateHelper.IsPlumpString(this.ErrorCode))
            {
                throw new Exception($"服务异常，msg：{this.ErrorMessage}，code：{this.ErrorCode}");
            }
            return this;
        }
    }

    /// <summary>
    /// 服务约束的实现
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceContractImplAttribute : Attribute
    {
        /// <summary>
        /// not started with '/'
        /// </summary>
        public virtual string RelativePath { get; set; }
    }
}
