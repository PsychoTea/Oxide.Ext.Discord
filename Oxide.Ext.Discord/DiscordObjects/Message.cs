namespace Oxide.Ext.Discord.DiscordObjects
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Oxide.Ext.Discord.Helpers;
    using Oxide.Ext.Discord.REST;
    public enum MessageType
    {
        DEFAULT = 0,
        RECIPIENT_ADD = 1,
        RECIPIENT_REMOVE = 2,
        CALL = 3,
        CHANNEL_NAME_CHANGE = 4,
        CHANNEL_ICON_CHANGE = 5,
        CHANNEL_PINNED_MESSAGE = 6,
        GUILD_MEMBER_JOIN = 7
    }
    public class Message
    {
        public string id { get; set; }

        public string channel_id { get; set; }

        public User author { get; set; }

        public string content { get; set; }

        public string timestamp { get; set; }

        public string edited_timestamp { get; set; }

        public bool tts { get; set; }

        public bool mention_everyone { get; set; }

        public List<User> mentions { get; set; }

        public List<string> mention_roles { get; set; }

        public List<Attachment> attachments { get; set; }

        public Embed embed { get; set; }

        public List<Embed> embeds { get; set; }

        public List<Reaction> reactions { get; set; }

        public string nonce { get; set; }

        public bool pinned { get; set; }

        public string webhook_id { get; set; }

        public MessageType type { get; set; }

        public void Reply(DiscordClient client, Message message, bool ping = true, Action<Message> callback = null)
        {
            if (ping && !string.IsNullOrEmpty(message.content) && !message.content.Contains($"<@{author.id}>"))
            {
                message.content = $"<@{author.id}> {message.content}";
            }

            client.REST.DoRequest($"/channels/{channel_id}/messages", RequestMethod.POST, message, callback);
        }

        public void Reply(DiscordClient client, string message, bool ping = true, Action<Message> callback = null)
        {
            Message newMessage = new Message()
            {
                content = ping ? $"<@{author.id}> {message}" : message
            };

            Reply(client, newMessage, ping, callback);
        }

        public void Reply(DiscordClient client, Embed embed, bool ping = true, Action<Message> callback = null)
        {
            Message newMessage = new Message()
            {
                content = ping ? $"<@{author.id}>" : null,
                embed = embed
            };

            Reply(client, newMessage, ping, callback);
        }

        public void CreateReaction(DiscordClient client, string emoji, Action callback = null)
        {
            client.REST.DoRequest($"/channels/{channel_id}/messages/{id}/reactions/{emoji}/@me", RequestMethod.PUT, null, callback);
        }

        public void DeleteOwnReaction(DiscordClient client, string emoji, Action callback = null)
        {
            client.REST.DoRequest($"/channels/{channel_id}/messages/{id}/reactions/{emoji}/@me", RequestMethod.DELETE, null, callback);
        }

        public void DeleteUserReaction(DiscordClient client, string emoji, User user, Action callback = null) => DeleteUserReaction(client, emoji, user.id, callback);

        public void DeleteUserReaction(DiscordClient client, string emoji, string userID, Action callback = null)
        {
            client.REST.DoRequest($"/channels/{channel_id}/messages/{id}/reactions/{emoji}/{userID}", RequestMethod.DELETE, null, callback);
        }

        public void GetReactions(DiscordClient client, string emoji, Action<List<User>> callback = null)
        {
            byte[] encodedEmoji = Encoding.UTF8.GetBytes(emoji);
            string hexString = HttpUtility.UrlEncode(encodedEmoji);

            client.REST.DoRequest($"/channels/{channel_id}/messages/{id}/reactions/{hexString}", RequestMethod.GET, null, callback);
        }

        public void DeleteAllReactions(DiscordClient client, Action callback = null)
        {
            client.REST.DoRequest($"/channels/{channel_id}/messages/{id}/reactions", RequestMethod.DELETE, null, callback);
        }

        public void EditMessage(DiscordClient client, Action<Message> callback = null)
        {
            client.REST.DoRequest<Message>($"/channels/{channel_id}/messages/{id}", RequestMethod.PATCH, this, callback);
        }

        public void DeleteMessage(DiscordClient client, Action<Message> callback = null)
        {
            client.REST.DoRequest<Message>($"/channels/{channel_id}/messages/{id}", RequestMethod.DELETE, null, callback);
        }

        public void AddPinnedChannelMessage(DiscordClient client, Action callback = null)
        {
            client.REST.DoRequest($"/channels/{channel_id}/pins/{id}", RequestMethod.PUT, null, callback);
        }

        public void DeletePinnedChannelMessage(DiscordClient client, Action callback = null)
        {
            client.REST.DoRequest($"/channels/{channel_id}/pins/{id}", RequestMethod.DELETE, null, callback);
        }
    }
}
