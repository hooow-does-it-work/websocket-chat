using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketChat.Chat.Payloads.Response
{
    /// <summary>
    /// 用户退出聊天室的响应消息
    /// </summary>
    [ResponseAction("exit")]
    class Exit : IPayload
    {
        public int connectionId { get; set; }
        public string name { get; set; }
    }
}
