using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketChat.Chat.Payloads.Request
{
    /// <summary>
    /// 用户发送聊天消息
    /// </summary>
    [RequestAction("post")]
    class Post : IPayload
    {
        public string message { get; set; }
    }
}
