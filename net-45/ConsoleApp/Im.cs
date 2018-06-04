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
using SuperSocket;
using SuperSocket.WebSocket;
using SuperSocket.SocketBase.Config;

namespace ConsoleApp
{
    class Im
    {
        public Im()
        {
            var server = new WebSocketServer();
            if (!server.Setup(new ServerConfig()
            {
                Port = 2017,
                MaxConnectionNumber = 10000
            }))
            {
                throw new Exception("配置服务器失败");
            }
            if (!server.Start())
            {
                throw new Exception("服务器启动失败");
            }

            server.NewSessionConnected += (sess) =>
            {
                Console.WriteLine($"{sess.SessionID}:Open");
                var valid = false;
                if (!valid)
                {
                    sess.Send(new { close = true, reason = "验证未通过" }.ToJson());
                    sess.Close();
                    return;
                }
                sess.Send("hello");
            };
            server.NewMessageReceived += (sess, msg) =>
            {
                //
            };
            server.SessionClosed += (sess, reason) =>
            {
                server.GetAllSessions();
            };

            Thread.Sleep(100000);
            server.Dispose();
        }
    }

    public class ClientHandler
    {
        public ClientHandler(Socket client)
        {
            var bs = new byte[1024];
            var len = client.Receive(bs, SocketFlags.Broadcast);
            client.Send(bs);

            client.BeginAccept(res =>
            {
                //
            }, null);
        }
    }

}
