using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Category
{
    /// <summary>
    /// 树结构
    /// </summary>
    [Table("wp_category")]
    public class CategoryModel : BaseEntity
    {
        /// <summary>
        /// 节点ID
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("category_id")]
        public virtual int CategoryID { get; set; }

        /// <summary>
        /// 节点名称
        /// </summary>
        [Column("category_name")]
        public virtual string CategoryName { get; set; }

        /// <summary>
        /// 节点描述
        /// </summary>
        [Column("category_description")]
        public virtual string CategoryDescription { get; set; }

        /// <summary>
        /// 节点图片
        /// </summary>
        [Column("category_image")]
        public virtual string CategoryImage { get; set; }

        /// <summary>
        /// 节点的背景图
        /// </summary>
        [Column("background_image")]
        public virtual string BackgroundImage { get; set; }

        /// <summary>
        /// 节点颜色
        /// </summary>
        [Column("category_color")]
        public virtual string CategoryColor { get; set; }

        /// <summary>
        /// 节点图标类
        /// </summary>
        [Column("icon_class")]
        public virtual string IconClass { get; set; }

        /// <summary>
        /// 节点URL
        /// </summary>
        [Column("link_url")]
        public virtual string LinkURL { get; set; }

        /// <summary>
        /// 节点是否新窗口打开
        /// </summary>
        [Column("open_in_new_window")]
        public virtual string OpenInNewWindow { get; set; }

        /// <summary>
        /// 是否新窗体打开
        /// </summary>
        /// <returns></returns>
        public virtual string OpenTarget()
        {
            return OpenInNewWindow == "true" ? "_blank" : "_self";
        }

        /// <summary>
        /// 节点排序
        /// </summary>
        [Column("order_num")]
        public virtual int OrderNum { get; set; }

        /// <summary>
        /// 节点父级
        /// </summary>
        [Column("category_parent")]
        public virtual int CategoryParent { get; set; }

        /// <summary>
        /// 节点层级
        /// </summary>
        [Column("category_level")]
        public virtual int CategoryLevel { get; set; }

        /// <summary>
        /// 节点类型
        /// </summary>
        [Column("category_type")]
        public virtual string CategoryType { get; set; }
    }
}
