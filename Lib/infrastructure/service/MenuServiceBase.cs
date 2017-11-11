using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure.entity;
using Lib.data;
using Lib.mvc;
using Lib.helper;
using Lib.extension;
using System.Data.Entity;
using Lib.core;
using Lib.infrastructure.extension;

namespace Lib.infrastructure.service
{
    public interface IMenuServiceBase<MenuBase>
    {
        Task<_<string>> AddMenu(MenuBase model);

        Task<_<string>> DeleteMenuRecursively(string menu_uid);

        Task<_<string>> DeleteMenus(params string[] menu_uids);

        Task<List<MenuBase>> QueryMenuList(string group, string parent = null);

        Task<_<string>> UpdateMenu(MenuBase model);
    }

    public abstract class MenuServiceBase<MenuBase> : IMenuServiceBase<MenuBase>
        where MenuBase : MenuEntityBase, new()
    {
        private readonly IRepository<MenuBase> _menuRepo;

        public MenuServiceBase(
            IRepository<MenuBase> _menuRepo)
        {
            this._menuRepo = _menuRepo;
        }

        public virtual async Task<_<string>> AddMenu(MenuBase model) =>
            await this._menuRepo.AddTreeNode(model, "mn");

        public virtual async Task<_<string>> DeleteMenuRecursively(string menu_uid) =>
            await this._menuRepo.DeleteTreeNodeRecursively(menu_uid);

        public virtual async Task<_<string>> DeleteMenus(params string[] menu_uids) =>
            await this._menuRepo.DeleteByMultipleUIDS(menu_uids);

        public virtual async Task<List<MenuBase>> QueryMenuList(string group, string parent = null)
        {
            return await this._menuRepo.PrepareIQueryableAsync_(async query =>
            {
                query = query.Where(x => x.Group == group);
                if (ValidateHelper.IsPlumpString(parent))
                {
                    query = query.Where(x => x.ParentUID == parent);
                }

                return await query.OrderByDescending(x => x.Sort).Take(5000).ToListAsync();
            });
        }

        public abstract void UpdateMenuEntity(ref MenuBase old_menu, ref MenuBase new_menu);

        public virtual async Task<_<string>> UpdateMenu(MenuBase model)
        {
            var data = new _<string>();
            var menu = await this._menuRepo.GetFirstAsync(x => x.UID == model.UID);
            Com.AssertNotNull(menu, "菜单不存在");
            menu.Update();
            if (!menu.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }
            if (await this._menuRepo.UpdateAsync(menu) > 0)
            {
                data.SetSuccessData(string.Empty);
                return data;
            }

            throw new Exception("更新菜单错误");
        }

    }
}
