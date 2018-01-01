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
        public static IEnumerable<ZTreeNode> BuildZTreeStructure(IEnumerable<ZTreeNode> list)
        {
            if (list.Any(x => !ValidateHelper.IsPlumpString(x.id))) { throw new Exception("每个节点都需要id"); }

            var data = list.Where(x => !ValidateHelper.IsPlumpString(x.pId));
            var repeat = new List<string>();

            void BindChildren(ref IEnumerable<ZTreeNode> nodes)
            {
                foreach (var m in nodes)
                {
                    repeat.AddOnceOrThrow(m.id, "树存在错误");

                    var children = list.Where(x => x.pId == m.id);
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
        /// 把扁平结构变成树形结构
        /// </summary>
        public static IEnumerable<IViewTreeNode> BuildIViewTreeStructure(IEnumerable<IViewTreeNode> list)
        {
            if (list.Any(x => !ValidateHelper.IsPlumpString(x.id))) { throw new Exception("每个节点都需要id"); }

            var data = list.Where(x => !ValidateHelper.IsPlumpString(x.pId));
            var repeat = new List<string>();

            void BindChildren(ref IEnumerable<IViewTreeNode> nodes)
            {
                foreach (var m in nodes)
                {
                    repeat.AddOnceOrThrow(m.id, "树存在错误");

                    var children = list.Where(x => x.pId == m.id);
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
    }
}
