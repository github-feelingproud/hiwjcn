using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using Lib.cache;
using Lib.extension;
using Lib.ioc;
using Lib.helper;

namespace Hiwjcn.Framework.Actors
{
    /// <summary>
    /// 清理缓存
    /// </summary>
    public class ClearCacheActor : ReceiveActor
    {
        public ClearCacheActor()
        {
            this.Receive<IEnumerable<string>>(x =>
            {
                this.ClearCache(x?.ToList());
            });

            this.Receive<string>(x =>
            {
                this.ClearCache(new List<string> { x });
            });
        }

        private void ClearCache(List<string> list)
        {
            if (!ValidateHelper.IsPlumpList(list))
            {
                return;
            }
            using (var s = AutofacIocContext.Instance.Scope())
            {
                var cache = s.Resolve_<ICacheProvider>();
                foreach (var key in list)
                {
                    cache.Remove(key);
                }
            }
        }

    }
}
