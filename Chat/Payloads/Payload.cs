using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Runtime.InteropServices;
using System.Reflection;

namespace WebSocketChat.Chat.Payloads
{

    /// <summary>
    /// 这里顺便普及下反射相关的知识
    /// </summary>
    class Payload
    {
        private static Dictionary<string, Type> _requestActions = new Dictionary<string, Type>();
        private static Dictionary<Type, string> _responseActions = new Dictionary<Type, string>();

        /// <summary>
        /// 将所有继承接口IPayload并带有RequestActionAttribute或ResponseActionAttribute属性的类找出来
        /// </summary>
        static Payload()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                if (!typeof(IPayload).IsAssignableFrom(type)) continue;

                RequestActionAttribute attrRequest = type.GetCustomAttribute<RequestActionAttribute>();
                if (attrRequest != null && !string.IsNullOrEmpty(attrRequest.Action))
                {

                    _requestActions[attrRequest.Action] = type;
                }

                ResponseActionAttribute attrResponse = type.GetCustomAttribute<ResponseActionAttribute>();
                if (attrResponse != null && !string.IsNullOrEmpty(attrResponse.Action))
                {
                    _responseActions[type] = attrResponse.Action;
                }
            }

        }

        /// <summary>
        /// 把客户端JSON消息解析成具体的消息类
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public static IPayload Parse(string payload)
        {
            try
            {
                var request = Json.Parse(payload);
                if (request is not Dictionary<string, object> payloadInfo
                    || !payloadInfo.TryGetValue("action", out object _action)
                    || !payloadInfo.TryGetValue("payload", out object _payload)
                    || _action is not string action) return null;

                if (!_requestActions.TryGetValue(action, out Type type)) return null;

                return Json.ConvertToType(_payload, type) as IPayload;

            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 把消息类序列化成JSON字符串
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public static string Stringify(IPayload payload)
        {
            if (!_responseActions.TryGetValue(payload.GetType(), out string action))
            {
                return null;
            }
            return Json.Stringify(new { action, payload });

        }
    }
}
