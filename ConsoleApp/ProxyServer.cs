using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lib.core;
using Lib.helper;
using Lib.threading;

namespace ConsoleApp
{
    public class ProxyServer
    {
        public void StartServer()
        {
            var mainList = new List<ThreadWrapper>();
            var clientList = new List<ThreadWrapper>();

            // Create a listener for the proxy port
            var sockServer = new TcpListener(IPAddress.Parse("192.168.1.114"), 7878);
            var serverThread = new ThreadWrapper();
            serverThread.Run((keep) =>
            {
                try
                {
                    sockServer.Start();
                    Console.WriteLine("启动监听 ");
                    while (keep())
                    {
                        // Accept connections on the proxy port.
                        var socket = sockServer.AcceptSocket();
                        //删除已经结束的任务
                        while (clientList.Count() > 10000)
                        {
                            Thread.Sleep(100);
                            clientList = clientList.Where(x => x != null && !x.ThreadIsStoped()).ToList();
                        }

                        var clientHandler = new ThreadWrapper();
                        clientHandler.Run((keep1) =>
                        {
                            new ProxyHandler(socket).Run();
                            return true;
                        }, "客户端线程 ");
                        clientList.Add(clientHandler);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    Com.TryScope(() => { sockServer.Stop(); });
                }

                return true;
            }, "服务器线程");

            mainList.Add(serverThread);

            while (!ConvertHelper.GetString(Console.ReadLine()).ToLower().Contains("stop"))
            {
                Thread.Sleep(500);
            }
            Console.WriteLine("正在关闭线程");

            //关闭监听
            Com.TryScope(() => { sockServer.Stop(); }, (e) => { Console.WriteLine(e.Message); });
            //关闭接收连接的线程
            Com.TryScope(() =>
            {
                ThreadWrapper.CloseAndRemove(ref mainList);
            }, (e) => { Console.WriteLine(e.Message); });
            //关闭所有客户端处理线程
            Com.TryScope(() =>
            {
                ThreadWrapper.CloseAndRemove(ref clientList);
            }, (e) => { Console.WriteLine(e.Message); });

            Console.WriteLine("安全关闭线程");
        }
    }

    class ProxyHandler
    {
        private readonly Socket client;
        public ProxyHandler(Socket sockClient)
        {
            client = sockClient;
        }

        public void Run()
        {
            string strFromClient = "";
            try
            {
                var readBuf = new Byte[1024];
                int bytes = ReadMessage(readBuf, ref strFromClient);
                if (bytes <= 0)
                {
                    Console.WriteLine("请求为空");
                    return;
                }
                log4net.LogManager.GetLogger("转发获取的数据").Error(strFromClient);
                int index1 = strFromClient.IndexOf(' ');
                int index2 = strFromClient.IndexOf(' ', index1 + 1);
                string strClientConnection = strFromClient.Substring(index1 + 1, index2 - index1);

                if ((index1 < 0) || (index2 < 0))
                {
                    Console.WriteLine("找不到URL");
                    return;
                }
                Console.WriteLine("Connecting to Site " + strClientConnection);
                Console.WriteLine("Connection from " + client.RemoteEndPoint);

                var req = (WebRequest)WebRequest.Create(strClientConnection);
                var response = req.GetResponse();
                var ResponseStream = response.GetResponseStream();
                var len = 0;
                var bs = new byte[32];

                while (true)
                {
                    len = ResponseStream.Read(bs, 0, 32);
                    if (len <= 0) { break; }
                    client.Send(bs, len, 0);
                }
                Console.WriteLine("==============================转发成功==============================");
            }
            catch (Exception e)
            {
                SendErrorPage(404, "File Not Found", e.Message);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (client != null && client.Connected)
                {
                    client.Close();
                }
            }
        }

        private void SendErrorPage(int status, string strReason, string strText)
        {
            try
            {
                SendMessage("HTTP/1.0" + " " +
                            status + " " + strReason + "\r\n");
                SendMessage("Content-Type: text/plain" + "\r\n");
                SendMessage("Proxy-Connection: close" + "\r\n");
                SendMessage("\r\n");
                SendMessage(status + " " + strReason);
                SendMessage(strText);
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
        }

        private void SendMessage(string strMessage)
        {
            var buffer = new Byte[strMessage.Length + 1];
            int len = Encoding.ASCII.GetBytes(strMessage.ToCharArray(),
                                      0, strMessage.Length, buffer, 0);
            client.Send(buffer, len, 0);
        }

        private int ReadMessage(byte[] buf, ref string strMessage)
        {
            int iBytes = client.Receive(buf, 1024, 0);
            strMessage = Encoding.ASCII.GetString(buf);
            return (iBytes);
        }
    }
}