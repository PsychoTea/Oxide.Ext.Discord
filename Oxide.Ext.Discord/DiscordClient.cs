namespace Oxide.Ext.Discord
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Timers;
    using Newtonsoft.Json;
    using Oxide.Core;
    using Oxide.Core.Plugins;
    using Oxide.Ext.Discord.Attributes;
    using Oxide.Ext.Discord.DiscordEvents;
    using Oxide.Ext.Discord.DiscordObjects;
    using Oxide.Ext.Discord.Exceptions;
    using Oxide.Ext.Discord.Gateway;
    using Oxide.Ext.Discord.Helpers;
    using Oxide.Ext.Discord.REST;
    using Oxide.Ext.Discord.WebSockets;

    public class DiscordClient
    {
        public List<Plugin> Plugins { get; private set; } = new List<Plugin>();

        public DiscordSettings Settings { get; private set; } = new DiscordSettings();

        public Guild DiscordServer { get; set; }

        public RESTHandler REST { get; private set; }

        public string WSSURL { get; private set; }

        public int Sequence;

        public string SessionID;

        private Socket _webSocket;

        private Timer _timer;

        private int _lastHeartbeat;

        public void Initialize(Plugin plugin, DiscordSettings settings)
        {
            if (plugin == null)
            {
                throw new PluginNullException();
            }

            if (settings == null)
            {
                throw new SettingsNullException();
            }

            if (string.IsNullOrEmpty(settings.ApiToken))
            {
                throw new APIKeyException();
            }

            RegisterPlugin(plugin);

            Settings = settings;

            REST = new RESTHandler(Settings.ApiToken);
            _webSocket = new Socket(this);

            if (!string.IsNullOrEmpty(WSSURL))
            {
                _webSocket.Connect(WSSURL);
                return;
            }

            this.GetURL(url =>
            {
                WSSURL = url;

                _webSocket.Connect(WSSURL);
            });
        }

        public void Disconnect()
        {
            _webSocket?.Disconnect();

            WSSURL = string.Empty;

            REST?.Shutdown();
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
                if (!specificPlugin.IsLoaded) return null;

                return specificPlugin.CallHook(hookname, args);
            }

            Dictionary<string, object> returnValues = new Dictionary<string, object>();

            foreach (var plugin in Plugins.Where(x => x.IsLoaded))
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

        public void CreateHeartbeat(float heartbeatInterval)
        {
            if (_timer != null)
            {
                Interface.Oxide.LogError($"[Oxide.Ext.Discord] Error: tried to create a heartbeat when one is already registered.");
                return;
            }

            _lastHeartbeat = (int)Time.TimeSinceEpoch();

            _timer = new Timer()
            {
                Interval = heartbeatInterval
            };
            _timer.Elapsed += HeartbeatElapsed;
            _timer.Start();
        }

        private void HeartbeatElapsed(object sender, ElapsedEventArgs e)
        {
            if (!_webSocket.IsAlive() ||
                _webSocket.IsClosed() ||
                _webSocket.IsClosed())
            {
                _timer.Dispose();
                _timer = null;
                return;
            }

            SendHeartbeat();
        }

        private void GetURL(Action<string> callback)
        {
            DiscordObjects.Gateway.GetGateway(this, (gateway) =>
            {
                // Example: wss://gateway.discord.gg/?v=6&encoding=json
                string fullURL = $"{gateway.URL}/?{Connect.Serialize()}";

                if (Settings.Debugging)
                {
                    Interface.Oxide.LogDebug($"Got Gateway url: {fullURL}");
                }

                callback.Invoke(fullURL);
            });
        }

        #region Discord Events
        
        public void Identify()
        {
            // Sent immediately after connecting. Opcode 2: Identify
            // Ref: https://discordapp.com/developers/docs/topics/gateway#identifying

            Identify identify = new Identify()
            {
                Token = this.Settings.ApiToken,
                Properties = new Properties()
                {
                    OS = "Oxide.Ext.Discord",
                    Browser = "Oxide.Ext.Discord",
                    Device = "Oxide.Ext.Discord"
                },
                Compress = false,
                LargeThreshold = 50,
                Shard = new List<int>() { 0, 1 }
            };

            var opcode = new SPayload()
            {
                OP = OpCodes.Identify,
                Payload = identify
            };
            var payload = JsonConvert.SerializeObject(opcode);

            _webSocket.Send(payload);
        }

        // TODO: Implement the usage of this event
        public void Resume()
        {
            var resume = new Resume()
            {
                Sequence = this.Sequence,
                SessionID = this.SessionID,
                Token = string.Empty // What is this meant to be?
            };

            var packet = new RPayload()
            {
                OpCode = OpCodes.Resume,
                Data = resume
            };

            string payload = JsonConvert.SerializeObject(packet);
            _webSocket.Send(payload);
        }
        
        public void SendHeartbeat()
        {
            var packet = new RPayload()
            {
                OpCode = OpCodes.Heartbeat,
                Data = _lastHeartbeat
            };

            string message = JsonConvert.SerializeObject(packet);
            _webSocket.Send(message);

            this.CallHook("DiscordSocket_HeartbeatSent");

            if (Settings.Debugging)
            {
                Interface.Oxide.LogDebug($"Heartbeat sent - {_timer.Interval}ms interval.");
            }
        }
        
        public void RequestGuildMembers(string query = "", int limit = 0)
        {
            var requestGuildMembers = new GuildMembersRequest()
            {
                GuildID = DiscordServer.id,
                Query = query,
                Limit = limit
            };

            var packet = new RPayload()
            {
                OpCode = OpCodes.RequestGuildMembers,
                Data = requestGuildMembers
            };

            string payload = JsonConvert.SerializeObject(packet);
            _webSocket.Send(payload);
        }

        public void UpdateVoiceState(string channelId, bool selfDeaf, bool selfMute)
        {
            var voiceState = new VoiceStateUpdate()
            {
                ChannelID = channelId,
                GuildID = DiscordServer.id,
                SelfDeaf = selfDeaf,
                SelfMute = selfMute
            };

            var packet = new RPayload()
            {
                OpCode = OpCodes.VoiceStatusUpdate,
                Data = voiceState
            };

            string payload = JsonConvert.SerializeObject(packet);
            _webSocket.Send(payload);
        }

        public void UpdateStatus(Presence presence)
        {
            var opcode = new SPayload()
            {
                OP = OpCodes.StatusUpdate,
                Payload = presence
            };

            var payload = JsonConvert.SerializeObject(opcode);
            _webSocket.Send(payload);
        }

        #endregion
    }
}
