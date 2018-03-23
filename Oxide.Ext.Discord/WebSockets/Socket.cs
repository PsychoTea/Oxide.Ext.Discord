namespace Oxide.Ext.Discord.WebSockets
{
    using System;
    using Oxide.Ext.Discord.Exceptions;
    using WebSocketSharp;

    public class Socket
    {
        private DiscordClient client;

        private WebSocket socket;

        private SocketListner listner;

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
    }
}
