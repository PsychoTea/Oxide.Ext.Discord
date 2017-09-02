using Oxide.Ext.Discord.Exceptions;
using System;
using System.Timers;

namespace Oxide.Ext.Discord.WebSockets
{
    public class UpkeepHandler
    {
        private DiscordClient client;
        private Timer timer;
        private DateTime lastUpdated;
        public UpkeepHandler(DiscordClient client)
        {
            if (client == null) return;
            lastUpdated = DateTime.UtcNow;
            this.client = client;
            timer = new Timer();
            timer.Elapsed += CheckForBeat;
            timer.AutoReset = true;
            timer.Interval = 60000;
            timer.Start();
        }
        public void SendBeat() => this.lastUpdated = DateTime.UtcNow;
        public void Shutdown()
        {
            timer.Dispose();
            timer = null;
        }
        private void CheckForBeat(object sender, ElapsedEventArgs args)
        {
            if (Math.Floor((DateTime.UtcNow - lastUpdated).TotalSeconds) > 55)
            {
                Discord.CloseClient(client);
                throw new UpkeepFailedException(client.Settings.ApiToken);
            }
        }
    }
}
