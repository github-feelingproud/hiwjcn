using Hiwjcn.Bll.User;
using Hiwjcn.Core.Domain.User;
using Hiwjcn.Core.Domain;
using Hiwjcn.Framework;
using Lib.extension;
using Lib.mvc.auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Lib.mvc;
using WebCore.MvcLib.Controller;
using Lib.infrastructure.model;
using Lib.infrastructure.helper;
using Lib.helper;
using Lib.infrastructure.extension;

namespace Hiwjcn.Web.Controllers
{
    /// <summary>
    /// 角色
    /// </summary>
    public class RoleController : UserBaseController
    {
        private readonly IRoleService _roleService;

        public RoleController(
            IRoleService _roleService)
        {
            this._roleService = _roleService;
        }

        /// <summary>
        /// 更具uid获取权限和相关联的权限id
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpPost]
        //[EpcAuth(Permission = "manage.role.query")]
        [EpcAuth]
        public async Task<ActionResult> GetRoleByUID(string uid)
        {
            return await RunActionAsync(async () =>
            {
                var role = await this._roleService.GetRoleByUID(uid);
                return GetJson(new _()
                {
                    success = true,
                    data = role,
                });
            });
        }

        /// <summary>
        /// 显示角色树菜单
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        //[EpcAuth(Permission = "manage.role.query")]
        [EpcAuth]
        public async Task<ActionResult> Query()
        {
            return await RunActionAsync(async () =>
            {
                var list = await this._roleService.QueryRoleList();
                var iviewdata = list.Select(x => (IViewTreeNode)x).ToList();

                foreach (var m in iviewdata)
                {
                    m.expand = false;
                }

                return GetJson(new _()
                {
                    success = true,
                    data = TreeHelper.BuildIViewTreeStructure(iviewdata)
                });
            });
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpPost]
        //[EpcAuth(Permission = "manage.role.delete")]
        [EpcAuth]
        public async Task<ActionResult> Delete(string uid)
        {
            return await RunActionAsync(async () =>
            {
                var res = await this._roleService.DeleteRoleWhenNoChildren(uid);
                if (res.error)
                {
                    return GetJsonRes(res.msg);
                }

                return GetJson(new _() { success = true });
            });
        }

        /// <summary>
        /// 添加角色
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        //[EpcAuth(Permission = "manage.role.edit")]
        [EpcAuth]
        public async Task<ActionResult> Save(string data)
        {
            return await RunActionAsync(async () =>
            {
                var model = data?.JsonToEntity<RoleEntity>(throwIfException: false);
                if (model == null)
                {
                    return GetJsonRes("参数为空");
                }

                if (ValidateHelper.IsPlumpString(model.UID))
                {
                    var res = await this._roleService.UpdateRole(model);
                    if (res.error)
                    {
                        return GetJsonRes(res.msg);
                    }
                }
                else
                {
                    model.AsFirstLevelIfParentIsNotValid();
                    var res = await this._roleService.AddRole(model);
                    if (res.error)
                    {
                        return GetJsonRes(res.msg);
                    }
                }

                return GetJson(new _() { success = true });
            });
        }

        /// <summary>
        /// 给用户设置角色
        /// </summary>
        /// <param name="user_uid"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        [HttpPost]
        //[EpcAuth(Permission = "manage.role.set_user_role")]
        [EpcAuth]
        public async Task<ActionResult> SetUserRole(string user_uid, string role)
        {
            return await RunActionAsync(async () =>
            {
                var roles = role?.JsonToEntity<List<string>>(throwIfException: false);
                if (!ValidateHelper.IsPlumpString(user_uid) || roles == null)
                {
                    return GetJsonRes("参数错误");
                }

                var map = roles.Select(x =>
                {
                    var m = new UserRoleEntity()
                    {
                        UserID = user_uid,
                        RoleID = x
                    };
                    m.Init("ud");
                    return m;
                }).ToList();

                var res = await this._roleService.SetUserRoles(user_uid, map);
                if (res.error)
                {
                    return GetJsonRes(res.msg);
                }

                return GetJsonRes(string.Empty);
            });
        }

        /// <summary>
        /// 给角色设置权限
        /// </summary>
        /// <param name="role_uid"></param>
        /// <param name="permission"></param>
        /// <returns></returns>
        [HttpPost]
        //[EpcAuth(Permission = "manage.deaprtment.set_role_permission")]
        [EpcAuth]
        public async Task<ActionResult> SetRolePermission(string role_uid, string permission)
        {
            return await RunActionAsync(async () =>
            {
                var pers = permission?.JsonToEntity<List<string>>(throwIfException: false);
                if (!ValidateHelper.IsPlumpString(role_uid) || pers == null)
                {
                    return GetJsonRes("参数错误");
                }

                var map = pers.Select(x =>
                {
                    var m = new RolePermissionEntity()
                    {
                        RoleID = role_uid,
                        PermissionID = x
                    };
                    m.Init("dr");
                    return m;
                }).ToList();

                var res = await this._roleService.SetRolePermissions(role_uid, map);
                if (res.error)
                {
                    return GetJsonRes(res.msg);
                }

                return GetJsonRes(string.Empty);
            });
        }

    }
}