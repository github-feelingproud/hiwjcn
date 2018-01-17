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
using Lib.data.ef;

namespace Lib.infrastructure.service
{
    public interface IMenuServiceBase<MenuBase>
    {
        Task<_<MenuBase>> AddMenu(MenuBase model);

        Task<MenuBase> GetMenuByUID(string uid);

        Task<_<string>> DeleteMenuRecursively(string group_key, string menu_uid);

        Task<_<string>> DeleteMenus(params string[] menu_uids);

        Task<_<string>> DeleteMenuWhenNoChildren(string uid);

        Task<List<MenuBase>> QueryMenuList(string group_key, string parent = null);

        Task<_<MenuBase>> UpdateMenu(MenuBase model);
    }

    public abstract class MenuServiceBase<MenuBase> :
        IMenuServiceBase<MenuBase>
        where MenuBase : MenuEntityBase, new()
    {
        protected readonly IEFRepository<MenuBase> _menuRepo;

        public MenuServiceBase(
            IEFRepository<MenuBase> _menuRepo)
        {
            this._menuRepo = _menuRepo;
        }

        public virtual async Task<_<MenuBase>> AddMenu(MenuBase model)
        {
            return await this._menuRepo.AddTreeNode(model, "mn");
        }

        public virtual async Task<_<string>> DeleteMenuRecursively(string group_key, string menu_uid)
        {
            if (!ValidateHelper.IsPlumpString(group_key)) { throw new ArgumentNullException(nameof(group_key)); }
            var data = new _<string>();
            var list = await this._menuRepo.GetListEnsureMaxCountAsync(x => x.GroupKey == group_key, 5000, "树结构节点数量达到上限");
            var node = list.Where(x => x.UID == menu_uid).FirstOrThrow_("节点不存在");
            var dead_nodes = list.FindNodeChildrenRecursively_(node);

            if (await this._menuRepo.DeleteAsync(dead_nodes.ToArray()) > 0)
            {
                data.SetSuccessData(string.Empty);
                return data;
            }

            throw new Exception("删除错误");
        }

        public virtual async Task<_<string>> DeleteMenus(params string[] menu_uids) =>
            await this._menuRepo.DeleteByUIDS_(menu_uids);

        public virtual async Task<List<MenuBase>> QueryMenuList(string group_key, string parent = null)
        {
            if (!ValidateHelper.IsPlumpString(group_key)) { throw new ArgumentNullException(nameof(group_key)); }
            return await this._menuRepo.PrepareIQueryableAsync(async query =>
            {
                query = query.Where(x => x.GroupKey == group_key);
                if (ValidateHelper.IsPlumpString(parent))
                {
                    query = query.Where(x => x.ParentUID == parent);
                }

                return await query.OrderBy(x => x.Sort).Take(5000).ToListAsync();
            });
        }

        public abstract void UpdateMenuEntity(ref MenuBase old_menu, ref MenuBase new_menu);

        public virtual async Task<_<MenuBase>> UpdateMenu(MenuBase model)
        {
            var data = new _<MenuBase>();
            var menu = await this._menuRepo.GetFirstAsync(x => x.UID == model.UID);
            Com.AssertNotNull(menu, "菜单不存在");

            this.UpdateMenuEntity(ref menu, ref model);

            menu.Update();
            if (!menu.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }
            if (await this._menuRepo.UpdateAsync(menu) > 0)
            {
                data.SetSuccessData(menu);
                return data;
            }

            throw new Exception("更新菜单错误");
        }

        public virtual async Task<_<string>> DeleteMenuWhenNoChildren(string uid) =>
            await this._menuRepo.DeleteSingleNodeWhenNoChildren_(uid);

        public virtual async Task<MenuBase> GetMenuByUID(string uid) =>
            await this._menuRepo.GetFirstAsync(x => x.UID == uid);
    }
}
