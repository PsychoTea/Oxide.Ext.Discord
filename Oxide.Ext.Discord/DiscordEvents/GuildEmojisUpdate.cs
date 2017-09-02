using System.Collections.Generic;

namespace Oxide.Ext.Discord.DiscordEvents
{
    public class GuildEmojisUpdate
    {
        public string guild_id { get; set; }
        public List<Emoji> emojis { get; set; }
    }
}
