using Lib.data;
using Lib.helper;
using Lib.mvc.user;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebLogic.Model.User;

namespace Model.User
{
    [Table("user_avatar")]
    public class UserAvatar : IDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int IID { get; set; }

        [Index]
        [Required(ErrorMessage = "UID必填")]
        [StringLength(100, MinimumLength = 20, ErrorMessage = "UID长度错误")]
        public virtual string UID { get; set; }

        [Required(ErrorMessage = "头像数据为空")]
        public virtual byte[] AvatarBytes { get; set; }

        [Index]
        [Required(ErrorMessage = "UserUID必填")]
        [StringLength(100, MinimumLength = 20, ErrorMessage = "UserUID长度错误")]
        public virtual string UserUID { get; set; }

        public virtual DateTime CreateTime { get; set; }

    }

    /// <summary>
    ///用户的账户模型
    /// </summary>
    [Table("wp_users")]
    public class UserModel : BaseEntity//, UserModelBase
    {

        /// <summary>
        /// 用户ID
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("user_id")]
        public virtual int UserID { get; set; }

        [Column(nameof(UID))]
        [Editable(false)]
        [Required(ErrorMessage = "UID不能为空")]
        [MinLength(30, ErrorMessage = "UID最短长度30")]
        [MaxLength(40, ErrorMessage = "UID长度最长40")]
        public virtual string UID { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        [Column("nick_name")]
        [StringLength(10, MinimumLength = 5, ErrorMessage = "昵称长度不匹配")]
        public virtual string NickName { get; set; }

        /// <summary>
        /// md5加密的密码
        /// </summary>
        [Column("user_pass")]
        [StringLength(50, MinimumLength = 10, ErrorMessage = "密码长度错误")]
        public virtual string PassWord { get; set; }

        /// <summary>
        /// 余额
        /// </summary>
        [Column("user_money")]
        public virtual decimal Money { get; set; }

        /// <summary>
        /// 电话
        /// </summary>
        [Column("user_phone")]
        public virtual string Phone { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        [Column("user_email")]
        [EmailAddress(ErrorMessage = "邮件格式错误")]
        public virtual string Email { get; set; }

        /// <summary>
        /// 用户介绍
        /// </summary>
        [Column("user_mark")]
        public virtual string Introduction { get; set; }

        /// <summary>
        /// qq
        /// </summary>
        [Column("user_qq")]
        public virtual string QQ { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        [Column("user_sex")]
        public virtual string Sex { get; set; }

        /// <summary>
        /// 解析性别
        /// </summary>
        /// <param name="sex"></param>
        /// <returns></returns>
        public static string ParseSex(string sex)
        {
            if (sex == "1") { return "男"; }
            if (sex == "0") { return "女"; }
            return "未知";
        }

        /// <summary>
        /// 头像链接
        /// </summary>
        [Column("user_img")]
        public virtual string UserImg { get; set; }

        /// <summary>
        /// 注册时间
        /// </summary>
        [Column("user_reg_time")]
        public virtual DateTime RegTime { get; set; }

        /// <summary>
        /// 用户拥有的权限（大分类）
        /// </summary>
        [Column("user_flag")]
        public virtual int Flag { get; set; }

        /// <summary>
        /// 用户所有角色id
        /// </summary>
        [NotMapped]
        public virtual List<int> RoleList { get; set; }

        /// <summary>
        /// 用户所有权限id（最终拿来验证用户行为是否有权限）
        /// </summary>
        [NotMapped]
        public virtual List<string> PermissionList { get; set; }

        /// <summary>
        /// 用户角色模型
        /// </summary>
        [NotMapped]
        public virtual List<RoleModel> RoleModelList { get; set; }

        /// <summary>
        /// Token
        /// </summary>
        [NotMapped]
        public virtual string UserToken { get; set; }

        /// <summary>
        /// 获取联系页面
        /// </summary>
        /// <returns></returns>
        public virtual string GetContactPage()
        {
            if (ValidateHelper.IsPlumpString(this.QQ))
            {
                return string.Format("http://wpa.qq.com/msgrd?v=3&uin={0}&site=qq&menu=yes", this.QQ);
            }
            return "/user/sendmessage/?to=" + this.UserID;
        }

        public virtual string GetUserImgUrl()
        {
            if (ValidateHelper.IsPlumpString(this.UserImg)) { return this.UserImg; }
            return "/user/usermask/" + this.UserID + "/";
        }
    }

    public class UserModelMapping : EFMappingBase<UserModel>
    {
        public UserModelMapping()
        {
            this.ToTable("wp_users").HasKey(x => x.UserID);
            this.Property(x => x.UserID).HasColumnName("user_id").HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.Property(x => x.NickName).HasColumnName("nick_name");
            this.Ignore(x => x.RoleModelList);
        }
    }

    public class UserCountGroupBySex
    {
        public virtual string Sex { get; set; }

        public virtual int Count { get; set; }

    }
}