using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebLogic.Model.Page
{
    /// <summary>
    /// 内容
    /// </summary>
    [Table("wp_section")]
    public class SectionModel : BaseEntity
    {
        public SectionModel()
        {
            SectionName = SectionDescription = SectionContent = string.Empty;
        }

        /// <summary>
        /// id
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("section_id")]
        public virtual int SectionID { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        [Column("section_name")]
        public virtual string SectionName { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [Column("section_title")]
        public virtual string SectionTitle { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [Column("section_description")]
        public virtual string SectionDescription { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [Column("section_content")]
        public virtual string SectionContent { get; set; }

        /// <summary>
        /// 内容类型(section/page/news)
        /// </summary>
        [Column("section_type")]
        public virtual string SectionType { get; set; }

        /// <summary>
        /// 关联key
        /// </summary>
        [Column("rel_group")]
        public virtual string RelGroup { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        [Column("update_time")]
        public virtual DateTime UpdateTime { get; set; }

    }
}
