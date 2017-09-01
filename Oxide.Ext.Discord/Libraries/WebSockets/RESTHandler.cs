using System;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Ext.Discord.Libraries.DiscordObjects;
using Newtonsoft.Json;

namespace Oxide.Ext.Discord.Libraries.WebSockets
{
    public class RESTHandler
    {
        private readonly Dictionary<string, string> headers = new Dictionary<string, string>()
        {
            { "Authorization", $"Bot {Discord.Settings.ApiToken}" },
            { "Content-Type", "application/json" }
        };
        private const string URLBase = "https://discordapp.com/api";
        private WebRequests WebRequest = Interface.Oxide.GetLibrary<WebRequests>();

        #region Channel

        // https://discordapp.com/developers/docs/resources/channel

        public void GetChannel(string channelID, Action<Server.Channel> callback)
        {
            string URL = $"{URLBase}/channeles/{channelID}";
            WebRequest.EnqueueGet(URL, (code, response) =>
            {
                if (!VerifyRequest(code, response)) return;

                var channel = JsonConvert.DeserializeObject<Server.Channel>(response);
                callback.Invoke(channel);
            }, null, headers);
        }

        public void ModifyChannel(string channelID, Server.Channel channel, Action<Server.Channel> callback)
        {
            string URL = $"{URLBase}/channe;s/{channelID}";
            string body = JsonConvert.SerializeObject(channel);
            WebRequest.EnqueuePut(URL, body, (code, response) =>
            {
                if (!VerifyRequest(code, response)) return;

                var retChannel = JsonConvert.DeserializeObject<Server.Channel>(response);
                callback?.Invoke(retChannel);
            }, null, headers);
        }

        public void CreateMessage(string channelID, Message message, Action<Message> callback = null)
        {
            string URL = $"{URLBase}/channels/{channelID}/messages";
            string body = JsonConvert.SerializeObject(message);
            WebRequest.EnqueuePost(URL, body, (code, response) =>
            {
                if (!VerifyRequest(code, response)) return;

                var retMessage = JsonConvert.DeserializeObject<Message>(response);
                callback?.Invoke(retMessage);
            }, null, headers);
        }

        #endregion

        private bool VerifyRequest(int code, string response)
        {
            if (code != 200)
            {
                Interface.Oxide.LogError($"[Discord Ext] Recieved code {code} from Discord API (Response: {response}).");
                return false;
            }

            if (response == null)
            {
                Interface.Oxide.LogError($"[Discord Ext] Recieved a null response from Discord API (Code: {code}).");
                return false;
            }

            return true;
        }
    }
}
