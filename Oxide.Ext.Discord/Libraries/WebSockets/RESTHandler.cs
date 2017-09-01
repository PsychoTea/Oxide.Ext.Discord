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
            string URL = $"{URLBase}/channels/{channelID}";
            DoGet(URL, typeof(Server.Channel), (obj) => callback.Invoke(obj as Server.Channel));
        }

        public void ModifyChannel(string channelID, Server.Channel channel, Action<Server.Channel> callback = null)
        {
            string URL = $"{URLBase}/channels/{channelID}";
            DoPut(URL, channel, (obj) => callback?.Invoke(obj as Server.Channel));
        }

        public void CreateMessage(string channelID, Message message, Action<Message> callback = null)
        {
            string URL = $"{URLBase}/channels/{channelID}/messages";
            DoPost(URL, message, (obj) => callback?.Invoke(obj as Message));
        }

        #endregion

        private void DoGet(string URL, Type returnType = null, Action<object> callback = null)
        {
            WebRequest.EnqueueGet(URL, (code, response) =>
            {
                if (!VerifyRequest(code, response)) return;

                if (returnType == null) return;
                var responseObj = JsonConvert.DeserializeObject(response, returnType);
                callback?.Invoke(responseObj);
            }, null, headers);
        }

        private void DoPut(string URL, object data, Action<object> callback = null)
        {
            string contents = JsonConvert.SerializeObject(data);
            WebRequest.EnqueuePut(URL, contents, (code, response) =>
            {
                if (!VerifyRequest(code, response)) return;
                
                var responseObj = JsonConvert.DeserializeObject(response, data.GetType());
                callback?.Invoke(responseObj);
            }, null, headers);
        }

        private void DoPost(string URL, object data, Action<object> callback = null)
        {
            string contents = JsonConvert.SerializeObject(data);
            WebRequest.EnqueuePost(URL, contents, (code, response) =>
            {
                if (!VerifyRequest(code, response)) return;
                
                var responseObj = JsonConvert.DeserializeObject(response, data.GetType());
                callback?.Invoke(responseObj);
            }, null, headers);
        }

        private bool VerifyRequest(int code, string response)
        {
            if (code != 200)
            {
                Interface.Oxide.LogError($"[Discord Ext] Received code {code} from Discord API (Response: {response}).");
                return false;
            }

            if (response == null)
            {
                Interface.Oxide.LogError($"[Discord Ext] Received a null response from Discord API (Code: {code}).");
                return false;
            }

            return true;
        }
    }
}
