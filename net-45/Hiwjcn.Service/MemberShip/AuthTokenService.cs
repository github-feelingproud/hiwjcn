using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data;
using Lib.infrastructure;
using Hiwjcn.Core.Domain.Auth;
using Lib.core;
using Lib.extension;
using Lib.helper;
using System.Data.Entity;
using Lib.events;
using System.Configuration;
using Lib.mvc;
using Lib.cache;
using Hiwjcn.Core;
using Lib.infrastructure.entity;
using Lib.data.ef;
using Lib.ioc;
using Lib.infrastructure.entity.auth;
using Lib.infrastructure.service.user;
using Hiwjcn.Core.Data;

namespace Hiwjcn.Service.MemberShip
{
    public interface IAuthService : IAuthServiceBase<AuthToken>,
        IAutoRegistered
    {

    }

    public class AuthService :
        AuthServiceBase<AuthToken>,
        IAuthService
    {
        public AuthService(
            IEventPublisher _publisher,
            IMSRepository<AuthToken> _AuthTokenRepository,
            ICacheProvider _cache) :
            base(_AuthTokenRepository)
        {
            //
        }
    }
}
