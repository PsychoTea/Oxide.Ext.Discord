namespace Oxide.Ext.Discord.DiscordObjects
{
    public class Embed
    {
        public string url { get; set; }
        public string type { get; set; }
        public string title { get; set; }
        public Thumbnail thumbnail { get; set; }
        public Provider provider { get; set; }
        public string description { get; set; }
        public Author author { get; set; }
    }
}
