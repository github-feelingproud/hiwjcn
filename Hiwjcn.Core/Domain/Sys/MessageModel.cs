using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model;
using Lib.model;
using Lib;
using Lib.core;
using Lib.http;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Model.Sys
{
    [Table("wp_msg")]
    public class MessageModel : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("msg_id")]
        public virtual int MsgID { get; set; }

        [Column("msg_title")]
        public virtual string Title { get; set; }

        [Column("msg_content")]
        public virtual string MsgContent { get; set; }

        [Column("msg_time")]
        public virtual DateTime UpdateTime { get; set; }

        [Column("msg_sender")]
        public virtual int SenderUserID { get; set; }

        [Column("msg_receiver")]
        public virtual int ReceiverUserID { get; set; }

        [Column("msg_new")]
        public virtual string IsNew { get; set; }
    }
}
