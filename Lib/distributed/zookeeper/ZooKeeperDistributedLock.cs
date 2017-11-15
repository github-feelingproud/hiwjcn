using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lib.extension;
using System.Threading;
using System.Diagnostics;
using Lib.helper;
using Polly;
using Polly.Retry;

namespace Lib.distributed.zookeeper
{
    public class ZooKeeperDistributedLock : IDistributedLock
    {
        private readonly ZooKeeperClient _client;
        private readonly string _path;
        private string _no;

        private RetryPolicy retry = Policy.Handle<Exception>()
                .WaitAndRetryAsync(5, i => TimeSpan.FromMilliseconds(i * 50));

        public ZooKeeperDistributedLock(string key, string configName)
        {
            var config = ZooKeeperConfigSection.FromSection(configName);
            _client = new ZooKeeperClient(config);
            _path = config.DistributedLockPath + "/" + key.ToMD5();

            //抢锁
            Task.Factory.StartNew(async () =>
            {
                await this.retry.ExecuteAsync(async () =>
                {
                    await this._client.Client.CreatePersistentPathIfNotExist_(_path);
                    _no = await this._client.Client.CreateSequential_(_path + "/", null, false);
                });
            }).Wait();
        }

        public void Dispose()
        {
            if (ValidateHelper.IsPlumpString(_no))
            {
                Task.Factory.StartNew(async () =>
                {
                    await this.retry.ExecuteAsync(async () =>
                    {
                        await this._client.Client.DeleteSingleNode_(this._path + "/" + this._no);
                        await this._client.Client.DeleteSingleNode_(this._path);
                    });
                }).Wait();
            }
            _client.Dispose();
        }
    }
}
