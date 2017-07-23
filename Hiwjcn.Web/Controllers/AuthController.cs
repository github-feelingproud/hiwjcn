using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lib.mvc;
using Lib.mvc.user;
using Lib.helper;
using Lib.core;
using Lib.extension;
using Lib.net;
using Lib.cache;
using System.Threading.Tasks;
using Hiwjcn.Core.Infrastructure.Auth;
using Hiwjcn.Bll.Auth;

namespace Hiwjcn.Web.Controllers
{
    [RoutePrefix("oauth2")]
    public class AuthController : BaseController
    {
        private readonly LoginStatus _LoginStatus;
        private readonly IAuthTokenService _IAuthTokenService;
        private readonly IAuthScopeService _IAuthScopeService;
        private readonly ICacheProvider _cache;

        public AuthController(
            LoginStatus _LoginStatus,
            ICacheProvider _cache,
            IAuthTokenService _IAuthTokenService,
            IAuthScopeService _IAuthScopeService)
        {
            this._LoginStatus = _LoginStatus;
            this._cache = _cache;

            this._IAuthTokenService = _IAuthTokenService;
            this._IAuthScopeService = _IAuthScopeService;
        }

        /// <summary>
        /// 授权页面
        /// </summary>
        /// <param name="client_id"></param>
        /// <param name="redirect_uri"></param>
        /// <param name="response_type"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [Route("authorize")]
        public async Task<ActionResult> Authorize(
            string client_id, string redirect_uri, string scope, string response_type, string state)
        {
            return await RunActionAsync(async () =>
            {
                var scope_list = ConvertHelper.NotNullList(scope?.Split(',').ToList()).Where(x => ValidateHelper.IsPlumpString(x)).ToList();

                var list = await this._IAuthScopeService.GetScopesOrDefault(scope_list.ToArray());
                list = list.OrderByDescending(x => x.Important).OrderBy(x => x.Name).ToList();

                ViewData["scopes"] = list;
                return View();
            });
        }

        [HttpPost]
        public async Task<ActionResult> AuthorizeCode(string client_id, List<string> scope)
        {
            return await RunActionAsync(async () =>
            {
                var loginuser = this._LoginStatus.GetLoginUser();
                if (loginuser == null)
                {
                    return GetJsonRes("未登录");
                }

                var data = await this._IAuthTokenService.CreateCode(client_id, scope, loginuser.UserID);

                return GetJson(new _() { success = !ValidateHelper.IsPlumpString(data.msg), data = data.code?.UID, msg = data.msg });
            });
        }

        /// <summary>
        /// 用code换token
        /// </summary>
        /// <param name="client_id"></param>
        /// <param name="client_secret"></param>
        /// <param name="code"></param>
        /// <param name="grant_type"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("access_token")]
        public async Task<ActionResult> AccessToken(
            string client_id, string client_secret, string code, string grant_type)
        {
            return await RunActionAsync(async () =>
            {
                await Task.FromResult(1);
                return View();
            });
        }

        /// <summary>
        /// 合法则返回200， 否则返回404
        /// </summary>
        /// <param name="client_id"></param>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [Route("check_token/{client_id}/{access_token}")]
        public async Task<ActionResult> CheckToken(string client_id, string access_token)
        {
            return await RunActionAsync(async () =>
            {
                await Task.FromResult(1);
                var bad = true;
                if (bad)
                {
                    return Http404();
                }

                var hit_status = CacheHitStatusEnum.Hit;

                var res = await this._cache.GetOrSetAsync(
                    AuthCacheKeyManager.TokenKey(access_token),
                    async () =>
                    {

                        hit_status = CacheHitStatusEnum.NotHit;
                        return await Task.FromResult("");
                    },
                    TimeSpan.FromMinutes(5));

                $"token：{access_token}缓存命中情况：{hit_status}".AddBusinessInfoLog();

                return GetJson(new _() { success = true });
            });
        }

        /// <summary>
        /// 检查token是否对某个scope有访问权限
        /// </summary>
        /// <param name="client_id"></param>
        /// <param name="access_token"></param>
        /// <returns></returns>
        [Route("check_token_scope/{access_token}/{scope}")]
        public async Task<ActionResult> CheckTokenScope(string access_token, string scope)
        {
            return await RunActionAsync(async () =>
            {
                await Task.FromResult(1);
                var bad = true;
                if (bad)
                {
                    return Http404();
                }

                return GetJson(new _() { success = true });
            });
        }

        public async Task<ActionResult> MyClients(string q, int? page)
        {
            return await RunActionAsync(async () =>
            {
                page = CheckPage(page);
                var pagesize = 100;

                var data = await this._IAuthTokenService.GetMyAuthorizedClients(this.X.User?.UserID, q, page.Value, pagesize);
                ViewData["pager"] = data.GetPagerHtml($"oauth/{nameof(MyClients)}", "page", page.Value, pagesize);
                ViewData["list"] = data.DataList;
                return View();
            });
        }
    }
}