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
using Lib.events;
using System.Configuration;
using Lib.mvc;
using Lib.cache;
using Hiwjcn.Core;
using Lib.infrastructure.entity;
using Lib.data.ef;
using Lib.ioc;
using Lib.infrastructure.entity.auth;
using Lib.infrastructure.service.user;
using Hiwjcn.Core.Data;

namespace Hiwjcn.Bll.Auth
{
    public interface IAuthService : IAuthServiceBase<AuthClient, AuthScope, AuthToken, AuthCode, AuthTokenScope>,
        IAutoRegistered
    {

    }

    public class AuthService :
        AuthServiceBase<AuthClient, AuthScope, AuthToken, AuthCode, AuthTokenScope>,
        IAuthService
    {
        public AuthService(
            IEventPublisher _publisher,
            IMSRepository<AuthToken> _AuthTokenRepository,
            IMSRepository<AuthTokenScope> _AuthTokenScopeRepository,
            IMSRepository<AuthScope> _AuthScopeRepository,
            IMSRepository<AuthCode> _AuthCodeRepository,
            IMSRepository<AuthClient> _AuthClientRepository,
            ICacheProvider _cache) :
            base(_cache, _AuthClientRepository, _AuthScopeRepository, _AuthTokenRepository, _AuthCodeRepository, _AuthTokenScopeRepository)
        {
            //
        }

        public override string AuthClientCacheKey(string client) =>
            CacheKeyManager.AuthClientKey(client);

        public override string AuthScopeCacheKey(string scope) =>
            CacheKeyManager.AuthScopeKey(scope);

        public override string AuthSSOUserInfoCacheKey(string user_uid) =>
            CacheKeyManager.AuthSSOUserInfoKey(user_uid);

        public override string AuthTokenCacheKey(string token) =>
            CacheKeyManager.AuthTokenKey(token);

        public override string AuthUserInfoCacheKey(string user_uid) =>
            CacheKeyManager.AuthUserInfoKey(user_uid);
    }
}
