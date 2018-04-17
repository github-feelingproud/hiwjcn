using EPC.Core.Entity;
using Hiwjcn.Service;
using Hiwjcn.Framework;
using Lib.core;
using Lib.extension;
using Lib.helper;
using Lib.mvc;
using Lib.mvc.auth;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Hiwjcn.Service.MemberShip;
using Lib.cache;
using Hiwjcn.Core;

namespace EPC.Api.Controllers
{
    /// <summary>
    /// 组织
    /// 以github为例，epc就是github，途虎在github中就等同于epc的客户。
    /// 途虎就是组织，组织下面是成员（这里的成员就是点检人员/保安）
    /// </summary>
    public class OrgController : EpcBaseController
    {
        private readonly ICacheProvider _cache;
        private readonly IOrgService _orgService;

        public OrgController(
            ICacheProvider _cache,
            IOrgService _orgService)
        {
            this._cache = _cache;
            this._orgService = _orgService;
        }

        /// <summary>
        /// 组织成员分页
        /// </summary>
        /// <param name="org_uid"></param>
        /// <param name="q"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> AllMembers(string q)
        {
            return await RunActionAsync(async () =>
            {
                var org_uid = this.GetSelectedOrgUID();
                var loginuser = await this.ValidMember(org_uid, this.AnyRole);

                var data = await this._orgService.AllMembers(org_uid);

                return GetJson(new _()
                {
                    success = true,
                    data = data
                });
            });
        }

        /// <summary>
        /// 查询组织
        /// </summary>
        /// <param name="q"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpPost]
        [EpcAuth(Permission = "manage.org")]
        public async Task<ActionResult> Query(string q, int? page)
        {
            return await RunActionAsync(async () =>
            {
                page = CheckPage(page);

                var pager = await this._orgService.QueryOrgPager(q, page.Value, this.PageSize);

                return GetJson(new _()
                {
                    success = true,
                    data = pager
                });
            });
        }

        /// <summary>
        /// 添加或者更新组织
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [EpcAuth(Permission = "manage.org")]
        public async Task<ActionResult> Save(string data)
        {
            return await RunActionAsync(async () =>
            {
                var model = this.JsonToEntity_<OrganizationEntity>(data);

                var loginuser = await this.GetLoginUserAsync();

                model.UserUID = loginuser.UserID;

                if (ValidateHelper.IsPlumpString(model.UID))
                {
                    var res = await this._orgService.UpdateOrg(model);
                    if (res.error)
                    {
                        return GetJsonRes(res.msg);
                    }
                    var map = new OrganizationMemberEntity()
                    {
                        OrgUID = res.data.UID,
                        UserUID = res.data.OwnerUID,
                        Flag = (int)MemberRoleEnum.管理员,
                        MemberApproved = (int)YesOrNoEnum.是,
                        OrgApproved = (int)YesOrNoEnum.是,
                        IsOwner = (int)YesOrNoEnum.是
                    };
                    await this._orgService.AddMember(map);
                }
                else
                {
                    var res = await this._orgService.AddOrg(model);
                    if (res.error)
                    {
                        return GetJsonRes(res.msg);
                    }
                    var map = new OrganizationMemberEntity()
                    {
                        OrgUID = res.data.UID,
                        UserUID = res.data.OwnerUID,
                        Flag = (int)MemberRoleEnum.管理员,
                        MemberApproved = (int)YesOrNoEnum.是,
                        OrgApproved = (int)YesOrNoEnum.是,
                    };
                    await this._orgService.AddMember(map);
                }

                return GetJsonRes(string.Empty);
            });
        }

        /// <summary>
        /// 删除组织
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpPost]
        [EpcAuth(Permission = "manage.org")]
        public async Task<ActionResult> Delete(string uid)
        {
            return await RunActionAsync(async () =>
            {
                var res = await this._orgService.DeleteOrg(uid);
                if (res.error)
                {
                    return GetJsonRes(res.msg);
                }

                return GetJsonRes(string.Empty);
            });
        }

        /// <summary>
        /// 我加入的组织
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> MyOrg()
        {
            return await RunActionAsync(async () =>
            {
                var loginuser = await this.GetLoginUserAsync();
                var map = await this._orgService.GetMyOrgMap(loginuser?.UserID);
                var data = await this._orgService.GetMyOrgEntity(map.Select(x => x.OrgUID).ToArray());

                return GetJson(new _()
                {
                    success = true,
                    data = data
                });
            });
        }

        /// <summary>
        /// 设置默认org
        /// </summary>
        /// <param name="org_uid"></param>
        /// <returns></returns>
        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> SelectOrgAction(string org_uid)
        {
            return await RunActionAsync(async () =>
            {
                if (!ValidateHelper.IsPlumpString(org_uid))
                {
                    return GetJsonRes("无效参数");
                }

                var loginuser = await this.GetLoginUserAsync();
                var data = await this._orgService.GetMyOrgMap(loginuser?.UserID);
                if (!data.Select(x => x.OrgUID).Contains(org_uid))
                {
                    return GetJsonRes("无权操作");
                }

                this.SetCookieOrgUID(org_uid);

                return GetJsonRes(string.Empty);
            });
        }

        /// <summary>
        /// 获取所有角色和响应的值
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [EpcAuth]
        public ActionResult QueryMemberRoles()
        {
            return RunAction(() =>
            {
                var data = MemberRoleHelper.GetRoles();

                return GetJson(new _()
                {
                    success = true,
                    data = data.Select(x => new { x.Key, x.Value }).ToList()
                });
            });
        }

        /// <summary>
        /// 添加成员
        /// </summary>
        /// <param name="org_uid"></param>
        /// <param name="user_uid"></param>
        /// <returns></returns>
        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> AddMember(string user_uid, int? flag)
        {
            return await RunActionAsync(async () =>
            {
                if (!ValidateHelper.IsPlumpString(user_uid) || flag == null)
                {
                    return GetJsonRes("参数错误");
                }
                if (!MemberRoleHelper.IsValid(flag.Value))
                {
                    return GetJsonRes("权限值错误");
                }

                var org_uid = this.GetSelectedOrgUID();
                var loginuser = await this.ValidMember(org_uid, this.ManagerRole);

                var map = new OrganizationMemberEntity()
                {
                    OrgUID = org_uid,
                    UserUID = user_uid,
                    Flag = flag.Value,
                    MemberApproved = (int)YesOrNoEnum.是,
                    OrgApproved = (int)YesOrNoEnum.是,
                };
                var res = await this._orgService.AddMember(map);
                if (res.error)
                {
                    return GetJsonRes(res.msg);
                }

                var key = CacheKeyManager.OrgListCacheKey(map.UserUID);
                this._cache.Remove(key);

                return GetJsonRes(string.Empty);
            });
        }

        /// <summary>
        /// 移除成员
        /// </summary>
        /// <param name="org_uid"></param>
        /// <param name="user_uid"></param>
        /// <returns></returns>
        [HttpPost]
        [EpcAuth]
        public async Task<ActionResult> RemoveMember(string user_uid)
        {
            return await RunActionAsync(async () =>
            {
                var org_uid = this.GetSelectedOrgUID();
                var loginuser = await this.ValidMember(org_uid, this.ManagerRole);

                var res = await this._orgService.RemoveMember(org_uid, user_uid);
                if (res.error)
                {
                    return GetJsonRes(res.msg);
                }

                return GetJsonRes(string.Empty);
            });
        }
    }
}