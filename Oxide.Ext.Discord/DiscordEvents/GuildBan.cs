namespace Oxide.Ext.Discord.DiscordEvents
{
    public class GuildBan
    {
        public string username { get; set; }

        public string id { get; set; }

        public string discriminator { get; set; }

        public string avatar { get; set; }

        public bool? bot { get; set; }

        public string guild_id { get; set; }
    }
}
