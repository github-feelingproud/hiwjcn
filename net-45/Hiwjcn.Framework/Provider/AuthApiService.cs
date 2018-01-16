using Akka.Actor;
using Hiwjcn.Core;
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
        AuthApiServiceFromDbBase<AuthClient, AuthScope, AuthToken, AuthCode, AuthTokenScope>
    {
        private readonly Lazy<IActorRef> LogActor;

        public AuthApiService(
            IAuthLoginService _loginService,
            ICacheProvider _cache,
            IEFRepository<AuthClient> _clientRepo,
            IEFRepository<AuthScope> _scopeRepo,
            IEFRepository<AuthToken> _tokenRepo,
            IEFRepository<AuthCode> _codeRepo,
            IEFRepository<AuthTokenScope> _tokenScopeRepo) :
            base(_loginService, _cache, _clientRepo, _scopeRepo, _tokenRepo, _codeRepo, _tokenScopeRepo)
        {
            this.LogActor = new Lazy<IActorRef>(() => ActorsManager<CacheHitLogActor>.Instance.DefaultClient);
        }

        public override string AuthClientCacheKey(string client) => CacheKeyManager.AuthClientKey(client);

        public override string AuthScopeCacheKey(string scope) => CacheKeyManager.AuthScopeKey(scope);

        public override string AuthSSOUserInfoCacheKey(string user_uid) => CacheKeyManager.AuthSSOUserInfoKey(user_uid);

        public override string AuthTokenCacheKey(string token) => CacheKeyManager.AuthTokenKey(token);

        public override string AuthUserInfoCacheKey(string user_uid) => CacheKeyManager.AuthUserInfoKey(user_uid);

        public override async Task CacheHitLog(string cache_key, CacheHitStatusEnum status)
        {
            this.LogActor.Value?.Tell(new CacheHitLogEntity(cache_key, status));
            await Task.FromResult(1);
        }
    }
}
