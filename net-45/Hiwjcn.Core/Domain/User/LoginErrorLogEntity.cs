using Lib.infrastructure.entity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hiwjcn.Core.Domain.User
{
    [Serializable]
    [Table("account_login_log")]
    public class LoginErrorLogEntity : BaseEntity
    {
        [MaxLength(200)]
        public virtual string LoginKey { get; set; }

        [MaxLength(200)]
        public virtual string LoginPwd { get; set; }

        [MaxLength(50)]
        public virtual string LoginIP { get; set; }

        [MaxLength(500)]
        public virtual string ErrorMsg { get; set; }
    }
}
