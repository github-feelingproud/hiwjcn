using Lib.core;
using System;

namespace Lib.ioc
{
    /// <summary>
    /// ioc中一个接口的多个实现用name来区分
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IServiceWrapper<T> : IDisposable
    {
        string Name { get; }

        T Value { get; }
    }

    /// <summary>
    /// ioc中一个接口的多个实现用name来区分
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class LazyServiceWrapperBase<T> : IServiceWrapper<T>
    {
        protected readonly string _name;
        protected readonly Lazy_<T> _lazy;

        public LazyServiceWrapperBase(string name, Func<T> source)
        {
            this._name = name;
            this._lazy = new Lazy_<T>(source);
        }

        public string Name => this._name;

        public T Value => this._lazy.Value;

        public virtual void Dispose() { }
    }

    public abstract class ServiceWrapperBase<T> : IServiceWrapper<T>
    {
        protected readonly string _name;
        protected readonly T _value;

        public ServiceWrapperBase(string name, T source)
        {
            this._name = name;
            this._value = source;
        }

        public string Name => this._name;

        public T Value => this._value;

        public virtual void Dispose() { }
    }
}
