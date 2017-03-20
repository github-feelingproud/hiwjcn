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

        #region ctor
        /// <summary>ctor</summary>
        /// <param name="key">锁Key</param>
        public DistributedLockClient(string key) : this(key, DefaultConfigurationName) { }
        /// <summary>ctor</summary>
        /// <param name="key">锁Key</param>
        /// <param name="configurationName">配置节点名称，默认distributedLock</param>
        public DistributedLockClient(string key, string configurationName) : this(key, ZooKeeperClientSection.FromSection(configurationName)) { }
        /// <summary>ctor</summary>
        /// <param name="key">锁Key</param>
        /// <param name="configuration">配置</param>
        public DistributedLockClient(string key, ZooKeeperClientSection configuration) : base(configuration)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            _basePath = configuration.DistributedLockPath + "/" + key.ToMD5();
        }
        #endregion

        /// <summary>是否是主</summary>
        public bool IsMaster => _preNo == null;

        /// <summary>Master变动事件通知</summary>
        public Action<DistributedLockClient> OnMasterChanged;

        private string _sequenceNo;
        private string _preNo;

        /// <summary></summary>
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
