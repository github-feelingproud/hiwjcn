using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lib.data;
using Lib.core;
using Lib.helper;
using Lib.extension;
using Polly;

namespace ConsoleApp
{
    class Im
    {
    }

    public class ClientHandler
    {
        public ClientHandler(Socket client)
        {
            var bs = new byte[1024];
            var len = client.Receive(bs, SocketFlags.Broadcast);
            client.Send(bs);
            
            client.BeginAccept(res=>
            {
                Task.Factory.FromAsync(res, r => { });
            },null);
        }
    }

}
