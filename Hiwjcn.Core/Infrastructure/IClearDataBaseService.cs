using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.ioc;

namespace Hiwjcn.Core.Infrastructure
{
    public interface IClearDataBaseService : IAutoRegistered
    {
        void ClearScope();

        void ClearToken();

        void ClearClient();

        void ClearUser();

        void ClearPage();

        void ClearRequestLog();

        void ClearCacheHitLog();

        void ClearTag();

        void ClearRole();

        void ClearLoginLog();
    }
}
