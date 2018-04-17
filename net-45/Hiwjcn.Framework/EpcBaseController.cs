using EPC.Core.Entity;
using Hiwjcn.Core;
using Hiwjcn.Service.MemberShip;
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
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Hiwjcn.Framework
{
    public abstract class EpcBaseController : BaseController
    {
        protected readonly int PageSize = 10;

        /// <summary>
        /// 管理员
        /// </summary>
        protected int ManagerRole => (int)(MemberRoleEnum.管理员);

        /// <summary>
        /// 管理员或者普通成员
        /// </summary>
        protected int MemberRole => (int)(MemberRoleEnum.管理员 | MemberRoleEnum.普通成员);

        /// <summary>
        /// 所有
        /// </summary>
        protected int AnyRole => (int)(MemberRoleEnum.管理员 | MemberRoleEnum.普通成员 | MemberRoleEnum.观察者);

        public EpcBaseController() { }

        protected const string OrgCookieName = "org_uid";

        [NonAction]
        protected string GetSelectedOrgUID(string specify = null)
        {
            var org_uid = new string[]
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
        protected async Task<LoginUserInfo> GetLoginUserAsync()
        {
            var loginuser = await this.X.context.GetAuthUserAsync();
            if (loginuser == null)
            {
                //没有登录
                throw new NoLoginException();
            }

            return loginuser;
        }

        [NonAction]
        protected async Task<LoginUserInfo> ValidMember(string org_uid, int? flag = null)
        {
            var loginuser = await this.GetLoginUserAsync();

            using (var s = AutofacIocContext.Instance.Scope())
            {
                var cache = s.Resolve_<ICacheProvider>();
                var key = CacheKeyManager.OrgListCacheKey(loginuser.UserID);

                var list = await cache.GetOrSetAsync(key,
                    async () => await s.Resolve_<IOrgService>().GetMyOrgMap(loginuser.UserID),
                    TimeSpan.FromMinutes(30));
                list = ConvertHelper.NotNullList(list);

                var org = list.Where(x => x.OrgUID == org_uid).OrderByDescending(x => x.IID).FirstOrDefault();
                if (org == null)
                {
                    //没有加入组织
                    throw new NoOrgException();
                }
                //多个权限只要有一个就算通过
                if (flag != null && !PermissionHelper.HasPermission(flag.Value, org.Flag))
                {
                    //没有权限
                    throw new NoPermissionInOrgException();
                }
            }
            return loginuser;
        }

        /// <summary>
        /// json转为实体
        /// </summary>
        [NonAction]
        protected T JsonToEntity_<T>(string json, string msg = null) where T : class =>
            json?.JsonToEntityOrDefault<T>() ?? throw new NoParamException(msg ?? "参数错误");

        [NonAction]
        public override ActionResult RunAction(Func<ActionResult> GetActionFunc)
        {
            try
            {
                var data = GetActionFunc.Invoke();

                return data;
            }
            catch (NoParamException)
            {
                return GetJson(new _()
                {
                    success = false,
                    msg = "参数错误",
                    code = "-100"
                });
            }
            catch (NoOrgException)
            {
                return GetJson(new _()
                {
                    success = false,
                    msg = "请选择组织",
                    code = "-111"
                });
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
                return GetJson(new _()
                {
                    success = false,
                    msg = "没有权限",
                    code = "-403"
                });
            }
            catch (NotImplementedException)
            {
                return GetJson(new _()
                {
                    success = false,
                    msg = "功能没有实现",
                    code = "-1111"
                });
            }
            catch (Exception e)
            {
                return this.WhenError(e);
            }
        }

        [NonAction]
        public override async Task<ActionResult> RunActionAsync(Func<Task<ActionResult>> GetActionFunc)
        {
            try
            {
                var data = await GetActionFunc.Invoke();

                return data;
            }
            catch (NoParamException)
            {
                return GetJson(new _()
                {
                    success = false,
                    msg = "参数错误",
                    code = "-100"
                });
            }
            catch (NoOrgException)
            {
                return GetJson(new _()
                {
                    success = false,
                    msg = "请选择组织",
                    code = "-111"
                });
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
                return GetJson(new _()
                {
                    success = false,
                    msg = "没有权限",
                    code = "-403"
                });
            }
            catch (NotImplementedException)
            {
                return GetJson(new _()
                {
                    success = false,
                    msg = "功能没有实现",
                    code = "-1111"
                });
            }
            catch (Exception e)
            {
                return this.WhenError(e);
            }
        }
    }
}
