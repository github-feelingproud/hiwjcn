using Lib.infrastructure.entity;
using Lib.infrastructure.entity.auth;
using Lib.infrastructure.entity.user;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EPC.Core.Entity
{
    [Serializable]
    [Table("tb_device")]
    public class DeviceEntity : BaseEntity, IEpcDBTable
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        [Required(ErrorMessage = "设备名不能为空")]
        public virtual string Name { get; set; }

        /// <summary>
        /// 设备描述
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// 最后编辑人
        /// </summary>
        [Required(ErrorMessage = "用户信息丢失")]
        public virtual string UserUID { get; set; }

        /// <summary>
        /// 所属组织
        /// </summary>
        [Required(ErrorMessage = "组织信息丢失")]
        public virtual string OrgUID { get; set; }

        /// <summary>
        /// 设备关联的参数
        /// </summary>
        [NotMapped]
        public virtual List<DeviceParameterEntity> ParamsList { get; set; }
    }
}
