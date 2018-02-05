using MySql.Data.Entity;
using MySql.Data.MySqlClient;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServer;

namespace Hiwjcn.Core
{
    /// <summary>
    /// mysql数据库
    /// </summary>
    public class MySqlConfiguration : DbConfiguration
    {
        public MySqlConfiguration()
        {
            this.SetExecutionStrategy(MySqlProviderInvariantName.ProviderName, () => new MySqlExecutionStrategy());
            this.SetDefaultConnectionFactory(new MySqlConnectionFactory());
            this.SetProviderServices("MySql.Data.MySqlClient", new MySqlProviderServices());
        }
    }

    /// <summary>
    /// sqlserver数据库
    /// </summary>
    public class SqlServerConfiguration : DbConfiguration
    {
        public SqlServerConfiguration()
        {
            this.SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy());
            this.SetDefaultConnectionFactory(new LocalDbConnectionFactory("mssqllocaldb"));
        }
    }
}
