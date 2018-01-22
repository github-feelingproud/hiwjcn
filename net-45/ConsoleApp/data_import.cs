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
using Hiwjcn.Core.Entity;
using Hiwjcn.Core.Domain.User;

namespace ConsoleApp
{
    public class data_import
    {
        private readonly DateTime now = DateTime.Now;

        private void menu(List<Auth_Menu> list)
        {
            using (var db = new EntityDB())
            {
                if (db.MenuEntity.Any())
                {
                    Console.WriteLine("菜单已存在");
                    return;
                }
            }
            bool first(Auth_Menu x) => (x.ParentMenuID ?? "0").ToInt(0) <= 0;
            void xx(string parent_uid, int parent_level, List<Auth_Menu> data)
            {
                if (!ValidateHelper.IsPlumpList(data)) { return; }
                using (var db = new EntityDB())
                {
                    foreach (var m in data)
                    {
                        var model = new MenuEntity()
                        {
                            UID = Com.GetUUID(),
                            ParentUID = parent_uid,
                            Level = parent_level + 1,
                            MenuName = m.MenuName,
                            Description = "导入的数据",
                            Url = m.Url,
                            PermissionJson = (m.Permissions ?? string.Empty).Split(',').Where(i => ValidateHelper.IsPlumpString(i)).ToJson(),
                            GroupKey = m.SysID.ToString(),
                            CreateTime = now,
                            UpdateTime = now,
                        };
                        db.MenuEntity.Add(model);

                        var children = list.Where(x => x.ParentMenuID == m.MenuID.ToString()).ToList();
                        xx(model.UID, model.Level, children);
                    }

                    db.SaveChanges();
                }
            }

            foreach (var sys in list.Select(x => x.SysID).Distinct())
            {
                xx(string.Empty, 1, list.Where(x => x.SysID == sys && first(x)).ToList());
            }
        }

        private void systemx(List<T_Systems> list)
        {
            if (!ValidateHelper.IsPlumpList(list)) { return; }
            using (var db = new EntityDB())
            {
                if (db.SystemEntity.Any())
                {
                    Console.WriteLine("系统已存在");
                    return;
                }
                db.SystemEntity.AddRange(list.Select(x => new SystemEntity()
                {
                    UID = x.SysID.ToString(),
                    Name = x.SysName,
                    Url = x.DefaultUrl,
                    ImageUrl = x.ImageUrl,
                    Flag = x.SysID.ToString(),
                    CreateTime = now,
                    UpdateTime = now
                }));
                db.SaveChanges();
            }
        }

        private void user(List<T_UserInfo> list)
        {
            if (!ValidateHelper.IsPlumpList(list)) { return; }
            using (var db = new EntityDB())
            {
                if (db.UserEntity.Any())
                {
                    Console.WriteLine("系统已存在");
                    return;
                }
                db.UserEntity.AddRange(list.Select(x => new UserEntity()
                {
                    UID = x.UID,
                    Flag = x.IID,//临时存放
                    UserName = x.UserName,
                    NickName = x.CompanyName,
                    PassWord = x.PassWord,
                    Phone = x.Mobile,
                    Email = x.Email,
                    Sex = x.Sex ?? 1,
                    CreateTime = now,
                    UpdateTime = now,
                }).ToList());

                db.SaveChanges();
            }
        }

        private void role(List<Auth_Role> list)
        {
            if (!ValidateHelper.IsPlumpList(list)) { return; }
            using (var db = new EntityDB())
            {
                if (db.RoleEntity.Any())
                {
                    Console.WriteLine("角色已存在");
                    return;
                }
                db.RoleEntity.AddRange(list.Select(x => new RoleEntity()
                {
                    UID = x.RoleID.ToString(),
                    RoleName = x.RoleName,
                    RoleDescription = x.RoleDescription,
                    CreateTime = now,
                    UpdateTime = now
                }.AsFirstLevel()).ToList());
                db.SaveChanges();
            }
        }

