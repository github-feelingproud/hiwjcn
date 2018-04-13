using Akka.Actor;
using Hiwjcn.Core;
using Hiwjcn.Core.Data;
using Hiwjcn.Core.Domain.Auth;
using Hiwjcn.Core.Domain.Sys;
using Hiwjcn.Framework.Actors;
using Lib.cache;
using Lib.data.ef;
using Lib.distributed.akka;
using Lib.infrastructure.provider;
using Lib.infrastructure.service;
using Lib.infrastructure.service.user;
using Lib.mvc.auth;
using System;
using System.Threading.Tasks;

namespace Hiwjcn.Bll.Auth
{
    public class AuthApiService :
        AuthApiServiceFromDbBase<AuthToken>
    {
        private readonly Lazy<IActorRef> LogActor;

        public AuthApiService(
            IAuthLoginProvider _loginService,
            ICacheProvider _cache,
            IMSRepository<AuthToken> _tokenRepo) :
            base(_loginService, _cache, _tokenRepo)
        {
            this.LogActor = new Lazy<IActorRef>(() => ActorsManager<CacheHitLogActor>.Instance.DefaultClient);
        }
        
        public override string AuthTokenCacheKey(string token) => CacheKeyManager.AuthTokenKey(token);

        public override string AuthUserInfoCacheKey(string user_uid) => CacheKeyManager.AuthUserInfoKey(user_uid);

        public override async Task CacheHitLog(string cache_key, CacheHitStatusEnum status)
        {
            this.LogActor.Value?.Tell(new CacheHitLogEntity(cache_key, status));
            await Task.FromResult(1);
        }
    }
}
