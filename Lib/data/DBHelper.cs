using Lib.core;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.SqlClient;
using Lib.ioc;
using Lib.helper;
using Lib.extension;
using System.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Lib.data
{
    public enum DBTypeEnum : int
    {
        None = -1,
        MySQL = 1,
        SqlServer = 2,
        Oracle = 3,
        DB2 = 4,
        PostgreSQL = 5,
        Sqlite = 6
    }
    public static class DBHelper
    {
        private static readonly string ConStr =
            ConfigurationManager.ConnectionStrings["db"]?.ToString() ??
            ConfigurationManager.AppSettings["db"];

        /// <summary>
        /// 使用ioc中注册的数据库
        /// </summary>
        /// <returns></returns>
        public static IDbConnection GetConnectionProvider()
        {
            if (!ValidateHelper.IsPlumpString(ConStr))
            {
                throw new Exception("请在connectionstring或者appsetting中配置节点为db的通用链接字符串");
            }
            var con = AppContext.GetObject<IDbConnection>();
            con.ConnectionString = ConStr;
            //打开链接，重试两次
            con.OpenIfClosedWithRetry(2);
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
        /// 异步链接
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static async Task PrepareConnectionAsync(Func<IDbConnection, Task> callback)
        {
            using (var con = GetConnectionProvider())
            {
                await callback.Invoke(con);
            }
        }
        /// <summary>
        /// 使用ioc中注册的数据库
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="iso"></param>
        public static void PrepareConnection(Func<IDbConnection, IDbTransaction, bool> callback,
            IsolationLevel? iso = null)
        {
            PrepareConnection(con =>
            {
                using (var t = con.StartTransaction(iso))
                {
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
                }
            });
        }

        /// <summary>
        /// 异步链接，带事务
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="iso"></param>
        /// <returns></returns>
        public static async Task PrepareConnectionAsync(Func<IDbConnection, IDbTransaction, Task<bool>> callback,
            IsolationLevel? iso = null)
        {
            await PrepareConnectionAsync(async con =>
            {
                using (var t = con.StartTransaction(iso))
                {
                    try
                    {
                        if (await callback.Invoke(con, t))
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

        /// <summary>
        /// c#类型转换为dbtype
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static DbType ConvertToDbType<T>()
        {
            return ConvertToDbType(typeof(T));
        }

        /// <summary>
        /// c#类型转换为dbtype
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static DbType ConvertToDbType(Type t)
        {
            if (!Type2DBTypeMapper.ContainsKey(t)) { throw new Exception($"{Type2DBTypeMapper}:不支持的类型转换"); }
            return Type2DBTypeMapper[t];
        }

        /// <summary>
        /// type和dbtype的映射表
        /// </summary>
        public static readonly ReadOnlyDictionary<Type, DbType> Type2DBTypeMapper = new ReadOnlyDictionary<Type, DbType>(new Dictionary<Type, DbType>()
        {
            [typeof(byte)] = DbType.Byte,
            [typeof(sbyte)] = DbType.SByte,
            [typeof(short)] = DbType.Int16,
            [typeof(ushort)] = DbType.UInt16,
            [typeof(int)] = DbType.Int32,
            [typeof(uint)] = DbType.UInt32,
            [typeof(long)] = DbType.Int64,
            [typeof(ulong)] = DbType.UInt64,
            [typeof(float)] = DbType.Single,
            [typeof(double)] = DbType.Double,
            [typeof(decimal)] = DbType.Decimal,
            [typeof(bool)] = DbType.Boolean,
            [typeof(string)] = DbType.String,
            [typeof(char)] = DbType.StringFixedLength,
            [typeof(Guid)] = DbType.Guid,
            [typeof(DateTime)] = DbType.DateTime,
            [typeof(DateTimeOffset)] = DbType.DateTimeOffset,
            [typeof(TimeSpan)] = DbType.Time,
            [typeof(byte[])] = DbType.Binary,
            [typeof(byte?)] = DbType.Byte,
            [typeof(sbyte?)] = DbType.SByte,
            [typeof(short?)] = DbType.Int16,
            [typeof(ushort?)] = DbType.UInt16,
            [typeof(int?)] = DbType.Int32,
            [typeof(uint?)] = DbType.UInt32,
            [typeof(long?)] = DbType.Int64,
            [typeof(ulong?)] = DbType.UInt64,
            [typeof(float?)] = DbType.Single,
            [typeof(double?)] = DbType.Double,
            [typeof(decimal?)] = DbType.Decimal,
            [typeof(bool?)] = DbType.Boolean,
            [typeof(char?)] = DbType.StringFixedLength,
            [typeof(Guid?)] = DbType.Guid,
            [typeof(DateTime?)] = DbType.DateTime,
            [typeof(DateTimeOffset?)] = DbType.DateTimeOffset,
            [typeof(TimeSpan?)] = DbType.Time,
            [typeof(object)] = DbType.Object
        });
    }
}
