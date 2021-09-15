using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketChat.Chat.Payloads.Response
{
    [ResponseAction("login")]
    class Login : IPayload
    {
        public int connectionId { get; set; }
    }
}
