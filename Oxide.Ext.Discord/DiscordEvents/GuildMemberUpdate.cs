using System.Collections.Generic;
using Oxide.Ext.Discord.DiscordObjects;

namespace Oxide.Ext.Discord.DiscordEvents
{
    public class GuildMemberUpdate
    {
        public string guild_id { get; set; }
        public List<string> roles { get; set; }
        public User user { get; set; }
        public string nick { get; set; }
    }
}
