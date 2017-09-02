using System.Linq;
using Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Ext.Discord.DiscordObjects;
using Oxide.Ext.Discord.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp; 

namespace Oxide.Ext.Discord.WebSockets
{
    public class DiscordClient
    {
        public DiscordSettings Settings { get; private set; } = new DiscordSettings();
        public Server DiscordServer { get; set; }
        public RESTHandler REST { get; private set; }
        public string WSSURL { get; private set; }
        private WebSocket Socket = null;
        private SocketHandler Handler;
        private Timer TimerLib = Interface.Oxide.GetLibrary<Timer>("Timer");
        private Timer.TimerInstance Timer;

        /// <exception cref="APIKeyException">Throws when a user does not provide a API key.</exception>
        /// <exception cref="NoURLException">Throws when CreateSocket is called, but no url is stored.</exception>
        /// <exception cref="SocketRunningException">Throws when CreateSocket is called, but a socket is already running.</exception>
        /// <exception cref="InvalidCreationException">Throws when a user tries to use this method to create a discord client. Should use the static method in the Discord class.</exception>

        public void Initialize(string apiKey, bool connectAuto)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new APIKeyException();
            
            Settings.ApiToken = apiKey;
            REST = new RESTHandler(Settings.ApiToken);

            if (!Discord.Clients.Any(x => x.Settings.ApiToken == apiKey))
                throw new InvalidCreationException();

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
                throw new NoURLException();

            if (Socket != null && Socket.ReadyState != WebSocketState.Closed)
                throw new SocketRunningException();

            if (Interface.Oxide.CallHook("DiscordSocket_SocketConnecting", WSSURL) != null)
                return;

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

            REST?.Shutdown();
        }

        public bool IsAlive() => Socket.IsAlive;

        public void SendData(string contents) => Socket.Send(contents);

        public void CreateHeartbeat(float heartbeatInterval, int lastHeartbeat)
        {
            if (Timer != null)
                return;

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
            var data = REST.DoRequestNow<JObject>("/gateway", "GET");
            WSSURL = data.GetValue("url").ToString();
        }
    }
}
