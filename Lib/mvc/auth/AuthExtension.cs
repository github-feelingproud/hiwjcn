using Lib.helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Lib.extension;
using Lib.io;
using Lib.ioc;
using Lib.mvc.user;
using System.Reflection;
using System.Web.SessionState;
using Autofac;

namespace Lib.mvc.auth
{
    public static class AuthExtension
    {
        public const string AuthedUserKey = "auth.user.data";
        public const string AuthedUserUIDKey = "auth.user.id";

        public static string AuthedUserUID(this HttpContext context)
        {
            return ConvertHelper.GetString(context.Items[AuthedUserUIDKey]);
        }

        public static void SetAuthedUserUID(this HttpContext context, string user_uid)
        {
            if (!ValidateHelper.IsPlumpString(user_uid))
            {
                throw new Exception("用户ID为空");
            }
            context.Items[AuthedUserUIDKey] = user_uid;
        }

        public static AuthContext AuthContext(this HttpContext context)
        {
            if (!AppContext.IsRegistered<ITokenProvider>())
            {
                throw new Exception("请注册token provider");
            }
            var tokenProvider = AppContext.GetObject<ITokenProvider>();
            return new AuthContext(context, tokenProvider);
        }

        public static async Task<LoginUserInfo> GetAuthUserAsync(this Controller controller)
        {
            var data = ServerHelper.CacheInHttpContext(AuthedUserKey, () =>
             {
                 return new LoginUserInfo();
             }, HttpContext.Current);

            await Task.FromResult(1);
            throw new NotImplementedException();
        }

        public static void UseTokenProvider(this ContainerBuilder builder, Func<ITokenProvider> provider)
        {
            builder.Register(x => provider.Invoke()).AsSelf();
        }


        public static void UseWebApiAuthentication(this ContainerBuilder builder)
        { }


        public static void UseDatabaseAuthentication(this ContainerBuilder builder)
        { }

        public static void AuthUseAuthServerValidation(this ContainerBuilder builder)
        { }

        public static void AuthUseCustomValidation(this ContainerBuilder builder)
        { }
    }
}
