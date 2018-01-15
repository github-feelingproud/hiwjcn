using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data.ef;
using Lib.infrastructure.service;
using Lib.infrastructure.service.common;
using Lib.infrastructure.service.organization;
using Lib.infrastructure.service.user;
using Lib.infrastructure.extension;
using Lib.ioc;
using Lib.mvc;
using Hiwjcn.Core.Domain;

namespace Hiwjcn.Bll.Common
{
    public interface IMenuService : IMenuServiceBase<MenuEntity>,
        IAutoRegistered
    {
        //
    }

    public class MenuService : MenuServiceBase<MenuEntity>,
        IMenuService
    {
        public MenuService(
            IEFRepository<MenuEntity> _menuRepo) :
            base(_menuRepo)
        {
            //
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
        }
    }
}
