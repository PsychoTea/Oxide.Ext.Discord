using System.Collections.Generic;
using System.Net;
using Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Ext.Discord.Libraries.DiscordObjects;
using Oxide.Ext.Discord.Libraries.SocketObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;

namespace Oxide.Ext.Discord.Libraries.WebSockets
{
    public class DiscordClient
    {
        public Server DiscordServer;
        public string WSSURL { get; private set; }
        private WebSocket Socket;
        private SocketHandler Handler;
        private Timer TimerLib = Interface.Oxide.GetLibrary<Timer>("Timer");
        private Timer.TimerInstance Timer;

        public DiscordClient(bool connectAuto = true)
        {
            if (!Discord.IsConfigured)
            {
                Interface.Oxide.LogWarning("[Discord Ext] DiscordExt has not been configured correctly. Please verify your config file.");
                return;
            }

            if (Discord.Settings.ApiToken == "change-me-please")
            {
                Interface.Oxide.LogWarning("[Discord Ext] Please change the API KEY before using this extension!");
                Interface.Oxide.CallHook("DiscordSocket_APIKeyException");
                return;
            }

            if (!GetURL())
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

        public void CreateSocket()
        {
            if (Interface.Oxide.CallHook("DiscordSocket_SocketConnecting", WSSURL) != null) return;

            if (string.IsNullOrEmpty(WSSURL))
            {
                Interface.Oxide.LogWarning("[Discord Ext] Error, no WSSURL was found.");
                return;
            }

            Socket = new WebSocket(WSSURL + "?v=5&encoding=json");
            Handler = new SocketHandler(this);
            Socket.OnOpen += Handler.SocketOpened;
            Socket.OnClose += Handler.SocketClosed;
            Socket.OnError += Handler.SocketErrored;
            Socket.OnMessage += Handler.SocketMessage;
            Socket.ConnectAsync();
        }

        public void Disconnect()
        {
            if (Socket.IsAlive)
            {
                Socket.Close();
            }

            WSSURL = "";
        }

        public bool IsAlive() => Socket.IsAlive;

        public void SendData(string contents) => Socket.Send(contents);

        public void DoHeartbeat(float heartbeatInterval, int lastHeartbeat)
        {
            Timer = TimerLib.Repeat(heartbeatInterval, -1, () =>
            {
                if (!Socket.IsAlive)
                {
                    Timer.Destroy();
                    return;
                }

                var packet = new Packet()
                {
                    op = 1,
                    d = lastHeartbeat
                };

                string message = JsonConvert.SerializeObject(packet);
                Socket.Send(message);
                Interface.Oxide.CallHook("DiscordSocket_HeartbeatSent");
            });
        }

        private bool GetURL()
        {
            JObject data = null;
            try
            {
                using (var client = new WebClient())
                {
                    data = JObject.Parse(client.DownloadString("https://discordapp.com/api/gateway"));
                }
                WSSURL = data.GetValue("url").ToString();
                return true;
            }
            catch (WebException ex)
            {
                Interface.Oxide.LogWarning("There was an error asking discord for the wss connection string: " + ex.StackTrace);
                return false;
            }
        }

        //this needs moving
        public void SendMessage(string id, string text)
        {
            string payloadJson = JsonConvert.SerializeObject(new DiscordPayload()
            {
                MessageText = text
            });

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization", string.Format("Bot {0}", Discord.Settings.ApiToken));
            headers.Add("Content-Type", "application/json");

            string url = BaseURLTemplate.Replace("{{ChannelID}}", id);
            Interface.Oxide.GetLibrary<Oxide.Core.Libraries.WebRequests>().EnqueuePost(url, payloadJson, (code, response) =>
            {
                if (code != 200)
                    Interface.Oxide.LogWarning($"[Discord Ext] There was an error with the discord API: {code} : {response}");
            }, null, headers);
        }
    }
}
