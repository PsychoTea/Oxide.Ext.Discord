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
                return;
            }

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
            if (Interface.Oxide.CallHook("DiscordSocket_SocketConnecting", WSSURL) != null) return;

            if (string.IsNullOrEmpty(WSSURL))
            {
                Interface.Oxide.LogWarning("[Discord Ext] Error, no WSSURL was found.");
                return;
            }
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
            var data = RESTHandler.DoRequest<JObject>("/gateway", "GET");
            WSSURL = data.GetValue("url").ToString();
        }
    }
}
