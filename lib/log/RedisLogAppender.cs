using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using log4net;
using log4net.Appender;
using log4net.Core;
using System.Collections;
using Lib.data;

namespace Lib.log
{
    /// <summary>
    /// 使用redis存储日志
    /// https://github.com/lokki/RedisAppender
    /// </summary>
    public class RedisLogAppender : AppenderSkeleton
    {
        protected override void Append(LoggingEvent loggingEvent)
        {
            var logline = new LogLine(loggingEvent);
            var client = new RedisHelper();
            client.ListLeftPush(nameof(RedisLogAppender), logline);
        }
    }
}
