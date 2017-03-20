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
    public class ZooKeeperDistributedLock : IDistributedLock
    {
        private readonly ZooKeeperClient _client;
        private readonly string _path;
        private string _no;

        public ZooKeeperDistributedLock(string key, string configName)
        {
            var config = ZooKeeperConfigSection.FromSection(configName);
            _client = new ZooKeeperClient(config);
            _path = config.DistributedLockPath + "/" + key.ToMD5();

            //抢锁
            new Action(() =>
            {
                _client.CreatePersistentPath(_path);
                _no = _client.CreateSequential(_path + "/", false);
            }).InvokeWithRetry(5);
        }

        public void Dispose()
        {
            if (ValidateHelper.IsPlumpString(_no))
            {
                new Action(() =>
                {
                    _client.DeleteNode(_path + "/" + _no);
                    _client.DeleteNode(_path);
                }).InvokeWithRetry(5);
            }
            _client.Dispose();
        }
    }
}
