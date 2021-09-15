using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace WebSocketChat.Chat
{
    /// <summary>
    /// 定义组，类似“房间”
    /// </summary>
    class ConnectionGroup
    {

        private static int _connectionId = 100000;
        private int _groupId = 0;

        private ConcurrentDictionary<int, Connection> _connections = new ConcurrentDictionary<int, Connection>();

        /// <summary>
        /// 获取应用内唯一的ID
        /// </summary>
        /// <returns></returns>
        public static int GetNewConnectionId()
        {
            Interlocked.CompareExchange(ref _connectionId, 100000, int.MaxValue);
            return Interlocked.Increment(ref _connectionId);
        }

        /// <summary>
        /// 默认组
        /// </summary>
        public static ConnectionGroup Default = new ConnectionGroup();

        /// <summary>
        /// 组ID
        /// </summary>
        public int Id => _groupId;

        public ConnectionGroup()
        {
            _groupId = GetNewConnectionId();
        }

        /// <summary>
        /// 新用户登录
        /// </summary>
        /// <param name="connection"></param>
        public void Enter(Connection connection)
        {
            _connections.TryAdd(connection.Id, connection);

            //广播给其他用户
            Broadcast(new Payloads.Response.Enter() { name = connection.Name, connectionId = connection.Id }, connection.Id);
        }

        /// <summary>
        /// 用户退出
        /// </summary>
        /// <param name="connection"></param>
        public void Exit(Connection connection)
        {
            _connections.TryRemove(connection.Id, out _);

            //广播给其他用户
            Broadcast(new Payloads.Response.Exit() { connectionId = connection.Id, name = connection.Name }, connection.Id);
        }

        /// <summary>
        /// 广播给所有用户
        /// </summary>
        /// <param name="payload"></param>
        public void Broadcast(Payloads.IPayload payload)
        {
            Broadcast(payload, 0);
        }

        /// <summary>
        /// 发送给特定用户
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="connectionId"></param>
        public void Emit(Payloads.IPayload payload, int connectionId)
        {
            if (!_connections.TryGetValue(connectionId, out Connection connection)) return;
            connection.Send(payload);
        }

        /// <summary>
        /// 广播给除特定id的其他用户
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="exceptId"></param>
        public void Broadcast(Payloads.IPayload payload, int exceptId)
        {
            ICollection<Connection> connections = _connections.Values;
            foreach (Connection connection in connections)
            {
                if (connection.Id == exceptId) continue;
                connection.Send(payload);
            }
        }
    }
}
