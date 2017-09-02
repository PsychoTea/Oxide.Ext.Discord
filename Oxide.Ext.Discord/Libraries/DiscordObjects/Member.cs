namespace Oxide.Ext.Discord.Libraries.DiscordObjects
{
    public class Member
    {
        public User user { get; set; }
        public List<string> roles { get; set; }
        public string nick { get; set; }
        public bool mute { get; set; }
        public string joined_at { get; set; }
        public bool deaf { get; set; }
    }
}
