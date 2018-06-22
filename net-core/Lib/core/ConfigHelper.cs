using Lib.extension;
using Lib.helper;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
    public class BasicConfigProvider : ISettings
    {
        /// <summary>
        /// 从web.config中加载配置
        /// </summary>
        public BasicConfigProvider()
        {
            this.AllowDomains = ConfigurationManager.AppSettings[nameof(AllowDomains)]?.Split(',')?.ToList();
            if (this.AllowDomains == null) { this.AllowDomains = new List<string>(); }
            //string
            this.RedisConnectionString = ConfigurationManager.ConnectionStrings[nameof(RedisConnectionString)]?.ToString();

            this.CookieDomain = ConfigurationManager.AppSettings[nameof(CookieDomain)];

            //邮件发送配置
            this.SmptServer = ConfigurationManager.AppSettings[nameof(SmptServer)];
            this.SmptLoginName = ConfigurationManager.AppSettings[nameof(SmptLoginName)];
            this.SmptPassWord = ConfigurationManager.AppSettings[nameof(SmptPassWord)];
            this.SmptSenderName = ConfigurationManager.AppSettings[nameof(SmptSenderName)];
            this.SmptEmailAddress = ConfigurationManager.AppSettings[nameof(SmptEmailAddress)];

            this.FeedBackEmail = ConfigurationManager.AppSettings[FeedBackEmail];
            
            //integer
            this.CookieExpiresMinutes = ConvertHelper.GetInt(ConfigurationManager.AppSettings[nameof(CookieExpiresMinutes)], 525600);

            this.LoadPlugin = (ConfigurationManager.AppSettings[nameof(LoadPlugin)] ?? "").ToBool();
        }

        public virtual List<string> AllowDomains { get; private set; }

        public virtual string RedisConnectionString { get; private set; }

        public virtual string CookieDomain { get; private set; }

        public virtual string SmptServer { get; private set; }

        public virtual string SmptLoginName { get; private set; }

        public virtual string SmptPassWord { get; private set; }

        public virtual string SmptSenderName { get; private set; }

        public virtual string SmptEmailAddress { get; private set; }

        public virtual string FeedBackEmail { get; private set; }
        /// <summary>
        /// 账户登录cookie保存分钟
        /// </summary>
        public virtual int CookieExpiresMinutes { get; private set; }
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
        private static ISettings ins = null;

        private static readonly object locker = new object();

        /// <summary>
        /// 获取单例
        /// </summary>
        /// <returns></returns>
        public static ISettings Instance
        {
            get
            {
                if (ins == null)
                {
                    lock (locker)
                    {
                        if (ins == null)
                        {
                            ins = new BasicConfigProvider();
                        }
                    }
                }
                return ins;
            }
        }
    }
}
