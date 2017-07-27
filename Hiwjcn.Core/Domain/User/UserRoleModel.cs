using Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebLogic.Model.User
{
    /// <summary>
    /// 用户角色关联
    /// </summary>
    [Table("account_user_role")]
    public class UserRoleModel : BaseEntity
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        [Column("user_id")]
        public virtual string UserID { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        [Column("role_id")]
        public virtual string RoleID { get; set; }
    }
}
