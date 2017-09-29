using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lib.extension;
using Lib.core;
using Lib.helper;
using Lib.ioc;
using Lib.data;
using org.apache.zookeeper;
using System.Threading.Tasks;
using static org.apache.zookeeper.ZooDefs;

namespace Hiwjcn.Test
{
    [TestClass]
    public class UnitTest7 : Watcher
    {
        private bool connected = false;
        public override async Task process(WatchedEvent @event)
        {
            if (@event.getState() == Event.KeeperState.SyncConnected)
            {
                this.connected = true;
            }
            await Task.FromResult(1);
        }

        [TestMethod]
        public async Task TestMethod1()
        {
            foreach (var i in Com.Range(100))
            {
                var client = new ZooKeeper("localhost:32771",
                    (int)TimeSpan.FromSeconds(5).TotalMilliseconds, this);
                try
                {
                    while (!this.connected)
                    {
                        await Task.Delay(10);
                    }

                    if (await client.existsAsync("/home", false) == null)
                    {
                        var path = await client.createAsync("/home", "".GetBytes(),
                            Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
                    }
                    
                    var bs = new { id = 2, name = "fas", age = 44, time = DateTime.Now }.ToJson().GetBytes();

                    await client.setDataAsync("/home", bs);

                    var data = await client.getDataAsync("/home", false);

                    var children = await client.getChildrenAsync("/home");

                    var t = client.transaction();

                    //t.delete("/home");
                    t.setData("/home", $"{DateTime.Now.Ticks}".GetBytes());

                    var res = await t.commitAsync();
                }
                catch (Exception e)
                {
                    //
                }
                finally
                {
                    await client.closeAsync();
                }
            }
        }
    }
}
