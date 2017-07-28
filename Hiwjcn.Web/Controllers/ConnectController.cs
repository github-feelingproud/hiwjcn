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
using Lib.mvc.auth;

namespace Hiwjcn.Web.Controllers
{
    public interface IAuthUserService
    {
        Task<LoginUserInfo> LoginByPassword();

        Task<LoginUserInfo> LoginByToken();

        Task<LoginUserInfo> LoginByOneTimeCode();

        Task<bool> SendOneTimeCode();

        Task<string[]> GetUserPemission();
    }

    [RoutePrefix("connect")]
    public class ConnectController : BaseController
    {
        private readonly IAuthUserService _IUserService;
        public ConnectController(IAuthUserService _IUserService)
        {
            this._IUserService = _IUserService;
        }

        public async Task<ActionResult> Login(string url)
        {
            return await RunActionAsync(async () =>
            {
                await Task.FromResult(1);
                return View();
            });
        }
        public async Task<ActionResult> LoginByPassword(string username, string password)
        {
            return await RunActionAsync(async () =>
            {
                await Task.FromResult(1);
                return View();
            });
        }
        public async Task<ActionResult> LoginByToken()
        {
            return await RunActionAsync(async () =>
            {
                await Task.FromResult(1);
                return View();
            });
        }
        public async Task<ActionResult> LoginByOneTimeCode(string phone, string code)
        {
            return await RunActionAsync(async () =>
            {
                await Task.FromResult(1);
                return View();
            });
        }
        public async Task<ActionResult> SendOneTimeCode(string phone)
        {
            return await RunActionAsync(async () =>
            {
                await Task.FromResult(1);
                return View();
            });
        }


        public async Task<ActionResult> Authorize(string client_id, string redirect_uri, string scope,
            string response_type, string state)
        {
            return await RunActionAsync(async () =>
            {
                await Task.FromResult(1);
                return View();
            });
        }
        public async Task<ActionResult> CreateAuthorizeCode(string client_id, List<string> scope)
        {
            return await RunActionAsync(async () =>
            {
                await Task.FromResult(1);
                return View();
            });
        }

        [ActionName("user_js")]
        public async Task<ActionResult> LoginUser()
        {
            return await RunActionAsync(async () =>
            {
                var loginuser = await this.X.context.GetAuthUserAsync();
                await Task.FromResult(1);
                return View();
            });
        }
    }
}