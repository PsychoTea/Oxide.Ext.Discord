using Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Ext.Discord.Libraries.DiscordObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;

namespace Oxide.Ext.Discord.Libraries.WebSockets
{
    public class DiscordClient
    {
        public Server DiscordServer;
        public RESTHandler REST { get; private set; }
        public DiscordSettings Settings { get; private set; }
        public string WSSURL { get; private set; }
        private WebSocket Socket;
        private SocketHandler Handler;
        private Timer TimerLib = Interface.Oxide.GetLibrary<Timer>("Timer");
        private Timer.TimerInstance Timer;

        public DiscordClient(string apiKey, bool connectAuto = true)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                Interface.Oxide.LogError("[Discord Ext] Error! Please supply a valid API key!");
                return;
            }

            REST = new RESTHandler(apiKey);

            this.GetURL();

            if (!connectAuto)
            {
                Interface.Oxide.CallHook("DiscordSocket_SocketReady", WSSURL, this);
                return;
            }

            this.CreateSocket();
        }

        public void CreateSocket()
        {
            if (string.IsNullOrEmpty(WSSURL))
            {
                Interface.Oxide.LogWarning("[Discord Ext] Error, no WSSURL was found.");
                return;
            }

            if (Socket != null && Socket.ReadyState != WebSocketState.Closed)
            {
                Interface.Oxide.LogWarning("[Discord Ext] Error, tried to create a socket when one is already running.");
                return;
            }

            if (Interface.Oxide.CallHook("DiscordSocket_SocketConnecting", WSSURL) != null) return;

            Socket = new WebSocket(WSSURL + "/?v=6&encoding=json");
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
                Socket.Close();

            WSSURL = "";
        }

        public bool IsAlive() => Socket.IsAlive;

        public void SendData(string contents) => Socket.Send(contents);

        public void CreateHeartbeat(float heartbeatInterval, int lastHeartbeat)
        {
            if (Timer != null) return;

            Timer = TimerLib.Repeat(heartbeatInterval, -1, () =>
            {
                if (!Socket.IsAlive)
                {
                    Timer.Destroy();
                    Timer = null;
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

        private void GetURL()
        {
            var data = REST.DoRequest<JObject>("/gateway", "GET");
            WSSURL = data.GetValue("url").ToString();
        }
    }
}
