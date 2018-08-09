﻿using Lib.extension;
using Lib.ioc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading.Tasks;

namespace Lib.data
{
    /// <summary>
    /// 数据库类型
    /// </summary>
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

    /// <summary>
    /// 获取数据库链接
    /// </summary>
    public static class DBHelper
    {
        /// <summary>
        /// 使用ioc中注册的数据库
        /// </summary>
        public static void PrepareConnection(Action<IDbConnection> callback)
        {
            using (var s = IocContext.Instance.Scope())
            {
                using (var con = s.Resolve_<IDbConnection>())
                {
                    callback.Invoke(con);
                }
            }
        }

        /// <summary>
        /// 异步链接
        /// </summary>
        public static async Task PrepareConnectionAsync(Func<IDbConnection, Task> callback)
        {
            using (var s = IocContext.Instance.Scope())
            {
                using (var con = s.Resolve_<IDbConnection>())
                {
                    await callback.Invoke(con);
                }
            }
        }

        /// <summary>
        /// 使用ioc中注册的数据库
        /// </summary>
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
            if (!Type2DBTypeMapper.ContainsKey(t))
                throw new NotSupportedException($"{Type2DBTypeMapper}:不支持的类型转换");

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
