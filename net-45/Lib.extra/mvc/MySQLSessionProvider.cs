using System;
using System.Configuration;
using System.Collections.Specialized;
using System.Web.Hosting;
using MySql.Data.MySqlClient;
using System.IO;
using System.Web.SessionState;
using System.Web.Configuration;
using System.Web;
using System.Threading;
using Lib.helper;
using Dapper;
using Lib.core;
using Lib.extension;

namespace Lib.extra.mvc
{
    /// <summary>
    /// mysql.web.dll
    /// 使用mysql保存session，使用mysql.web组件（安装此组建会自动添加很多配置节点，只要保留下面节点就可以了）
    /// http://dev.mysql.com/downloads/connector/net/ 选择source code下载
    /// </summary>
    public class MySQLSessionProvider : SessionStateStoreProviderBase
    {
        private const string CreateTableSql = @"CREATE TABLE 
IF NOT EXISTS `my_aspnet_sessions` (
	`SessionId` VARCHAR (191) NOT NULL,
	`ApplicationId` INT (11) NOT NULL,
	`Created` datetime NOT NULL,
	`Expires` datetime NOT NULL,
	`LockDate` datetime NOT NULL,
	`LockId` INT (11) NOT NULL,
	`Timeout` INT (11) NOT NULL,
	`Locked` TINYINT (1) NOT NULL,
	`SessionItems` LONGBLOB,
	`Flags` INT (11) NOT NULL,
	PRIMARY KEY (
		`SessionId`,
		`ApplicationId`
	)
) ENGINE = INNODB DEFAULT CHARSET = utf8;";

        private string connectionString { get; set; }
        private SessionStateSection sessionStateConfig { get; set; }
        private Timer cleanupTimer { get; set; }
        private long ApplicationId { get; set; }

        /// <summary>
        /// Initializes the provider with the property values specified in the ASP.NET application configuration file
        /// </summary>
        /// <param name="name">The name of the provider instance to initialize.</param>
        /// <param name="config">Object that contains the names and values of configuration options for the provider.
        /// </param>
        public override void Initialize(string name, NameValueCollection config)
        {
            //Initialize values from web.config.
            base.Initialize(name, config);

            this.ApplicationId = long.Parse(config["AppID"]);

            // Get <sessionState> configuration element.
            var webConfig = WebConfigurationManager.OpenWebConfiguration(HostingEnvironment.ApplicationVirtualPath);
            sessionStateConfig = (SessionStateSection)webConfig.SectionGroups["system.web"].Sections["sessionState"];

            // Initialize connection.
            this.connectionString = ConfigurationManager.ConnectionStrings["MySqlConnectionString"]?.ConnectionString ?? 
                throw new ArgumentNullException("MySqlConnectionString for session");
            if (connectionString?.Length <= 0) { throw new Exception("连接字符串不能为空"); }

            using (var con = new MySqlConnection(connectionString))
            {
                con.Open();
                con.Execute(CreateTableSql);
            }

            //建议使用有事务功能的存储引擎

            cleanupTimer = new Timer(new TimerCallback(CleanupOldSessions), null, 0, 3 * 1000 * 60);
        }

        /// <summary>
        /// This method creates a new SessionStateStoreData object for the current request.
        /// </summary>
        /// <param name="context">
        /// The HttpContext object for the current request.
        /// </param>
        /// <param name="timeout">
        /// The timeout value (in minutes) for the SessionStateStoreData object that is created.
        /// </param>
        public override SessionStateStoreData CreateNewStoreData(System.Web.HttpContext context, int timeout)
        {

            return new SessionStateStoreData(new SessionStateItemCollection(),
                SessionStateUtility.GetSessionStaticObjects(context), timeout);
        }

