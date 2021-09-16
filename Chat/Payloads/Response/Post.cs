using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketChat.Chat.Payloads.Response
{
    /// <summary>
    /// 广播用户发送内容的响应消息
    /// </summary>
    [ResponseAction("post")]
    class Post : IPayload
    {
        public int connectionId { get; set; }
        public string message { get; set; }
        public string name { get; set; }
    }
}
