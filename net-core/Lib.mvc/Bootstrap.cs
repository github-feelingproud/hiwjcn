using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Lib.mvc
{
    public static class MvcBootstrap
    {
        public static IServiceProvider CurrentServiceProvider(this HttpContext context) =>
            context.RequestServices ?? throw new Exception("无法获取当前ioc scope");

        public static IServiceCollection AddHttpContextAccessor_(this IServiceCollection collection) =>
            collection.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    }
}
