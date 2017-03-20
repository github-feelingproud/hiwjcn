using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.extension;
using ZooKeeperNet;
using System.Threading;
using System.Diagnostics;
using Lib.helper;

namespace Lib.distributed
{
    public class ZooKeeperDistributedLock : ZooKeeperClient, IDistributedLock
    {
        private readonly string _basePath;

        public ZooKeeperDistributedLock(string key) : this(key, "zookeeper") { }

        public ZooKeeperDistributedLock(string key, string configurationName) :
            this(key, ZooKeeperConfigSection.FromSection(configurationName))
        { }

        public ZooKeeperDistributedLock(string key, ZooKeeperConfigSection configuration) : base(configuration)
        {
            if (!ValidateHelper.IsPlumpString(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            _basePath = configuration.DistributedLockPath + "/" + key.ToMD5();
        }

        public Action<ZooKeeperDistributedLock> OnMasterChanged { get; set; }

        private string _sequenceNo { get; set; }
        private string _preNo { get; set; }

        public void Lock()
        {
            if (_sequenceNo == null)
            {
                new Action(() =>
                {
                    CreatePersistentPath(_basePath);
                    _sequenceNo = CreateSequential(_basePath + "/", false);
                }).InvokeWithRetry(5);
            }
            else
                throw new Exception("锁释放前不能重复锁");

            Task.Run(new Action(Revote));
        }

        private void Revote()
        {
            var children = GetChildren(_basePath, true);

            //取小于它的最大值
            _preNo = children.Where(node => string.CompareOrdinal(node, _sequenceNo) < 0).OrderByDescending(node => node).FirstOrDefault();

            if (_preNo == null)
            {
                OnMasterChanged?.Invoke(this);
            }
            else if (_sequenceNo != null)
            {
                Watch(_basePath + "/" + _preNo, new DistributedLockWatcher(Revote));
            }
        }

        public void Release()
        {
            if (_sequenceNo == null)
                return;

            try
            {
                new Action(() =>
                {
                    DeleteNode(_basePath + "/" + _sequenceNo);
                }).InvokeWithRetry(5);

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