        /// <summary>
        /// This method adds a new session state item to the database.
        /// </summary>
        /// <param name="context">
        /// The HttpContext object for the current request.
        /// </param>
        /// <param name="id">
        /// The session ID for the current request.
        /// </param>
        /// <param name="timeout">
        /// The timeout value for the current request.
        /// </param>
        public override void CreateUninitializedItem(System.Web.HttpContext context, string id, int timeout)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                MySqlCommand cmd = new MySqlCommand(
        @"INSERT INTO my_aspnet_sessions
    (SessionId, ApplicationId, Created, Expires, LockDate,
    LockId, Timeout, Locked, SessionItems, Flags)
    Values (@SessionId, @ApplicationId, NOW(), NOW() + INTERVAL @Timeout MINUTE, NOW(),
    @LockId , @Timeout, @Locked, @SessionItems, @Flags) 
  on duplicate key update Created = values( Created ), Expires = values( Expires ),
    LockDate = values( LockDate ), LockId = values( LockId ), Timeout = values( Timeout) ,
    Locked = values( Locked ), SessionItems = values( SessionItems ), Flags = values( Flags )",
                   conn);

                cmd.Parameters.AddWithValue("@SessionId", id);
                cmd.Parameters.AddWithValue("@ApplicationId", ApplicationId);
                cmd.Parameters.AddWithValue("@LockId", 0);
                cmd.Parameters.AddWithValue("@Timeout", timeout);
                cmd.Parameters.AddWithValue("@Locked", 0);
                cmd.Parameters.AddWithValue("@SessionItems", null);
                cmd.Parameters.AddWithValue("@Flags", 1);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// This method releases all the resources for this instance.
        /// </summary>
        public override void Dispose()
        {
            cleanupTimer?.Dispose();
        }

        /// <summary>
        /// This method allows the MySqlSessionStateStore object to perform any cleanup that may be 
        /// required for the current request.
        /// </summary>
        /// <param name="context">The HttpContext object for the current request</param>
        public override void EndRequest(System.Web.HttpContext context)
        {
        }

        /// <summary>
        /// This method returns a read-only session item from the database.
        /// </summary>
        public override SessionStateStoreData GetItem(System.Web.HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            return GetSessionStoreItem(false, context, id, out locked, out lockAge, out lockId, out actions);
        }

        /// <summary>
        /// This method locks a session item and returns it from the database
        /// </summary>
        /// <param name="context">The HttpContext object for the current request</param>
        /// <param name="id">The session ID for the current request</param>
        /// <param name="locked">
        /// true if the session item is locked in the database; otherwise, it is false.
        /// </param>
        /// <param name="lockAge">
        /// TimeSpan object that indicates the amount of time the session item has been locked in the database.
        /// </param>
        /// <param name="lockId">
        /// A lock identifier object.
        /// </param>
        /// <param name="actions">
        /// A SessionStateActions enumeration value that indicates whether or
        /// not the session is uninitialized and cookieless.
        /// </param>
        /// <returns></returns>
        public override SessionStateStoreData GetItemExclusive(System.Web.HttpContext context, string id,
            out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actions)
        {
            return GetSessionStoreItem(true, context, id, out locked, out lockAge, out lockId, out actions);
        }

        /// <summary>
        ///  This method performs any per-request initializations that the MySqlSessionStateStore provider requires.
        /// </summary>
        public override void InitializeRequest(System.Web.HttpContext context)
        {
        }

        /// <summary>
        /// This method forcibly releases the lock on a session item in the database, if multiple attempts to 
        /// retrieve the session item fail.
        /// </summary>
        /// <param name="context">The HttpContext object for the current request.</param>
        /// <param name="id">The session ID for the current request.</param>
        /// <param name="lockId">The lock identifier for the current request.</param>
        public override void ReleaseItemExclusive(System.Web.HttpContext context, string id, object lockId)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                MySqlCommand cmd = new MySqlCommand(
                    "UPDATE my_aspnet_sessions SET Locked = 0, Expires = NOW() + INTERVAL @Timeout MINUTE " +
                    "WHERE SessionId = @SessionId AND ApplicationId = @ApplicationId AND LockId = @LockId",
                    conn);

