using Hiwjcn.Core.Infrastructure.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data;
using Lib.infrastructure;
using Hiwjcn.Core.Domain.Auth;
using Lib.core;
using Lib.extension;
using Lib.helper;
using System.Data.Entity;
using Model.User;
using Lib.events;
using Lib.cache;
using System.Configuration;

namespace Hiwjcn.Bll.Auth
{
    /// <summary>
    /// 用来清理缓存等
    /// </summary>
    public class TokenEventHandler :
        IConsumer<EntityInserted<AuthToken>>,
        IConsumer<EntityDeleted<AuthToken>>,
        IConsumer<EntityUpdated<AuthToken>>
    {
        private readonly ICacheProvider _cache;
        public TokenEventHandler(ICacheProvider _cache)
        {
            this._cache = _cache;
        }

        private string CacheKey(AuthToken token) => $"auth.token:{token?.UID}";

        public void HandleEvent(EntityUpdated<AuthToken> eventMessage)
        {
            this._cache.Remove(CacheKey(eventMessage.Entity));
        }

        public void HandleEvent(EntityInserted<AuthToken> eventMessage)
        {
            this._cache.Remove(CacheKey(eventMessage.Entity));
        }

        public void HandleEvent(EntityDeleted<AuthToken> eventMessage)
        {
            this._cache.Remove(CacheKey(eventMessage.Entity));
        }
    }
}
