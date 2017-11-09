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

            var search = Clients.Find(x => x.Plugins.Find(y => y.Filename == plugin.Filename) != null);
            if(search != null)
            {
                if (!search.Settings.ApiToken.Equals(apiKey))
                    throw new LimitedClientException();


                search.Plugins.Remove(search.Plugins.Find(x => x.Filename == plugin.Filename));
                search.RegisterPlugin(plugin);
                search.SetDiscordClient(plugin);
                search.CallHook("DiscordSocket_Initialized", plugin);
            }
            else
            {
                search = Clients.Find(x => x.Settings.ApiToken.Equals(apiKey));
                if (search != null)
                {
                    if (search.IsAlive() && search.DiscordServer != null)
                    {
                        search.RegisterPlugin(plugin);
                        search.SetDiscordClient(plugin);
                        search.CallHook("DiscordSocket_Initialized", plugin);
                    }
                    else
                    {
                        search.Initialize(plugin, apiKey);
                        search.SetDiscordClient();
                        search.CallHook("DiscordSocket_Initialized", plugin);
                    }
                }
                else
                {
                    var newClient = new DiscordClient();
                    Clients.Add(newClient);
                    newClient.Initialize(plugin, apiKey);
                }
            }
        }

        public static void CloseClient(DiscordClient client)
        {
            if (client == null) return;
            client.Disconnect();
            Clients.Remove(client);
        }
    }
}
