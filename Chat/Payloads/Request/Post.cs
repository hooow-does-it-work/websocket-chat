using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketChat.Chat.Payloads.Request
{
    [RequestAction("post")]
    class Post : IPayload
    {
        public string message { get; set; }
    }
}
