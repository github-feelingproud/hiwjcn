using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.mvc.user;
using Lib.mvc;
using Lib.helper;
using Autofac;

namespace Hiwjcn.Core.Infrastructure.Auth
{
    public interface IAuthLoginService : Lib.infrastructure.service.IAuthLoginService { }

    public static class LoginAccountSystemExtension
    {
        public static void UseAccountSystem<T>(this ContainerBuilder builder) where T : IAuthLoginService
        {
            builder.RegisterType<T>().AsSelf().As<IAuthLoginService>().SingleInstance();
        }
    }
}
