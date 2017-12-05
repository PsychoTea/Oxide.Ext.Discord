namespace Oxide.Ext.Discord.DiscordObjects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Oxide.Ext.Discord.REST;

    public class User
    {
        public string id { get; set; }

        public string username { get; set; }

        public string discriminator { get; set; }

        public string avatar { get; set; }

        public bool? bot { get; set; }

        public bool? mfa_enabled { get; set; }

        public bool? verified { get; set; }

        public string email { get; set; }

        public static void GetCurrentUser(DiscordClient client, Action<User> callback = null)
        {
            client.REST.DoRequest($"/users/@me", RequestMethod.GET, null, callback);
        }

        public static void GetUser(DiscordClient client, string userID, Action<User> callback = null)
        {
            client.REST.DoRequest($"/users/{userID}", RequestMethod.GET, null, callback);
        }

        public void ModifyCurrentUser(DiscordClient client, Action<User> callback = null) => ModifyCurrentUser(client, this.username, this.avatar, callback);

        public void ModifyCurrentUser(DiscordClient client, string username = "", string avatarData = "", Action<User> callback = null)
        {
            var jsonObj = new Dictionary<string, string>()
            {
                { "username", username },
                { "avatar", avatarData }
            };

            client.REST.DoRequest($"/users/@me", RequestMethod.PATCH, jsonObj, callback);
        }

        public void GetCurrentUserGuilds(DiscordClient client, Action<List<Guild>> callback = null)
        {
            client.REST.DoRequest($"/users/@me/guilds", RequestMethod.GET, null, callback);
        }

        public void LeaveGuild(DiscordClient client, Guild guild, Action callback = null) => LeaveGuild(client, guild.id, callback);

        public void LeaveGuild(DiscordClient client, string guildID, Action callback = null)
        {
            client.REST.DoRequest($"/users/@me/guilds/{guildID}", RequestMethod.DELETE, null, callback);
        }

        public void GetUserDMs(DiscordClient client, Action<List<Channel>> callback = null)
        {
            client.REST.DoRequest($"/users/@me/channels", RequestMethod.GET, null, callback);
        }

        public void CreateGroupDM(DiscordClient client, string[] accessTokens, List<Nick> nicks, Action<Channel> callback = null)
        {
            var nickDict = nicks.ToDictionary(k => k.id, v => v.nick);

            var jsonObj = new Dictionary<string, object>()
            {
                { "access_tokens", accessTokens },
                { "nicks", nicks }
            };

            client.REST.DoRequest($"/users/@me/channels", RequestMethod.POST, jsonObj, callback);
        }

        public void GetUserConnections(DiscordClient client, Action<List<Connection>> callback = null)
        {
            client.REST.DoRequest($"/users/@me/connections", RequestMethod.GET, null, callback);
        }

        public void CreateDM(DiscordClient client, Action<Channel> callback = null)
        {
            var jsonObj = new Dictionary<string, string>()
            {
                { "recipient_id", this.id }
            };

            client.REST.DoRequest("/users/@me/channels", RequestMethod.POST, jsonObj, callback);
        }

        public void GroupDMAddRecipient(DiscordClient client, Channel channel, string accessToken, Action callback = null) => GroupDMAddRecipient(client, channel.id, accessToken, this.username, callback);

        public void GroupDMAddRecipient(DiscordClient client, string channelID, string accessToken, string nick, Action callback = null)
        {
            var jsonObj = new Dictionary<string, string>()
            {
                { "access_token", accessToken },
                { "nick", nick }
            };

            client.REST.DoRequest($"/channels/{channelID}/recipients/{id}", RequestMethod.PUT, jsonObj, callback);
        }

        public void GroupDMRemoveRecipient(DiscordClient client, Channel channel) => GroupDMRemoveRecipient(client, channel.id);

        public void GroupDMRemoveRecipient(DiscordClient client, string channelID, Action callback = null)
        {
            client.REST.DoRequest($"/channels/{channelID}/recipients/{id}", RequestMethod.DELETE, null, callback);
        }

        public override bool Equals(object obj)
        {
            if (this == null || obj == null)
            {
                return false;
            }

            if (obj is User user)
            {
                return user.id == this.id;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode() => base.GetHashCode();

        public static bool operator ==(User user1, User user2) => user1.Equals(user2);

        public static bool operator !=(User user1, User user2) => !user1.Equals(user2);
    }
}
