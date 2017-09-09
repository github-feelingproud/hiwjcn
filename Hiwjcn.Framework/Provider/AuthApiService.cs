using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.extension;
using Lib.helper;
using Lib.core;
using Lib.mvc.user;
using Lib.mvc.auth;
using Lib.mvc;
using Hiwjcn.Core.Infrastructure.Auth;
using Lib.cache;
using Lib.data;
using Hiwjcn.Core.Domain.Auth;
using Hiwjcn.Core.Model.Sys;
using Hiwjcn.Core;
using Lib.events;
using Hiwjcn.Framework.Actors;
using Akka;
using Akka.Actor;

namespace Hiwjcn.Bll.Auth
{
    /// <summary>
    /// 读取数据库实现auth相关api
    /// </summary>
    public class AuthApiServiceFromDB : IAuthApi
    {
        private readonly IAuthLoginService _IAuthLoginService;
        private readonly IAuthTokenService _IAuthTokenService;
        private readonly IRepository<AuthScope> _AuthScopeRepository;
        private readonly ICacheProvider _cache;

        public AuthApiServiceFromDB(
            IAuthLoginService _IAuthLoginService,
            IAuthTokenService _IAuthTokenService,
            IRepository<AuthScope> _AuthScopeRepository,
            ICacheProvider _cache)
        {
            this._IAuthLoginService = _IAuthLoginService;
            this._IAuthTokenService = _IAuthTokenService;
            this._AuthScopeRepository = _AuthScopeRepository;
            this._cache = _cache;
        }

        public async Task<_<TokenModel>> GetAccessTokenAsync(string client_id, string client_secret, string code, string grant_type)
        {
            var res = new _<TokenModel>();

            var func = $"{nameof(AuthApiServiceFromDB)}.{nameof(GetAccessTokenAsync)}";
            var p = new { client_id = client_id, client_secret = client_secret, code = code, grant_type = grant_type }.ToJson();

            var data = await this._IAuthTokenService.CreateTokenAsync(client_id, client_secret, code);
            if (data.error)
            {
                $"获取token异常|{data.msg}|{func}|{p}".AddBusinessInfoLog();

                res.SetErrorMsg(data.msg);
                return res;
            }
            var token = data.data;
            var token_data = new TokenModel()
            {
                Token = token.UID,
                RefreshToken = token.RefreshToken,
                Expire = token.ExpiryTime
            };
            res.SetSuccessData(token_data);
            return res;
        }

        public async Task<_<LoginUserInfo>> GetLoginUserInfoByTokenAsync(string client_id, string access_token)
        {
            var data = new _<LoginUserInfo>();

            var func = $"{nameof(AuthApiServiceFromDB)}.{nameof(GetLoginUserInfoByTokenAsync)}";
            var p = new { client_id = client_id, access_token = access_token }.ToJson();

            if (!ValidateHelper.IsAllPlumpString(access_token, client_id))
            {
                $"验证token异常|参数为空|{func}|{p}".AddBusinessInfoLog();

                data.SetErrorMsg("参数为空");
                return data;
            }

            var cache_expire = TimeSpan.FromMinutes(10);
            var Actor = ActorsManager<CacheHitLogActor>.Instance.DefaultClient;

            var hit_status = CacheHitStatusEnum.Hit;
            var cache_key = CacheKeyManager.AuthTokenKey(access_token);

            //查找token
            var token = await this._cache.GetOrSetAsync(cache_key, async () =>
            {
                hit_status = CacheHitStatusEnum.NotHit;

                return await this._IAuthTokenService.FindTokenAsync(client_id, access_token);
            }, cache_expire);

            //统计缓存命中
            Actor?.Tell(new CacheHitLog(cache_key, hit_status));

            if (token == null)
            {
                $"token不存在|{func}|{p}".AddBusinessInfoLog();

                data.SetErrorMsg("token不存在");
                return data;
            }

            hit_status = CacheHitStatusEnum.Hit;
            cache_key = CacheKeyManager.AuthUserInfoKey(token.UserUID);
            //查找用户
            var loginuser = await this._cache.GetOrSetAsync(cache_key, async () =>
            {
                hit_status = CacheHitStatusEnum.NotHit;

                return await this._IAuthLoginService.GetUserInfoByUID(token.UserUID);
            }, cache_expire);

            //统计缓存命中
            Actor?.Tell(new CacheHitLog(cache_key, hit_status));

            if (loginuser == null)
            {
                $"用户不存在|{func}|{p}".AddBusinessInfoLog();

                data.SetErrorMsg("用户不存在");
                return data;
            }

            loginuser.LoginToken = token.UID;
            loginuser.RefreshToken = token.RefreshToken;
            loginuser.TokenExpire = token.ExpiryTime;
            loginuser.Scopes = token.Scopes?.Select(x => x.Name).ToList();

            data.SetSuccessData(loginuser);

            return data;
        }
        
        public async Task<_<string>> GetAuthCodeByOneTimeCodeAsync(string client_id, List<string> scopes, string phone, string sms)
        {
            var data = new _<string>();

            var func = $"{nameof(AuthApiServiceFromDB)}.{nameof(GetAuthCodeByOneTimeCodeAsync)}";
            var p = new { client_id = client_id, scope = scopes, phone = phone, sms = sms }.ToJson();

            var loginuser = await this._IAuthLoginService.LoginByCode(phone, sms);
            if (loginuser.error)
            {
                $"验证码登录失败|{loginuser.msg}|{func}|{p}".AddBusinessInfoLog();

                data.SetErrorMsg(loginuser.msg);
                return data;
            }
            var scopeslist = AuthHelper.ParseScopes(scopes);
            if (!ValidateHelper.IsPlumpList(scopeslist))
            {
                scopeslist = (await this._AuthScopeRepository.GetListAsync(null)).Select(x => x.Name).ToList();
            }

            var code = await this._IAuthTokenService.CreateCodeAsync(client_id, scopeslist, loginuser.data.UserID);
            if (code.error)
            {
                $"创建Code失败|{code.msg}|{func}|{p}".AddBusinessInfoLog();

                data.SetErrorMsg(code.msg);
                return data;
            }

            data.SetSuccessData(code.data.UID);
            return data;
        }

        public async Task<_<string>> GetAuthCodeByPasswordAsync(string client_id, List<string> scopes, string username, string password)
        {
            var data = new _<string>();

            var func = $"{nameof(AuthApiServiceFromDB)}.{nameof(GetAuthCodeByPasswordAsync)}";
            var p = new { client_id = client_id, scope = scopes, username = username, password = password }.ToJson();

            var loginuser = await this._IAuthLoginService.LoginByPassword(username, password);
            if (loginuser.error)
            {
                $"密码登录失败|{loginuser.msg}|{func}|{p}".AddBusinessInfoLog();

                data.SetErrorMsg(loginuser.msg);
                return data;
            }
            var scopeslist = AuthHelper.ParseScopes(scopes);
            if (!ValidateHelper.IsPlumpList(scopeslist))
            {
                scopeslist = (await this._AuthScopeRepository.GetListAsync(null)).Select(x => x.Name).ToList();
            }

            var code = await this._IAuthTokenService.CreateCodeAsync(client_id, scopeslist, loginuser.data.UserID);
            if (code.error)
            {
                $"创建Code失败|{code.msg}|{func}|{p}".AddBusinessInfoLog();

                data.SetErrorMsg(code.msg);
                return data;
            }

            data.SetSuccessData(code.data.UID);
            return data;
        }

    }
}
