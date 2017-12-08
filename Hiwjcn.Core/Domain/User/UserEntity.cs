using Lib.core;
using Lib.data.ef;
using Lib.helper;
using Lib.infrastructure.entity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hiwjcn.Core.Domain.User
{
    [Serializable]
    [Table("account_user_avatar")]
    public class UserAvatarEntity : UserAvatarEntityBase { }

    /// <summary>
    /// 一次性登录用的code
    /// </summary>
    [Serializable]
    [Table("account_user_onetimecode")]
    public class UserOneTimeCodeEntity : UserOneTimeCodeEntityBase { }

    /// <summary>
    ///用户的账户模型
    /// </summary>
    [Serializable]
    [Table("account_user")]
    public class UserEntity : UserEntityBase
    {
        /// <summary>
        /// 余额
        /// </summary>
        [Column("user_money")]
        public virtual decimal Money { get; set; }
        
        /// <summary>
        /// 用户介绍
        /// </summary>
        [Column("user_mark")]
        [StringLength(500)]
        public virtual string Introduction { get; set; }

        /// <summary>
        /// qq
        /// </summary>
        [Column("user_qq")]
        [StringLength(30)]
        public virtual string QQ { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        [Column("user_sex")]
        public virtual int Sex { get; set; }

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
    }
    
    public class UserModelMapping : EFMappingBase<UserEntity>
    {
        public UserModelMapping()
        {
            this.ToTable("wp_users").HasKey(x => x.IID);
            this.Property(x => x.IID).HasColumnName("user_id").HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.Property(x => x.NickName).HasColumnName("nick_name");
            this.Ignore(x => x.RoleIds);
        }
    }

    public class UserCountGroupBySex
    {
        public virtual int Sex { get; set; }

        public virtual string SexStr
        {
            get
            {
                return UserEntity.ParseSex(this.Sex);
            }
        }

        public virtual int Count { get; set; }

    }
}