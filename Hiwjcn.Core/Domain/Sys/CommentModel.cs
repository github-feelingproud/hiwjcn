using Model;
using Model.User;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiwjcn.Core.Model.Sys
{
    /// <summary>
    /// 评论系统
    /// </summary>
    [Table("wp_comment")]
    public class CommentModel : BaseEntity
    {
        /// <summary>
        /// 评论ID
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("comment_id")]
        public virtual int CommentID { get; set; }

        /// <summary>
        /// 评论内容
        /// </summary>
        [Column("comment_content")]
        public virtual string CommentContent { get; set; }

        /// <summary>
        /// 评论人
        /// </summary>
        [Column("user_id")]
        public virtual int UserID { get; set; }

        /// <summary>
        /// 评论人
        /// </summary>
        [NotMapped]
        public virtual UserModel UserModel { get; set; }

        /// <summary>
        /// 评论对象ID
        /// </summary>
        [Column("thread_id")]
        public virtual string ThreadID { get; set; }

        /// <summary>
        /// 回复对象id
        /// </summary>
        [Column("parent_comment_id")]
        public virtual int ParentCommentID { get; set; }

        /// <summary>
        /// 父级回复
        /// </summary>
        [NotMapped]
        public virtual CommentModel ParentComment { get; set; }

        /// <summary>
        /// 评论时间
        /// </summary>
        [Column("update_time")]
        public virtual DateTime UpdateTime { get; set; }
    }

}
