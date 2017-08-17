using Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebLogic.Model.User
{
    [Serializable]
    [Table("account_login_log")]
    public class LoginErrorLogModel : BaseEntity
    {
        public virtual string LoginKey { get; set; }

        public virtual string LoginPwd { get; set; }

        public virtual string LoginIP { get; set; }

        public virtual string ErrorMsg { get; set; }
    }
}