                cmd.Parameters.AddWithValue("@Timeout", sessionStateConfig.Timeout.TotalMinutes);
                cmd.Parameters.AddWithValue("@SessionId", id);
                cmd.Parameters.AddWithValue("@ApplicationId", ApplicationId);
                cmd.Parameters.AddWithValue("@LockId", lockId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// This method removes the specified session item from the database
        /// </summary>
        /// <param name="context">The HttpContext object for the current request</param>
        /// <param name="id">The session ID for the current request</param>
        /// <param name="lockId">The lock identifier for the current request.</param>
        /// <param name="item">The session item to remove from the database.</param>
        public override void RemoveItem(System.Web.HttpContext context, string id, object lockId, SessionStateStoreData item)
        {
            bool sessionDeleted;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                MySqlCommand cmd = new MySqlCommand("DELETE FROM my_aspnet_sessions " +
                    " WHERE SessionId = @SessionId AND ApplicationId = @ApplicationId AND LockId = @LockId",
                    conn);

                cmd.Parameters.AddWithValue("@SessionId", id);
                cmd.Parameters.AddWithValue("@ApplicationId", ApplicationId);
                cmd.Parameters.AddWithValue("@LockId", lockId);
                conn.Open();
                sessionDeleted = cmd.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// This method resets the expiration date and timeout for a session item in the database.
        /// </summary>
        /// <param name="context">The HttpContext object for the current request</param>
        /// <param name="id">The session ID for the current request</param>
        public override void ResetItemTimeout(System.Web.HttpContext context, string id)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                MySqlCommand cmd = new MySqlCommand(
                    "UPDATE my_aspnet_sessions SET Expires = NOW() + INTERVAL @Timeout MINUTE" +
                   " WHERE SessionId = @SessionId AND ApplicationId = @ApplicationId", conn);

                cmd.Parameters.AddWithValue("@Timeout", sessionStateConfig.Timeout.TotalMinutes);
                cmd.Parameters.AddWithValue("@SessionId", id);
                cmd.Parameters.AddWithValue("@ApplicationId", ApplicationId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// This method updates the session time information in the database with the specified session item,
        /// and releases the lock.
        /// </summary>
        /// <param name="context">The HttpContext object for the current request</param>
        /// <param name="id">The session ID for the current request</param>
        /// <param name="item">The session item containing new values to update the session item in the database with.
        /// </param>
        /// <param name="lockId">The lock identifier for the current request.</param>
        /// <param name="newItem">A Boolean value that indicates whether or not the session item is new in the database.
        /// A false value indicates an existing item.
        /// </param>
        public override void SetAndReleaseItemExclusive(System.Web.HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                // Serialize the SessionStateItemCollection as a byte array
                byte[] sessItems = Serialize((SessionStateItemCollection)item.Items);
                MySqlCommand cmd;
                if (newItem)
                {
                    //Insert the new session item . If there was expired session
                    //with the same SessionId and Application id, it will be removed

                    cmd = new MySqlCommand(
          @"INSERT INTO my_aspnet_sessions
    (SessionId, ApplicationId, Created, Expires, LockDate,
    LockId, Timeout, Locked, SessionItems, Flags)
    Values (@SessionId, @ApplicationId, NOW(), NOW() + INTERVAL @Timeout MINUTE, NOW(),
    @LockId , @Timeout, @Locked, @SessionItems, @Flags) 
  on duplicate key update Created = values( Created ), Expires = values( Expires ),
    LockDate = values( LockDate ), LockId = values( LockId ), Timeout = values( Timeout) ,
    Locked = values( Locked ), SessionItems = values( SessionItems ), Flags = values( Flags )", conn);
                    cmd.Parameters.AddWithValue("@SessionId", id);
                    cmd.Parameters.AddWithValue("@ApplicationId", ApplicationId);
                    cmd.Parameters.AddWithValue("@Timeout", item.Timeout);
                    cmd.Parameters.AddWithValue("@LockId", 0);
                    cmd.Parameters.AddWithValue("@Locked", 0);
                    cmd.Parameters.AddWithValue("@SessionItems", sessItems);
                    cmd.Parameters.AddWithValue("@Flags", 0);
                }
                else
                {
                    //Update the existing session item.
                    cmd = new MySqlCommand(
                         "UPDATE my_aspnet_sessions SET Expires = NOW() + INTERVAL @Timeout MINUTE," +
                         " SessionItems = @SessionItems, Locked = @Locked " +
                         " WHERE SessionId = @SessionId AND ApplicationId = @ApplicationId AND LockId = @LockId",
                         conn);

                    cmd.Parameters.AddWithValue("@Timeout", item.Timeout);
                    cmd.Parameters.AddWithValue("@SessionItems", sessItems);
                    cmd.Parameters.AddWithValue("@Locked", 0);
                    cmd.Parameters.AddWithValue("@SessionId", id);
                    cmd.Parameters.AddWithValue("@ApplicationId", ApplicationId);
                    cmd.Parameters.AddWithValue("@LockId", lockId);
                }
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        ///  GetSessionStoreItem is called by both the GetItem and  GetItemExclusive methods. GetSessionStoreItem 
        ///  retrieves the session data from the data source. If the lockRecord parameter is true (in the case of 
        ///  GetItemExclusive), then GetSessionStoreItem  locks the record and sets a New LockId and LockDate.
        /// </summary>
        private SessionStateStoreData GetSessionStoreItem(bool lockRecord,
               HttpContext context,
               string id,
               out bool locked,
               out TimeSpan lockAge,
               out object lockId,
               out SessionStateActions actionFlags)
        {

            // Initial values for return value and out parameters.
            SessionStateStoreData item = null;
            lockAge = TimeSpan.Zero;
            lockId = null;
            locked = false;
            actionFlags = SessionStateActions.None;

            // MySqlCommand for database commands.
            MySqlCommand cmd = null;
            // serialized SessionStateItemCollection.
            byte[] serializedItems = null;
            // True if a record is found in the database.
            bool foundRecord = false;
            // True if the returned session item is expired and needs to be deleted.
            bool deleteData = false;
            // Timeout value from the data store.
            int timeout = 0;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                // lockRecord is True when called from GetItemExclusive and
                // False when called from GetItem.
                // Obtain a lock if possible. Ignore the record if it is expired.
                if (lockRecord)
                {
                    cmd = new MySqlCommand(
                      "UPDATE my_aspnet_sessions SET " +
                      " Locked = 1, LockDate = NOW()" +
                      " WHERE SessionId = @SessionId AND ApplicationId = @ApplicationId AND" +
                      " Locked = 0 AND Expires > NOW()", conn);

                    cmd.Parameters.AddWithValue("@SessionId", id);
                    cmd.Parameters.AddWithValue("@ApplicationId", ApplicationId);

                    if (cmd.ExecuteNonQuery() == 0)
                    {
                        // No record was updated because the record was locked or not found.
                        locked = true;
                    }
                    else
                    {
                        // The record was updated.
                        locked = false;
                    }
                }

                // Retrieve the current session item information.
                cmd = new MySqlCommand(
                  "SELECT NOW(), Expires , SessionItems, LockId,  Flags, Timeout, " +
            "  LockDate, Locked " +
                  "  FROM my_aspnet_sessions" +
                  "  WHERE SessionId = @SessionId AND ApplicationId = @ApplicationId", conn);

                cmd.Parameters.AddWithValue("@SessionId", id);
                cmd.Parameters.AddWithValue("@ApplicationId", ApplicationId);

                // Retrieve session item data from the data source.
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        DateTime now = reader.GetDateTime(0);
                        DateTime expires = reader.GetDateTime(1);
                        if (now.CompareTo(expires) > 0)
                        {
                            //The record was expired. Mark it as not locked.
                            locked = false;
                            // The session was expired. Mark the data for deletion.
                            deleteData = true;
                        }
                        else
                        {
                            foundRecord = true;
                        }

                        object items = reader.GetValue(2);
                        serializedItems = (items is DBNull) ? null : (byte[])items;
                        lockId = reader.GetValue(3);
                        if (lockId is DBNull)
                            lockId = (int)0;

                        actionFlags = (SessionStateActions)(reader.GetInt32(4));
                        timeout = reader.GetInt32(5);
                        DateTime lockDate = reader.GetDateTime(6);
                        lockAge = now.Subtract(lockDate);
                        // If it's a read-only session set locked to the current lock
                        // status (writable sessions have already done this)
                        if (!lockRecord)
                            locked = reader.GetBoolean(7);
                    }
                }

                // The record was not found. Ensure that locked is false.
                if (!foundRecord)
                    locked = false;

                // If the record was found and you obtained a lock, then set 
                // the lockId, clear the actionFlags,
                // and create the SessionStateStoreItem to return.
                if (foundRecord && !locked)
                {
                    lockId = (int)(lockId) + 1;

                    cmd = new MySqlCommand("UPDATE my_aspnet_sessions SET" +
                      " LockId = @LockId, Flags = 0 " +
                      " WHERE SessionId = @SessionId AND ApplicationId = @ApplicationId", conn);

                    cmd.Parameters.AddWithValue("@LockId", lockId);
                    cmd.Parameters.AddWithValue("@SessionId", id);
                    cmd.Parameters.AddWithValue("@ApplicationId", ApplicationId);
                    cmd.ExecuteNonQuery();


                    // If the actionFlags parameter is not InitializeItem, 
                    // deserialize the stored SessionStateItemCollection.
                    if (actionFlags == SessionStateActions.InitializeItem)
                    {
                        item = CreateNewStoreData(context, (int)sessionStateConfig.Timeout.TotalMinutes);
                    }
                    else
                    {
                        item = Deserialize(context, serializedItems, timeout);
                    }
                }
            }

            return item;
        }

        ///<summary>
        /// Serialize is called by the SetAndReleaseItemExclusive method to 
        /// convert the SessionStateItemCollection into a byte array to
        /// be stored in the blob field.
        /// </summary>
        private byte[] Serialize(SessionStateItemCollection items)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            if (items != null)
            {
                items.Serialize(writer);
            }
            writer.Close();
            return ms.ToArray();
        }

        ///<summary>
        /// Deserialize is called by the GetSessionStoreItem method to 
        /// convert the byte array stored in the blob field to a 
        /// SessionStateItemCollection.
        /// </summary>
        private SessionStateStoreData Deserialize(HttpContext context,
          byte[] serializedItems, int timeout)
        {

            SessionStateItemCollection sessionItems = new SessionStateItemCollection();

            if (serializedItems != null)
            {
                MemoryStream ms = new MemoryStream(serializedItems);
                if (ms.Length > 0)
                {
                    BinaryReader reader = new BinaryReader(ms);
                    sessionItems = SessionStateItemCollection.Deserialize(reader);
                }
            }

            return new SessionStateStoreData(sessionItems, SessionStateUtility.GetSessionStaticObjects(context),
                timeout);
        }

        private bool cleanupRunning { get; set; } = false;
        private void CleanupOldSessions(object o)
        {

            lock (this)
            {
                if (cleanupRunning)
                    return;

                cleanupRunning = true;
            }

            try
            {
                using (MySqlConnection con = new MySqlConnection(connectionString))
                {
                    con.Open();
                    MySqlCommand cmd = new MySqlCommand("DELETE FROM my_aspnet_sessions WHERE Expires < NOW() AND ApplicationId = @ApplicationId", con);
                    cmd.Parameters.AddWithValue("@ApplicationId", ApplicationId);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                e.AddLog(this.GetType());
            }

            lock (this)
            {
                cleanupRunning = false;
            }
        }

        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            return true;
        }
    }
}
