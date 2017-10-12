using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lib.extension;
using Lib.ioc;

namespace Lib.mvc.user
{
    /// <summary>
    /// 为汽配龙业务提供的数据
    /// </summary>
    [Serializable]
    public class QPLData
    {
        public virtual double Lon { get; set; }

        public virtual double Lat { get; set; }

        public virtual string TraderShopType { get; set; }

        public virtual decimal MaxServiceDistance { get; set; }

        public virtual bool IsGeneralDelivery { get; set; }

        public virtual bool IsQuickArrive { get; set; }

        public virtual string TraderId { get; set; }

        public virtual string TraderName { get; set; }

        public virtual int IsCheck { get; set; }

        public virtual string CustomerType { get; set; }

        public virtual int IsHaveInquiry { get; set; }

        public virtual string LocationId { get; set; }

        public virtual int IsSelf { get; set; }
    }

    /// <summary>
    /// 记录登陆信息，可以序列化
    /// </summary>
    [Serializable]
    public class LoginUserInfo : QPLData
    {
        public LoginUserInfo() { }

        public virtual long IID { get; set; }

        public virtual string UserID { get; set; }

        [Obsolete("等同于UserID")]
        public virtual string UserUID
        {
            get => this.UserID;
            set => this.UserID = value;
        }

        public virtual string UserName { get; set; }

        public virtual string NickName { get; set; }

        public virtual string Email { get; set; }

        public virtual string HeadImgUrl { get; set; }

        public virtual int IsActive { get; set; }

        public virtual bool IsValid { get; set; } = false;

        public virtual string LoginToken { get; set; }

        public virtual string RefreshToken { get; set; }

        public virtual DateTime TokenExpire { get; set; }

        public virtual List<string> Scopes { get; set; }

        public virtual List<string> Permissions { get; set; }

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
            loginuser.Permissions?.Clear();
            loginuser.Scopes?.Clear();
        }

        /// <summary>
        /// 判断用户是否有权限
        /// </summary>
        public static bool HasPermission(this LoginUserInfo loginuser, string permission) =>
            ValidateHelper.IsPlumpList(loginuser.Permissions) && loginuser.Permissions.Contains(permission);


        /// <summary>
        /// 判断是否有scope
        /// </summary>
        public static bool HasScope(this LoginUserInfo loginuser, string scope) =>
            ValidateHelper.IsPlumpList(loginuser.Scopes) && loginuser.Scopes.Contains(scope);

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
