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

namespace Hiwjcn.Bll.Auth
{
    public class AuthApiService : IAuthApi
    {
        private readonly IAuthLoginService _IAuthLoginService;
        private readonly IAuthTokenService _IAuthTokenService;
        private readonly IAuthScopeService _IAuthScopeService;
        private readonly ICacheProvider _cache;
        private readonly IRepository<AuthScope> _AuthScopeRepository;
        private readonly IRepository<AuthClient> _AuthClientRepository;
        private readonly IAuthTokenToUserService _IAuthTokenToUserService;

        public AuthApiService(
            IAuthLoginService _IAuthLoginService,
            ICacheProvider _cache,
            IAuthTokenService _IAuthTokenService,
            IAuthScopeService _IAuthScopeService,
            IRepository<AuthScope> _AuthScopeRepository,
            IRepository<AuthClient> _AuthClientRepository,
            IAuthTokenToUserService _IAuthTokenToUserService)
        {
            this._IAuthLoginService = _IAuthLoginService;
            this._cache = _cache;

            this._IAuthTokenService = _IAuthTokenService;
            this._IAuthScopeService = _IAuthScopeService;

            this._AuthScopeRepository = _AuthScopeRepository;
            this._AuthClientRepository = _AuthClientRepository;
            this._IAuthTokenToUserService = _IAuthTokenToUserService;
        }

        public async Task<_<TokenModel>> GetAccessTokenAsync(string client_id, string client_secret, string code, string grant_type)
        {
            var res = new _<TokenModel>();
            var data = await this._IAuthTokenService.CreateTokenAsync(client_id, client_secret, code);
            if (!data.success)
            {
                res.SetErrorMsg(data.msg);
                return res;
            }
            var token_data = new TokenModel()
            {
                Token = data.data.UID,
                RefreshToken = data.data.RefreshToken,
                Expire = data.data.ExpiryTime
            };
            res.SetSuccessData(token_data);
            return res;
        }

        public async Task<_<LoginUserInfo>> GetLoginUserInfoByTokenAsync(string client_id, string access_token)
        {
            var data = new _<LoginUserInfo>();

            var loginuser = await this._IAuthTokenToUserService.FindUserByTokenAsync(access_token, client_id);

            if (!loginuser.success)
            {
                data.SetErrorMsg(loginuser.msg);
                return data;
            }
            data.SetSuccessData(loginuser.data);
            return data;
        }

        private List<string> ParseScopes(string scope)
        {
            var scopeslist = ConvertHelper.NotNullList(scope?.JsonToEntity<List<string>>());
            scopeslist = scopeslist.Select(x => x?.Trim()).Where(x => ValidateHelper.IsPlumpString(x)).ToList();
            return scopeslist;
        }

        public async Task<_<string>> GetAuthCodeByOneTimeCodeAsync(string client_id, string scope, string phone, string sms)
        {
            var data = new _<string>();

            var loginuser = await this._IAuthLoginService.LoginByCode(phone, sms);
            if (!loginuser.success)
            {
                data.SetErrorMsg(loginuser.msg);
                return data;
            }
            var scopeslist = this.ParseScopes(scope);
            if (!ValidateHelper.IsPlumpList(scopeslist))
            {
                scopeslist = (await this._AuthScopeRepository.GetListAsync(null)).Select(x => x.Name).ToList();
            }

            var code = await this._IAuthTokenService.CreateCodeAsync(client_id, scopeslist, loginuser.data.UserID);
            if (!code.success)
            {
                data.SetErrorMsg(code.msg);
                return data;
            }

            data.SetSuccessData(code.data.UID);
            return data;
        }

        public async Task<_<string>> GetAuthCodeByPasswordAsync(string client_id, string scope, string username, string password)
        {
            var data = new _<string>();

            var loginuser = await this._IAuthLoginService.LoginByPassword(username, password);
            if (!loginuser.success)
            {
                data.SetErrorMsg(loginuser.msg);
                return data;
            }
            var scopeslist = this.ParseScopes(scope);
            if (!ValidateHelper.IsPlumpList(scopeslist))
            {
                scopeslist = (await this._AuthScopeRepository.GetListAsync(null)).Select(x => x.Name).ToList();
            }

            var code = await this._IAuthTokenService.CreateCodeAsync(client_id, scopeslist, loginuser.data.UserID);
            if (!code.success)
            {
                data.SetErrorMsg(code.msg);
                return data;
            }

            data.SetSuccessData(code.data.UID);
            return data;
        }

    }
}
