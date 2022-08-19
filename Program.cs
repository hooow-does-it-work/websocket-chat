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

#if !SVR
            StartWebSocketServer(4189);

            Console.ReadLine();
#else
            Jazor.HostingService.Startup((worker, stopToken, agvs) => {
                var server = StartWebSocketServer(4189);
                worker.RegisterStopHandler(stopToken, (state) =>
                {
                    server.Stop();

                }, null);
            });
#endif
        }

        private static HttpServerBase StartWebSocketServer(int port)
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
            return server;
        }
    }
}
