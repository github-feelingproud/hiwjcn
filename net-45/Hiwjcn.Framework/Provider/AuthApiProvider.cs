using Akka.Actor;
using Hiwjcn.Core;
using Hiwjcn.Core.Data;
using Hiwjcn.Core.Domain.Auth;
using Hiwjcn.Core.Domain.Sys;
using Hiwjcn.Core.Domain.User;
using Hiwjcn.Framework.Actors;
using Hiwjcn.Service.MemberShip;
using Lib.cache;
using Lib.data.ef;
using Lib.distributed.akka;
using Lib.infrastructure.provider;
using Lib.infrastructure.service;
using Lib.infrastructure.service.user;
using Lib.mvc;
using Lib.mvc.auth;
using Lib.mvc.user;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hiwjcn.Framework.Provider
{
    /// <summary>
    /// 登录逻辑的实现
    /// </summary>
    public class AuthApiProvider : AuthApiServiceFromDbBase<AuthToken>
    {
        private readonly Lazy<IActorRef> LogActor;

        private readonly IUserLoginService _login;
        private readonly IUserService _user;

        public AuthApiProvider(
            ICacheProvider _cache,
            IMSRepository<AuthToken> _tokenRepo,

            IUserLoginService _login,
            IUserService _user) :
            base(_cache, _tokenRepo)
        {
            this._login = _login;
            this._user = _user;

            this.LogActor = new Lazy<IActorRef>(() => ActorsManager<CacheHitLogActor>.Instance.DefaultClient);
        }

        public override string AuthTokenCacheKey(string token) => CacheKeyManager.AuthTokenKey(token);

        public override string AuthUserInfoCacheKey(string user_uid) => CacheKeyManager.AuthUserInfoKey(user_uid);

        public override async Task CacheHitLog(string cache_key, CacheHitStatusEnum status)
        {
            this.LogActor.Value.Tell(new CacheHitLogEntity(cache_key, status));
            await Task.FromResult(1);
        }

        public override async Task<LoginUserInfo> GetLoginUserInfoByUserUID(string uid)
        {
            var user = await this._user.GetUserByUID(uid);
            if (user == null)
            {
                return null;
            }
            user = await this._login.LoadPermission(user);
            return this.Parse(user);
        }

        public override async Task<_<LoginUserInfo>> ValidUserByOneTimeCodeAsync(string phone, string sms)
        {
            var data = new _<LoginUserInfo>();
            var res = await this._login.ValidUserOneTimeCode(phone, sms);
            if (res.error)
            {
                data.SetErrorMsg(res.msg);
                return data;
            }

            data.SetSuccessData(this.Parse(res.data));
            return data;
        }

        public override async Task<_<LoginUserInfo>> ValidUserByPasswordAsync(string username, string password)
        {
            var data = new _<LoginUserInfo>();
            var res = await this._login.ValidUserPassword(username, password);
            if (res.error)
            {
                data.SetErrorMsg(res.msg);
                return data;
            }

            data.SetSuccessData(this.Parse(res.data));
            return data;
        }

        private LoginUserInfo Parse(UserEntity model)
        {
            if (model == null) { throw new ArgumentNullException($"登录异常{nameof(UserEntity)}"); }

            var loginuser = new LoginUserInfo()
            {
                IID = model.IID,
                UserID = model.UID,
                NickName = model.NickName,
                UserName = model.UserName,
                HeadImgUrl = model.UserImg,
                Email = model.Email,

                Permissions = model.PermissionNames ?? new List<string>() { },
                Roles = model.RoleIds ?? new List<string>() { },
                IsActive = model.IsActive,
            };

            return loginuser;
        }
    }
}
