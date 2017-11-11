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
    { }

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

        public virtual async Task<_<string>> DeleteMenus(params string[] menu_uids)
        {
            var data = new _<string>();

            throw new Exception("删除菜单失败");
        }

    }
}
