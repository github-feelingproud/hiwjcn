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
        
        Task<_<int>> DeleteMenuWhenNoChildren(string uid);

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

        public virtual async Task<_<int>> DeleteMenuWhenNoChildren(string uid) =>
            await this._menuRepo.DeleteSingleNodeWhenNoChildren_(uid);

        public virtual async Task<MenuBase> GetMenuByUID(string uid) =>
            await this._menuRepo.GetFirstAsync(x => x.UID == uid);
    }
}
