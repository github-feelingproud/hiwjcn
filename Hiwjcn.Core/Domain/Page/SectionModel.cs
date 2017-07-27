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
    [Table("sys_section")]
    public class SectionModel : BaseEntity
    {
        public SectionModel()
        {
            SectionName = SectionDescription = SectionContent = string.Empty;
        }

        /// <summary>
        /// 名称
        /// </summary>
        [Column("section_name")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "页面名称为空")]
        public virtual string SectionName { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [Column("section_title")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "页面标题为空")]
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
        [StringLength(20, MinimumLength = 2, ErrorMessage = "页面类型为空")]
        public virtual string SectionType { get; set; }

        /// <summary>
        /// 关联key
        /// </summary>
        [Column("rel_group")]
        public virtual string RelGroup { get; set; }
    }
}
