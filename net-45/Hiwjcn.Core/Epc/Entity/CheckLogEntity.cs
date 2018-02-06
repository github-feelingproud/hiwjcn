using Hiwjcn.Core.Domain.User;
using Lib.infrastructure.entity;
using Lib.infrastructure.entity.auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPC.Core.Entity
{
    /// <summary>
    /// 设备点检信息
    /// </summary>
    [Serializable]
    [Table("tb_check_log")]
    public class CheckLogEntity : TimeEntityBase, IEpcDBTable
    {
        [Required]
        public virtual string OrgUID { get; set; }

        [Required]
        public virtual string UserUID { get; set; }

        [Required]
        public virtual string DeviceUID { get; set; }

        public virtual int StatusOK { get; set; }

        [NotMapped]
        public virtual List<string> Tips { get; set; }

        [NotMapped]
        public virtual UserEntity UserModel { get; set; }

        [NotMapped]
        public virtual DeviceEntity DeviceModel { get; set; }
    }

    /// <summary>
    /// 设备参数点检信息
    /// </summary>
    [Serializable]
    [Table("tb_check_log_item")]
    public class CheckLogItemEntity : TimeEntityBase, IEpcDBTable
    {
        [Required]
        public virtual string CheckUID { get; set; }

        [Required]
        public virtual string DeviceUID { get; set; }

        [Required]
        public virtual string ParameterUID { get; set; }

        [Required]
        public virtual string ParameterName { get; set; }

        public virtual string Rule { get; set; }

        public virtual string InputDataJson { get; set; }

        public virtual int StatusOK { get; set; }

        public virtual int InputType { get; set; }

        [NotMapped]
        public virtual List<string> Tips { get; set; }
    }
}
