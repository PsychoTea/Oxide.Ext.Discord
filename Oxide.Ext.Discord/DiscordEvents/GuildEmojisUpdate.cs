using System.Collections.Generic;
using Oxide.Ext.Discord.DiscordObjects;

namespace Oxide.Ext.Discord.DiscordEvents
{
    public class GuildEmojisUpdate
    {
        public string guild_id { get; set; }
        public List<Emoji> emojis { get; set; }
    }
}
