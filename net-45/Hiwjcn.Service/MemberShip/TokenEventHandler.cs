using Hiwjcn.Core;
using Hiwjcn.Core.Domain.Auth;
using Lib.cache;
using Lib.events;

namespace Hiwjcn.Service.MemberShip
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

        public void HandleEvent(EntityUpdated<AuthToken> eventMessage)
        {
            this._cache.Remove(CacheKeyManager.AuthTokenKey(eventMessage.Entity?.UID));
        }

        public void HandleEvent(EntityInserted<AuthToken> eventMessage)
        {
            this._cache.Remove(CacheKeyManager.AuthTokenKey(eventMessage.Entity?.UID));
        }

        public void HandleEvent(EntityDeleted<AuthToken> eventMessage)
        {
            this._cache.Remove(CacheKeyManager.AuthTokenKey(eventMessage.Entity?.UID));
        }
    }
}
