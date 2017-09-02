using System;
using System.Timers;

namespace Oxide.Ext.Discord.WebSockets
{
    public class UpkeepHandler
    {
        private DiscordClient Client;
        private Timer Timer;
        private DateTime LastUpdate;

        public UpkeepHandler(DiscordClient client)
        {
            if (client == null) return;

            LastUpdate = DateTime.UtcNow;
            this.Client = client;

            Timer = new Timer();
            Timer.Elapsed += CheckForBeat;
            Timer.AutoReset = true;
            Timer.Interval = 5000;
            Timer.Start();
        }

        public void SendBeat() => this.LastUpdate = DateTime.UtcNow;

        public void Shutdown()
        {
            Timer.Dispose();
            Timer = null;
        }

        private void CheckForBeat(object sender, ElapsedEventArgs args)
        {
            if (Math.Floor((DateTime.UtcNow - LastUpdate).TotalSeconds) > 5)
            {
                Discord.CloseClient(Client);
            }
        }
    }
}
