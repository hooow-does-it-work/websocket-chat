using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketChat.Chat.Payloads.Request
{
    /// <summary>
    /// 用户主动退出
    /// </summary>
    [RequestAction("quit")]
    class Quit : IPayload
    {
    }
}
