using Hiwjcn.Service;
using Hiwjcn.Core;
using Lib.cache;
using Lib.extension;
using Lib.helper;
using Lib.ioc;
using Lib.mvc;
using Lib.mvc.auth;
using Lib.mvc.user;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Hiwjcn.Service.MemberShip;

namespace Hiwjcn.Framework
{
    public abstract class EpcBaseController : BaseController
    {
        protected readonly int PageSize = 10;

        public EpcBaseController() { }

        protected const string OrgCookieName = "org_uid";

        [NonAction]
        protected string GetSelectedOrgUID(string specify = null)
        {
            var org_uid = new List<string>()
            {
                specify,
                this.X.context.Request.QueryString[OrgCookieName],
                this.X.context.Request.Form[OrgCookieName],
                this.X.context.GetCookie(OrgCookieName)
            }.FirstNotEmpty_();

            if (!ValidateHelper.IsPlumpString(org_uid))
            {
                throw new NoOrgException();
            }

            return org_uid;
        }

        [NonAction]
        protected void SetCookieOrgUID(string uid) =>
            this.X.context.SetCookie(OrgCookieName, uid, expires_minutes: TimeSpan.FromDays(365).TotalMinutes);

        [NonAction]
        protected async Task<LoginUserInfo> ValidMember(string org_uid, int? flag = null)
        {
            var loginuser = await this.X.context.GetAuthUserAsync();
            if (loginuser == null)
            {
                //没有登录
                throw new NoLoginException();
            }

            using (var s = IocContext.Instance.Scope())
            {
                var cache = s.Resolve_<ICacheProvider>();
                var key = CacheKeyManager.OrgListCacheKey(loginuser.UserID);

                var list = await cache.GetOrSetAsync(key,
                    async () => await s.Resolve_<IOrgService>().GetMyOrgMap(loginuser.UserID),
                    TimeSpan.FromMinutes(5));
                list = ConvertHelper.NotNullList(list);

                var org = list.Where(x => x.OrgUID == org_uid).OrderByDescending(x => x.IID).FirstOrDefault();
                if (org == null)
                {
                    //没有加入组织
                    throw new NoOrgException();
                }
                if (flag != null && !PermissionHelper.HasPermission(org.Flag, flag.Value))
                {
                    //没有权限
                    throw new NoPermissionInOrgException();
                }
            }
            return loginuser;
        }

        [NonAction]
        protected ActionResult AccessDeny() => GetJson(new _() { success = false, msg = "无权访问", code = "-111" });

        [NonAction]
        public override ActionResult RunAction(Func<ActionResult> GetActionFunc)
        {
            return base.RunAction(GetActionFunc);
        }

        [NonAction]
        public override async Task<ActionResult> RunActionAsync(Func<Task<ActionResult>> GetActionFunc)
        {
            try
            {
                var data = await GetActionFunc.Invoke();

                return data;
            }
            catch (NoOrgException)
            {
                return GetJson(new _() { success = false, msg = "无权访问", code = "-111" });
            }
            catch (NoLoginException)
            {
                return GetJson(new _()
                {
                    success = false,
                    msg = "没有登录",
                    code = "-401"
                });
            }
            catch (NoPermissionInOrgException)
            {
                return GetJsonRes("无权操作");
            }
            catch (Exception e)
            {
                return this.WhenError(e);
            }
        }
    }
}
