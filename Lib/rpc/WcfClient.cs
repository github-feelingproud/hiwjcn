using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Lib.helper;
using Lib.extension;

namespace Lib.rpc
{
    [DataContract]
    public abstract class OperationResult
    {
        [DataMember]
        public string ErrorCode { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }

        [DataMember]
        public bool Success { get; set; } = false;

        [DataMember]
        public Exception Ex { get; set; }

        /// <summary>
        /// 如果有异常就抛出
        /// </summary>
        public void ThrowIfException()
        {
            if (this.Ex != null) { throw this.Ex; }
            if (ValidateHelper.IsPlumpString(this.ErrorMessage) || ValidateHelper.IsPlumpString(this.ErrorCode))
            {
                throw new Exception($"服务异常，msg：{this.ErrorMessage}，code：{this.ErrorCode}");
            }
        }
    }
    /// <summary>
    /// 操作返回值
    /// </summary>
    /// <typeparam name="T">T</typeparam>
    [DataContract]
    public class OperationResult<T> : OperationResult
    {
        [DataMember]
        public T Result { get; set; }
    }
    /// <summary>
    /// wcf请求client，直接使用using就可以正确关闭链接
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ServiceClient<T> : ClientBase<T>, IDisposable where T : class
    {
        /// <summary>
        /// 接口实例
        /// </summary>
        public T Instance
        {
            get { return Channel; }
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            try
            {
                this.Close();
            }
            catch
            {
                try
                {
                    this.Abort();
                }
                catch { }
            }
        }
    }

    /// <summary>
    /// 请求客户端扩展
    /// </summary>
    public static class ServiceClientExtension
    {
        /// <summary>
        /// 执行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="client"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static OperationResult<R> Invoke<T, R>(this ServiceClient<T> client, Func<T, R> func) where T : class
        {
            try
            {
                return new OperationResult<R>() { Result = func.Invoke(client.Instance), Success = true };
            }
            catch (Exception e)
            {
                e.AddErrorLog();
                return new OperationResult<R>() { Ex = e, ErrorMessage = e.Message };
            }
        }
        /// <summary>
        /// 同步变异步
        /// https://www.zhihu.com/question/56539006
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="client"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        //[Obsolete("不推荐使用，详情见:https://blogs.msdn.microsoft.com/pfxteam/2012/03/24/should-i-expose-asynchronous-wrappers-for-synchronous-methods/")]
        public static async Task<OperationResult<R>> InvokeAsync<T, R>(this ServiceClient<T> client, Func<T, R> func) where T : class
        {
            return await Task.Run(() => client.Invoke(func));
        }
        /// <summary>
        /// 异步执行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="client"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static async Task<OperationResult<R>> InvokeAsync<T, R>(this ServiceClient<T> client, Func<T, Task<R>> func) where T : class
        {
            try
            {
                var data = await func.Invoke(client.Instance);
                return new OperationResult<R>() { Result = data, Success = true };
            }
            catch (Exception e)
            {
                e.AddErrorLog();
                return await Task.FromResult(new OperationResult<R>() { Ex = e, ErrorMessage = e.Message });
            }
        }
    }

    /// <summary>
    /// 直接使用，不用using
    /// </summary>
    public static class ServiceHelper<T> where T : class
    {
        /// <summary>
        /// 同步执行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public static OperationResult<R> Invoke<R>(Func<T, R> func)
        {
            using (var client = new ServiceClient<T>())
            {
                return client.Invoke(func);
            }
        }
        /// <summary>
        /// 同步转异步执行
        /// https://www.zhihu.com/question/56539006
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        //[Obsolete("不推荐使用，详情见:https://blogs.msdn.microsoft.com/pfxteam/2012/03/24/should-i-expose-asynchronous-wrappers-for-synchronous-methods/")]
        public static async Task<OperationResult<R>> InvokeAsync<R>(Func<T, R> func)
        {
            using (var client = new ServiceClient<T>())
            {
                return await client.InvokeAsync(func);
            }
        }
        /// <summary>
        /// 异步执行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public static async Task<OperationResult<R>> InvokeAsync<R>(Func<T, Task<R>> func)
        {
            using (var client = new ServiceClient<T>())
            {
                return await client.InvokeAsync(func);
            }
        }
    }
}
