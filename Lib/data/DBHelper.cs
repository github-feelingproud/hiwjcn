using Lib.core;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.SqlClient;
using Lib.ioc;
using Lib.helper;
using Lib.extension;
using System.Configuration;

namespace Lib.data
{
    public enum DBTypeEnum : int
    {
        MySQL = 1, SqlServer = 2
    }
    public static class DBHelper
    {
        private static readonly string ConStr = ConfigurationManager.AppSettings["db"] ?? ConfigurationManager.ConnectionStrings["db"]?.ToString();
        /// <summary>
        /// 使用ioc中注册的数据库
        /// </summary>
        /// <returns></returns>
        public static IDbConnection GetConnectionProvider()
        {
            if (!ValidateHelper.IsPlumpString(ConStr))
            {
                throw new Exception("请在appsetting中配置节点为db的通用链接字符串");
            }
            IDbConnection con = null;
            if (!AppContext.IsRegistered<IDbConnection>())
            {
                con = new SqlConnection();
            }
            else
            {
                con = AppContext.GetObject<IDbConnection>();
            }
            con.ConnectionString = ConStr;
            //打开链接，重试两次
            new Action(() => { con.Open(); }).InvokeWithRetry(2);
            return con;
        }
        /// <summary>
        /// 使用ioc中注册的数据库
        /// </summary>
        /// <param name="callback"></param>
        public static void PrepareConnection(Action<IDbConnection> callback)
        {
            using (var con = GetConnectionProvider())
            {
                callback.Invoke(con);
            }
        }
        /// <summary>
        /// 使用ioc中注册的数据库
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="iso"></param>
        public static void PrepareConnection(Func<IDbConnection, IDbTransaction, bool> callback, IsolationLevel? iso = null)
        {
            PrepareConnection(con =>
            {
                IDbTransaction t = null;
                if (iso == null)
                {
                    t = con.BeginTransaction();
                }
                else
                {
                    t = con.BeginTransaction(iso.Value);
                }

                try
                {
                    if (callback.Invoke(con, t))
                    {
                        t.Commit();
                    }
                    else
                    {
                        t.Rollback();
                    }
                }
                catch (Exception e)
                {
                    t.Rollback();
                    throw e;
                }
                finally
                {
                    t.Dispose();
                }
            });
        }




        //============================================================================
        [Obsolete("方法已过期，请使用通用方法")]
        public static MySqlConnection GetMySqlConnection()
        {
            var con = new MySqlConnection(ConfigHelper.Instance.MySqlConnectionString);
            con.Open();
            return con;
        }
        [Obsolete("方法已过期，请使用通用方法")]
        public static void PrepareMySqlConnection(Action<MySqlConnection> callback)
        {
            using (var db = GetMySqlConnection())
            {
                callback.Invoke(db);
            }
        }
        //==============================================================================
        [Obsolete("方法已过期，请使用通用方法")]
        public static SqlConnection GetSqlServerConnection()
        {
            var con = new SqlConnection(ConfigHelper.Instance.MsSqlConnectionString);
            con.Open();
            return con;
        }
        [Obsolete("方法已过期，请使用通用方法")]
        public static void PrepareSqlServerConnection(Action<SqlConnection> callback)
        {
            using (var db = GetSqlServerConnection())
            {
                callback.Invoke(db);
            }
        }
    }
}
