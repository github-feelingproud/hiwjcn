using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.User;
using Lib.core;
using Dal;
using WebLogic.Model.User;
using Lib.data;

namespace WebLogic.Dal.User
{
    public class LoginErrorLogDal : EFRepository<LoginErrorLogModel>
    {
        public LoginErrorLogDal() { }
    }
}
