using Lib.infrastructure.entity;
using Lib.infrastructure.entity.auth;
using Lib.infrastructure.entity.user;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace EPC.Core.Entity
{
    [Serializable]
    [Table("tb_device")]
    public class DeviceEntity : BaseEntity, IEpcDBTable
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// 设备描述
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// 最后编辑人
        /// </summary>
        public virtual string UserUID { get; set; }

        /// <summary>
        /// 所属组织
        /// </summary>
        public virtual string OrgUID { get; set; }

        /// <summary>
        /// 设备关联的参数
        /// </summary>
        [NotMapped]
        public virtual List<DeviceParameterEntity> ParamsList { get; set; }
    }
}
