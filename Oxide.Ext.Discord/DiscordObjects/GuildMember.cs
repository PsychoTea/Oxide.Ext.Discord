namespace Oxide.Ext.Discord.DiscordObjects
{
    using System.Collections.Generic;

    public class GuildMember
    {
        public User user { get; set; }

        public string nick { get; set; }

        public List<string> roles { get; set; }

        public string joined_at { get; set; }

        public bool deaf { get; set; }

        public bool mute { get; set; }
    }
}
