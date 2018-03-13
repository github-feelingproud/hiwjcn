using System;

namespace Lib.distributed.zookeeper
{
    public class AlwaysOnZooKeeperClient : ZooKeeperClient
    {
        public event Action OnRecconected;

        public AlwaysOnZooKeeperClient(string host) : base(host)
        {
            this.OnUnConnected += () => this.ReConnect();
        }

        protected virtual void ReConnect()
        {
            //销毁的时候取消重试链接
            if (this.IsDisposing) { return; }

            this.CloseClient();
            this.CreateClient();
            this.OnRecconected.Invoke();
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
