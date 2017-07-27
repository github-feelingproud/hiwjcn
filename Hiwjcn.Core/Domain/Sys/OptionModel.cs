using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Model.Sys
{
    [Table("sys_options")]
    public class OptionModel : BaseEntity
    {
        /// <summary>
        /// 名称
        /// </summary>
        [Column("option_name")]
        public virtual string Key { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        [Column("option_value")]
        public virtual string Value { get; set; }
    }
}
