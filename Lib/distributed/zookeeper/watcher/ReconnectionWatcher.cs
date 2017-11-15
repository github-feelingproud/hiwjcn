using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.distributed.zookeeper.watcher
{
    public class ReconnectionWatcher : Watcher
    {
        private readonly Action _connectioned;
        private readonly Action _disconnect;

        public ReconnectionWatcher(Action connectioned, Action disconnect)
        {
            this._connectioned = connectioned;
            this._disconnect = disconnect;
        }
        
        public override async Task process(WatchedEvent watchedEvent)
        {
            if (watchedEvent.getState() == Event.KeeperState.SyncConnected)
            {
                _connectioned();
            }
            else
            {
                _disconnect();
            }
            await Task.FromResult(1);
        }
    }
}
