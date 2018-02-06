using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data.ef;
using Lib.infrastructure.service;
using Lib.infrastructure.service.user;
using Lib.infrastructure.extension;
using Lib.extension;
using Lib.ioc;
using Lib.mvc;
using Lib.helper;
using Hiwjcn.Core.Domain;
using Hiwjcn.Core.Domain.User;
using Hiwjcn.Core.Data;

namespace Hiwjcn.Service.MemberShip
{
    public interface IMenuService : IMenuServiceBase<MenuEntity>,
        IAutoRegistered
    {
        Task<List<MenuEntity>> LoadPermissionIds(List<MenuEntity> data);
    }

    public class MenuService : MenuServiceBase<MenuEntity>,
        IMenuService
    {
        private readonly IMSRepository<PermissionEntity> _perRepo;
        public MenuService(
            IMSRepository<MenuEntity> _menuRepo,
            IMSRepository<PermissionEntity> _perRepo) :
            base(_menuRepo)
        {
            this._perRepo = _perRepo;
        }

        public override void UpdateMenuEntity(ref MenuEntity old_menu, ref MenuEntity new_menu)
        {
            old_menu.MenuName = new_menu.MenuName;
            old_menu.Description = new_menu.Description;
            old_menu.HtmlCls = new_menu.HtmlCls;
            old_menu.HtmlId = new_menu.HtmlId;
            old_menu.HtmlOnClick = new_menu.HtmlOnClick;
            old_menu.HtmlStyle = new_menu.HtmlStyle;
            old_menu.IconCls = new_menu.IconCls;
            old_menu.IconUrl = new_menu.IconUrl;
            old_menu.ImageUrl = new_menu.ImageUrl;
            old_menu.Url = new_menu.Url;
            old_menu.Sort = new_menu.Sort;
            old_menu.PermissionJson = new_menu.PermissionJson;
        }

        public async Task<List<MenuEntity>> LoadPermissionIds(List<MenuEntity> data)
        {
            if (ValidateHelper.IsPlumpList(data))
            {
                var names = new List<string>();
                data.ForEach(x => names.AddWhenNotEmpty(x.PermissionNames));
                names = names.Where(x => ValidateHelper.IsPlumpString(x)).ToList();

                var list = await this._perRepo.GetListAsync(x => names.Contains(x.Name));

                foreach (var m in data)
                {
                    m.PermissionIds = list.Where(x => m.PermissionNames.Contains(x.Name)).Select(x => x.UID).ToList();
                }

            }
            return data;
        }

        private async Task<List<string>> ParsePermissionNames(List<string> uids)
        {
            if (!ValidateHelper.IsPlumpList(uids)) { return new List<string>(); }
            return (await this._perRepo.GetListAsync(x => uids.Contains(x.UID))).Select(x => x.Name).ToList();
        }

        public override async Task<_<MenuEntity>> UpdateMenu(MenuEntity model)
        {
            model.PermissionJson = (await this.ParsePermissionNames(model.PermissionIds)).ToJson();
            return await base.UpdateMenu(model);
        }

        public override async Task<_<MenuEntity>> AddMenu(MenuEntity model)
        {
            model.PermissionJson = (await this.ParsePermissionNames(model.PermissionIds)).ToJson();
            return await base.AddMenu(model);
        }
    }
}
