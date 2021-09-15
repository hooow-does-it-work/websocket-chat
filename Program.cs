using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IocpSharp.Http;

namespace WebSocketChat
{
    class Program
    {
        static void Main(string[] args)
        {
            StartWebSocketServer(4189);

            Console.ReadLine();
        }

        private static void StartWebSocketServer(int port)
        {
            HttpServerBase server = new Chat.Server();
            try
            {
                server.Start("0.0.0.0", port);
                Console.WriteLine("WebSocket服务器启动成功，监听地址：" + server.LocalEndPoint.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
