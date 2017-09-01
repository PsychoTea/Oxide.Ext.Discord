using System.Net;
using Newtonsoft.Json.Linq;
using Oxide.Core;
using Oxide.Ext.Discord.Libraries.DiscordObjects;
using WebSocketSharp;

namespace Oxide.Ext.Discord.Libraries.WebSockets
{
    public class DiscordClient
    {
        public WebSocket socket;
        public WebClient client = new WebClient();
        public Server server;
        public Core.Libraries.Timer timer = Interface.Oxide.GetLibrary<Core.Libraries.Timer>("Timer");
        public string WSSURL;
        public int lastS;
        public SocketHandler handler;

        public DiscordClient(bool connectAuto = true)
        {
            if (!Discord.IsConfigured)
            {
                Interface.Oxide.LogWarning("[Discord Ext] You have not correctly configured DiscordExt. Please verify your config file is setup correctly.");
                return;
            }

            if (Discord.settings.ApiToken == "change-me-please")
            {
                Interface.Oxide.LogWarning("[Discord Ext] Please change the API KEY before using this extension!");
                Interface.Oxide.CallHook("DiscordSocket_APIKeyException");
                return;
            }

            if (!CanConnect())
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

            if (Interface.Oxide.CallHook("DiscordSocket_SocketConnecting", WSSURL) != null) return;

            CreateSocket();
        }

        public void CreateSocket()
        {
            if (string.IsNullOrEmpty(WSSURL)) return;

            socket = new WebSocket(WSSURL + "?v=5&encoding=json");
            handler = new SocketHandler(this);
            socket.OnOpen += handler.SocketOpened;
            socket.OnClose += handler.SocketClosed;
            socket.OnError += handler.SocketErrored;
            socket.OnMessage += handler.SocketMessage;
            //socket.Connect();
        }

        public void Disconnect()
        {
            if (socket.IsAlive) socket.Close();
            WSSURL = "";
            lastS = 0;
        }

        public bool IsAlive() => socket.IsAlive;

        public bool CanConnect() => GetURL();

        private bool GetURL()
        {
            JObject data = null;
            try
            {
                data = JObject.Parse(client.DownloadString("https://discordapp.com/api/gateway"));
                WSSURL = data.GetValue("url").ToString();
                return true;
            }
            catch (WebException ex)
            {
                Interface.Oxide.LogWarning("There was an error getting the WSS URL: " + ex.StackTrace);
                return false;
            }
        }
    }
}
