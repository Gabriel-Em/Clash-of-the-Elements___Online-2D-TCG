using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM___Server.Models
{
    public class Message
    {
        public string Type { get; set; }
        public JToken Value { get; set; }

        public static Message FromValue<T>(T value)
        {
            return new Message { Type = typeof(T).Name, Value = JToken.FromObject(value) };
        }

        public static string Serialize(Message message)
        {
            return JToken.FromObject(message).ToString();
        }

        public static Message Deserialize(string data)
        {
            return JToken.Parse(data).ToObject<Message>();
        }
    }
}
