using Newtonsoft.Json;
using Oxide.Ext.Discord.Libraries.WebSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oxide.Ext.Discord.Libraries.DiscordObjects
{
    public class Channel
    {
        class DiscordPayload
        {
            [JsonProperty("content")]
            public string MessageText { get; set; }
        }
        public string id { get; set; }
        public string type { get; set; }
        public int? position { get; set; }
        public List<object> permission_overwrites { get; set; }
        public string name { get; set; }
        public string topic { get; set; }
        public int? bitrate { get; set; }
        public int? user_limit { get; set; }
        public List<User> recipients { get; set; }
        public string icon { get; set; }
        public static void GetChannel(string channelID, Action<Channel> Channel)
        {
            RESTHandler.DoGet($"/channels/{channelID}", typeof(Channel), (obj) =>
            {
                Channel?.Invoke(obj as Channel);
            });
        }
        public void DeleteChannel()
        {
            RESTHandler.DoDelete($"/channels/{id}", typeof(object));
        }
        public void SendMessage(string text, Action<Message> message, bool tts = false)
        {
            string payloadJson = JsonConvert.SerializeObject(new DiscordPayload()
            {
                MessageText = text
            });
            RESTHandler.DoPost($"/channels/{id}/messages", payloadJson, (m) => {;
                Message msg = m as Message;
                message?.Invoke(msg);
            });
        }
        public void GetMessages(Action<List<Message>> Message)
        {
            RESTHandler.DoGet($"/channels/{id}/messages", typeof(List<Message>), (msg) =>
            {
                List<Message> Messages = msg as List<Message>;
                Message?.Invoke(Messages);
            });
        }
    }
}
