using Hiwjcn.Core.Data;
using Hiwjcn.Core.Domain;
using Hiwjcn.Dal;
using Lib.infrastructure.extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lib.extension;
using Lib.helper;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public class data_import
    {
        private void menu(List<Auth_Menu> list)
        {
            bool first(Auth_Menu x) => (x.ParentMenuID ?? "0").ToInt(0) <= 0;
            void xx(List<Auth_Menu> data)
            {
                using (var db = new EntityDB())
                {
                    foreach (var x in data)
                    {
                        db.MenuEntity.Add(new MenuEntity()
                        {
                            UID = x.MenuID.ToString(),
                            ParentUID = first(x) ? string.Empty : x.ParentMenuID,
                            Level = x.Level + 1,
                            MenuName = x.MenuName,
                            Description = "导入的数据",
                            Url = x.Url,
                            PermissionJson = (x.Permissions ?? string.Empty).Split(',').Where(m => ValidateHelper.IsPlumpString(m)).ToJson(),
                            GroupKey = x.SysID.ToString(),
                        });
                    }

                    db.SaveChanges();
                }
            }

            foreach (var sys in list.Select(x => x.SysID).Distinct())
            {
                xx(list.Where(x => x.SysID == sys && first(x)).ToList());
            }
        }

        public void run()
        {
            using (var db = new EntityDB())
            {
                var list = db.AuthClient.ToList();
            }
            using (var db = new SSODB())
            {
                var syslist = db.T_Systems.ToList();
                var pers = db.Auth_Permission.ToList();
                var persmap = db.Auth_PermissionMap.ToList();
                var user = db.T_UserInfo.ToList();
                var menu = db.Auth_Menu.ToList();
                var role = db.Auth_Role.ToList();
                var userrole = db.Auth_UserRole.ToList();
                Console.WriteLine("数据读取完成");
                //
                this.menu(menu);
            }
        }
    }
}
