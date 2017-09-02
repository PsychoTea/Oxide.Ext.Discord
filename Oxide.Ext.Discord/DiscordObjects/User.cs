using System;
using System.Collections.Generic;
using Oxide.Ext.Discord.WebSockets;

namespace Oxide.Ext.Discord.DiscordObjects
{
    public class User
    {
        public string username { get; set; }
        public string id { get; set; }
        public string discriminator { get; set; }
        public string avatar { get; set; }
        public bool? bot { get; set; }

        public void GetCurrentUser(DiscordClient client, Action<User> callback = null)
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

        public void CreateGroupDM(DiscordClient client, string[] accessTokens, Dictionary<string, string> nicks, Action<Channel> callback = null)
        {
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

        public void GroupDMAddRecipient(DiscordClient client, string channelID, string accessToken, string nick)
        {
            var jsonObj = new Dictionary<string, string>()
            {
                { "access_token", accessToken },
                { "nick", nick }
            };
            client.REST.DoRequest($"/channels/{channelID}/recipients/{id}", "PUT", jsonObj);
        }

        public void GroupDMRemoveRecipient(DiscordClient client, string channelID)
        {
            client.REST.DoRequest($"/channels/{channelID}/recipients/{id}", "DELETE");
        }
    }
}
