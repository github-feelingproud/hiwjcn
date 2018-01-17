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

        public static implicit operator IViewTreeNode(RoleEntityBase data) =>
            new IViewTreeNode()
            {
                id = data.UID,
                pId = data.ParentUID,
                title = data.RoleName,
                raw_data = data
            };

        public static implicit operator IViewTreeNode(DepartmentEntityBase data) =>
            new IViewTreeNode()
            {
                id = data.UID,
                pId = data.ParentUID,
                title = data.DepartmentName,
                raw_data = data
            };

        public static implicit operator IViewTreeNode(MenuEntityBase data) =>
            new IViewTreeNode()
            {
                id = data.UID,
                pId = data.ParentUID,
                title = data.MenuName,
                raw_data = data
            };

        public static implicit operator IViewTreeNode(PermissionEntityBase data) =>
            new IViewTreeNode()
            {
                id = data.UID,
                pId = data.ParentUID,
                title = data.Description,
                raw_data = data
            };
    }
}
