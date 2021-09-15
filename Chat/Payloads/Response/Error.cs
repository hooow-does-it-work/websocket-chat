using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketChat.Chat.Payloads.Response
{
    [ResponseAction("error")]
    class Error : IPayload
    {
        public string message { get; set; }
    }
}
