namespace Oxide.Ext.Discord.WebSockets
{
    using System;
    using System.Timers;
    using Newtonsoft.Json;
    using Oxide.Ext.Discord.DiscordObjects;
    using Oxide.Ext.Discord.Exceptions;
    using WebSocketSharp;

    public class Socket
    {
        private DiscordClient client;

        private WebSocket socket;

        private SocketListner listner;

        public bool shouldResume = false;

        public Resume resume;

        private Timer timer;

        public int lastHeartbeat;
        public Socket(DiscordClient client)
        {
            this.client = client;
        }

        public void Connect(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new NoURLException();
            }

            if (socket != null && socket.ReadyState != WebSocketState.Closed)
            {
                throw new SocketRunningException(client);
            }

            socket = new WebSocket($"{url}/?v=6&encoding=json");

            listner = new SocketListner(client, this);

            socket.OnOpen += listner.SocketOpened;
            socket.OnClose += listner.SocketClosed;
            socket.OnError += listner.SocketErrored;
            socket.OnMessage += listner.SocketMessage;

            socket.ConnectAsync();
        }

        public void Disconnect()
        {
            if (IsClosed()) return;

            socket?.CloseAsync();
        }

        public void Send(string message, Action<bool> completed = null) => socket?.SendAsync(message, completed);

        public bool IsAlive() => socket?.IsAlive ?? false;

        public bool IsClosing() => socket?.ReadyState == WebSocketState.Closing;

        public bool IsClosed() => socket?.ReadyState == WebSocketState.Closed;

        public void CreateHeartbeat(float heartbeatInterval, int lastHeartbeat)
        {
            this.lastHeartbeat = lastHeartbeat;

            if (timer != null) return;

            timer = new Timer()
            {
                Interval = heartbeatInterval
            };
            timer.Elapsed += HeartbeatElapsed;
            timer.Start();
        }

        public void SendHeartbeat()
        {
            var packet = new Packet()
            {
                op = 1,
                d = lastHeartbeat
            };

            string message = JsonConvert.SerializeObject(packet);
            socket.Send(message);
            client.CallHook("DiscordSocket_HeartbeatSent");
        }

        private void HeartbeatElapsed(object sender, ElapsedEventArgs e)
        {
            if (!IsAlive() || IsClosed())
            {
                timer.Dispose();
                timer = null;
                return;
            }
            SendHeartbeat();
        }
    }
}
