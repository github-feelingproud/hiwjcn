using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Lib.helper;
using Lib.data;
using Lib.core;
using Lib.extension;
using Lib.infrastructure;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Data.Entity;
using Lib.infrastructure.entity;

namespace Lib.infrastructure.service
{
    /// <summary>
    /// 树结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITreeServiceBase<T> : IServiceBase<T>
        where T : TreeEntityBase
    {
        Task<List<T>> FindNodeChildrenRecursively_(IQueryable<T> data_source, T first_node,
               string tree_error = null);

        Task<(bool success, List<T> node_path)> CheckNodeIfCanFindRoot(IQueryable<T> data_source, T first_node);

        Task<List<T>> FindTreeBadNodes(IQueryable<T> data_source);
    }

    /// <summary>
    /// 树结构
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class TreeServiceBase<T> : ServiceBase<T>, ITreeServiceBase<T>
        where T : TreeEntityBase
    {
        public async Task<List<T>> FindNodeChildrenRecursively_(IQueryable<T> data_source, T first_node,
            string tree_error = "树存在无限递归")
        {
            var repeat_check = new List<string>();
            var list = new List<T>();

            async Task FindRecursively(T node)
            {
                if (node == null) { return; }

                repeat_check.AddOnceOrThrow(node.UID, error_msg: tree_error);

                var child_parent_uid = node.UID;
                var child_level = node.Level + 1;
                var children = await data_source.Where(x => x.ParentUID == child_parent_uid && x.Level == child_level).ToListAsync();
                if (ValidateHelper.IsPlumpList(children))
                {
                    foreach (var child in children)
                    {
                        //递归
                        await FindRecursively(child);
                    }
                }

                list.Add(node);
            }

            await FindRecursively(first_node);

            return list;
        }

        public async Task<(bool success, List<T> node_path)> CheckNodeIfCanFindRoot(IQueryable<T> data_source, T first_node)
        {
            var repeat_check = new List<string>();
            var current_uid = first_node.UID;

            var top_level = default(int?);
            var top_parent = default(string);

            var node = default(T);

            var node_path = new List<T>();

            while (true)
            {
                node = await data_source.Where(x => x.UID == current_uid).FirstOrDefaultAsync();
                if (node == null) { break; }

                repeat_check.AddOnceOrThrow(node.UID, error_msg: "存在无限循环");

                {
                    //设置检查值
                    top_level = node.Level;
                    top_parent = node.ParentUID;
                }

                current_uid = node.ParentUID;
                node_path.Add(node);
            }
            var success = top_level == TreeEntityBase.FIRST_LEVEL && top_parent == TreeEntityBase.FIRST_PARENT_UID;
            return (success, node_path);
        }

        public async Task<List<T>> FindTreeBadNodes(IQueryable<T> data_source)
        {
            var list = await data_source.ToListAsync();
            var error_list = new List<string>();

            foreach (var node in list.OrderByDescending(x => x.Level))
            {
                if (error_list.Contains(node.UID))
                {
                    //防止中间节点被重复计算
                    //node1->node2->node3->node4->node5
                    //计算了node1到node5为错误节点之后将跳过1,2,3,4的检查
                    continue;
                }
                var check = await this.CheckNodeIfCanFindRoot(list.AsQueryable(), node);
                if (!check.success)
                {
                    error_list.AddRange(check.node_path.Select(x => x.UID));
                }
            }

            return list.Where(x => error_list.Distinct().Contains(x.UID)).ToList();
        }
    }
}
