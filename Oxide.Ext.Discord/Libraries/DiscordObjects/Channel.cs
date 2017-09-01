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

        public static void GetChannel(string channelID, Action<Channel> callback = null)
        {
            var channel = RESTHandler.DoRequest<Channel>($"/channels/{channelID}", "GET");
            callback?.Invoke(channel);
        }

        public void ModifyChannel(Channel newChannel, Action<Channel> callback = null)
        {
            var channel = RESTHandler.DoRequest<Channel>($"/channels/{id}", "PATCH");
            callback?.Invoke(channel);
        }

        public void DeleteChannel(Action<Channel> callback = null)
        {
            var channel = RESTHandler.DoRequest<Channel>($"/channels/{id}", "DELETE");
            callback?.Invoke(channel);
        }

        public void GetChannelMessages(Action<List<Message>> callback = null)
        {
            var messages = RESTHandler.DoRequest<List<Message>>($"/channels/{id}/messages", "GET");
            callback?.Invoke(messages);
        }

        public void GetChannelMessage(string messageID, Action<Message> callback = null)
        {
            var message = RESTHandler.DoRequest<Message>($"/channels/{id}/messages/{messageID}", "GET");
            callback?.Invoke(message);
        }

        public void CreateMessage(Message message, Action<Message> callback = null)
        {
            var resMessage = RESTHandler.DoRequest<Message>($"/channels/{id}/messages", "POST", message);
            callback?.Invoke(resMessage);
        }

        public void CreateReaction(string messageID, string emoji)
        {
            RESTHandler.DoRequest($"/channels/{id}/messages/{messageID}/reactions/{emoji}/@me", "PUT");
        }

        public void DeleteOwnReaction(string messageID, string emoji)
        {
            RESTHandler.DoRequest($"/channels/{id}/messages/{messageID}/reactions/{emoji}/@me", "DELETE");
        }

        public void DeleteOwnReaction(string messageID, string emoji, string userID)
        {
            RESTHandler.DoRequest($"/channels/{id}/messages/{messageID}/reactions/{emoji}/{userID}", "DELETE");
        }

        public void GetReactions(string messageID, string emoji, Action<List<User>> callback = null)
        {
            var users = RESTHandler.DoRequest<List<User>>($"/channels/{id}/messages/{messageID}/reactions/{emoji}", "GET");
            callback?.Invoke(users);
        }

        public void DeleteAllReactions(string messageID)
        {
            RESTHandler.DoRequest($"/channels/{id}/messages/{messageID}/reactions", "DELETE");
        }

        public void EditMessage(Message message, Action<Message> callback = null)
        {
            var newMessage = RESTHandler.DoRequest<Message>($"/channels/{id}/messages/{message.id}", "PATCH", message);
            callback?.Invoke(newMessage);
        }

        public void DeleteMessage(Message message, Action<Message> callback = null)
        {
            RESTHandler.DoRequest($"/channels/{id}/messages/{message.id}", "DELETE");
        }

        public void BulkDeleteMessages(List<string> messageIds)
        {
            RESTHandler.DoRequest($"/channels/{id}/messages/bulk-delete", "POST", messageIds);
        }
    }
}