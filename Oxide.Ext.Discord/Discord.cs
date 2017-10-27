using System.Collections.Generic;
using System.Linq;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Ext.Discord.Exceptions;
using Oxide.Ext.Discord.WebSockets;

namespace Oxide.Ext.Discord
{
    public class Discord
    {
        public static List<DiscordClient> Clients { get; private set; } = new List<DiscordClient>();

        public static void CreateClient(Plugin plugin, string apiKey)
        {
            if (plugin == null)
                throw new PluginNullException();

            if (string.IsNullOrEmpty(apiKey))
                throw new APIKeyException();

            var search = Clients.Where(x => x.Settings.ApiToken == apiKey);
            if (search.Count() > 1)
            {
                Interface.Oxide.LogWarning("[Discord Ext] Multiple DiscordClient's found for one APIKey, destroying...");
                search.ToList().ForEach(x =>
                {
                    x.Disconnect();
                    Clients.Remove(x);
                });
            }

            if (search.Count() == 1)
            {
                var client = search.First();

                // Hmm... if the WS is connected and DiscordServer is null
                // a SocketRunningException will (probably) be thrown
                if (client.IsAlive() && client.DiscordServer != null)
                {
                    client.RegisterPlugin(plugin);
                    client.SetDiscordClient();
                    client.CallHook("DiscordSocket_Initialized");
                    return;
                }

                client.Initialize(plugin, apiKey);
                return;
            }

            var newClient = new DiscordClient();
            Clients.Add(newClient);
            newClient.Initialize(plugin, apiKey);
        }

        public static void CloseClient(DiscordClient client)
        {
            if (client == null) return;
            client.Disconnect();
            Clients.Remove(client);
        }
    }
}
