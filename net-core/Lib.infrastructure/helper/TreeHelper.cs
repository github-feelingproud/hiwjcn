using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure.model;
using Lib.helper;
using Lib.extension;
using Lib.infrastructure.extension;
using Lib.infrastructure.entity;

namespace Lib.infrastructure.helper
{
    public static class TreeHelper
    {
        /// <summary>
        /// 把扁平结构变成树形结构
        /// </summary>
        public static IEnumerable<IViewTreeNode> BuildIViewTreeStructure(IEnumerable<IViewTreeNode> list)
        {
            if (list.Any(x => !ValidateHelper.IsPlumpString(x.id))) { throw new Exception("每个节点都需要id"); }

            var data = list.Where(x => !ValidateHelper.IsPlumpString(x.pId)).ToList();
            var repeat = new List<string>();

            void BindChildren(ref List<IViewTreeNode> nodes)
            {
                foreach (var m in nodes)
                {
                    repeat.AddOnceOrThrow(m.id, "树存在错误");

                    var children = list.Where(x => x.pId == m.id).ToList();
                    if (ValidateHelper.IsPlumpList(children))
                    {
                        BindChildren(ref children);

                        m.children = children.ToList();
                    }
                }
            }

            BindChildren(ref data);

            return data;
        }

        /// <summary>
        /// 搜索树结构，找到的节点设置match=true,
        /// 树必须是多层结构
        /// </summary>
        public static List<IViewTreeNode> SearchIViewTree(List<IViewTreeNode> list, string keyword)
        {
            var q = (keyword ?? string.Empty).ToLower();

            bool SearchTree(ref List<IViewTreeNode> data)
            {
                var match_res = false;
                foreach (var node in data)
                {
                    node.title = node.title ?? string.Empty;
                    var match = node.title.ToLower().Contains(q);
                    node.match = q.Length > 0 && match;

                    var children_match = false;

                    if (ValidateHelper.IsPlumpList(node.children))
                    {
                        var children = node.children;
                        children_match = SearchTree(ref children);
                    }
                    node.expand = match || children_match;
                    if (node.expand)
                    {
                        match_res = true;
                    }
                }
                return match_res;
            }

            SearchTree(ref list);

            return list;
        }
    }
}
