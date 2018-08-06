using System.Configuration;
using System;

namespace Lib.distributed.zookeeper
{
    /// <summary>
    /// zookeeper配置
    /// </summary>
    public class ZooKeeperConfigSection
    {
        public static ZooKeeperConfigSection FromSection(string name) => throw new NotImplementedException();

        public int SessionTimeOut { get; set; }

        public string Server { get; set; }

        public string MasterSlavePath { get; set; }

        public string DistributedLockPath { get; set; }
    }
}
