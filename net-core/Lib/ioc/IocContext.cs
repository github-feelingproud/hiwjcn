using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Lib.ioc
{
    public class IocContext : IDisposable
    {
        public static readonly IocContext Instance = new IocContext();

        private IServiceProvider _root { get; set; }

        public IocContext SetRootContainer(IServiceProvider root)
        {
            this._root = root;
            return this;
        }

        public bool Inited => this._root != null;

        public IServiceProvider Container => this._root ?? throw new Exception("没有设置依赖注入容器");

        public IServiceScope Scope() => this.Container.CreateScope();

        public bool IsRegistered<T>()
        {
            using (var s = this.Scope())
            {
                return s.ServiceProvider.GetServices<T>().Any();
            }
        }

        public void Dispose() { }
    }
}
