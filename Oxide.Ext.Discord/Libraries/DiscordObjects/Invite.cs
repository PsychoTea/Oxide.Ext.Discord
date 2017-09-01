using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oxide.Ext.Discord.Libraries.DiscordObjects
{
    public class Invite
    {
        public string code { get; set; }
        public Guild guild { get; set; }
        public Channel channel { get; set; }
    }
}
