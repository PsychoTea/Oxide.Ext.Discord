using System.Collections.Generic;
using Oxide.Ext.Discord.DiscordObjects;

namespace Oxide.Ext.Discord.DiscordEvents
{
    public class GuildMemberAdd
    {
        public User user { get; set; }
        public string nick { get; set; }
        public List<string> roles { get; set; }
        public string joined_at { get; set; }
        public bool? deaf { get; set; }
        public bool? mute { get; set; }
        public string guild_id { get; set; }
    }
}
