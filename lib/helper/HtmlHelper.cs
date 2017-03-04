using System.Collections.Generic;
using System.Text;

namespace Lib.helper
{
    /// <summary>
    /// 生成bootstrap重panel的机构
    /// </summary>
    public class PanelBox
    {
        public string Title { get; set; }
        public string ID { get; set; }

        private readonly List<string> content = new List<string>();

        public void Clear()
        {
            content.Clear();
        }

        public void Append(string html)
        {
            this.content.Add(html);
        }

        public void Append(PanelBox box)
        {
            if (box != null)
            {
                Append(box.ToString());
            }
        }

        public override string ToString()
        {
            if (!ValidateHelper.IsPlumpString(ID)) { ID = Com.GetUUID(); }
            StringBuilder html = new StringBuilder();
            html.Append($"<div class='panel panel-info' id='{this.ID}'>");
            html.Append($"<div class='panel-heading'>{(ValidateHelper.IsPlumpString(Title) ? Title : ID)}</div>");
            html.Append("<div class='panel-body'>");
            content.ForEach(x => { html.Append(x); });
            html.Append("</div>");
            html.Append("</div>");
            return html.ToString();
        }
    }
}
