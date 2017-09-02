namespace Oxide.Ext.Discord.DiscordEvents
{
    public class TypingStart
    {
        public string channel_id { get; set; }
        public string user_id { get; set; }
        public int? timestamp { get; set; }
    }
}
