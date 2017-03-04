using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lib.mvc.inject
{
    public class LoadNavigationBeforeActionExecutingAttribute : RunBeforeAttribute
    {
        public override void Run(System.Web.Mvc.Controller controller)
        {
            //dosomething
        }
    }
}
