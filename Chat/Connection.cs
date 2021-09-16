using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using WebSocketChat.Chat.Payloads;
using WebSocketChat.Chat.Payloads.Request;
using IocpSharp.WebSocket;

namespace WebSocketChat.Chat
{
    /// <summary>
    /// 新连接
    /// </summary>
    class Connection : Messager
    {
        private ConnectionGroup _group = null;
        private int _connectionId = 0;
        private IPEndPoint _remoteEndPoint = null;
        private string _name = null;

        /// <summary>
        /// 登录用户名
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// 连接ID
        /// </summary>
        public int Id => _connectionId;

        /// <summary>
        /// 连接所在组
        /// </summary>
        public ConnectionGroup Group
        {
            get => _group;
            set
            {
                if (_group != null) _group.Exit(this);
                _group = value;
                _group.Enter(this);
            }
        }

        /// <summary>
        /// 实例化连接
        /// </summary>
        /// <param name="stream"></param>
        public Connection(Stream stream) : base(stream)
        {
            if (stream is IocpSharp.ISocketBasedStream socketBasedStream)
            {
                _remoteEndPoint = socketBasedStream.BaseSocket.RemoteEndPoint as IPEndPoint;
            }
            _connectionId = ConnectionGroup.GetNewConnectionId();
        }

        /// <summary>
        /// 用户连接
        /// </summary>
        protected override void OnConnected()
        {
            Console.WriteLine($"用户连接：{_remoteEndPoint}, 连接ID：{_connectionId}");
        }

        /// <summary>
        /// 用户断开连接
        /// </summary>
        protected override void OnDisconnected()
        {
            if (_group == null) return;
            _group.Exit(this);
        }

        /// <summary>
        /// 收到文本消息
        /// </summary>
        /// <param name="payload"></param>
        protected override void OnText(string payload)
        {
            IPayload payload_ = Payload.Parse(payload);
            if (payload_ == null) return;

            //登录
            if (payload_ is Login login)
            {
                _name = login.name;

                _group = ConnectionGroup.Default;
                //先回复自己
                Send(new Payloads.Response.Login() { 
                    connectionId = _connectionId 
                });

                //然后广播给分组内的所有人
                _group.Enter(this);
                return;
            }

            //发布
            if (payload_ is Post post)
            {
                //广播给分组内的所有人
                _group.Broadcast(new Payloads.Response.Post()
                {
                    connectionId = _connectionId,
                    message = post.message,
                    name = _name
                });
                return;
            }

            //主动退出
            if (payload_ is Quit)
            {
                Close();
                return;
            }
        }


        private ConcurrentQueue<IPayload> _queue = new ConcurrentQueue<IPayload>();
        private int _sending = 0;

        /// <summary>
        /// 实现一个新的发送方法，使用线程安全队列处理消息的发送
        /// </summary>
        /// <param name="payload"></param>
        public void Send(IPayload payload)
        {
            _queue.Enqueue(payload);
            if (Interlocked.CompareExchange(ref _sending, 1, 0) == 1)
            {
                return;
            }

            while (_queue.TryDequeue(out IPayload next))
            {
                try
                {
                    Send(Payload.Stringify(next));
                }
                catch
                {
                    break;
                }
            }
            Interlocked.CompareExchange(ref _sending, 0, 1);
        }
    }
}
