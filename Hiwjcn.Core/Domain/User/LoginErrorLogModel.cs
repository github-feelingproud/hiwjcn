using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebLogic.Model.User
{
    [Table("account_login_log")]
    public class LoginErrorLogModel : BaseEntity
    {
        [Column("login_key")]
        public virtual string LoginKey { get; set; }

        [Column("login_pwd")]
        public virtual string LoginPwd { get; set; }

        [Column("login_ip")]
        public virtual string LoginIP { get; set; }
    }
}
