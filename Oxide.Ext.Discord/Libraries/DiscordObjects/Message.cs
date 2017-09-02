using Oxide.Core;
using Oxide.Ext.Discord.Libraries.WebSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oxide.Ext.Discord.Libraries.DiscordObjects
{
    public class Message
    {
        public string webhook_id { get; set; }
        public int type { get; set; }
        public bool tts { get; set; }
        public string timestamp { get; set; }
        public bool pinned { get; set; }
        public object nonce { get; set; }
        public List<User> mentions { get; set; }
        public List<string> mention_roles { get; set; }
        public bool mention_everyone { get; set; }
        public string id { get; set; }
        public List<Embed> embeds { get; set; }
        public Embed embed { get; set; }
        public object edited_timestamp { get; set; }
        public string content { get; set; }
        public string channel_id { get; set; }
        public Author author { get; set; }
        public List<object> attachments { get; set; }

        public void CreateReaction(DiscordClient client, string emoji)
        {
            client.REST.DoRequest($"/channels/{channel_id}/messages/{id}/reactions/{emoji}/@me", "PUT");
        }
        public void DeleteOwnReaction(DiscordClient client, string emoji)
        {
            client.REST.DoRequest($"/channels/{channel_id}/messages/{id}/reactions/{emoji}/@me", "DELETE");
        }
        public void DeleteUsersReaction(DiscordClient client, string emoji, string userID)
        {
            client.REST.DoRequest($"/channels/{channel_id}/messages/{id}/reactions/{emoji}/{userID}", "DELETE");
        }
        public void GetReactions(DiscordClient client, string emoji, Action<List<User>> callback = null)
        {
            var users = client.REST.DoRequest<List<User>>($"/channels/{channel_id}/messages/{id}/reactions/{emoji}", "GET");
            callback?.Invoke(users);
        }
        public void DeleteAllReactions(DiscordClient client)
        {
            client.REST.DoRequest($"/channels/{channel_id}/messages/{id}/reactions", "DELETE");
        }
        public void Edit(DiscordClient client, Message message, Action<Message> callback = null)
        {
            var newMessage = client.REST.DoRequest<Message>($"/channels/{channel_id}/messages/{id}", "PATCH", message);
            callback?.Invoke(newMessage);
        }
        public void Delete(DiscordClient client)
        {
            client.REST.DoRequest($"/channels/{channel_id}/messages/{id}", "DELETE");
        }
        public void AddPinnedChannelMessage(DiscordClient client)
        {
            client.REST.DoRequest($"/channels/{channel_id}/pins/{id}", "PUT");
        }

        public void DeletePinnedChannelMessage(DiscordClient client)
        {
            client.REST.DoRequest($"/channels/{channel_id}/pins/{id}", "DELETE");
        }
    }
}
