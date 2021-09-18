using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using IocpSharp.Http.Responsers;
using IocpSharp.Http.Streams;
using System.IO.Compression;
using IocpSharp.Server;
using IocpSharp.Http.Utils;
using IocpSharp.WebSocket;
using IocpSharp.WebSocket.Frames;
using System.Net;
using IocpSharp.Http;

namespace WebSocketChat.Chat
{
    public class Server : HttpServerBase
    {
        public Server() : base()
        {
            //设置根目录
            WebRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "web"));
        }
        protected override Messager GetMessager(HttpRequest request, Stream stream, EndPoint localEndPoint, EndPoint remoteEndPoint)
        {
            return new Connection(stream, localEndPoint, remoteEndPoint);
        }
    }
}
