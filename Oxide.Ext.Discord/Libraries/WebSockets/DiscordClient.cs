using Oxide.Core;
using Newtonsoft.Json;
using WebSocketSharp;
using System.Net;
using Newtonsoft.Json.Linq;
using Oxide.Ext.Discord.Libraries.DiscordObjects;
using System.Collections.Generic;
using Oxide.Ext.Discord.Libraries.SocketObjects;

namespace Oxide.Ext.Discord.Libraries.WebSockets
{
    public class DiscordClient
    {
        public WebSocket socket;
        private static string BaseURLTemplate = "https://discordapp.com/api/channels/{{ChannelID}}/messages";
        public WebClient client = new WebClient();
        public Server server;
        public Core.Libraries.Timer timer = Interface.Oxide.GetLibrary<Core.Libraries.Timer>("Timer");
        public string WSSURL = "starter_url";
        public int lastS = 0;
        public SocketHandler handler;
        public DiscordClient(bool connectAuto = true)
        {
            if (!Discord.IsConfigured)
            {
                Interface.Oxide.LogWarning("[Discord Ext] DiscordExt has not been configured correctly. Please verify your config file.");
                return;
            }

            if (Discord.settings.ApiToken == "change-me-please")
            {
                Interface.Oxide.LogWarning("[Discord Ext] Please change the API KEY before using this extension!");
                Interface.Oxide.CallHook("DiscordSocket_APIKeyException");
                return;
            }

            if (!Connect())
            {
                Interface.Oxide.LogWarning("[Discord Ext] There was an error grabbing the connection url.");
                Interface.Oxide.CallHook("DiscordSocket_SocketUrlError");
                return;
            }

            if (!connectAuto)
            {
                Interface.Oxide.CallHook("DiscordSocket_SocketReady", WSSURL, this);
                return;
            }

            CreateSocket();
        }
        public void SendMessage(string id, string text)
        {
            string payloadJson = JsonConvert.SerializeObject(new DiscordPayload()
            {
                MessageText = text
            });

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization", string.Format("Bot {0}", Discord.settings.ApiToken));
            headers.Add("Content-Type", "application/json");

            string url = BaseURLTemplate.Replace("{{ChannelID}}", id);
            Interface.Oxide.GetLibrary<Oxide.Core.Libraries.WebRequests>().EnqueuePost(url, payloadJson, (code, response) =>
            {
                if(code != 200)
                    Interface.Oxide.LogWarning($"[Discord Ext] There was an error with the discord API: {code} : {response}");
            }, null, headers);
        }
        public bool IsAlive() => socket.IsAlive;
        public void Disconnect()
        {
            if(socket.IsAlive) socket.Close();
            WSSURL = "starter_url";
            lastS = 0;
        }
        public bool Connect() => GetURL();
        private bool GetURL()
        {
            JObject data = null;
            try
            {
                data = JObject.Parse(client.DownloadString("https://discordapp.com/api/gateway"));
                WSSURL = data.GetValue("url").ToString();
                return true;
            } catch (WebException ex)
            {
                Interface.Oxide.LogWarning("There was an error asking discord for the wss connection string: "+ex.StackTrace);
                return false;
            }
        }
        public void CreateSocket()
        {
            if (Interface.Oxide.CallHook("DiscordSocket_SocketConnecting", WSSURL) != null) return;

            if (WSSURL == "starter_url") return;
            
            socket = new WebSocket(WSSURL + "?v=5&encoding=json");
            handler = new SocketHandler(this);
            socket.OnOpen += handler.SocketOpened;
            socket.OnClose += handler.SocketClosed;
            socket.OnError += handler.SocketErrored;
            socket.OnMessage += handler.SocketMessage;
            socket.Connect();
        }
    }
}
