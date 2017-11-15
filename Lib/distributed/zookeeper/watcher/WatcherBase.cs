using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.distributed.zookeeper.watcher
{
    public abstract class WatcherBase : Watcher
    {
        protected readonly string Path;

        protected WatcherBase(string path)
        {
            this.Path = path;
        }

        public override async Task process(WatchedEvent watchedEvent)
        {
            if (watchedEvent.getState() != Event.KeeperState.SyncConnected || watchedEvent.getPath() != Path)
                return;
            await ProcessImpl(watchedEvent);
        }

        protected abstract Task ProcessImpl(WatchedEvent watchedEvent);
    }
}
