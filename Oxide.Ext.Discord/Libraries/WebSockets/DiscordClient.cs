using System;
using Oxide.Core;
using System.IO;
using Newtonsoft.Json;
using WebSocketSharp;
using System.Net;
using Newtonsoft.Json.Linq;
using Oxide.Ext.Discord.Libraries.DiscordObjects;

namespace Oxide.Ext.Discord.Libraries.WebSockets
{
    public class DiscordClient
    {
        public WebSocket socket;
        public WebClient client = new WebClient();
        public Server server;
        public Core.Libraries.Timer timer = Interface.Oxide.GetLibrary<Core.Libraries.Timer>("Timer");
        public string WSSURL = "starter_url";
        public int lastS = 0;
        public SocketHandler handler;
        public DiscordClient(bool connectAuto = true)
        {
            if (Discord.settings.ApiToken == "change-me-please")
            {
                Interface.Oxide.LogWarning("[Discord Ext] Please change the API KEY before using this extension!");
                Interface.Oxide.CallHook("DiscordSocket_APIKeyException");
            }
            if (Connect())
            {
                if (connectAuto)
                    if (Interface.Oxide.CallHook("DiscordSocket_SocketConnecting", WSSURL) != null) return;
                    else CreateSocket();
                else Interface.Oxide.CallHook("DiscordSocket_SocketReady", WSSURL, this);
            } else
            {
                Interface.Oxide.LogWarning("[Discord Ext] There was an error grabbing the connection url.");
                Interface.Oxide.CallHook("DiscordSocket_SocketUrlError");
            }
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
