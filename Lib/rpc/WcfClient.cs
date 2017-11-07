using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Lib.helper;
using Lib.extension;
using System.ServiceModel.Channels;
using System.Configuration;
using System.IO;
using System.Web;
using System.Reflection;
using Castle.DynamicProxy;
using System.ServiceModel.Description;

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
    /// wcf请求client，直接使用using就可以正确关闭链接
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ServiceClient<T> : ClientBase<T>, IDisposable where T : class
    {
        public ServiceClient()
        {
            //read config
        }

        public ServiceClient(Binding binding, EndpointAddress endpoint) : base(binding, endpoint)
        {
            //manual config
        }

        public ServiceClient(string url, bool http = true) :
            this(http ? new BasicHttpBinding()
            {
                MaxBufferSize = (ConfigurationManager.AppSettings["WCF.MaxBufferSize"] ?? "2147483647").ToInt(null),
                MaxReceivedMessageSize = (ConfigurationManager.AppSettings["WCF.MaxBufferSize"] ?? "2147483647").ToInt(null),
                ReceiveTimeout = TimeSpan.FromSeconds((ConfigurationManager.AppSettings["WCF.ReceiveTimeoutSecond"] ?? "20").ToInt(null)),
                SendTimeout = TimeSpan.FromSeconds((ConfigurationManager.AppSettings["WCF.SendTimeoutSecond"] ?? "20").ToInt(null))
            } : throw new Exception("暂不支持"), new EndpointAddress(url))
        {
            //with default config
        }

        class LogProxy : IInterceptor
        {
            public void Intercept(IInvocation invocation)
            {
                invocation.Proceed();

                var valueType = invocation.ReturnValue;
            }
        }

        private T _ins = null;

        private readonly object _create_proxy_lock = new object();

        [Obsolete("无法代理channel，方法没有实现")]
        public bool UseProxy { get; set; } = false;

        /// <summary>
        /// 接口实例
        /// </summary>
        public T Instance
        {
            get
            {
                if (this.UseProxy)
                {
                    if (this._ins == null)
                    {
                        lock (this._create_proxy_lock)
                        {
                            if (this._ins == null)
                            {
                                var p = new ProxyGenerator();
                                var classToProxy = this.Channel.GetType();
                                var proxy_ins = (T)p.CreateClassProxy(classToProxy, new LogProxy());

                                this._ins = proxy_ins;
                            }
                        }
                    }
                    return this._ins;
                }
                else
                {
                    return this.Channel;
                }
            }
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this.SafeClose_();
        }
    }

    /// <summary>
    /// 请求客户端扩展
    /// </summary>
    public static class ServiceClientExtension
    {
        public static List<Type> FindContracts(this Assembly ass) =>
            ass.GetTypes().Where(x => x.IsInterface && x.IsAssignableTo_<IWcfContract>()).ToList();

        public static void FindSvc(this Assembly ass)
        {
            var tps = ass.GetTypes();
            var contracts = tps.Where(x => x.IsInterface && x.GetCustomAttributes<IsWcfContractAttribute>().Any()).ToList();

            var svcTypes = new List<Type>();
            foreach (var con in contracts)
            {
                var impls = tps.Where(x => x.IsNormalClass() && x.IsAssignableTo_(con)).ToList();
                if (impls.Count <= 0) { continue; }
                if (impls.Count > 1) { throw new Exception("一个wcf contracts只能有一个实现"); }
                svcTypes.Add(impls.First());
            }
            var names = svcTypes.Select(x => $"{x.Name.ToLower()}.svc");
            var path = HttpContext.Current.Server.MapPath("~/");
            //从磁盘上找到svc文件
            Com.FindFiles(path, f =>
            {
                if (names.Contains(f.Name.ToLower()))
                {
                    //parse path
                }
            });
        }

        public static void SafeClose_<T>(this ClientBase<T> client) where T : class
        {
            try
            {
                client.Close();
            }
            catch (Exception e)
            {
                e.AddErrorLog();
                try
                {
                    client.Abort();
                }
                catch (Exception err)
                {
                    err.AddErrorLog();
                }
            }
        }

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
        /// 同步转异步执行
        /// https://www.zhihu.com/question/56539006
        /// https://blogs.msdn.microsoft.com/pfxteam/2012/03/24/should-i-expose-asynchronous-wrappers-for-synchronous-methods/
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        [Obsolete("使用Task.Run。调用时请确保是IO密集型操作，而不是计算密集型！具体参见此方法备注中的URL")]
        public static async Task<OperationResult<R>> InvokeSyncAsAsync<T, R>(this ServiceClient<T> client, Func<T, R> func) where T : class
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
        /// https://blogs.msdn.microsoft.com/pfxteam/2012/03/24/should-i-expose-asynchronous-wrappers-for-synchronous-methods/
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        [Obsolete("使用Task.Run。调用时请确保是IO密集型操作，而不是计算密集型！具体参见此方法备注中的URL")]
        public static async Task<OperationResult<R>> InvokeSyncAsAsync<R>(Func<T, R> func)
        {
            using (var client = new ServiceClient<T>())
            {
                return await client.InvokeSyncAsAsync(func);
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

    public interface IWcfContract { }

    public class IsWcfContractAttribute : Attribute
    {
        /// <summary>
        /// not started with '/'
        /// </summary>
        public string RelativePath { get; set; }
    }
}
