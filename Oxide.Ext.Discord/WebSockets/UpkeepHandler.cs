namespace Oxide.Ext.Discord.REST
{
    using System.Linq;
    using System.Timers;

    public class UpkeepHandler
    {
        private DiscordClient client;
        private Timer guildMemberRefreshTimer;

        public UpkeepHandler(DiscordClient client)
        {
            if (client == null) return;

            this.client = client;

            guildMemberRefreshTimer = new Timer();
            guildMemberRefreshTimer.Elapsed += GuildMemberRefresh;
            guildMemberRefreshTimer.AutoReset = true;
            guildMemberRefreshTimer.Interval = 60000;
            guildMemberRefreshTimer.Start();
        }

        public void Shutdown()
        {
            guildMemberRefreshTimer.Dispose();
            guildMemberRefreshTimer = null;
        }
        
        // This is retarded
        private void GuildMemberRefresh(object sender, ElapsedEventArgs args)
        {
            client.DiscordServer.ListGuildMembers(client, guildMembers =>
            {
                client.DiscordServer.members = guildMembers.ToList();
            });
        }
    }
}
