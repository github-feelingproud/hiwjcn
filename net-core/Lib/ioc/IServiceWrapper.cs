using System;
using Lib.core;

namespace Lib.ioc
{
    /// <summary>
    /// ioc中一个接口的多个实现用name来区分
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IServiceWrapper<T>
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
        private readonly string _name;
        private readonly Lazy_<T> _lazy;

        public LazyServiceWrapperBase(string name, Func<T> source)
        {
            this._name = name;
            this._lazy = new Lazy_<T>(source);
        }

        public string Name => this._name;

        public T Value => this._lazy.Value;
    }

    public abstract class ServiceWrapperBase<T> : IServiceWrapper<T>
    {
        private readonly string _name;
        private readonly T _value;

        public ServiceWrapperBase(string name, T source)
        {
            this._name = name;
            this._value = source;
        }

        public string Name => this._name;

        public T Value => this._value;
    }
}
