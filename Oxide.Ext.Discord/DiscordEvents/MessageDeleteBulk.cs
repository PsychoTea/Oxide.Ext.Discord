using System.Collections.Generic;

namespace Oxide.Ext.Discord.DiscordEvents
{
    public class MessageDeleteBulk
    {
        public List<string> ids { get; set;}
        public string channel_id { get; set; }
    }
}
