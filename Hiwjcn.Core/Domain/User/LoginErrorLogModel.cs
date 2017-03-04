using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using Model;
using Lib;
using Lib.core;
using Lib.helper;
using Lib.model;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebLogic.Model.User
{
    [Table("wp_login_log")]
    public class LoginErrorLogModel : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("log_id")]
        public virtual int LogID { get; set; }

        [Column("login_key")]
        public virtual string LoginKey { get; set; }

        [Column("login_pwd")]
        public virtual string LoginPwd { get; set; }

        [Column("login_ip")]
        public virtual string LoginIP { get; set; }

        [Column("login_time")]
        public virtual DateTime LoginTime { get; set; }
    }
}
