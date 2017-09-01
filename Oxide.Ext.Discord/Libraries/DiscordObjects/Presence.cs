using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oxide.Ext.Discord.Libraries.DiscordObjects
{
    public class Presence
    {
        public User user { get; set; }
        public string status { get; set; }
        public Game game { get; set; }
    }
}
