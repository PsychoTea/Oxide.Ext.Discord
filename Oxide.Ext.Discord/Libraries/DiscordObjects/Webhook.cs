using System;
using System.Collections.Generic;
using Oxide.Ext.Discord.Libraries.WebSockets;

namespace Oxide.Ext.Discord.Libraries.DiscordObjects
{
    public class Webhook
    {
        public string id;
        public string guild_id;
        public string channel_id;
        public User user;
        public string name;
        public string avatar;
        public string token;

        public static void CreateWebhook(DiscordClient client, string channelID, string name, string avatar, Action<Webhook> callback = null)
        {
            var jsonObj = new Dictionary<string, string>()
            {
                { "name", name },
                { "avatar", avatar }
            };
            client.REST.DoRequest<Webhook>($"/channels/{channelID}/webhooks", "POST", jsonObj, (returnValue) =>
            {
                callback?.Invoke(returnValue as Webhook);
            });
        }

        public static void GetChannelWebhooks(DiscordClient client, string channelID, Action<List<Webhook>> callback = null)
        {
            client.REST.DoRequest<List<Webhook>>($"/channels/{channelID}/webhooks", "GET", null, (returnValue) =>
            {
                callback?.Invoke(returnValue as List<Webhook>);
            });
        }

        public static void GetGuildWebhooks(DiscordClient client, string guildID, Action<List<Webhook>> callback = null)
        {
            client.REST.DoRequest<List<Webhook>>($"/guilds/{guildID}/webhooks", "GEt", null, (returnValue) =>
            {
                callback?.Invoke(returnValue as List<Webhook>);
            });
        }

        public static void GetWebhook(DiscordClient client, string webhookID, Action<Webhook> callback = null)
        {
            client.REST.DoRequest<Webhook>($"/webhooks/{webhookID}", "GET", null, (returnValue) =>
            {
                callback?.Invoke(returnValue as Webhook);
            });
        }
    }
}
