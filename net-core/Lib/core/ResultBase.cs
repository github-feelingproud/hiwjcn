using Lib.helper;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Lib.core
{
    public abstract class ResultBase<T>
    {
        public virtual string Msg { get; set; }
        public virtual T Data { get; set; }
    }
}

namespace System
{
    /// <summary>
    /// 接口公共返回值的缩写
    /// </summary>
    [Serializable]
    [DataContract]
    [JsonObject(MemberSerialization.OptOut)]
    public class _ : ResJson { }

    /// <summary>
    /// 接口公共返回值的缩写
    /// </summary>
    [Serializable]
    [DataContract]
    [JsonObject(MemberSerialization.OptOut)]
    public class _<T> : ResJson<T> { }

    /// <summary>
    /// 通用返回
    /// </summary>
    [Serializable]
    [DataContract]
    [JsonObject(MemberSerialization.OptOut)]
    public class ResJson : ResJson<object> { }

    /// <summary>
    /// json格式
    /// </summary>
    [Serializable]
    [DataContract]
    [JsonObject(MemberSerialization.OptOut)]
    public class ResJson<T> : ResultMsg<T>
    {
        [DataMember]
        [JsonIgnore]
        public virtual bool error
        {
            get => !this.success;
            set => this.success = !value;
        }

        [DataMember]
        [JsonIgnore]
        public virtual bool success
        {
            get => this.Success;
            set => this.Success = value;
        }

        [DataMember]
        [JsonIgnore]
        public virtual string msg
        {
            get { return this.ErrorMsg; }
            set { this.ErrorMsg = value; }
        }

        [DataMember]
        [JsonIgnore]
        public virtual T data
        {
            get { return this.Data; }
            set { this.Data = value; }
        }

        [DataMember]
        [JsonIgnore]
        public virtual string code
        {
            get { return this.ErrorCode; }
            set { this.ErrorCode = value; }
        }
    }

    [Serializable]
    [DataContract]
    [JsonObject(MemberSerialization.OptOut)]
    public class ResultMsg : ResultMsg<object> { }

    /// <summary>
    /// 汽配龙的model
    /// </summary>
    [Serializable]
    [DataContract]
    [JsonObject(MemberSerialization.OptOut)]
    public class ResultMsg<T>
    {
        public void ThrowIfNotSuccess()
        {
            if (!this.Success)
            {
                throw new Exception(this.ErrorMsg ?? $"{nameof(ResultMsg)}默认错误信息");
            }
        }

        public void SetSuccessData(T data)
        {
            this.Data = data;
            this.Success = true;
        }

        public void SetErrorMsg(string msg)
        {
            if (!ValidateHelper.IsPlumpString(msg)) { throw new Exception("错误信息不能为空"); }
            this.ErrorMsg = msg;
            this.Success = false;
        }

        [DataMember]
        public bool Success { get; set; } = false;

        [DataMember]
        public string ErrorCode { get; set; }

        [DataMember]
        public string ErrorMsg { get; set; }

        [DataMember]
        public T Data { get; set; }
    }

}
