namespace Oxide.Ext.Discord.DiscordEvents
{
    using System.Collections.Generic;
    using Oxide.Ext.Discord.DiscordObjects;

    public class Ready
    {
        public int? v { get; set; }

        public User user { get; set; }

        public List<Channel> private_channels { get; set; }

        public List<Guild> guilds { get; set; }

        public string session_id { get; set; }

        public List<string> _trace { get; set; }
    }
}
