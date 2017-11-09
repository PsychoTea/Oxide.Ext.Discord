using System;
using System.Collections.Generic;
using Oxide.Ext.Discord.WebSockets;

namespace Oxide.Ext.Discord.DiscordObjects
{
    public class Channel
    {
        public string id { get; set; }
        public string type { get; set; }
        public int? position { get; set; }
        public List<Overwrite> permission_overwrites { get; set; }
        public string name { get; set; }
        public string topic { get; set; }
        public int? bitrate { get; set; }
        public int? user_limit { get; set; }
        public List<User> recipients { get; set; }
        public string icon { get; set; }

        public static void GetChannel(DiscordClient client, string channelID, Action<Channel> callback = null)
        {
            client.REST.DoRequest<Channel>($"/channels/{channelID}", "GET", null, (returnValue) =>
            {
                callback?.Invoke(returnValue as Channel);
            });
        }
        
        public void ModifyChannel(DiscordClient client, Channel newChannel, Action<Channel> callback = null)
        {
            client.REST.DoRequest<Channel>($"/channels/{id}", "PATCH", newChannel, (returnValue) =>
            {
                callback?.Invoke(returnValue as Channel);
            });
        }

        public void DeleteChannel(DiscordClient client, Action<Channel> callback = null)
        {
            client.REST.DoRequest<Channel>($"/channels/{id}", "DELETE", null, (returnValue) =>
            {
                callback?.Invoke(returnValue as Channel);
            });
        }

        public void GetChannelMessages(DiscordClient client, Action<List<Message>> callback = null)
        {
            client.REST.DoRequest<List<Message>>($"/channels/{id}/messages", "GET", null, (returnValue) =>
            {
                callback?.Invoke(returnValue as List<Message>);
            });
        }

        public void GetChannelMessage(DiscordClient client, Message message, Action<Message> callback = null) => GetChannelMessage(client, message, callback);
        public void GetChannelMessage(DiscordClient client, string messageID, Action<Message> callback = null)
        {
            client.REST.DoRequest<Message>($"/channels/{id}/messages/{messageID}", "GET", null, (returnValue) =>
            {
                callback?.Invoke(returnValue as Message);
            });
        }

        public void CreateMessage(DiscordClient client, Message message, Action<Message> callback = null)
        {
            client.REST.DoRequest<Message>($"/channels/{id}/messages", "POST", message, (returnValue) =>
            {
                callback?.Invoke(returnValue as Message);
            });
        }

        public void CreateMessage(DiscordClient client, string message, Action<Message> callback = null)
        {
            Message cMessage = new Message();
            cMessage.content = message;
            client.REST.DoRequest<Message>($"/channels/{id}/messages", "POST", cMessage, (returnValue) =>
            {
                callback?.Invoke(returnValue as Message);
            });
        }

        public void CreateMessage(DiscordClient client, Embed embed, Action<Message> callback = null)
        {
            Message cMessage = new Message();
            cMessage.embed = embed;
            client.REST.DoRequest<Message>($"/channels/{id}/messages", "POST", cMessage, (returnValue) =>
            {
                callback?.Invoke(returnValue as Message);
            });
        }

        public void BulkDeleteMessages(DiscordClient client, string[] messageIds)
        {
            var jsonObj = new Dictionary<string, string[]>() { { "messages", messageIds } };
            client.REST.DoRequest($"/channels/{id}/messages/bulk-delete", "POST", jsonObj);
        }

        public void EditChannelPermissions(DiscordClient client, Overwrite overwrite, int? allow, int? deny, string type) => EditChannelPermissions(client, overwrite, allow, deny, type);
        public void EditChannelPermissions(DiscordClient client, string overwriteID, int? allow, int? deny, string type)
        {
            var jsonObj = new Dictionary<string, object>()
            {
                { "allow", allow },
                { "deny", deny },
                { "type", type }
            };
            client.REST.DoRequest($"/channels/{id}/permissions/{overwriteID}", "PUT", jsonObj);
        }

        public void GetChannelInvites(DiscordClient client, Action<List<Invite>> callback = null)
        {
            client.REST.DoRequest<List<Invite>>($"/channels/{id}/invites", "GET", null, (returnValue) =>
            {
                callback?.Invoke(returnValue as List<Invite>);
            });
        }

        public void CreateChannelInvite(DiscordClient client, Action<Invite> callback = null, int? max_age = 86400, int? max_uses = 0, bool temporary = false, bool unique = false)
        {
            var jsonObj = new Dictionary<string, object>()
            {
                { "max_age", max_age },
                { "max_uses", max_uses },
                { "temporary", temporary },
                { "unique", unique }
            };
            client.REST.DoRequest<Invite>($"/channels/{id}/invites", "POST", jsonObj, (returnValue) =>
            {
                callback?.Invoke(returnValue as Invite);
            });
        }

        public void DeleteChannelPermission(DiscordClient client, Overwrite overwrite) => DeleteChannelPermission(client, overwrite.id);
        public void DeleteChannelPermission(DiscordClient client, string overwriteID)
        {
            client.REST.DoRequest($"/channels/{id}/permissions/{overwriteID}", "DELETE");
        }

        public void TriggerTypingIndicator(DiscordClient client)
        {
            client.REST.DoRequest($"/channels/{id}/typing", "POST");
        }

        public void GetPinnedMessages(DiscordClient client, Action<List<Message>> callback = null)
        {
            client.REST.DoRequest<List<Message>>($"/channels/{id}/pins", "GET", null, (returnValue) =>
            {
                callback?.Invoke(returnValue as List<Message>);
            });
        }

        public void GroupDMAddRecipient(DiscordClient client, User user, string accessToken, Action callback = null) => GroupDMAddRecipient(client, user.id, accessToken, user.username, callback);
        public void GroupDMAddRecipient(DiscordClient client, string userID, string accessToken, string nick, Action callback = null)
        {
            var jsonObj = new Dictionary<string, string>()
            {
                { "access_token", accessToken },
                { "nick", nick }
            };
            client.REST.DoRequest($"/channels/{id}/recipients/{userID}", "PUT", jsonObj, () =>
            {
                callback?.Invoke();
            });
        }

        public void GroupDMRemoveRecipient(DiscordClient client, string userID)
        {
            client.REST.DoRequest($"/channels/{id}/recipients/{userID}", "DELETE");
        }
    }
}