using System.Collections.Generic;
using System.Linq;
using Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Ext.Discord.Libraries.WebSockets;

namespace Oxide.Ext.Discord.Libraries
{
    public class Discord : Library
    {
        private static List<DiscordClient> Clients = new List<DiscordClient>();

        public static DiscordClient GetClient(string apiKey)
        {
            var search = Clients.Where(x => x.APIKey == apiKey);
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
                return search.First();
            }

            var newClient = new DiscordClient(apiKey);
            Clients.Add(newClient);
            return newClient;
        }
    }
}
