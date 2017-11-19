namespace Oxide.Ext.Discord.DiscordObjects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Oxide.Ext.Discord.RESTObjects;
    using Oxide.Ext.Discord.WebSockets;

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
            client.REST.DoRequest<User>($"/users/@me", "GET", null, (returnValue) =>
            {
                callback?.Invoke(returnValue as User);
            });
        }

        public static void GetUser(DiscordClient client, string userID, Action<User> callback = null)
        {
            client.REST.DoRequest<User>($"/users/{userID}", "GET", null, (returnValue) =>
            {
                callback?.Invoke(returnValue as User);
            });
        }

        public void ModifyCurrentUser(DiscordClient client, Action<User> callback = null) => ModifyCurrentUser(client, this.username, this.avatar, callback);

        public void ModifyCurrentUser(DiscordClient client, string username = "", string avatarData = "", Action<User> callback = null)
        {
            var jsonObj = new Dictionary<string, string>()
            {
                { "username", username },
                { "avatar", avatarData }
            };

            client.REST.DoRequest<User>($"/users/@me", "PATCH", jsonObj, (returnValue) =>
            {
                callback?.Invoke(returnValue as User);
            });
        }

        public void GetCurrentUserGuilds(DiscordClient client, Action<List<Guild>> callback = null)
        {
            client.REST.DoRequest<List<Guild>>($"/users/@me/guilds", "GET", null, (returnValue) =>
            {
                callback?.Invoke(returnValue as List<Guild>);
            });
        }

        public void LeaveGuild(DiscordClient client, Guild guild) => LeaveGuild(client, guild.id);

        public void LeaveGuild(DiscordClient client, string guildID)
        {
            client.REST.DoRequest($"/users/@me/guilds/{guildID}", "DELETE");
        }

        public void GetUserDMs(DiscordClient client, Action<List<Channel>> callback = null)
        {
            client.REST.DoRequest<List<Channel>>($"/users/@me/channels", "GET", null, (returnValue) =>
            {
                callback?.Invoke(returnValue as List<Channel>);
            });
        }

        public void CreateGroupDM(DiscordClient client, string[] accessTokens, List<Nick> nicks, Action<Channel> callback = null)
        {
            var nickDict = nicks.Select(x => new KeyValuePair<string, string>(x.id, x.nick)).ToDictionary(x => x.Key, x => x.Value);

            var jsonObj = new Dictionary<string, object>()
            {
                { "access_tokens", accessTokens },
                { "nicks", nicks }
            };
            client.REST.DoRequest<Channel>($"/users/@me/channels", "POST", jsonObj, (returnValue) =>
            {
                callback?.Invoke(returnValue as Channel);
            });
        }

        public void GetUserConnections(DiscordClient client, Action<List<Connection>> callback = null)
        {
            client.REST.DoRequest<List<Connection>>($"/users/@me/connections", "GET", null, (returnValue) =>
            {
                callback?.Invoke(returnValue as List<Connection>);
            });
        }

        public void CreateDM(DiscordClient client, Action<Channel> callback = null)
        {
            var jsonObj = new Dictionary<string, string>()
            {
                { "recipient_id", this.id }
            };

            client.REST.DoRequest<Channel>("/users/@me/channels", "POST", jsonObj, (returnValue) => 
            {
                callback?.Invoke(returnValue as Channel);
            });
        }
        
        public void GroupDMAddRecipient(DiscordClient client, Channel channel, string accessToken, Action callback = null) => GroupDMAddRecipient(client, channel.id, accessToken, this.username, callback);

        public void GroupDMAddRecipient(DiscordClient client, string channelID, string accessToken, string nick, Action callback = null)
        {
            var jsonObj = new Dictionary<string, string>()
            {
                { "access_token", accessToken },
                { "nick", nick }
            };

            client.REST.DoRequest($"/channels/{channelID}/recipients/{id}", "PUT", jsonObj, () =>
            {
                callback?.Invoke();
            });
        }

        public void GroupDMRemoveRecipient(DiscordClient client, Channel channel) => GroupDMRemoveRecipient(client, channel.id);

        public void GroupDMRemoveRecipient(DiscordClient client, string channelID)
        {
            client.REST.DoRequest($"/channels/{channelID}/recipients/{id}", "DELETE");
        }
    }
}
