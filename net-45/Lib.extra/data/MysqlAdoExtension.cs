using Lib.core;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.extra.data
{
    /// <summary>
    /// mysql ado扩展
    /// </summary>
    public static class MysqlAdoExtension
    {
        /// <summary>
        /// 获取带timeout的链接字符串
        /// </summary>
        /// <param name="con"></param>
        /// <returns></returns>
        public static string GetTimeoutConnectionString(this MySqlConnection con)
        {
            var b = new MySqlConnectionStringBuilder(string.Empty);
            b.ConnectionTimeout = 3;
            var str = b.ToString();
            return str;
        }
    }
}
