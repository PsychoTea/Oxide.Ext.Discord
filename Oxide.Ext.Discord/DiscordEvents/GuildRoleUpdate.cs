using Oxide.Ext.Discord.DiscordObjects;

namespace Oxide.Ext.Discord.DiscordEvents
{
    public class GuildRoleUpdate
    {
        public string guild_id { get; set; }
        public Role role { get; set; }
    }
}
