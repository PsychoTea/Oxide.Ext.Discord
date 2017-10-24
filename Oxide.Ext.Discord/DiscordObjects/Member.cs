using System.Collections.Generic;

namespace Oxide.Ext.Discord.DiscordObjects
{
    public class Member
    {
        public User user { get; set; }
        public List<string> roles { get; set; }
        public string nick { get; set; }
        public bool mute { get; set; }
        public string joined_at { get; set; }
        public bool deaf { get; set; }

        public Member() { }

        public Member(GuildMember guildMember)
        {
            this.user = guildMember.user;
            this.roles = guildMember.roles;
            this.nick = guildMember.nick;
            this.mute = guildMember.mute;
            this.joined_at = guildMember.joined_at;
            this.deaf = guildMember.deaf;
        }
    }
}
