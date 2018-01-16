using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Lib.helper;
using Lib.data;
using Lib.core;
using Lib.extension;
using Lib.infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Lib.infrastructure.entity
{
    [Serializable]
    public abstract class TreeEntityBase : BaseEntity
    {
        /// <summary>
        /// 默认值是1
        /// </summary>
        public const int FIRST_LEVEL = 1;

        /// <summary>
        /// 默认值是空字符串
        /// </summary>
        public static readonly string FIRST_PARENT_UID = string.Empty;

        /// <summary>
        /// 默认值是default
        /// </summary>
        public static readonly string DEFAULT_GROUP = "default".Trim();

        /// <summary>
        /// 层级
        /// </summary>
        [Range(FIRST_LEVEL, FIRST_LEVEL + 500, ErrorMessage = "层级不在范围之内")]
        public virtual int Level { get; set; } = FIRST_LEVEL;

        /// <summary>
        /// 父级UID
        /// </summary>
        [StringLength(100, ErrorMessage = "父级UID长度错误")]
        public virtual string ParentUID { get; set; } = FIRST_PARENT_UID;

        /// <summary>
        /// 分组
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "分组长度错误")]
        public virtual string GroupKey { get; set; } = DEFAULT_GROUP;
    }
}
