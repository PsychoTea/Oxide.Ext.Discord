using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oxide.Ext.Discord.Libraries.DiscordObjects
{
    public class GuildRemove
    {
        public class User
        {
            public string username { get; set; }
            public string id { get; set; }
            public string discriminator { get; set; }
            public string avatar { get; set; }
        }
        public User user { get; set; }
        public string guild_id { get; set; }
    }
}
