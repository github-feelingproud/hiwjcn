using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebLogic.Bll.User;

namespace Hiwjcn.Web
{
    public partial class index : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //用户可以购物和发帖
            int userpermission = (int)(FunctionsEnum.购物 | FunctionsEnum.发帖);

            bool pass = FunctionPermission.AllPass(userpermission, FunctionsEnum.发帖);
            pass = FunctionPermission.AllPass(userpermission, FunctionsEnum.购物);
            pass = FunctionPermission.AllPass(userpermission, FunctionsEnum.开店);

            pass = FunctionPermission.AllPass(userpermission, (FunctionsEnum.发帖 | FunctionsEnum.购物));
            pass = FunctionPermission.AllPass(userpermission, (FunctionsEnum.发帖 | FunctionsEnum.开店));

            Response.Redirect("/page/home/");
            return;
        }
    }
}