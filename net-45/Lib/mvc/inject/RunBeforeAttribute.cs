using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Lib.mvc.inject
{
    public abstract class RunBeforeAttribute : System.Attribute
    {
        public abstract void Run(Controller controller);
    }
}
