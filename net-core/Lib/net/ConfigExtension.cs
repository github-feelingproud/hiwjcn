using Lib.ioc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Lib.net
{
    public static class ConfigExtension
    {
        public static void UseHttpClient(this IServiceCollection collection, string name, Func<HttpClient> func) =>
            collection.AddSingleton<IServiceWrapper<HttpClient>>(new HttpClientWrapper(name, func));
    }

    public class HttpClientWrapper : LazyServiceWrapperBase<HttpClient>
    {
        public HttpClientWrapper(string name, Func<HttpClient> func) : base(name, func)
        {
            //
        }
    }
}
