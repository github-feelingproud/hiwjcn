using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.distributed
{
    /// <summary>
    /// zookeeper配置
    /// </summary>
    public class ZooKeeperClientSection : ConfigurationSection
    {
        public static ZooKeeperClientSection FromSection(string name)
        {
            return (ZooKeeperClientSection)ConfigurationManager.GetSection(name);
        }

        [ConfigurationProperty(nameof(SessionTimeOut), IsRequired = true)]
        public int SessionTimeOut
        {
            get { return int.Parse(this[nameof(SessionTimeOut)].ToString()); }
        }

        [ConfigurationProperty(nameof(Server), IsRequired = true)]
        public string Server
        {
            get { return this[nameof(Server)].ToString(); }
        }

        [ConfigurationProperty(nameof(MasterSlavePath), IsRequired = false)]
        public string MasterSlavePath
        {
            get { return this[nameof(MasterSlavePath)].ToString(); }
        }

        [ConfigurationProperty(nameof(DistributedLockPath), IsRequired = false)]
        public string DistributedLockPath
        {
            get { return this[nameof(DistributedLockPath)].ToString(); }
        }
    }
}
