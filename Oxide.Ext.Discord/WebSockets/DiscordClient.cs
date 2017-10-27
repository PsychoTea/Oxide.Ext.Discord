using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Timers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord.Attributes;
using Oxide.Ext.Discord.DiscordObjects;
using Oxide.Ext.Discord.Exceptions;
using WebSocketSharp;

namespace Oxide.Ext.Discord.WebSockets
{
    public class DiscordClient
    {
        public List<Plugin> Plugins { get; private set; } = new List<Plugin>();
        public DiscordSettings Settings { get; private set; } = new DiscordSettings();
        public Guild DiscordServer { get; set; }
        public RESTHandler REST { get; private set; }
        public string WSSURL { get; private set; }
        public UpkeepHandler UpHandler { get; private set; }
        private WebSocket Socket = null;
        private SocketHandler Handler;
        private Timer Timer;
        private int LastHeartbeat;

        /// <exception cref="APIKeyException">Throws when a user does not provide a API key.</exception>
        /// <exception cref="NoURLException">Throws when CreateSocket is called, but no url is stored.</exception>
        /// <exception cref="SocketRunningException">Throws when CreateSocket is called, but a socket is already running.</exception>
        /// <exception cref="InvalidCreationException">Throws when a user tries to use this method to create a discord client. Should use the static method in the Discord class.</exception>

        public void Initialize(Plugin plugin, string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new APIKeyException();

            if (plugin == null)
                throw new PluginNullException();

            RegisterPlugin(plugin);

            Settings.ApiToken = apiKey;
            REST = new RESTHandler(Settings.ApiToken);

            if (!Discord.Clients.Any(x => x.Settings.ApiToken == apiKey))
                throw new InvalidCreationException();

            this.Connect();
        }

        public void SetDiscordClient()
        {
            foreach (var plugin in Plugins)
            {
                foreach (var field in plugin.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    if (field.GetCustomAttributes(typeof(DiscordClientAttribute), true).Any())
                    {
                        field.SetValue(plugin, this);
                    }
                }
            }
        }

        public void Connect()
        {
            if (!string.IsNullOrEmpty(WSSURL))
            {
                this.CreateSocket();
                return;
            }

            this.GetURL(() =>
            {
                this.CreateSocket();
            });
        }

        public void CreateSocket()
        {
            if (string.IsNullOrEmpty(WSSURL))
                throw new NoURLException();

            if (Socket != null && Socket.ReadyState != WebSocketState.Closed)
                throw new SocketRunningException(this);

            if (this.CallHook("DiscordSocket_SocketConnecting", WSSURL) != null)
                return;

            Socket = new WebSocket(WSSURL + "/?v=6&encoding=json");
            Handler = new SocketHandler(this);
            UpHandler = new UpkeepHandler(this);
            Socket.OnOpen += Handler.SocketOpened;
            Socket.OnClose += Handler.SocketClosed;
            Socket.OnError += Handler.SocketErrored;
            Socket.OnMessage += Handler.SocketMessage;
            Socket.ConnectAsync();
        }

        public void Disconnect()
        {
            if (!IsClosed())
            {
                Socket.CloseAsync();
            }

            WSSURL = "";
            REST?.Shutdown();
        }

        public bool IsAlive() => Socket?.IsAlive ?? false;

        public bool IsClosing() => Socket.ReadyState == WebSocketState.Closing;

        public bool IsClosed() => Socket.ReadyState == WebSocketState.Closed;

        public void SendData(string contents) => Socket.Send(contents);

        public void RegisterPlugin(Plugin plugin)
        {
            var search = Plugins.Where(x => x.Title == plugin.Title);
            search.ToList().ForEach(x => Plugins.Remove(x));
            Plugins.Add(plugin);
        }

        public object CallHook(string hookname, params object[] args)
        {
            Dictionary<string, object> returnValues = new Dictionary<string, object>();

            foreach (var plugin in Plugins)
            {
                var retVal = plugin.CallHook(hookname, args);
                returnValues.Add(plugin.Title, retVal);
            }

            if (returnValues.Count(x => x.Value != null) > 1)
            {
                string conflicts = string.Join("\n", returnValues.Select(x => $"Plugin {x.Key}: {returnValues.Values.ToString()}").ToArray());
                Interface.Oxide.LogWarning($"[Discord Ext] A hook conflict was triggered on {hookname} between: {conflicts}");
                return null;
            }

            return returnValues.FirstOrDefault(x => x.Value != null).Value;
        }

        public string GetPluginNames(string delimiter = ", ") => string.Join(delimiter, Plugins.Select(x => x.Name).ToArray());

        public void CreateHeartbeat(float heartbeatInterval, int lastHeartbeat)
        {
            LastHeartbeat = lastHeartbeat;

            if (Timer != null) return;

            Timer = new Timer()
            {
                Interval = heartbeatInterval
            };
            Timer.Elapsed += HeartbeatElapsed;
            Timer.Start();
        }
        
        private void HeartbeatElapsed(object sender, ElapsedEventArgs e)
        {
            if (!Socket.IsAlive || IsClosing() || IsClosed())
            {
                Timer.Dispose();
                Timer = null;
                return;
            }

            var packet = new Packet()
            {
                op = 1,
                d = LastHeartbeat
            };

            string message = JsonConvert.SerializeObject(packet);
            Socket.Send(message);

            this.CallHook("DiscordSocket_HeartbeatSent");
        }

        private void GetURL(Action callback)
        {
            REST.DoRequest<JObject>("/gateway", "GET", null, (data) =>
            {
                WSSURL = (data as JObject).GetValue("url").ToString();
                callback.Invoke();
            });
        }
    }
}
