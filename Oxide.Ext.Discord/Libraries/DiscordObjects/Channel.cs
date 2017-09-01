using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Oxide.Ext.Discord.Libraries.WebSockets;

namespace Oxide.Ext.Discord.Libraries.DiscordObjects
{
    public class Channel
    {
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

        public static void GetChannel(DiscordClient client, string channelID, Action<Channel> callback = null)
        {
            var channel = client.REST.DoRequest<Channel>($"/channels/{channelID}", "GET");
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

        public void DeleteMessage(string messageID, Action<Message> callback = null)
        {
            RESTHandler.DoRequest($"/channels/{id}/messages/{messageID}", "DELETE");
        }

        public void BulkDeleteMessages(string[] messageIds)
        {
            var jsonObj = new Dictionary<string, string[]>() { { "messages", messageIds } };
            RESTHandler.DoRequest($"/channels/{id}/messages/bulk-delete", "POST", jsonObj);
        }
        
        public void EditChannelPermissions(string overwriteID, int allow, int deny, string type)
        {
            var jsonObj = new Dictionary<string, object>()
            {
                { "allow", allow },
                { "deny", deny },
                { "type", type }
            };
            RESTHandler.DoRequest($"/channels/{id}/permissions/{overwriteID}", "PUT", jsonObj);
        }

        public void GetChannelInvites(Action<List<Invite>> callback = null)
        {
            var invites = RESTHandler.DoRequest<List<Invite>>($"/channels/{id}/invites", "GET");
            callback?.Invoke(invites);
        }
        
        public void CreateChannelInvite(Action<Invite> callback = null, int max_age = 86400, int max_uses = 0, bool temporary = false, bool unique = false)
        {
            var jsonObj = new Dictionary<string, object>()
            {
                { "max_age", max_age },
                { "max_uses", max_uses },
                { "temporary", temporary },
                { "unique", unique }
            };
            var invite = RESTHandler.DoRequest<Invite>($"/channels/{id}/invites", "POST", jsonObj);
            callback?.Invoke(invite);
        }

        public void DeleteChannelPermission(string overwriteID)
        {
            RESTHandler.DoRequest($"/channels/{id}/permissions/{overwriteID}", "DELETE");
        }

        public void TriggerTypingIndicator()
        {
            RESTHandler.DoRequest($"/channels/{id}/typing", "POST");
        }

        public void GetPinnedMessages(Action<List<Message>> callback = null)
        {
            var messages = RESTHandler.DoRequest<List<Message>>($"/channels/{id}/pins", "GET");
            callback?.Invoke(messages);
        }

        public void AddPinnedChannelMessage(string messageID)
        {
            RESTHandler.DoRequest($"/channels/{id}/pins/{messageID}", "PUT");
        }

        public void DeletePinnedChannelMessage(string messageID)
        {
            RESTHandler.DoRequest($"/channels/{id}/pins/{messageID}", "DELETE");
        }

        public void GroupDMAddRecipient(string userID, string accessToken, string nick)
        {
            var jsonObj = new Dictionary<string, string>()
            {
                { "access_token", accessToken },
                { "nick", nick }
            };
            RESTHandler.DoRequest($"/channels/{id}/recipients/{userID}", "PUT", jsonObj);
        }

        public void GroupDMRemoveRecipient(string userID)
        {
            RESTHandler.DoRequest($"/channels/{id}/recipients/{userID}", "DELETE");
        }
    }
}