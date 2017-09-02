using System.Collections.Generic;
using Oxide.Ext.Discord.DiscordObjects;

namespace Oxide.Ext.Discord.DiscordEvents
{
    public class GuildMembersChunk
    {
        public string guild_id { get; set; }
        public List<GuildMember> members { get; set; }
    }
}
