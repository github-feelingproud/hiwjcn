﻿using System;
using System.Threading.Tasks;

namespace Lib.auth
{
    /// <summary>
    /// 对外提供的服务（webapi，wcf）
    /// </summary>
    public interface IAuthApi
    {
        /// <summary>
        /// 用code换token
        /// </summary>
        Task<_<TokenModel>> CreateAccessTokenAsync(string user_uid);

        /// <summary>
        /// 用token换取登录信息
        /// </summary>
        Task<_<LoginUserInfo>> GetLoginUserInfoByTokenAsync(string access_token);

        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<_<string>> RemoveCacheAsync(CacheBundle data);
    }
}
