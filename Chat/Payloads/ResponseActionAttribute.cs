using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketChat.Chat.Payloads
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    class ResponseActionAttribute : Attribute
    {
        private string _action = null;
        public string Action => _action;
        public ResponseActionAttribute(string action)
        {
            _action = action;
        }
    }
}
