using Hiwjcn.Core.Domain.User;
using Lib.infrastructure.entity;
using Lib.infrastructure.entity.auth;
using Lib.infrastructure.entity.user;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EPC.Core.Entity
{
    [Serializable]
    [Table("tb_issue")]
    public class IssueEntity : TimeEntityBase, IEpcDBTable
    {
        [Required(ErrorMessage = "标题不能为空")]
        public virtual string Title { get; set; }

        public virtual string Content { get; set; }

        public virtual string ContentMarkdown { get; set; }

        [Required(ErrorMessage = "组织ID为空")]
        public virtual string OrgUID { get; set; }

        [Required(ErrorMessage = "用户ID为空")]
        public virtual string UserUID { get; set; }

        public virtual string AssignedUserUID { get; set; }

        public virtual int IsClosed { get; set; }

        public virtual int SecondsToTakeToClose { get; set; }

        public virtual DateTime? Start { get; set; }

        public virtual string DeviceUID { get; set; }

        public virtual int CommentCount { get; set; }

        [NotMapped]
        public virtual List<IssueOperationLogEntity> OperationLog { get; set; }

        /// <summary>
        /// 设备
        /// </summary>
        [NotMapped]
        public virtual DeviceEntity DeviceModel { get; set; }

        /// <summary>
        /// 发起人
        /// </summary>
        [NotMapped]
        public virtual UserEntity UserModel { get; set; }

        /// <summary>
        /// 被指派的人
        /// </summary>
        [NotMapped]
        public virtual UserEntity AssignedUserModel { get; set; }
    }

    [Serializable]
    [Table("tb_issue_operation_log")]
    public class IssueOperationLogEntity : TimeEntityBase, IEpcDBTable
    {
        [Required(ErrorMessage = "组织ID为空")]
        public virtual string OrgUID { get; set; }

        [Required(ErrorMessage = "工单ID为空")]
        public virtual string IssueUID { get; set; }

        public virtual string Content { get; set; }

        public virtual int IsClosed { get; set; }

        public virtual string Operation { get; set; }

        [Required(ErrorMessage = "用户ID为空")]
        public virtual string UserUID { get; set; }

        [NotMapped]
        public virtual UserEntity UserModel { get; set; }
    }

}
