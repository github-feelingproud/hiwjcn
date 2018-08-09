using Lib.core;
using Lib.data;
using Lib.helper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace Lib.extension
{
    /// <summary>
    /// 针对不同数据库的sql方言
    /// </summary>
    public interface ISqlDialect { }

    public class MySqlDialect : ISqlDialect { }

    public class SqlServerDialect : ISqlDialect { }

    public class PostgreSqlDialect : ISqlDialect { }

    public static class SimpleOrmExtension
    {
        /// <summary>
        /// 这个字段是数据库自动生成
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static bool IsGeneratedInDatabase(this PropertyInfo prop)
        {
            return prop.GetCustomAttributes<DatabaseGeneratedAttribute>().Where(m => m.DatabaseGeneratedOption != DatabaseGeneratedOption.None).Any();
        }

        /// <summary>
        /// 获取表名
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string GetTableName(this Type t)
        {
            return t.GetCustomAttribute<TableAttribute>()?.Name ?? t.Name;
        }

        /// <summary>
        /// 获取字段对应的属性
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetColumnsProperties(this Type t)
        {
            return t.GetProperties().Where(x => x.CanRead && x.CanWrite).Where(x => !x.GetCustomAttributes<NotMappedAttribute>().Any());
        }

        /// <summary>
        /// 获取字段属性的信息
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static (string column, string placeholder) GetColumnInfo(this PropertyInfo p)
        {
            var column = p.GetCustomAttribute<ColumnAttribute>()?.Name ?? p.Name;
            var placeholder = p.Name;
            return (column, placeholder);
        }

        /// <summary>
        /// 是否是主键
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool IsPrimaryKey(this PropertyInfo p) => p.GetCustomAttributes<KeyAttribute>().Any();

    }

    /// <summary>
    /// 对dapper的扩展
    /// </summary>
    public static class DapperExtension
    {
        /// <summary>
        /// 如果没有打开链接就打开链接
        /// </summary>
        public static IDbConnection OpenIfClosedWithRetry(this IDbConnection con, int retryCount = 1)
        {
            if (con.State == ConnectionState.Closed)
            {
                var func = new Action(() => con.Open());

                func.InvokeWithRetryAndWait<Exception>(retryCount, i => TimeSpan.FromMilliseconds(i * 100));
            }
            return con;
        }

        /// <summary>
        /// 开启事务
        /// </summary>
        /// <param name="con"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static IDbTransaction StartTransaction(this IDbConnection con, IsolationLevel? level = null)
        {
            if (level == null)
            {
                return con.BeginTransaction();
            }
            else
            {
                return con.BeginTransaction(level.Value);
            }
        }
        
        /// <summary>
        /// 获取表结构
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public static (string table_name, Dictionary<string, string> keys,
            Dictionary<string, string> auto_generated_columns, Dictionary<string, string> columns)
            GetTableStructure<T>(this T model) where T : IDBTable
        {
            return model.GetType().GetTableStructure();
        }

        /// <summary>
        /// 获取表结构
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static (string table_name, Dictionary<string, string> keys,
            Dictionary<string, string> auto_generated_columns, Dictionary<string, string> columns)
            GetTableStructure(this Type t)
        {
            var pps = t.GetColumnsProperties();
            //表名
            var table_name = t.GetTableName();

            //读取字段名和sql的placeholder
            (string column, string placeholder) GetColumn(PropertyInfo p) => p.GetColumnInfo();

            //主键
            var key_props = pps.Where(x => x.IsPrimaryKey()).ToList();
            if (!ValidateHelper.IsPlumpList(key_props))
            {
                throw new NoPrimaryKeyException("Model没有设置主键");
            }
            var keys = key_props.Select(x => GetColumn(x)).ToDictionary(x => x.column, x => x.placeholder);

            //自动生成的字段
            var auto_generated_props = pps.Where(x => x.IsGeneratedInDatabase()).ToList();
            var auto_generated_columns = auto_generated_props.Select(x => GetColumn(x)).ToDictionary(x => x.column, x => x.placeholder);

            //普通字段
            var column_props = pps.Where(x => !keys.Values.Contains(x.Name)).ToList();
            column_props = pps.Where(x => !auto_generated_columns.Values.Contains(x.Name)).ToList();
            if (!ValidateHelper.IsPlumpList(column_props))
            {
                throw new NoValidFieldsException("无法提取到有效字段");
            }
            var columns = column_props.Select(x => GetColumn(x)).ToDictionary(x => x.column, x => x.placeholder);

            return (table_name, keys, auto_generated_columns, columns);
        }

        /// <summary>
        /// 获取插入SQL
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string GetInsertSql<T>(this T model) where T : IDBTable
        {
            var structure = model.GetTableStructure();

            var k = ",".Join(structure.columns.Keys);
            var v = ",".Join(structure.columns.Values.Select(x => $"@{x}"));
            var sql = $"INSERT INTO {structure.table_name} ({k}) VALUES ({v})";
            return sql;
        }

        /// <summary>
        /// 获取更新sql
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string GetUpdateSql<T>(this T model) where T : IDBTable
        {
            var structure = model.GetTableStructure();

            var set = ",".Join(structure.columns.Select(x => $"{x.Key}=@{x.Value}"));
            var where = " AND ".Join(structure.keys.Select(x => $"{x.Key}=@{x.Value}"));
            var sql = $"UPDATE {structure.table_name} SET {set} WHERE {where}";
            return sql;
        }
    }

    /// <summary>
    /// 对Ado的扩展
    /// </summary>
    public static class AdoExtension
    {
        /// <summary>
        /// 复制参数
        /// </summary>
        public static T[] CloneParams<T>(IEnumerable<T> list) where T : DbParameter
            => Com.CloneParams(list.ToList());
        
        /// <summary>
        /// 这个方法来自途虎养车网，自己做了一些小修改
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        [Obsolete("强烈推荐使用Dapper")]
        public static T CurrentModel<T>(this IDataReader reader) where T : class, new()
        {
            return MapperHelper.GetModelFromReader<T>(reader);
        }

        /// <summary>
        /// 读取list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        [Obsolete("强烈推荐使用Dapper")]
        public static List<T> WholeList<T>(this IDataReader reader) where T : class, new()
        {
            return MapperHelper.GetListFromReader<T>(reader);
        }

        /*
         JArray array = new JArray();
        array.Add("Manual text");
        array.Add(new DateTime(2000, 5, 23));

        JObject o = new JObject();
        o["MyArray"] = array;

        string json = o.ToString();
        // {
        //   "MyArray": [
        //     "Manual text",
        //     "2000-05-23T00:00:00"
        //   ]
        // }
         */

        /// <summary>
        /// 获取Json（测试可用）
        /// </summary>
        public static string GetJson(this IDataReader reader)
        {
            var fields = new List<string>();
            for (int i = 0; i < reader.FieldCount; ++i)
            {
                fields.Add(reader.GetName(i));
            }

            var arr = new JArray();
            while (reader.Read())
            {
                var jo = new JObject();
                fields.ForEach(x =>
                {
                    var val = JToken.FromObject(reader[x]);
                    jo[x] = val;
                });

                arr.Add(jo);
            }
            return arr.ToString();
        }

        /// <summary>
        /// 转为实体
        /// </summary>
        public static List<T> ToEntityList_<T>(this IDataReader reader)
        {
            return reader.GetJson().JsonToEntity<List<T>>();
        }

        /// <summary>
        /// 多种方式绑定参数
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="parameters"></param>
        [Obsolete("强烈推荐使用Dapper")]
        public static void AddParamsSmartly(this IDbCommand cmd, params object[] parameters)
        {
            if (!ValidateHelper.IsPlumpList(parameters)) { return; }

            if (parameters.All(x => x is IDataParameter))
            {
                parameters.ToList().ForEach(x => cmd.Parameters.Add(x));
            }
            else if (parameters.Length == 1)
            {
                var p = parameters[0];
                var dict = Com.ObjectToSqlParamsDict(p);
                foreach (var key in dict.Keys)
                {
                    var param = cmd.CreateParameter();
                    param.ParameterName = $"@{key}";
                    param.Value = dict[key];
                    cmd.Parameters.Add(param);
                }
            }
            else
            {
                throw new ArgumentException("只能有一个object参数");
            }
        }

        /// <summary>
        /// AddParameters
        /// </summary>
        /// <param name="command"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        [Obsolete("添加参数")]
        public static void AddParameters(this IDbCommand command, string key, object value)
        {
            var p = command.CreateParameter();
            p.ParameterName = key;
            p.Value = value ?? System.DBNull.Value;
            command.Parameters.Add(p);
        }

        /// <summary>
        /// 执行SQL
        /// </summary>
        /// <param name="con"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [Obsolete("强烈推荐使用Dapper")]
        public static int ExecuteSql(this IDbConnection con, string sql, params object[] parameters)
        {
            var count = 0;
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                cmd.AddParamsSmartly(parameters);
                count = cmd.ExecuteNonQuery();
            }
            return count;
        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="con"></param>
        /// <param name="procedure_name"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [Obsolete("强烈推荐使用Dapper")]
        public static int ExecuteProcedure(this IDbConnection con, string procedure_name, params object[] parameters)
        {
            var count = 0;
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = procedure_name;
                cmd.AddParamsSmartly(parameters);
                count = cmd.ExecuteNonQuery();
            }
            return count;
        }

        /// <summary>
        /// 读取第一行第一个记录
        /// </summary>
        /// <param name="con"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [Obsolete("强烈推荐使用Dapper")]
        public static object ExecuteScalar(this IDbConnection con, string sql, params object[] parameters)
        {
            object obj = null;
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                cmd.AddParamsSmartly(parameters);
                obj = cmd.ExecuteScalar();
            }
            return obj;
        }


        /// <summary>
        /// 用reader读取list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [Obsolete("强烈推荐使用Dapper")]
        public static List<T> GetList<T>(this IDbConnection con, string sql, params object[] parameters) where T : class, new()
        {
            List<T> list = null;
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                cmd.AddParamsSmartly(parameters);
                using (var reader = cmd.ExecuteReader())
                {
                    list = reader.WholeList<T>();
                }
            }
            return list;
        }

        /// <summary>
        /// 读取reader的json格式（测试可用）
        /// </summary>
        /// <param name="con"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [Obsolete("强烈推荐使用Dapper")]
        public static string GetJson(this IDbConnection con, string sql, params object[] parameters)
        {
            var json = string.Empty;
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                cmd.AddParamsSmartly(parameters);
                using (var reader = cmd.ExecuteReader())
                {
                    json = reader.GetJson();
                }
            }
            return json;
        }

        /// <summary>
        /// 读取reader第一条
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [Obsolete("强烈推荐使用Dapper")]
        public static T GetFirst<T>(this IDbConnection con, string sql, params object[] parameters) where T : class, new()
        {
            T model = default(T);
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                cmd.AddParamsSmartly(parameters);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader != null && reader.Read())
                    {
                        model = reader.CurrentModel<T>();
                    }
                }
            }
            return model;
        }

    }
}
