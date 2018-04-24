using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Hiwjcn.Core.Domain.User;

namespace Hiwjcn.Framework
{
    public class MapperContext
    {
        /// <summary>
        /// 也许功能强大，但是用起来感觉并不方便
        /// </summary>
        public void Init()
        {
            var configuration = new MapperConfiguration(config =>
            {
                config.CreateMap<UserEntity, UserAvatarEntity>()
                .ForMember(to => to.CreateTime, from => from.MapFrom(x => x.CreateTime.Date))
                .ForMember(to => to.UserUID, from => from.MapFrom(x => x.UID));

            });
            var mapper = configuration.CreateMapper();
            //register to ioc context as single instance
            
            var model = mapper.Map<UserAvatarEntity>(new UserEntity());
        }
    }
}
