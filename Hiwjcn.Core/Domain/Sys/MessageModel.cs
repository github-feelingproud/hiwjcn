using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Sys
{
    [Table("sys_msg")]
    public class MessageModel : BaseEntity
    {
        [Column("msg_title")]
        public virtual string Title { get; set; }

        [Column("msg_content")]
        public virtual string MsgContent { get; set; }
        
        [Column("msg_sender")]
        public virtual string SenderUserID { get; set; }

        [Column("msg_receiver")]
        public virtual string ReceiverUserID { get; set; }

        [Column("msg_new")]
        public virtual string IsNew { get; set; }
    }
}
