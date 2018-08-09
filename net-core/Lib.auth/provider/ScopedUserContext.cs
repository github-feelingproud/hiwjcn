using System;
using System.Collections.Generic;
using System.Text;

namespace Lib.auth.provider
{
    public class ScopedUserContext : IScopedUserContext
    {
        public LoginUserInfo User => throw new NotImplementedException();

        public bool IsLogin => throw new NotImplementedException();


        public ScopedUserContext()
        { }
        
        public bool HasPermission(string permission)
        {
            throw new NotImplementedException();
        }

        public void Dispose() { }
    }
}
