namespace Oxide.Ext.Discord
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
    using Oxide.Ext.Discord.REST;
    using Oxide.Ext.Discord.WebSockets;

    public class DiscordClient
    {
        public List<Plugin> Plugins { get; private set; } = new List<Plugin>();

        public DiscordSettings Settings { get; private set; } = new DiscordSettings();

        public Guild DiscordServer { get; set; }

        public RESTHandler REST { get; private set; }

        public string WSSURL { get; private set; }

        private Socket webSocket;

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
            webSocket = new Socket(this);

            if (!Discord.Clients.Any(x => x.Settings.ApiToken == apiKey))
            {
                throw new InvalidCreationException();
            }

            if (!string.IsNullOrEmpty(WSSURL))
            {
                webSocket.Connect(WSSURL);
                return;
            }

            this.GetURL(url =>
            {
                WSSURL = url;

                webSocket.Connect(WSSURL);
            });
        }

        public void Disconnect()
        {
            webSocket?.Disconnect();

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

        private void GetURL(Action<string> callback)
        {
            REST.DoRequest<JObject>("/gateway", RequestMethod.GET, null, (data) =>
            {
                string wsURL = (data as JObject).GetValue("url").ToString();

                callback.Invoke(wsURL);
            });
        }
    }
}
