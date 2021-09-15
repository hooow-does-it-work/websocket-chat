﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketChat.Chat.Payloads.Response
{
    [ResponseAction("enter")]
    class Enter : IPayload
    {
        public int connectionId { get; set; }
        public string name { get; set; }
    }
}