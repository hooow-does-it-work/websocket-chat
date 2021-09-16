using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketChat.Chat.Payloads.Request
{
    /// <summary>
    /// 用户登录消息
    /// </summary>
    [RequestAction("login")]
    class Login : IPayload
    {
        public string name { get; set; }
    }
}
