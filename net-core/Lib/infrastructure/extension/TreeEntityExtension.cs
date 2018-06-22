using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.data;
using Lib.infrastructure.entity;
using Lib.mvc;
using Lib.helper;
using Lib.extension;
using System.Data.Entity;
using Lib.data.ef;
using Lib.core;

namespace Lib.infrastructure.extension
{
    public static class TreeEntityExtension
    {
        #region 递归相关逻辑

        public static void FindNodeChildrenRecursively__<T>(this IEnumerable<T> data_source,
            T first_node, Action<T, List<T>> callback, string tree_error = "树存在无限递归")
            where T : TreeEntityBase
        {
            var repeat_check = new List<string>();

            void FindChildren(ref List<T> nodes)
            {
                foreach (var m in nodes)
                {
                    repeat_check.AddOnceOrThrow(m.UID, error_msg: tree_error);

                    var children = data_source.Where(x => x.ParentUID == m.UID).ToList();
                    if (ValidateHelper.IsPlumpList(children))
                    {
                        FindChildren(ref children);
                    }

                    callback.Invoke(m, children);
                }
            }

            var start = new List<T>() { first_node };
            FindChildren(ref start);
        }

        public static List<T> FindNodeChildrenRecursively_<T>(this IEnumerable<T> data_source, T first_node,
            string tree_error = "树存在无限递归")
            where T : TreeEntityBase
        {
            var list = new List<T>();
            data_source.FindNodeChildrenRecursively__(
                first_node,
                (node, children) => list.Add(node),
                tree_error);
            return list;
        }

        public static (bool success, List<T> node_path) CheckNodeIfCanFindRoot<T>(this IEnumerable<T> data_source, T first_node)
            where T : TreeEntityBase
        {
            var repeat_check = new List<string>();
            var current_uid = first_node.UID;

            var top_parent = default(string);

            var node_path = new List<T>();

            while (true)
            {
                var node = data_source.Where(x => x.UID == current_uid).FirstOrDefault();
                if (node == null) { break; }

                repeat_check.AddOnceOrThrow(node.UID, error_msg: "存在无限循环");

                //设置检查值
                top_parent = node.ParentUID;

                current_uid = node.ParentUID;
                node_path.Add(node);
            }
            var success = top_parent == TreeEntityBase.FIRST_PARENT_UID;
            return (success, node_path);
        }

        public static List<T> FindTreeBadNodes<T>(this IEnumerable<T> data_source)
            where T : TreeEntityBase
        {
            var list = data_source.ToList();
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
                var check = data_source.CheckNodeIfCanFindRoot(node);
                if (!check.success)
                {
                    error_list.AddRange(check.node_path.Select(x => x.UID));
                }
            }

            return list.Where(x => error_list.Distinct().Contains(x.UID)).ToList();
        }

        #endregion

        public static async Task<_<T>> AddTreeNode<T>(this IRepository<T> repo, T model, string model_flag,
            ISaveCheck<T> checker = null)
            where T : TreeEntityBase
        {
            var data = new _<T>();
            if (model.IsFirstLevel())
            {
                model.AsFirstLevel();
            }
            else
            {
                var parent = await repo.GetFirstAsync(x => x.UID == model.ParentUID);
                Com.AssertNotNull(parent, "父节点为空");
                model.Level = parent.Level + 1;
            }
            model.Init(model_flag);
            if (!model.IsValid(out var msg))
            {
                data.SetErrorMsg(msg);
                return data;
            }

            if (checker != null)
            {
                var res = await checker.CheckEntityWhenAdd(model);
                if (res.error)
                {
                    data.SetErrorMsg(res.msg);
                    return data;
                }
            }

            if (await repo.AddAsync(model) > 0)
            {
                data.SetSuccessData(model);
                return data;
            }

            throw new Exception("添加失败");
        }

        public static async Task<List<T>> QueryNodeList<T>(this ILinqRepository<T> repo,
            string parent = null, int? level = null, string group = null, int max = 5000)
            where T : TreeEntityBase
        {
            return await repo.PrepareIQueryableAsync(async query =>
            {
                if (ValidateHelper.IsPlumpString(group))
                {
                    query = query.Where(x => x.GroupKey == group);
                }
                if (ValidateHelper.IsPlumpString(parent))
                {
                    query = query.Where(x => x.ParentUID == parent);
                }
                if (level != null)
                {
                    query = query.Where(x => x.Level == level);
                }
                return await query.OrderByDescending(x => x.CreateTime).Take(max).ToListAsync();
            });
        }

        public static async Task<_<int>> DeleteTreeNodeRecursively<T>(this IRepository<T> repo, string node_uid)
            where T : TreeEntityBase
        {
            var data = new _<int>();
            var node = await repo.GetFirstAsync(x => x.UID == node_uid);
            Com.AssertNotNull(node, "节点不存在");

            var list = await repo.GetListEnsureMaxCountAsync(x => x.GroupKey == node.GroupKey, 5000, "树节点数量达到上线");

            var dead_nodes = list.FindNodeChildrenRecursively_(node);

            var count = await repo.DeleteAsync(dead_nodes.ToArray());

            data.SetSuccessData(count);
            return data;
        }

        public static async Task<_<int>> DeleteSingleNodeWhenNoChildren_<T>(this IRepository<T> repo, string node_uid)
            where T : TreeEntityBase
        {
            var data = new _<int>();
            if (await repo.ExistAsync(x => x.ParentUID == node_uid))
            {
                data.SetErrorMsg("节点存在子节点，不能删除");
                return data;
            }

            var count = await repo.DeleteWhereAsync(x => x.UID == node_uid);

            data.SetSuccessData(count);
            return data;
        }

        /// <summary>
        /// 判断是父级节点
        /// </summary>
        /// <returns></returns>
        public static bool IsFirstLevel<T>(this T model)
            where T : TreeEntityBase
            => !ValidateHelper.IsPlumpStringAfterTrim(model.ParentUID);

        /// <summary>
        /// 修改节点层级和父级为第一级
        /// </summary>
        public static T AsFirstLevel<T>(this T model)
            where T : TreeEntityBase
        {
            model.ParentUID = TreeEntityBase.FIRST_PARENT_UID;
            model.Level = TreeEntityBase.FIRST_LEVEL;
            return model;
        }

        /// <summary>
        /// 如果节点的层级和父级错误，就修改为第一级
        /// </summary>
        public static T AsFirstLevelIfParentIsNotValid<T>(this T model)
            where T : TreeEntityBase
        {
            if (!ValidateHelper.IsPlumpStringAfterTrim(model.ParentUID) || model.ParentUID == TreeEntityBase.FIRST_PARENT_UID)
            {
                model.AsFirstLevel();
            }
            return model;
        }
    }
}
