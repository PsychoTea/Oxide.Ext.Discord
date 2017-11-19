namespace Oxide.Ext.Discord.DiscordEvents
{
    using Oxide.Ext.Discord.DiscordObjects;

    public class GuildRoleUpdate
    {
        public string guild_id { get; set; }

        public Role role { get; set; }
    }
}
