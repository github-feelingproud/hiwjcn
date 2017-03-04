using Lib.core;
using Model.Category;
using Model.User;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;

namespace Model.Post
{
    /// <summary>
    ///日志的模型类
    /// </summary>
    [Table("wp_posts")]
    public class PostModel : BaseEntity
    {
        /// <summary>
        /// 论坛分类类别
        /// </summary>
        public const string PostCategoryType = "post_category";

        /// <summary>
        /// 文章ID
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("post_id")]
        public virtual int PostID { get; set; }

        /// <summary>
        /// 所属酒店
        /// </summary>
        [Column("HotelNo")]
        public virtual string HotelNo { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [Column("post_title")]
        public virtual string Title { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [Column("post_content")]
        public virtual string Content { get; set; }

        /// <summary>
        /// 预览
        /// </summary>
        [Column("post_preview")]
        public virtual string Preview { get; set; }

        /// <summary>
        /// 更新日期
        /// </summary>
        [Column("post_date")]
        public virtual DateTime UpdateTime { get; set; }

        /// <summary>
        /// 文章GUID
        /// </summary>
        [Column("post_guid")]
        public virtual string PostGuid { get; set; }

        /// <summary>
        /// 标签的数组字段，不和数据库关联
        /// </summary>
        [NotMapped]
        public virtual List<string> PostTagsList { get; set; }

        /// <summary>
        /// 作者
        /// </summary>
        [Column("post_author")]
        public virtual int AuthorID { get; set; }

        /// <summary>
        /// 作者详细信息
        /// </summary>
        [NotMapped]
        public virtual UserModel UserModel { get; set; }

        /// <summary>
        /// 阅读次数
        /// </summary>
        [Column("read_count")]
        public virtual int ReadCount { get; set; }

        /// <summary>
        /// 评论数
        /// </summary>
        [Column("comment_count")]
        public virtual int CommentCount { get; set; }

        /// <summary>
        /// 置顶
        /// </summary>
        [Column("sticky_top")]
        public virtual string StickyTop { get; set; }

        /// <summary>
        /// 是否置顶
        /// </summary>
        /// <returns></returns>
        public virtual bool IsStickyTop()
        {
            return StickyTop == "true";
        }

    }

    public class PostCountGroupByDate
    {
        public virtual string Date { get; set; }

        public virtual int Count { get; set; }
    }

}