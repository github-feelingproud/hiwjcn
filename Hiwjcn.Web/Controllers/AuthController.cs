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
using System.Threading.Tasks;

namespace Hiwjcn.Web.Controllers
{
    [RoutePrefix("oauth2")]
    public class AuthController : BaseController
    {
        private readonly LoginStatus _LoginStatus;
        public AuthController(LoginStatus _LoginStatus)
        {
            this._LoginStatus = _LoginStatus;
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
                await Task.FromResult(1);
                return View();
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

        [HttpPost]
        [Route("refresh_token/{refresh_token}")]
        public async Task<ActionResult> RefreshToken(string refresh_token)
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
    }
}