using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.core
{
    public class Lazy_<T>
    {
        private readonly Func<T> _creator;

        public Lazy_(Func<T> _creator)
        {
            this._creator = _creator ?? throw new ArgumentNullException(nameof(_creator));
        }

        private readonly object _lock = new object();

        private T _value;
        private bool _valueCreated = false;

        public T Value
        {
            get
            {
                if (!this._valueCreated)
                {
                    lock (this._lock)
                    {
                        if (!this._valueCreated)
                        {
                            this._value = this._creator.Invoke();
                        }
                    }
                }

                return this._value;
            }
        }

        public bool ValueCreated => this._valueCreated;
    }
}
