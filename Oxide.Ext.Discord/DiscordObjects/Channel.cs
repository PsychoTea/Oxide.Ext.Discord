namespace Oxide.Ext.Discord.DiscordObjects
{
    using System;
    using System.Collections.Generic;
    using Oxide.Ext.Discord.REST;

    public class Channel
    {
        public string id { get; set; }

        public int? type { get; set; }

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
            client.REST.DoRequest($"/channels/{channelID}", RequestMethod.GET, null, callback);
        }
        
        public void ModifyChannel(DiscordClient client, Channel newChannel, Action<Channel> callback = null)
        {
            client.REST.DoRequest($"/channels/{id}", RequestMethod.PATCH, newChannel, callback);
        }

        public void DeleteChannel(DiscordClient client, Action<Channel> callback = null)
        {
            client.REST.DoRequest($"/channels/{id}", RequestMethod.DELETE, null, callback);
        }

        public void GetChannelMessages(DiscordClient client, Action<List<Message>> callback = null)
        {
            client.REST.DoRequest($"/channels/{id}/messages", RequestMethod.GET, null, callback);
        }

        public void GetChannelMessage(DiscordClient client, Message message, Action<Message> callback = null) => GetChannelMessage(client, message.id, callback);

        public void GetChannelMessage(DiscordClient client, string messageID, Action<Message> callback = null)
        {
            client.REST.DoRequest($"/channels/{id}/messages/{messageID}", RequestMethod.GET, null, callback);
        }

        public void CreateMessage(DiscordClient client, Message message, Action<Message> callback = null)
        {
            client.REST.DoRequest($"/channels/{id}/messages", RequestMethod.POST, message, callback);
        }

        public void CreateMessage(DiscordClient client, string message, Action<Message> callback = null)
        {
            Message createMessage = new Message()
            {
                content = message
            };

            client.REST.DoRequest($"/channels/{id}/messages", RequestMethod.POST, createMessage, callback);
        }

        public void CreateMessage(DiscordClient client, Embed embed, Action<Message> callback = null)
        {
            Message createMessage = new Message()
            {
                embed = embed
            };

            client.REST.DoRequest($"/channels/{id}/messages", RequestMethod.POST, createMessage, callback);
        }

        public void BulkDeleteMessages(DiscordClient client, string[] messageIds, Action callback = null)
        {
            var jsonObj = new Dictionary<string, string[]>()
            {
                { "messages", messageIds }
            };

            client.REST.DoRequest($"/channels/{id}/messages/bulk-delete", RequestMethod.POST, jsonObj, callback);
        }

        public void EditChannelPermissions(DiscordClient client, Overwrite overwrite, int? allow, int? deny, string type) => EditChannelPermissions(client, overwrite, allow, deny, type);

        public void EditChannelPermissions(DiscordClient client, string overwriteID, int? allow, int? deny, string type, Action callback = null)
        {
            var jsonObj = new Dictionary<string, object>()
            {
                { "allow", allow },
                { "deny", deny },
                { "type", type }
            };

            client.REST.DoRequest($"/channels/{id}/permissions/{overwriteID}", RequestMethod.PUT, jsonObj, callback);
        }

        public void GetChannelInvites(DiscordClient client, Action<List<Invite>> callback = null)
        {
            client.REST.DoRequest($"/channels/{id}/invites", RequestMethod.GET, null, callback);
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

            client.REST.DoRequest<Invite>($"/channels/{id}/invites", RequestMethod.POST, jsonObj, callback);
        }

        public void DeleteChannelPermission(DiscordClient client, Overwrite overwrite, Action callback) => DeleteChannelPermission(client, overwrite.id, callback);

        public void DeleteChannelPermission(DiscordClient client, string overwriteID, Action callback)
        {
            client.REST.DoRequest($"/channels/{id}/permissions/{overwriteID}", RequestMethod.DELETE, null, callback);
        }

        public void TriggerTypingIndicator(DiscordClient client, Action callback)
        {
            client.REST.DoRequest($"/channels/{id}/typing", RequestMethod.POST, null, callback);
        }

        public void GetPinnedMessages(DiscordClient client, Action<List<Message>> callback = null)
        {
            client.REST.DoRequest<List<Message>>($"/channels/{id}/pins", RequestMethod.GET, null, callback);
        }

        public void GroupDMAddRecipient(DiscordClient client, User user, string accessToken, Action callback = null) => GroupDMAddRecipient(client, user.id, accessToken, user.username, callback);

        public void GroupDMAddRecipient(DiscordClient client, string userID, string accessToken, string nick, Action callback = null)
        {
            var jsonObj = new Dictionary<string, string>()
            {
                { "access_token", accessToken },
                { "nick", nick }
            };

            client.REST.DoRequest($"/channels/{id}/recipients/{userID}", RequestMethod.PUT, jsonObj, callback);
        }

        public void GroupDMRemoveRecipient(DiscordClient client, string userID, Action callback)
        {
            client.REST.DoRequest($"/channels/{id}/recipients/{userID}", RequestMethod.DELETE, null, callback);
        }
    }
}