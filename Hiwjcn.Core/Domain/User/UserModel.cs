using Lib.data;
using Lib.helper;
using Lib.mvc.user;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebLogic.Model.User;
using Lib.core;

namespace Model.User
{
    [Table("account_user_avatar")]
    public class UserAvatar : BaseEntity
    {
        [Required(ErrorMessage = "头像数据为空")]
        public virtual byte[] AvatarBytes { get; set; }

        [Index]
        [Required(ErrorMessage = "UserUID必填")]
        [StringLength(100, MinimumLength = 20, ErrorMessage = "UserUID长度错误")]
        public virtual string UserUID { get; set; }
    }

    /// <summary>
    ///用户的账户模型
    /// </summary>
    [Table("account_user")]
    public class UserModel : BaseEntity
    {
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
        public virtual int Sex { get; set; }

        /// <summary>
        /// 解析性别
        /// </summary>
        /// <param name="sex"></param>
        /// <returns></returns>
        public static string ParseSex(int sex)
        {
            try
            {
                return ((SexEnum)sex).ToString();
            }
            catch
            {
                return sex.ToString();
            }
        }

        /// <summary>
        /// 头像链接
        /// </summary>
        [Column("user_img")]
        public virtual string UserImg { get; set; }

        /// <summary>
        /// 用户拥有的权限（大分类）
        /// </summary>
        [Column("user_flag")]
        public virtual int Flag { get; set; }

        /// <summary>
        /// 用户所有角色id
        /// </summary>
        [NotMapped]
        public virtual List<string> RoleList { get; set; }

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
            return "/user/sendmessage/?to=" + this.IID;
        }

        public virtual string GetUserImgUrl()
        {
            if (ValidateHelper.IsPlumpString(this.UserImg)) { return this.UserImg; }
            return "/user/usermask/" + this.IID + "/";
        }
    }

    public class UserModelMapping : EFMappingBase<UserModel>
    {
        public UserModelMapping()
        {
            this.ToTable("wp_users").HasKey(x => x.IID);
            this.Property(x => x.IID).HasColumnName("user_id").HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.Property(x => x.NickName).HasColumnName("nick_name");
            this.Ignore(x => x.RoleModelList);
        }
    }

    public class UserCountGroupBySex
    {
        public virtual int Sex { get; set; }

        public virtual string SexStr
        {
            get
            {
                return UserModel.ParseSex(this.Sex);
            }
        }

        public virtual int Count { get; set; }

    }
}