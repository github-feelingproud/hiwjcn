using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Lib.cache
{
    /// <summary>
    /// ÄÚ´æ»º´æ
    /// </summary>
    public class MemoryCacheProvider : CacheBase, ICacheProvider
    {
        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public CacheResult<T> Get<T>(string key)
        {
            throw new NotImplementedException();
        }

        public bool IsSet(string key)
        {
            throw new NotImplementedException();
        }

        public void Remove(string key)
        {
            throw new NotImplementedException();
        }

        public void RemoveByPattern(string pattern)
        {
            throw new NotImplementedException();
        }

        public void Set(string key, object data, TimeSpan expire)
        {
            throw new NotImplementedException();
        }
    }
}