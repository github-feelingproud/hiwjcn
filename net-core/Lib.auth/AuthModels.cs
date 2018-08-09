﻿using Lib.extension;
using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Lib.auth
{
    [Serializable]
    [DataContract]
    public class TokenModel
    {
        [DataMember]
        public virtual string Token { get; set; }
        [DataMember]
        public virtual string RefreshToken { get; set; }
        [DataMember]
        public virtual DateTime Expire { get; set; }
    }

    [Serializable]
    [DataContract]
    public class CacheBundle
    {
        [DataMember]
        public virtual string[] UserUID { get; set; }

        [DataMember]
        public virtual string[] TokenUID { get; set; }
    }

    [Serializable]
    [DataContract]
    public class UserPermissions
    {
        [DataMember]
        public virtual List<string> PermissionIds { get; set; }

        [DataMember]
        public virtual List<string> RoleIds { get; set; }

        [DataMember]
        public virtual List<string> DepartmentIds { get; set; }
    }

    /// <summary>
    /// 为汽配龙业务提供的数据
    /// </summary>
    [Serializable]
    [DataContract]
    public class QPLData
    {
        [DataMember]
        public virtual double Lon { get; set; }

        [DataMember]
        public virtual double Lat { get; set; }

        [DataMember]
        public virtual string TraderShopType { get; set; }

        [DataMember]
        public virtual decimal MaxServiceDistance { get; set; }

        [DataMember]
        public virtual bool IsGeneralDelivery { get; set; }

        [DataMember]
        public virtual bool IsQuickArrive { get; set; }

        [DataMember]
        public virtual string TraderId { get; set; }

        [DataMember]
        public virtual string TraderName { get; set; }

        [DataMember]
        public virtual int IsCheck { get; set; }

        [DataMember]
        public virtual string CustomerType { get; set; }

        [DataMember]
        public virtual int IsHaveInquiry { get; set; }

        [DataMember]
        public virtual string LocationId { get; set; }

        [DataMember]
        public virtual int IsSelf { get; set; }
    }

    /// <summary>
    /// 记录登陆信息，可以序列化
    /// </summary>
    [Serializable]
    [DataContract]
    public class LoginUserInfo : QPLData
    {
        public LoginUserInfo() { }

        [DataMember]
        public virtual long IID { get; set; }

        [DataMember]
        public virtual string UserID { get; set; }

        [Obsolete("等同于UserID")]
        [DataMember]
        public virtual string UserUID
        {
            get => this.UserID;
            set => this.UserID = value;
        }

        [DataMember]
        public virtual string UserName { get; set; }

        [DataMember]
        public virtual string NickName { get; set; }

        [DataMember]
        public virtual string Email { get; set; }

        [DataMember]
        public virtual string Phone { get; set; }

        [DataMember]
        public virtual string HeadImgUrl { get; set; }

        [DataMember]
        public virtual int IsActive { get; set; }

        [DataMember]
        public virtual bool IsValid { get; set; } = false;

        [DataMember]
        public virtual string LoginToken { get; set; }

        [DataMember]
        public virtual string RefreshToken { get; set; }

        [DataMember]
        public virtual DateTime TokenExpire { get; set; }

        [DataMember]
        public virtual List<string> Roles { get; set; }

        [DataMember]
        public virtual List<string> Permissions { get; set; }

        [DataMember]
        public virtual Dictionary<string, string> ExtraData { get; set; }


        public static implicit operator LoginUserInfo(string json) => json.JsonToEntity<LoginUserInfo>();
    }

    public static class LoginUserInfoExtension
    {
        private static readonly IReadOnlyCollection<Type> map_type = new List<Type>()
        {
            typeof(string),typeof(DateTime),typeof(DateTime?),
            typeof(int),typeof(double),typeof(float),typeof(decimal),typeof(long),
            typeof(int?),typeof(double?),typeof(float?),typeof(decimal?),typeof(long?)
        }.AsReadOnly();

        public static void MapExtraData(this LoginUserInfo loginuser, object model)
        {
            if (model == null)
            {
                throw new Exception($"{nameof(LoginUserInfoExtension)}.{nameof(MapExtraData)}.{nameof(model)}不能为空");
            }
            var props = model.GetType().GetProperties().ToList();
            props = props.Where(x => x.CanRead && x.CanWrite && map_type.Contains(x.PropertyType)).ToList();

            foreach (var p in props)
            {
                var value = ConvertHelper.GetString(p.GetValue(model));
                loginuser.AddExtraData(p.Name, value);
            }
        }

        public static string UserNameOrNickName(this LoginUserInfo loginuser) => loginuser.NickName ?? loginuser.UserName;

        /// <summary>
        /// 去除权限等敏感信息
        /// </summary>
        public static void ClearPrivateInfo(this LoginUserInfo loginuser)
        {
            loginuser.Roles?.Clear();
            loginuser.Permissions?.Clear();
        }

        /// <summary>
        /// 判断用户是否有角色
        /// </summary>
        /// <param name="loginuser"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public static bool HasRole(this LoginUserInfo loginuser, string role) =>
            ValidateHelper.IsPlumpList(loginuser.Roles) && loginuser.Roles.Contains(role);

        /// <summary>
        /// 判断用户是否有权限
        /// </summary>
        public static bool HasPermission(this LoginUserInfo loginuser, string permission) =>
            ValidateHelper.IsPlumpList(loginuser.Permissions) && loginuser.Permissions.Contains(permission);

        /// <summary>
        /// 添加额外信息
        /// </summary>
        public static void AddExtraData(this LoginUserInfo loginuser, string key, string value)
        {
            if (loginuser.ExtraData == null) { loginuser.ExtraData = new Dictionary<string, string>(); }
            loginuser.ExtraData[key] = value;
        }

        /// <summary>
        /// 获取额外信息
        /// </summary>
        public static string GetExtraData(this LoginUserInfo loginuser, string key)
        {
            if (loginuser.ExtraData == null) { loginuser.ExtraData = new Dictionary<string, string>(); }
            if (loginuser.ExtraData.TryGetValue(key, out var value))
            {
                return value;
            }
            return null;
        }

    }
}
