using System;
using System.Collections.Generic;
using System.Text;

namespace Lib.core
{
    /// <summary>
    /// 设置
    /// </summary>
    public interface ISettings
    {
        List<string> AllowDomains { get; }

        string RedisConnectionString { get; }

        string CookieDomain { get; }

        string SmptServer { get; }

        string SmptLoginName { get; }

        string SmptPassWord { get; }

        string SmptSenderName { get; }

        string SmptEmailAddress { get; }

        string FeedBackEmail { get; }
        /// <summary>
        /// 账户登录cookie保存分钟
        /// </summary>
        int CookieExpiresMinutes { get; }
        /// <summary>
        /// 是否加载插件
        /// </summary>
        bool LoadPlugin { get; }
        /// <summary>
        /// 系统默认编码
        /// </summary>
        Encoding SystemEncoding { get; }
    }

    /// <summary>
    /// 加载当前上下文项目config文件中的配置
    /// </summary>
    public static class ConfigHelper
    {
        private static ISettings ins = null;

        private static readonly object locker = new object();

        /// <summary>
        /// 获取单例
        /// </summary>
        /// <returns></returns>
        public static ISettings Instance => throw new NotImplementedException();
    }
}
