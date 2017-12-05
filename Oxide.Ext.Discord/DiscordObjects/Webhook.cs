namespace Oxide.Ext.Discord.DiscordObjects
{
    using System;
    using System.Collections.Generic;
    using Oxide.Ext.Discord.REST;

    public class Webhook
    {
        public string id { get; set; }

        public string guild_id { get; set; }

        public string channel_id { get; set; }

        public User user { get; set; }

        public string name { get; set; }

        public string avatar { get; set; }

        public string token { get; set; }

        public static void CreateWebhook(DiscordClient client, string channelID, string name, string avatar, Action<Webhook> callback = null)
        {
            var jsonObj = new Dictionary<string, string>()
            {
                { "name", name },
                { "avatar", avatar }
            };

            client.REST.DoRequest($"/channels/{channelID}/webhooks", RequestMethod.POST, jsonObj, callback);
        }

        public static void GetChannelWebhooks(DiscordClient client, string channelID, Action<List<Webhook>> callback = null)
        {
            client.REST.DoRequest($"/channels/{channelID}/webhooks", RequestMethod.GET, null, callback);
        }

        public static void GetGuildWebhooks(DiscordClient client, string guildID, Action<List<Webhook>> callback = null)
        {
            client.REST.DoRequest($"/guilds/{guildID}/webhooks", RequestMethod.GET, null, callback);
        }

        public static void GetWebhook(DiscordClient client, string webhookID, Action<Webhook> callback = null)
        {
            client.REST.DoRequest($"/webhooks/{webhookID}", RequestMethod.GET, null, callback);
        }

        public static void GetWebhookWithToken(DiscordClient client, string webhookID, string webhookToken, Action<Webhook> callback = null)
        {
            client.REST.DoRequest($"/webhooks/{webhookID}/{webhookToken}", RequestMethod.GET, null, callback);
        }

        public void ModifyWebhook(DiscordClient client, string name, string avatar, Action<Webhook> callback = null)
        {
            var jsonObj = new Dictionary<string, object>()
            {
                { "name", name },
                { "avatar", avatar }
            };

            client.REST.DoRequest($"/webhooks/{id}", RequestMethod.POST, jsonObj, callback);
        }

        public void ModifyWebhookWithToken(DiscordClient client, string name, string avatar, Action<Webhook> callback = null)
        {
            var jsonObj = new Dictionary<string, object>()
            {
                { "name", name },
                { "avatar", avatar }
            };

            client.REST.DoRequest<Webhook>($"/webhooks/{id}/{token}", RequestMethod.POST, jsonObj, callback);
        }

        public void DeleteWebhook(DiscordClient client, Action callback = null)
        {
            client.REST.DoRequest($"/webhooks/{id}", RequestMethod.DELETE, null, callback);
        }

        public void DeleteWebhookWithToken(DiscordClient client, Action callback = null)
        {
            client.REST.DoRequest($"/webhooks/{id}/{token}", RequestMethod.DELETE, null, callback);
        }

        public void ExecuteWebhook(DiscordClient client, bool wait, WebhookPayload payload, Action callback = null)
        {
            client.REST.DoRequest($"/webhooks/{id}/{token}?wait={wait}", RequestMethod.POST, payload, callback);
        }

        public void ExecuteWebhookSlack(DiscordClient client, bool wait, WebhookPayload payload, Action callback = null)
        {
            client.REST.DoRequest($"/webhooks/{id}/{token}/slack?wait={wait}", RequestMethod.POST, payload, callback);
        }

        public void ExecuteWebhookGitHub(DiscordClient client, bool wait, WebhookPayload payload, Action callback = null)
        {
            client.REST.DoRequest($"/webhooks/{id}/{token}/github?wait={wait}", RequestMethod.POST, payload, callback);
        }
    }
}
