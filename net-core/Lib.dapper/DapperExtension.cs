using Dapper;
using Lib.core;
using Lib.data;
using Lib.extension;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace Lib.dapper
{
    public static class DapperExtension
    {
        /// <summary>
        /// HowToUseParametersInDapperFramework
        /// </summary>
        /// <param name="con"></param>
        public static void HowToUseParametersInDapperFramework(this IDbConnection con)
        {
            var args = new DynamicParameters(new { });
            args.Add("age", 1);
            //con.Execute("", args);
            throw new Exception($"这个方法只做记录，不可以调用，请使用{nameof(ParseToDapperParameters)}");
        }

        /// <summary>
        /// 把dbparameter转换为dapper的参数（测试可用）
        /// </summary>
        /// <param name="con"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public static DynamicParameters ParseToDapperParameters(this IDbConnection con, IEnumerable<DbParameter> ps)
        {
            var args = new DynamicParameters(new { });
            foreach (var p in ps)
            {
                args.Add(p.ParameterName, p.Value);
            }
            return args;
        }

        /// <summary>
        /// 获取dapper的动态参数
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DynamicParameters ToDapperParams(this Dictionary<string, object> data)
        {
            var args = new DynamicParameters(new { });
            foreach (var row in data.AsTupleEnumerable())
            {
                args.Add(row.key, row.value);
            }
            return args;
        }
        /// <summary>
        /// 插入数据（使用System.ComponentModel.DataAnnotations.Schema配置数据表映射）
        /// </summary>
        /// <param name="con"></param>
        /// <param name="model"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Insert<T>(this IDbConnection con, T model,
            IDbTransaction transaction = null, int? commandTimeout = default(int?)) where T : IDBTable
        {
            var sql = model.GetInsertSql();
            try
            {
                return con.Execute(sql, model, transaction: transaction, commandTimeout: commandTimeout);
            }
            catch (Exception e)
            {
                throw new Exception($"无法执行SQL:{sql}", e);
            }
        }

        /// <summary>
        /// 异步插入
        /// </summary>
        /// <param name="con"></param>
        /// <param name="model"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static async Task<int> InsertAsync<T>(this IDbConnection con, T model,
            IDbTransaction transaction = null, int? commandTimeout = default(int?)) where T : IDBTable
        {
            var sql = model.GetInsertSql();
            try
            {
                return await con.ExecuteAsync(sql, model, transaction: transaction, commandTimeout: commandTimeout);
            }
            catch (Exception e)
            {
                throw new Exception($"无法执行SQL:{sql}", e);
            }
        }

        /// <summary>
        /// 通过主键更新数据，自动生成字段和主键不更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="model"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static int Update<T>(this IDbConnection con, T model,
            IDbTransaction transaction = null, int? commandTimeout = default(int?)) where T : IDBTable
        {
            var sql = model.GetUpdateSql();
            try
            {
                return con.Execute(sql, model, transaction: transaction, commandTimeout: commandTimeout);
            }
            catch (Exception e)
            {
                throw new Exception($"无法执行SQL:{sql}", e);
            }
        }

        /// <summary>
        /// 通过主键更新数据，自动生成字段和主键不更新
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="con"></param>
        /// <param name="model"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns></returns>
        public static async Task<int> UpdateAsync<T>(this IDbConnection con, T model,
            IDbTransaction transaction = null, int? commandTimeout = default(int?)) where T : IDBTable
        {
            var sql = model.GetUpdateSql();
            try
            {
                return await con.ExecuteAsync(sql, model, transaction: transaction, commandTimeout: commandTimeout);
            }
            catch (Exception e)
            {
                throw new Exception($"无法执行SQL:{sql}", e);
            }
        }

        [Obsolete("实现比较垃圾")]
        public static async Task<T> FindByPrimaryKeyAsync<T>(this IDbConnection con, object[] keys,
            IDbTransaction transaction = null, int? commandTimeout = default(int?))
        {
            var structure = typeof(T).GetTableStructure();
            if (keys.Length != structure.keys.Count) { throw new Exception("传入主键数量和数据表不一致"); }
            var where = " AND ".Join(structure.keys.Select(x => $"{x.Key}=@{x.Value}"));

            var sql = $"SELECT * FROM {structure.table_name} WHERE {where}";

            var param_dict = new Dictionary<string, object>();
            var index = 0;
            foreach (var row in structure.keys.AsTupleEnumerable())
            {
                param_dict[row.value] = keys[index++];
            }

            return (await con.QueryAsync<T>(sql, param_dict.ToDapperParams(),
                transaction: transaction, commandTimeout: commandTimeout)).FirstOrDefault();
        }

        [Obsolete("实现比较垃圾")]
        public static T FindByPrimaryKey<T>(this IDbConnection con, object[] keys,
            IDbTransaction transaction = null, int? commandTimeout = default(int?))
        {
            var structure = typeof(T).GetTableStructure();
            if (keys.Length != structure.keys.Count) { throw new Exception("传入主键数量和数据表不一致"); }
            var where = " AND ".Join(structure.keys.Select(x => $"{x.Key}=@{x.Value}"));

            var sql = $"SELECT * FROM {structure.table_name} WHERE {where}";

            var param_dict = new Dictionary<string, object>();
            var index = 0;
            foreach (var row in structure.keys.AsTupleEnumerable())
            {
                param_dict[row.value] = keys[index++];
            }

            return con.Query<T>(sql, param_dict.ToDapperParams(),
                transaction: transaction, commandTimeout: commandTimeout).FirstOrDefault();
        }
    }
}
