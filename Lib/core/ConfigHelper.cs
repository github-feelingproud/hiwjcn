using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Lib.core;
using Lib.helper;
using Lib.extension;

namespace Lib.core
{
    /// <summary>
    /// 加载当前上下文项目config文件中的配置
    /// </summary>
    public class ConfigStore
    {
        /// <summary>
        /// 从web.config中加载配置
        /// </summary>
        public ConfigStore()
        {
            this.AllowDomains = ConfigurationManager.AppSettings[nameof(AllowDomains)]?.Split(',')?.ToList();
            if (this.AllowDomains == null) { this.AllowDomains = new List<string>(); }
            //string
            this.ComDbConStr = ConfigurationManager.ConnectionStrings[nameof(ComDbConStr)]?.ToString();
            this.MySqlConnectionString = ConfigurationManager.ConnectionStrings[nameof(MySqlConnectionString)]?.ToString();
            this.RedisConnectionString = ConfigurationManager.ConnectionStrings[nameof(RedisConnectionString)]?.ToString();
            this.MsSqlConnectionString = ConfigurationManager.ConnectionStrings[nameof(MsSqlConnectionString)]?.ToString();
            //单点登录配置
            this.SSOLoginUrl = ConfigurationManager.AppSettings[nameof(SSOLoginUrl)];
            this.SSOLogoutUrl = ConfigurationManager.AppSettings[nameof(SSOLogoutUrl)];
            this.CheckLoginInfoUrl = ConfigurationManager.AppSettings[nameof(CheckLoginInfoUrl)];
            this.CallBackUrl = ConfigurationManager.AppSettings[nameof(CallBackUrl)];
            this.DefaultRedirectUrl = ConfigurationManager.AppSettings[nameof(DefaultRedirectUrl)];

            this.CookieDomain = ConfigurationManager.AppSettings[nameof(CookieDomain)];

            //邮件发送配置
            this.SmptServer = ConfigurationManager.AppSettings[nameof(SmptServer)];
            this.SmptLoginName = ConfigurationManager.AppSettings[nameof(SmptLoginName)];
            this.SmptPassWord = ConfigurationManager.AppSettings[nameof(SmptPassWord)];
            this.SmptSenderName = ConfigurationManager.AppSettings[nameof(SmptSenderName)];
            this.SmptEmailAddress = ConfigurationManager.AppSettings[nameof(SmptEmailAddress)];

            this.FeedBackEmail = ConfigurationManager.AppSettings[FeedBackEmail];

            this.QiniuAccessKey = ConfigurationManager.AppSettings[nameof(QiniuAccessKey)];
            this.QiniuSecretKey = ConfigurationManager.AppSettings[nameof(QiniuSecretKey)];
            this.QiniuBucketName = ConfigurationManager.AppSettings[nameof(QiniuBucketName)];
            this.QiniuBaseUrl = ConfigurationManager.AppSettings[QiniuBaseUrl];

            //integer
            this.CookieExpiresMinutes = ConvertHelper.GetInt(ConfigurationManager.AppSettings[nameof(CookieExpiresMinutes)], 525600);
            this.CacheExpiresMinutes = ConvertHelper.GetInt(ConfigurationManager.AppSettings[nameof(CacheExpiresMinutes)], 5);

            this.LoadPlugin = (ConfigurationManager.AppSettings[nameof(LoadPlugin)] ?? "").ToBool();
        }

        public virtual List<string> AllowDomains { get; private set; }

        public virtual string ComDbConStr { get; private set; }

        public virtual string MySqlConnectionString { get; private set; }

        public virtual string MsSqlConnectionString { get; private set; }

        public virtual string RedisConnectionString { get; private set; }

        public virtual string CallBackUrl { get; private set; }

        public virtual string SSOLoginUrl { get; private set; }

        public virtual string SSOLogoutUrl { get; private set; }

        public virtual string CheckLoginInfoUrl { get; private set; }

        public virtual string DefaultRedirectUrl { get; private set; }

        public virtual string CookieDomain { get; private set; }

        public virtual string SmptServer { get; private set; }

        public virtual string SmptLoginName { get; private set; }

        public virtual string SmptPassWord { get; private set; }

        public virtual string SmptSenderName { get; private set; }

        public virtual string SmptEmailAddress { get; private set; }

        public virtual string FeedBackEmail { get; private set; }

        public virtual string QiniuAccessKey { get; private set; }

        public virtual string QiniuSecretKey { get; private set; }

        public virtual string QiniuBucketName { get; private set; }

        public virtual string QiniuBaseUrl { get; private set; }
        /// <summary>
        /// 账户登录cookie保存分钟
        /// </summary>
        public virtual int CookieExpiresMinutes { get; private set; }

        /// <summary>
        /// 缓存失效时间
        /// </summary>
        public virtual int CacheExpiresMinutes { get; private set; }
        /// <summary>
        /// 是否是调试状态
        /// </summary>
        public virtual bool IsDebug { get; private set; }
        /// <summary>
        /// 是否加载插件
        /// </summary>
        public virtual bool LoadPlugin { get; private set; }
        /// <summary>
        /// 系统默认编码
        /// </summary>
        public virtual Encoding SystemEncoding { get { return Encoding.UTF8; } }
    }

    /// <summary>
    /// 加载当前上下文项目config文件中的配置
    /// </summary>
    public static class ConfigHelper
    {
        private static ConfigStore ins = null;

        private static readonly object locker = new object();

        /// <summary>
        /// 获取单例
        /// </summary>
        /// <returns></returns>
        public static ConfigStore Instance
        {
            get
            {
                if (ins == null)
                {
                    lock (locker)
                    {
                        if (ins == null)
                        {
                            ins = new ConfigStore();
                        }
                    }
                }
                return ins;
            }
        }
    }

    public class ConfigKV
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class Config
    {
        public ConfigKV AppSettings { get; }
        public ConfigKV ConnectionStrings { get; }
    }
}
