using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.core
{
    public class Lazy_<T> : IDisposable
    {
        private readonly object _lock = new object();
        private readonly Func<T> _creator;

        private RefAction<T> _disposer;
        private T _value;
        private bool _created = false;

        public bool IsValueCreated => this._created;

        public Lazy_(Func<T> _creator)
        {
            this._creator = _creator ?? throw new ArgumentNullException(nameof(_creator));
            this._created = false;
        }

        public Lazy_<T> WhenDispose(RefAction<T> _disposer)
        {
            this._disposer = _disposer ?? throw new ArgumentNullException(nameof(_disposer));
            return this;
        }

        public T Value
        {
            get
            {
                if (!this._created)
                {
                    lock (this._lock)
                    {
                        if (!this._created)
                        {
                            this._value = this._creator.Invoke();
                            this._created = true;
                        }
                    }
                }

                return this._value;
            }
        }

        public void Dispose()
        {
            if (this.IsValueCreated)
            {
                this._disposer?.Invoke(ref this._value);
            }
            this._value = default(T);
            this._created = false;
        }
    }
}