        private void user_role(List<Auth_UserRole> list)
        {
            if (!ValidateHelper.IsPlumpList(list)) { return; }
            using (var db = new EntityDB())
            {
                if (db.UserRoleEntity.Any())
                {
                    Console.WriteLine("角色已存在");
                    return;
                }
                db.UserRoleEntity.AddRange(list.Select(x => new UserRoleEntity()
                {
                    UID = x.UserRoleID.ToString(),
                    UserID = x.UserID,
                    RoleID = x.RoleID,
                    CreateTime = now,
                    UpdateTime = now
                }).ToList());
                db.SaveChanges();
            }
        }

        private void permission(List<Auth_Permission> list, List<T_Systems> sys)
        {
            if (!ValidateHelper.IsPlumpList(list)) { return; }
            using (var db = new EntityDB())
            {
                if (db.PermissionEntity.Any())
                {
                    Console.WriteLine("角色已存在");
                    return;
                }
            }

            void xx(string parent_uid, int parent_level, List<Auth_Permission> data)
            {
                if (!ValidateHelper.IsPlumpList(data)) { return; }

                using (var db = new EntityDB())
                {
                    foreach (var m in data)
                    {
                        var model = new PermissionEntity()
                        {
                            UID = Com.GetUUID(),
                            Name = m.PermissionID,
                            Description = m.PermissionName,
                            Level = parent_level + 1,
                            ParentUID = parent_uid,
                            CreateTime = now,
                            UpdateTime = now
                        };
                        db.PermissionEntity.Add(model);

                        var children = list.Where(x => x.ParentPermissionID == m.PermissionID).ToList();
                        xx(model.UID, model.Level, children);
                    }

                    db.SaveChanges();
                }
            }

            foreach (var m in sys.Select(x => x.SysID).Distinct())
            {
                var s = sys.First(x => x.SysID == m);
                var sys_root = new Auth_Permission()
                {
                    PermissionID = Com.GetUUID(),
                    ParentPermissionID = string.Empty,
                    PermissionName = s.SysName,
                    SysID = m
                };
                foreach (var per in list.Where(x => x.SysID == m))
                {
                    if (!ValidateHelper.IsPlumpString(per.ParentPermissionID))
                    {
                        per.ParentPermissionID = sys_root.PermissionID;
                    }
                }
                list.Add(sys_root);

                xx(string.Empty, 1, list.Where(x => x.SysID == m &&
                !ValidateHelper.IsPlumpString(x.ParentPermissionID?.Trim())).ToList());
            }
        }

        private void role_permission(List<Auth_PermissionMap> list)
        {
            if (!ValidateHelper.IsPlumpList(list)) { return; }
            using (var db = new EntityDB())
            {
                if (db.RolePermissionEntity.Any())
                {
                    Console.WriteLine("角色已存在");
                    return;
                }
                var data = db.PermissionEntity.ToList();

                db.RolePermissionEntity.AddRange(list.Select(x => new RolePermissionEntity()
                {
                    UID = x.MapID.ToString(),
                    RoleID = x.MapKey.Split(':')[1],
                    PermissionID = data.Where(m => m.Name == x.PermissionID).Select(d => d.UID).FirstOrDefault() ?? "错误",
                    CreateTime = now,
                    UpdateTime = now
                }).ToList());
                db.SaveChanges();
            }
        }

        public void run()
        {
            using (var db = new SSODB())
            {
                var syslist = db.T_Systems.ToList();
                var menu = db.Auth_Menu.ToList();

                var user = db.T_UserInfo.ToList();
                var userrole = db.Auth_UserRole.ToList();
                var role = db.Auth_Role.ToList();

                var pers = db.Auth_Permission.ToList();
                var persmap = db.Auth_PermissionMap.ToList();
                Console.WriteLine("数据读取完成");
                //
                this.menu(menu);
                Console.WriteLine("menu finish");
                this.systemx(syslist);
                Console.WriteLine("system finish");
                this.user(user);
                Console.WriteLine("user finish");
                this.user_role(userrole);
                Console.WriteLine("user-role finish");
                this.role(role);
                Console.WriteLine("role finish");
                this.permission(pers, syslist);
                Console.WriteLine("permission finish");
                this.role_permission(persmap);
                Console.WriteLine("role-permission finish");
            }
            Console.WriteLine("finish");
        }
    }
}
