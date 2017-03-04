using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model;
using Lib.model;
using Lib;
using Lib.core;
using Lib.helper;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Model.Sys
{
    [Table("wp_options")]
    public class OptionModel : BaseEntity
    {
        /// <summary>
        /// id
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("option_id")]
        public virtual int OptionID { get; set; }

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
