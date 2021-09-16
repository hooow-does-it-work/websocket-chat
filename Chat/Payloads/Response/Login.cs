using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketChat.Chat.Payloads.Response
{
    /// <summary>
    /// 用户登录的响应消息
    /// </summary>
    [ResponseAction("login")]
    class Login : IPayload
    {
        public int connectionId { get; set; }
    }
}
