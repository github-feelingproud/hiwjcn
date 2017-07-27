using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Sys
{
    [Table("sys_links")]
    public class LinkModel : BaseEntity
    {
        /// <summary>
        /// 链接id
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("link_id")]
        public virtual int LinkID { get; set; }

        /// <summary>
        /// 链接名称
        /// </summary>
        [Column("link_name")]
        public virtual string Name { get; set; }

        /// <summary>
        /// 链接url
        /// </summary>
        [Column("link_url")]
        public virtual string Url { get; set; }

        /// <summary>
        /// 链接标题
        /// </summary>
        [Column("link_title")]
        public virtual string Title { get; set; }

        /// <summary>
        /// 链接图片
        /// </summary>
        [Column("link_image")]
        public virtual string Image { get; set; }

        /// <summary>
        /// 目标（self blank）
        /// </summary>
        [Column("link_target")]
        public virtual string Target { get; set; }

        /// <summary>
        /// 排序字段
        /// </summary>
        [Column("order_num")]
        public virtual int OrderNum { get; set; }

        /// <summary>
        /// 所有人
        /// </summary>
        [Column("link_owner")]
        public virtual int UserID { get; set; }

        /// <summary>
        /// 链接的类型，比如友情链接、轮播图、合作伙伴
        /// </summary>
        [Column("link_type")]
        public virtual string LinkType { get; set; }

    }
}
