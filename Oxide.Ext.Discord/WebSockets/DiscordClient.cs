namespace Oxide.Ext.Discord.WebSockets
{
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

    public class DiscordClient
    {
        public List<Plugin> Plugins { get; private set; } = new List<Plugin>();

        public DiscordSettings Settings { get; private set; } = new DiscordSettings();

        public Guild DiscordServer { get; set; }

        public RESTHandler REST { get; private set; }

        public string WSSURL { get; private set; }

        public UpkeepHandler UpHandler { get; private set; }

        public WebSocket Socket { get; private set; } = null;

        private SocketHandler handler;

        private Timer timer;

        private int lastHeartbeat;

        public void Initialize(Plugin plugin, string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new APIKeyException();
            }

            if (plugin == null)
            {
                throw new PluginNullException();
            }

            RegisterPlugin(plugin);

            Settings.ApiToken = apiKey;
            REST = new RESTHandler(Settings.ApiToken);

            if (!Discord.Clients.Any(x => x.Settings.ApiToken == apiKey))
            {
                throw new InvalidCreationException();
            }

            this.Connect();
        }

        public void UpdatePluginReference(Plugin plugin = null)
        {
            List<Plugin> affectedPlugins = (plugin == null) ? Plugins : new List<Plugin>() { plugin };

            foreach (var pluginItem in affectedPlugins)
            {
                foreach (var field in pluginItem.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                {
                    if (field.GetCustomAttributes(typeof(DiscordClientAttribute), true).Any())
                    {
                        field.SetValue(pluginItem, this);
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
            {
                throw new NoURLException();
            }

            if (Socket != null && Socket.ReadyState != WebSocketState.Closed)
            {
                throw new SocketRunningException(this);
            }

            Socket = new WebSocket(WSSURL + "/?v=6&encoding=json");
            handler = new SocketHandler(this);
            UpHandler = new UpkeepHandler(this);
            Socket.OnOpen += handler.SocketOpened;
            Socket.OnClose += handler.SocketClosed;
            Socket.OnError += handler.SocketErrored;
            Socket.OnMessage += handler.SocketMessage;
            Socket.ConnectAsync();
        }

        public void Disconnect()
        {
            if (!IsClosed())
            {
                Socket.CloseAsync();
            }

            WSSURL = string.Empty;
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

        public object CallHook(string hookname, Plugin specificPlugin = null, params object[] args)
        {
            if (specificPlugin != null)
            {
                return specificPlugin.CallHook(hookname, args);
            }

            Dictionary<string, object> returnValues = new Dictionary<string, object>();

            foreach (var plugin in Plugins)
            {
                var retVal = plugin.CallHook(hookname, args);
                returnValues.Add(plugin.Title, retVal);
            }

            if (returnValues.Count(x => x.Value != null) > 1)
            {
                string conflicts = string.Join("\n", returnValues.Select(x => $"Plugin {x.Key} - {x.Value}").ToArray());
                Interface.Oxide.LogWarning($"[Discord Ext] A hook conflict was triggered on {hookname} between:\n{conflicts}");
                return null;
            }

            return returnValues.FirstOrDefault(x => x.Value != null).Value;
        }

        public string GetPluginNames(string delimiter = ", ") => string.Join(delimiter, Plugins.Select(x => x.Name).ToArray());

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
            Socket.Send(message);

            this.CallHook("DiscordSocket_HeartbeatSent");
        }

        private void HeartbeatElapsed(object sender, ElapsedEventArgs e)
        {
            if (!Socket.IsAlive || IsClosing() || IsClosed())
            {
                timer.Dispose();
                timer = null;
                return;
            }

            SendHeartbeat();
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
