using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oxide.Ext.Discord.Libraries.DiscordObjects
{
    public class Emoji
    {
        public List<Role> roles { get; set; }
        public bool require_colons { get; set; }
        public string name { get; set; }
        public bool managed { get; set; }
        public string id { get; set; }
    }
}
