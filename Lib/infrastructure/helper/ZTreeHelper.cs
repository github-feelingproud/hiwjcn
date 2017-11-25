using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.infrastructure.model;
using Lib.helper;
using Lib.extension;

namespace Lib.infrastructure.helper
{
    public static class ZTreeHelper
    {
        private static void BindChildren(ref IEnumerable<ZTreeNode> nodes, ref IEnumerable<ZTreeNode> source,
            ref List<string> repeat)
        {
            foreach (var m in nodes)
            {
                repeat.AddOnceOrThrow(m.id, $"树节点错误:{source.ToJson()}");

                var children = source.Where(x => x.pId == m.id);
                if (ValidateHelper.IsPlumpList(children))
                {
                    BindChildren(ref children, ref source, ref repeat);

                    //没有就给null
                    m.children = children.ToList();
                }
            }
        }

        /// <summary>
        /// 把扁平结构变成树形结构
        /// </summary>
        public static IEnumerable<ZTreeNode> BuildTreeStructure(IEnumerable<ZTreeNode> list)
        {
            if (list.Any(x => !ValidateHelper.IsPlumpString(x.id))) { throw new Exception("每个节点都需要id"); }

            //first level
            var data = list.Where(x => !ValidateHelper.IsPlumpString(x.pId));

            var repeat = new List<string>();

            BindChildren(ref data, ref list, ref repeat);

            return data;
        }
    }
}
