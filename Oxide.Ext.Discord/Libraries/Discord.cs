using System.Collections.Generic;
using System.Linq;
using Oxide.Core;
using Oxide.Ext.Discord.Libraries.WebSockets;

namespace Oxide.Ext.Discord.Libraries
{
    public class Discord
    {
        private static List<DiscordClient> Clients = new List<DiscordClient>();
        public static List<string> ToClients = new List<string>();

        public static DiscordClient GetClient(string apiKey, bool autoConnect = true, bool check = false)
        {

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
                return search.First();
            if (check)
                return null;

            ToClients.Add(apiKey);
            var newClient = new DiscordClient(apiKey, autoConnect);
            Clients.Add(newClient);
            return newClient;
        }

        public static bool CloseClient(DiscordClient client)
        {
            if (client == null) return false;
            client.Disconnect();
            Clients.Remove(client);
            if (!client.IsAlive()) return true;
            else return false;
        }
    }
}
