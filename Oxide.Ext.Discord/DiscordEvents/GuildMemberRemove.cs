using Oxide.Ext.Discord.DiscordObjects;

namespace Oxide.Ext.Discord.DiscordEvents
{
    public class GuildMemberRemove
    {
        public string guild_id { get; set; }
        public User user { get; set; }
    }
}
