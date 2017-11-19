using System.Linq;
using System.Timers;
using Oxide.Ext.Discord.DiscordObjects;

namespace Oxide.Ext.Discord.WebSockets
{
    public class UpkeepHandler
    {
        private DiscordClient Client;
        private Timer GuildMemberRefreshTimer;

        public UpkeepHandler(DiscordClient client)
        {
            if (client == null) return;

            this.Client = client;

            GuildMemberRefreshTimer = new Timer();
            GuildMemberRefreshTimer.Elapsed += GuildMemberRefresh;
            GuildMemberRefreshTimer.AutoReset = true;
            GuildMemberRefreshTimer.Interval = 60000;
            GuildMemberRefreshTimer.Start();
        }

        public void Shutdown()
        {
            GuildMemberRefreshTimer.Dispose();
            GuildMemberRefreshTimer = null;
        }
        
        // This is retarded
        private void GuildMemberRefresh(object sender, ElapsedEventArgs args)
        {
            Client.DiscordServer.ListGuildMembers(Client, guildMembers =>
            {
                Client.DiscordServer.members = guildMembers.ToList();
            });
        }
    }
}
