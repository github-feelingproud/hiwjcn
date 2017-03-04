using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Lib.core;
using Lib.helper;

namespace WebLogic.Bll.User
{
    /// <summary>
    /// 写死的权限
    /// </summary>
    public class PermissionRecord
    {
        #region 权限模型
        public PermissionRecord()
        {
            PermissionID = PermissionName = string.Empty;
        }
        public PermissionRecord(string permission_id, string permission_name)
        {
            this.PermissionID = permission_id;
            this.PermissionName = permission_name;
        }
        /// <summary>
        /// 权限ID
        /// </summary>
        public virtual string PermissionID { get; set; }

        /// <summary>
        /// 权限名
        /// </summary>
        public virtual string PermissionName { get; set; }

        #endregion

        #region 定义的静态权限常量
        //管理系统
        public const string ManageSystemPermissionID = "1";
        public static readonly PermissionRecord ManageSystem = new PermissionRecord()
        {
            PermissionID = ManageSystemPermissionID,
            PermissionName = "管理系统"
        };
        //卖东西
        public const string SellGoodsPermissionID = "2";
        public static readonly PermissionRecord SellGoods = new PermissionRecord()
        {
            PermissionID = SellGoodsPermissionID,
            PermissionName = "卖东西"
        };
        //发帖子
        public const string PostPermissionID = "3";
        public static readonly PermissionRecord PostPost = new PermissionRecord()
        {
            PermissionID = PostPermissionID,
            PermissionName = "发帖子"
        };
        #endregion

        /// <summary>
        /// 获取所有权限
        /// 使用了反射
        /// </summary>
        /// <returns></returns>
        public static List<PermissionRecord> GetAllPermission()
        {
            List<PermissionRecord> list = new List<PermissionRecord>();

            Type cls = typeof(PermissionRecord);
            ConvertHelper.NotNullList(cls.GetFields()).ForEach(p =>
            {
                if (p.FieldType == cls)
                {
                    list.Add(p.GetValue(null) as PermissionRecord);
                }
            });

            return list.Where(x => x != null).ToList();
        }

        /// <summary>
        /// 全部验证通过
        /// </summary>
        /// <param name="UserPermissions"></param>
        /// <param name="permissions"></param>
        /// <returns></returns>
        public static bool AllPass(IEnumerable<string> UserPermissions, params string[] permissions)
        {
            if (!ValidateHelper.IsPlumpList(permissions)) { return false; }
            return permissions.Count(x => UserPermissions.Contains(x)) == permissions.Count();
        }

        /// <summary>
        /// 部分验证通过
        /// </summary>
        /// <param name="UserPermissions"></param>
        /// <param name="permissions"></param>
        /// <returns></returns>
        public static bool AnyPass(IEnumerable<string> UserPermissions, params string[] permissions)
        {
            if (!ValidateHelper.IsPlumpList(permissions)) { return false; }
            return permissions.Count(x => UserPermissions.Contains(x)) > 0;
        }
    }
}
