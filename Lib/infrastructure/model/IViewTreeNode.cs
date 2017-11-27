using Lib.infrastructure.entity;
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


        public static implicit operator IViewTreeNode(RoleEntityBase role) =>
            new IViewTreeNode()
            {
                id = role.UID,
                pId = role.ParentUID,
                title = role.RoleName,
            };

        public static implicit operator IViewTreeNode(DepartmentEntityBase department) =>
            new IViewTreeNode()
            {
                id = department.UID,
                pId = department.ParentUID,
                title = department.DepartmentName
            };

        public static implicit operator IViewTreeNode(MenuEntityBase menu) =>
            new IViewTreeNode()
            {
                id = menu.UID,
                pId = menu.ParentUID,
                title = menu.MenuName
            };
    }
}
