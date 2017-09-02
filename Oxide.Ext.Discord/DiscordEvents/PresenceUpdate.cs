using System.Collections.Generic;
using Oxide.Ext.Discord.DiscordObjects;

namespace Oxide.Ext.Discord.DiscordEvents
{
    public class PresenceUpdate
    {
        public User user { get; set; }
        public List<string> roles { get; set; }
        public DiscordObjects.Game game { get; set; }
        public string guild_id { get; set; }
        public string status { get; set; }
    }
}
