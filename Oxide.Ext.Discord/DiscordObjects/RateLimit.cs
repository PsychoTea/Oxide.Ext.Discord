namespace Oxide.Ext.Discord.DiscordObjects
{
    public class RateLimit
    {
        public string message { get; set; }

        public int retry_after { get; set; }

        public bool global { get; set; }
    }
}
