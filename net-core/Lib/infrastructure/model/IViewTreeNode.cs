using Lib.infrastructure.entity;
using Lib.infrastructure.entity.user;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.infrastructure.model
{
    /// <summary>
    /// https://www.iviewui.com/components/tree
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.OptOut)]
    public class IViewTreeNode
    {
        [JsonProperty(Required = Required.Always)]
        public virtual string id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public virtual string pId { get; set; }

        public virtual string title { get; set; }

        public virtual bool expand { get; set; } = false;

        public virtual bool disabled { get; set; } = false;

        public virtual bool disableCheckbox { get; set; } = false;

        public virtual bool selected { get; set; } = false;

        public virtual bool @checked { get; set; } = false;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public virtual List<IViewTreeNode> children { get; set; }

        public virtual bool match { get; set; } = false;

        public virtual object raw_data { get; set; }

        public static IViewTreeNode FromTreeData<T>(T data, Func<T, string> title_selector)
            where T : TreeEntityBase
        {
            return new IViewTreeNode()
            {
                id = data.UID,
                pId = data.ParentUID,
                title = title_selector.Invoke(data),
                raw_data = data
            };
        }

        public static implicit operator IViewTreeNode(RoleEntityBase data) =>
            IViewTreeNode.FromTreeData(data, x => x.RoleName);

        public static implicit operator IViewTreeNode(MenuEntityBase data) =>
            IViewTreeNode.FromTreeData(data, x => x.MenuName);

        public static implicit operator IViewTreeNode(PermissionEntityBase data) =>
            IViewTreeNode.FromTreeData(data, x => x.Description);
    }
}
