using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.extension;
using ZooKeeperNet;
using System.Threading;

namespace Lib.distributed
{
    public class DistributedLockClient : ZooKeeperClient
    {
        private readonly string _basePath;


        public DistributedLockClient(string key) : this(key, "zookeeper") { }

        public DistributedLockClient(string key, string configurationName) : this(key, ZooKeeperConfigSection.FromSection(configurationName)) { }
        
        public DistributedLockClient(string key, ZooKeeperConfigSection configuration) : base(configuration)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            _basePath = configuration.DistributedLockPath + "/" + key.ToMD5();
        }
        
        public Action<DistributedLockClient> OnMasterChanged;

        private string _sequenceNo;
        private string _preNo;
        
        public void Lock()
        {
            if (_sequenceNo == null)
                AlwaysRun(() =>
                {
                    CreatePersistentPath(_basePath);
                    _sequenceNo = CreateSequential(_basePath + "/", false);
                }, 5);
            else
                throw new Exception("锁释放前不能重复锁");

            Task.Run(new Action(Revote));
        }

        private void AlwaysRun(Action action, int times)
        {
            var counter = 0;
            do
            {
                try
                {
                    action();

                    break;
                }
                catch (KeeperException.ConnectionLossException ex)
                {
                    ex.AddErrorLog();
                    Thread.Sleep(TimeSpan.FromSeconds(Math.Pow(1.5, counter++ - 5)));
                }
                catch (KeeperException.NodeExistsException ex)
                {
                    ex.AddErrorLog();
                    counter++;
                }
                catch (KeeperException.NoNodeException ex)
                {
                    ex.AddErrorLog();
                    counter++;
                }
            } while (counter < times);
        }

        private void Revote()
        {
            var children = GetChildren(_basePath, true);

            //取小于它的最大值
            _preNo = children.Where(node => string.CompareOrdinal(node, _sequenceNo) < 0).OrderByDescending(node => node).FirstOrDefault();

            if (_preNo == null)
                OnMasterChanged?.Invoke(this);
            else if (_sequenceNo != null)
                Watch(_basePath + "/" + _preNo, new DistributedLockWatcher(Revote));
        }

        public void Release()
        {
            if (_sequenceNo == null)
                return;

            try
            {
                AlwaysRun(() => DeleteNode(_basePath + "/" + _sequenceNo), 5);
                DeleteNode(_basePath);
            }
            catch (Exception ex)
            {
                ex.AddErrorLog();
            }

            _sequenceNo = null;
        }

        class DistributedLockWatcher : IWatcher
        {
            private readonly Action _onNodeDeleted;

            public DistributedLockWatcher(Action onNodeDeleted)
            {
                _onNodeDeleted = onNodeDeleted;
            }

            public void Process(WatchedEvent @event)
            {
                if (@event.Type == EventType.NodeDeleted)
                    _onNodeDeleted?.Invoke();
            }
        }
    }
}
