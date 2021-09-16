using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketChat.Chat.Payloads
{
    /// <summary>
    /// 客户端请求消息属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    class RequestActionAttribute : Attribute
    {
        private string _action = null;
        public string Action => _action;
        public RequestActionAttribute(string action)
        {
            _action = action;
        }
    }
}
